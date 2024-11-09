using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToX_Free_Utility.Tabs
{
    public partial class WindowsTweaksP2 : Form
    {
        public WindowsTweaksP2()
        {
            InitializeComponent();
        }

        private void WindowsTweaks_Load(object sender, EventArgs e)
        {

        }

        private void guna2ImageButton2_Click(object sender, EventArgs e)
        {
            if (Main.Instance != null)
            {
                // Create an instance of the WindowsTweaksP2 form
                WindowsTweaks myForm = new WindowsTweaks();
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

        private void NextPage_Click(object sender, EventArgs e)
        {
            if (Main.Instance != null)
            {
                // Create an instance of the WindowsTweaksP2 form
                WindowsTweaksP3 myForm = new WindowsTweaksP3();
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
