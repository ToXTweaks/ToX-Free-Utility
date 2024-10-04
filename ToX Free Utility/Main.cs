using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility.Properties;

namespace ToX_Free_Utility
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Show Home Form
            Home HomeForm = new Home();

            // Set TopLevel property to false to indicate that it's a non-top-level control
            HomeForm.TopLevel = false;
            MainPanel.Controls.Clear();  // Clear existing controls

            // Add the form to the Controls collection of the panel
            MainPanel.Controls.Add(HomeForm);

            // Show the form
            HomeForm.Show();
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
            cpucircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            // nav bar colors
            homebutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            cpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
            // nav bar icons setup
            homebutton.Image = Resources.homew;
            winbutton.Image = Resources.wing;
            debloatbutton.Image = Resources.debloatg;
            cpubutton.Image = Resources.cpug;
            gpubutton.Image = Resources.gpug;
            infobutton.Image = Resources.infog;
            gamingbutton.Image = Resources.gamingg;
            // Show Home Form
            Home HomeForm = new Home();

            // Set TopLevel property to false to indicate that it's a non-top-level control
            HomeForm.TopLevel = false;
            MainPanel.Controls.Clear();  // Clear existing controls

            // Add the form to the Controls collection of the panel
            MainPanel.Controls.Add(HomeForm);

            // Show the form
            HomeForm.Show();
        }

        private void winbutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = false;
            wincircle.Visible = true;
            debloatcircle.Visible = false;
            cpucircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            // nav bar colors
            winbutton.FillColor = Color.FromArgb(40, 40, 40);
            homebutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            cpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
            // nav bar icons setup
            homebutton.Image = Resources.homeg;
            winbutton.Image = Resources.winw;
            debloatbutton.Image = Resources.debloatg;
            cpubutton.Image = Resources.cpug;
            gpubutton.Image = Resources.gpug;
            infobutton.Image = Resources.infog;
            gamingbutton.Image = Resources.gamingg;
        }

        private void debloatbutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = true;
            cpucircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            // nav bar colors
            debloatbutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            cpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
            // nav bar icons setup
            homebutton.Image = Resources.homeg;
            winbutton.Image = Resources.wing;
            debloatbutton.Image = Resources.debloatw;
            cpubutton.Image = Resources.cpug;
            gpubutton.Image = Resources.gpug;
            infobutton.Image = Resources.infog;
            gamingbutton.Image = Resources.gamingg;
        }

        private void cpubutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            cpucircle.Visible = true;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            // nav bar colors
            cpubutton.FillColor = Color.FromArgb(40, 40, 40);
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
            cpubutton.Image = Resources.cpuw;
            gpubutton.Image = Resources.gpug;
            infobutton.Image = Resources.infog;
            gamingbutton.Image = Resources.gamingg;
        }

        private void gpubutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            cpucircle.Visible = false;
            gpucircle.Visible = true;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            // nav bar colors
            gpubutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            cpubutton.FillColor = Color.FromArgb(33, 33, 33);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
            // nav bar icons setup
            homebutton.Image = Resources.homeg;
            winbutton.Image = Resources.wing;
            debloatbutton.Image = Resources.debloatg;
            cpubutton.Image = Resources.cpug;
            gpubutton.Image = Resources.gpuw;
            infobutton.Image = Resources.infog;
            gamingbutton.Image = Resources.gamingg;
        }

        private void gamingbutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            cpucircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = true;
            infocircle.Visible = false;
            // nav bar colors
            gamingbutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            cpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            homebutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
            // nav bar icons setup
            homebutton.Image = Resources.homeg;
            winbutton.Image = Resources.wing;
            debloatbutton.Image = Resources.debloatg;
            cpubutton.Image = Resources.cpug;
            gpubutton.Image = Resources.gpug;
            infobutton.Image = Resources.infog;
            gamingbutton.Image = Resources.gamingw;
        }

        private void infobutton_Click(object sender, EventArgs e)
        {
            // nav bar configuration
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            cpucircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = true;
            // nav bar colors
            infobutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            cpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            homebutton.FillColor = Color.FromArgb(33, 33, 33);
            // nav bar icons setup
            homebutton.Image = Resources.homeg;
            winbutton.Image = Resources.wing;
            debloatbutton.Image = Resources.debloatg;
            cpubutton.Image = Resources.cpug;
            gpubutton.Image = Resources.gpug;
            infobutton.Image = Resources.infow;
            gamingbutton.Image = Resources.gamingg;
        }
        // nav bar circle decoration
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

        private void cpubutton_MouseEnter(object sender, EventArgs e)
        {
            cpucircle.BackColor = Color.FromArgb(72, 72, 72);
        }

        private void cpubutton_MouseLeave(object sender, EventArgs e)
        {
            if (cpucircle.Visible)
            {
                cpucircle.BackColor = Color.FromArgb(40, 40, 40);
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
    }
}
