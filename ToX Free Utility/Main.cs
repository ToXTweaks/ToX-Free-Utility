using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility.Properties;
using ToX_Free_Utility.Tabs;

namespace ToX_Free_Utility
{
    public partial class Main : Form
    {
        public static Main Instance { get; private set; }
        public Main()
        {
            InitializeComponent();
            InitializeComponents();
            Instance = this;
            //ensures that the form will be on top when it loads
            this.TopMost = true;
            this.TopMost = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Show Home Form
            Home HomeForm = new Home();

            // Set TopLevel property to false to indicate that it's a non-top-level control
            HomeForm.TopLevel = false;
            MainPanel.Controls.Clear();  // Clear existing controls
            // Fade out the current content in the panel
            guna2Transition1.HideSync(MainPanel);

            // Show the new form and apply the transition after a short delay
            Task.Delay(1).ContinueWith(_ =>
            {
                MainPanel.Invoke((Action)(() =>
                {
                    MainPanel.Controls.Add(HomeForm); // Add the new form to the panel
                    HomeForm.Show(); // Show the new form
                    guna2Transition1.ShowSync(MainPanel); // Fade in the new content
                }));
            });
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void guna2ImageButton2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized; // Minimize the form
        }

        private void homebutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = true;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            usbcircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            // nav bar colors
            homebutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            usbbutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
            // nav bar icons setup
            homebutton.Image = Resources.homew;
            winbutton.Image = Resources.wing;
            debloatbutton.Image = Resources.debloatg;
            usbbutton.Image = Resources.usbg;
            gpubutton.Image = Resources.gpug;
            infobutton.Image = Resources.infog;
            gamingbutton.Image = Resources.gamingg;
            // Show Home Form
            Home HomeForm = new Home();

            // Set TopLevel property to false to indicate that it's a non-top-level control
            HomeForm.TopLevel = false;
            MainPanel.Controls.Clear();  // Clear existing controls
            // Fade out the current content in the panel
            guna2Transition1.HideSync(MainPanel);

            // Show the new form and apply the transition after a short delay
            Task.Delay(1).ContinueWith(_ =>
            {
                MainPanel.Invoke((Action)(() =>
                {
                    MainPanel.Controls.Add(HomeForm); // Add the new form to the panel
                    HomeForm.Show(); // Show the new form
                    guna2Transition1.ShowSync(MainPanel); // Fade in the new content
                }));
            });
        }

        private void winbutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = false;
            wincircle.Visible = true;
            debloatcircle.Visible = false;
            usbcircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            // nav bar colors
            winbutton.FillColor = Color.FromArgb(40, 40, 40);
            homebutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            usbbutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
            // nav bar icons setup
            homebutton.Image = Resources.homeg;
            winbutton.Image = Resources.winw;
            debloatbutton.Image = Resources.debloatg;
            usbbutton.Image = Resources.usbg;
            gpubutton.Image = Resources.gpug;
            infobutton.Image = Resources.infog;
            gamingbutton.Image = Resources.gamingg;
            // Create an instance of the WindowsTweaks form
            WindowsTweaks win = new WindowsTweaks();
            win.TopLevel = false; // Set to false since it's being shown in a panel

            // Clear existing controls in the MainPanel
            MainPanel.Controls.Clear();

            // Fade out the current content in the panel
            guna2Transition1.HideSync(MainPanel);

            // Show the new form and apply the transition after a short delay
            Task.Delay(1).ContinueWith(_ =>
            {
                MainPanel.Invoke((Action)(() =>
                {
                    MainPanel.Controls.Add(win); // Add the new form to the panel
                    win.Show(); // Show the new form
                    guna2Transition1.ShowSync(MainPanel); // Fade in the new content
                }));
            });
        }

        private void debloatbutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = true;
            usbcircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            // nav bar colors
            debloatbutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            usbbutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
            // nav bar icons setup
            homebutton.Image = Resources.homeg;
            winbutton.Image = Resources.wing;
            debloatbutton.Image = Resources.debloatw;
            usbbutton.Image = Resources.usbg;
            gpubutton.Image = Resources.gpug;
            infobutton.Image = Resources.infog;
            gamingbutton.Image = Resources.gamingg;
            // Show Debloat Form
            Debloat deb = new Debloat();

            // Set TopLevel property to false to indicate that it's a non-top-level control
            deb.TopLevel = false;
            MainPanel.Controls.Clear();  // Clear existing controls

            // Fade out the current content in the panel
            guna2Transition1.HideSync(MainPanel);

            // Show the new form and apply the transition after a short delay
            Task.Delay(1).ContinueWith(_ =>
            {
                MainPanel.Invoke((Action)(() =>
                {
                    MainPanel.Controls.Add(deb); // Add the new form to the panel
                    deb.Show(); // Show the new form
                    guna2Transition1.ShowSync(MainPanel); // Fade in the new content
                }));
            });
        }

        private void usbbutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            usbcircle.Visible = true;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            // nav bar colors
            usbbutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
            // nav bar icons setup
            homebutton.Image = Resources.homeg;
            winbutton.Image = Resources.wing;
            debloatbutton.Image = Resources.debloatg;
            usbbutton.Image = Resources.usbw;
            gpubutton.Image = Resources.gpug;
            infobutton.Image = Resources.infog;
            gamingbutton.Image = Resources.gamingg;
            // Show USB Form
            peripherals usb = new peripherals();

            // Set TopLevel property to false to indicate that it's a non-top-level control
            usb.TopLevel = false;

            // Clear existing controls in the MainPanel twice
            MainPanel.Controls.Clear();  // First clear

            // Add the form to the Controls collection of the panel
            MainPanel.Controls.Add(usb); // Add the new form to the panel

            // Fade out the current content in the panel
            guna2Transition1.HideSync(MainPanel);
            usb.Show();
            // Show the new form and apply the transition after a short delay
            Task.Delay(1).ContinueWith(_ =>
            {
                MainPanel.Invoke((Action)(() =>
                {
                    // Clear the panel again to reset the Guna scrollbar
                    MainPanel.Controls.Clear();  // Second clear
                    MainPanel.Controls.Add(usb); // Re-add the new form to the panel
                    usb.Show(); // Show the new form
                    guna2Transition1.ShowSync(MainPanel); // Fade in the new content
                }));
            });
        }

        private void gpubutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            usbcircle.Visible = false;
            gpucircle.Visible = true;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            // nav bar colors
            gpubutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            usbbutton.FillColor = Color.FromArgb(33, 33, 33);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
            // nav bar icons setup
            homebutton.Image = Resources.homeg;
            winbutton.Image = Resources.wing;
            debloatbutton.Image = Resources.debloatg;
            usbbutton.Image = Resources.usbg;
            gpubutton.Image = Resources.gpuw;
            infobutton.Image = Resources.infog;
            gamingbutton.Image = Resources.gamingg;
            // Show GPU Form
            GPUTweaks gpu = new GPUTweaks();

            // Set TopLevel property to false to indicate that it's a non-top-level control
            gpu.TopLevel = false;

            // Clear existing controls in the MainPanel twice
            MainPanel.Controls.Clear();  // First clear

            // Add the form to the Controls collection of the panel
            MainPanel.Controls.Add(gpu); // Add the new form to the panel

            // Fade out the current content in the panel
            guna2Transition1.HideSync(MainPanel);
            gpu.Show();
            // Show the new form and apply the transition after a short delay
            Task.Delay(1).ContinueWith(_ =>
            {
                MainPanel.Invoke((Action)(() =>
                {
                    // Clear the panel again to reset the Guna scrollbar
                    MainPanel.Controls.Clear();  // Second clear
                    MainPanel.Controls.Add(gpu); // Re-add the new form to the panel
                    gpu.Show(); // Show the new form
                    guna2Transition1.ShowSync(MainPanel); // Fade in the new content
                }));
            });

        }

        private void gamingbutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            usbcircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = true;
            infocircle.Visible = false;
            // nav bar colors
            gamingbutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            usbbutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            homebutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
            // nav bar icons setup
            homebutton.Image = Resources.homeg;
            winbutton.Image = Resources.wing;
            debloatbutton.Image = Resources.debloatg;
            usbbutton.Image = Resources.usbg;
            gpubutton.Image = Resources.gpug;
            infobutton.Image = Resources.infog;
            gamingbutton.Image = Resources.gamingw;
            // Show Gaming Form
            GameBoosters game = new GameBoosters();

            // Set TopLevel property to false to indicate that it's a non-top-level control
            game.TopLevel = false;

            // Clear existing controls in the MainPanel twice
            MainPanel.Controls.Clear();  // First clear

            // Add the form to the Controls collection of the panel
            MainPanel.Controls.Add(game); // Add the new form to the panel

            // Fade out the current content in the panel
            guna2Transition1.HideSync(MainPanel);
            game.Show();
            // Show the new form and apply the transition after a short delay
            Task.Delay(1).ContinueWith(_ =>
            {
                MainPanel.Invoke((Action)(() =>
                {
                    // Clear the panel again to reset the Guna scrollbar
                    MainPanel.Controls.Clear();  // Second clear
                    MainPanel.Controls.Add(game); // Re-add the new form to the panel
                    game.Show(); // Show the new form
                    guna2Transition1.ShowSync(MainPanel); // Fade in the new content
                }));
            });
        }

        private void infobutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            usbcircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = true;

            // nav bar colors
            infobutton.FillColor = Color.FromArgb(40, 40, 40);
            homebutton.FillColor = Color.FromArgb(33, 33, 33);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            usbbutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);

            // nav bar icons setup
            homebutton.Image = Resources.homeg;
            winbutton.Image = Resources.wing;
            debloatbutton.Image = Resources.debloatg;
            usbbutton.Image = Resources.usbg;
            gpubutton.Image = Resources.gpug;
            infobutton.Image = Resources.infow;  // Assuming you have an "infow" icon
            gamingbutton.Image = Resources.gamingg;

            // Show Info Form
            Info infoForm = new Info();

            // Set TopLevel property to false to indicate that it's a non-top-level control
            infoForm.TopLevel = false;

            // Clear existing controls in the MainPanel
            MainPanel.Controls.Clear();

            // Fade out the current content in the panel
            guna2Transition1.HideSync(MainPanel);

            // Show the new form and apply the transition after a short delay
            Task.Delay(1).ContinueWith(_ =>
            {
                MainPanel.Invoke((Action)(() =>
                {
                    MainPanel.Controls.Add(infoForm); // Add the new form to the panel
                    infoForm.Show(); // Show the new form
                    guna2Transition1.ShowSync(MainPanel); // Fade in the new content
                }));
            });
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
        private void homebutton_MouseEnter(object sender, EventArgs e)
        {
            homecircle.BackColor = Color.FromArgb(72, 72, 72);
        }

        private void homebutton_MouseLeave(object sender, EventArgs e)
        {
            if (homecircle.Visible) 
            {
                homecircle.BackColor = Color.FromArgb(40, 40, 40);
            }
        }

        private void winbutton_MouseEnter(object sender, EventArgs e)
        {
            wincircle.BackColor = Color.FromArgb(72, 72, 72);
        }

        private void winbutton_MouseLeave(object sender, EventArgs e)
        {
            if (wincircle.Visible)
            {
                wincircle.BackColor = Color.FromArgb(40, 40, 40);
            }
        }

        private void debloatbutton_MouseEnter(object sender, EventArgs e)
        {
            debloatcircle.BackColor = Color.FromArgb(72, 72, 72);
        }

        private void debloatbutton_MouseLeave(object sender, EventArgs e)
        {
            if (debloatcircle.Visible)
            {
                debloatcircle.BackColor = Color.FromArgb(40, 40, 40);
            }
        }

        private void usbbutton_MouseEnter(object sender, EventArgs e)
        {
            usbcircle.BackColor = Color.FromArgb(72, 72, 72);
        }

        private void usbbutton_MouseLeave(object sender, EventArgs e)
        {
            if (usbcircle.Visible)
            {
                usbcircle.BackColor = Color.FromArgb(40, 40, 40);
            }
        }

        private void gpubutton_MouseEnter(object sender, EventArgs e)
        {
            gpucircle.BackColor = Color.FromArgb(72, 72, 72);

        }

        private void gpubutton_MouseLeave(object sender, EventArgs e)
        {
            if (gpucircle.Visible)
            {
                gpucircle.BackColor = Color.FromArgb(40, 40, 40);
            }
        }

        private void gamingbutton_MouseEnter(object sender, EventArgs e)
        {
            gamingcircle.BackColor = Color.FromArgb(72, 72, 72);

        }

        private void gamingbutton_MouseLeave(object sender, EventArgs e)
        {
            if (gamingcircle.Visible)
            {
                gamingcircle.BackColor = Color.FromArgb(40, 40, 40);
            }
        }

        private async void premiumbutton_Click(object sender, EventArgs e)
        {
            await Purchase.ShowModal(this, new Purchase());
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            //ensures that the form will be on top when it loads
            this.TopMost = true;
            this.TopMost = false;
            InitializeComponents();
        }

        private void LoadingFormsPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void infobutton_MouseEnter(object sender, EventArgs e)
        {
            infocircle.BackColor = Color.FromArgb(72, 72, 72);
        }

        private void infobutton_MouseLeave(object sender, EventArgs e)
        {
            if (infocircle.Visible)
            {
                infocircle.BackColor = Color.FromArgb(40, 40, 40);
            }
        }

    }
}
