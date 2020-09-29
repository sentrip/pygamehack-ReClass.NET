using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using ReClassNET.CodeGenerator;
using ReClassNET.Logger;
using ReClassNET.Nodes;


namespace pygamehack
{
	public class pygamehackCodeGenerator : CustomCppCodeGenerator
	{
		private bool WritingFirstClass = false;

		private static Dictionary<Type, string> nodeTypeToTypeDefinationMap = new Dictionary<Type, string>
			{
				[typeof(BoolNode)] = "bool",
				[typeof(DoubleNode)] = "double",
				[typeof(FloatNode)] = "float",
				[typeof(Int8Node)] = "int8",
				[typeof(Int16Node)] = "int16",
				[typeof(Int32Node)] = "int32",
				[typeof(Int64Node)] = "int64",
				[typeof(UInt8Node)] = "uint8",
				[typeof(UInt16Node)] = "uint16",
				[typeof(UInt32Node)] = "uint32",
				[typeof(UInt64Node)] = "uint64",
				[typeof(Utf8TextNode)] = "RawString",
				[typeof(Utf8TextPtrNode)] = "c_str",
		};

		public override void OnGenerationBegin()
		{
			WritingFirstClass = true;
		}

		public override bool CanHandleClass(ClassNode @class)
		{
			return pygamehackExt.GeneratePython;
		}

		public override bool CanHandle(BaseNode node)
		{
			return pygamehackExt.GeneratePython;
		}

		public override BaseNode TransformNode(BaseNode node)
		{
			return node;
		}

		public override void WriteClass(IndentedTextWriter writer, ClassNode @class, IEnumerable<ClassNode> classes, ILogger logger)
		{
			if (WritingFirstClass)
			{
				WritePrefix(writer);
			}
			WritingFirstClass = false;

			// Do not write classes that are aliases to basic types (used in pointers)
			if (nodeTypeToTypeDefinationMap.ContainsValue(@class.Name))
			{
				return;
			}

			writer.Write("class ");
			writer.Write(@class.Name);
			writer.Write($"(metaclass=HackStruct, size={@class.MemorySize}):");
			writer.WriteLine();
		}

		public override bool WriteNode(IndentedTextWriter writer, BaseNode node, WriteNodeFunc defaultWriteNodeFunc, ILogger logger)
		{
			// pygamehack does not need padding variables
			if (node.Name.StartsWith("pad"))
			{
				return true;
			}

			// Name and type hint prefix
			writer.Write(node.Name);
			writer.Write(": ");

			// Basic type
			var simpleTypeNode = GetSimpleType(node);
			if (simpleTypeNode != null)
			{
				writer.Write(simpleTypeNode);
			}
			// Class Instance
			else if (node is ClassInstanceNode classInstanceNode)
			{
				writer.Write(classInstanceNode.InnerNode.Name);
			}
			// Pointer
			else if (node is PointerNode pointerNode)
			{
				WritePointer(writer, pointerNode);
			}

			// Offset
			writer.Write(" = 0x");
			writer.Write($"{node.Offset:X}");
			writer.WriteLine();

			return true;
		}

		private void WritePointer(IndentedTextWriter writer, PointerNode node)
		{
			writer.Write("Ptr[");
			// Pointer to class or to aliased basic type
			if (node.InnerNode is ClassInstanceNode ptrInstanceNode)
			{
				var className = ptrInstanceNode.InnerNode.Name;
				// Pointer to basic type (Class with name of basic type)
				if (nodeTypeToTypeDefinationMap.ContainsValue(className))
				{
					writer.Write($"{pygamehackExt.BasicTypePrefix}{className}");
				}
				// Pointer to class
				else
				{
					writer.Write(className);
				}
			}
			// Pointer to basic type
			var simpleTypePtr = GetSimpleType(node);
			if (simpleTypePtr != null)
			{
				writer.Write(simpleTypePtr);
			}
			// Closing bracket
			writer.Write("]");
		}

		private void WritePrefix(IndentedTextWriter writer)
		{
			writer.Write("from pygamehack_extensions.raw_string import c_str, RawString");
			writer.WriteLine();
			writer.WriteLine();
			string ptr_size = System.Environment.Is64BitProcess ? "64" : "32";
			writer.Write($"HackStruct.set_ptr_type({ptr_size})");
			writer.WriteLine();
			writer.WriteLine();
			writer.WriteLine();
		}

		private string GetSimpleType(BaseNode node)
		{
			if (nodeTypeToTypeDefinationMap.TryGetValue(node.GetType(), out var simpleType))
			{
				return pygamehackExt.BasicTypePrefix + simpleType;
			}
			return null;
		}
	}
}
