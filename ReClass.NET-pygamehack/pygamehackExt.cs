using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReClassNET.Forms;
using ReClassNET.Memory;
using ReClassNET.Nodes;
using ReClassNET.Plugins;
using ReClassNET.UI;

namespace pygamehack
{
	public class pygamehackExt : Plugin
	{

		public static string BasicTypePrefix = "gh.";
		public static bool GeneratePython = true;

		private CheckBox pygamehackToggle;
		private TextBox pygamehackPrefix;

		public override Image Icon => Properties.Resources.logo_v;

		public override CustomNodeTypes GetCustomNodeTypes()
		{
			return new CustomNodeTypes
			{
				CodeGenerator = new pygamehackCodeGenerator()
			};
		}

		public override bool Initialize(IPluginHost host)
		{
			GlobalWindowManager.WindowAdded += OnWindowAdded;
			return true;
		}

		public override void Terminate()
		{
			GlobalWindowManager.WindowAdded -= OnWindowAdded;
		}

		private void OnWindowAdded(object sender, GlobalWindowManagerEventArgs e)
		{
			if (e.Form is SettingsForm settingsForm)
			{
				settingsForm.Shown += delegate (object sender2, EventArgs e2)
				{
					try
					{
						var settingsTabControl = settingsForm.Controls.Find("settingsTabControl", true).FirstOrDefault() as TabControl;
						if (settingsTabControl != null)
						{
							var newTab = new TabPage("# pygamehack")
							{
								UseVisualStyleBackColor = true
							};
							
							setupSettingsTab(newTab);

							settingsTabControl.TabPages.Add(newTab);
						}
					}
					catch
					{

					}
				};
			}
		}

		private void setupSettingsTab(TabPage settings)
		{
			pygamehackToggle = new CheckBox
			{
				Text = "Generate Python instead of C++"

			};
			pygamehackToggle.AutoSize = true;
			pygamehackToggle.Click += pygamehackToggle_Click;
			pygamehackToggle.Checked = GeneratePython;
			settings.Controls.Add(pygamehackToggle);


			var label = new Label();
			label.Text = "Prefix for basic types:";
			label.AutoSize = true;
			label.Anchor = AnchorStyles.Right;
			label.TextAlign = ContentAlignment.MiddleLeft;
			label.Dock = System.Windows.Forms.DockStyle.Right;
			settings.Controls.Add(label);

			pygamehackPrefix = new TextBox
			{
				Text = BasicTypePrefix
			};
			pygamehackPrefix.Anchor = AnchorStyles.Right;
			pygamehackPrefix.AutoSize = true;
			pygamehackPrefix.AcceptsReturn = true;
			pygamehackPrefix.AcceptsTab = true;
			pygamehackPrefix.Multiline = false;
			pygamehackPrefix.Dock = System.Windows.Forms.DockStyle.Right;
			pygamehackPrefix.TextChanged += pygamehackPrefix_TextChanged;
			settings.Controls.Add(pygamehackPrefix);
		}

		private void pygamehackToggle_Click(object sender, System.EventArgs e)
		{
			// The CheckBox control's Text property is changed each time the
			// control is clicked, indicating a checked or unchecked state.  
			if (pygamehackToggle.Checked)
			{
				GeneratePython = true;
			}
			else
			{
				GeneratePython = false;
			}
		}


		private void pygamehackPrefix_TextChanged(object sender, System.EventArgs e)
		{
			// TODO: Validate input
			BasicTypePrefix = pygamehackPrefix.Text;
		}
	}
}
