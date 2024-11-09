using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToX_Free_Utility.Tabs
{
    public partial class WindowsTweaks : Form
    {
        public WindowsTweaks()
        {
            InitializeComponent();
            InitializeComponents();
        }

        private void WindowsTweaks_Load(object sender, EventArgs e)
        {

        }
        #region -
        public void InitializeComponents()
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name;

            string exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

            if (!appName.Equals("ToX Free Utility", StringComparison.OrdinalIgnoreCase))
            {
                Environment.Exit(0);
            }

            if (!exeName.Equals("ToX Free Utility.exe", StringComparison.OrdinalIgnoreCase))
            {
                Environment.Exit(0);
            }

            if (!this.Text.Contains("ToX Free Utility"))
            {
                Environment.Exit(0);
            }
        }
        #endregion
        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            if (Main.Instance != null)
            {
                // Create an instance of the WindowsTweaksP2 form
                WindowsTweaksP2 myForm = new WindowsTweaksP2();
                myForm.TopLevel = false; // Set to false since it's being shown in a panel

                // Clear existing controls in the MainPanel
                Main.Instance.MainPanel.Controls.Clear();

                // Fade out the current content in the panel
                Main.Instance.guna2Transition1.HideSync(Main.Instance.MainPanel);

                // Show the new form and apply the transition after a short delay
                Task.Delay(1).ContinueWith(_ =>
                {
                    Main.Instance.MainPanel.Invoke((Action)(() =>
                    {
                        Main.Instance.MainPanel.Controls.Add(myForm); // Add the new form to the panel
                        myForm.Show(); // Show the new form
                        Main.Instance.guna2Transition1.ShowSync(Main.Instance.MainPanel); // Fade in the new content
                    }));
                });
            }
        }

    }
}
