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
            homecircle.Visible = true;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            cpucircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            homebutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            cpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
        }

        private void winbutton_Click(object sender, EventArgs e)
        {
            homecircle.Visible = false;
            wincircle.Visible = true;
            debloatcircle.Visible = false;
            cpucircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            winbutton.FillColor = Color.FromArgb(40, 40, 40);
            homebutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            cpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
        }

        private void debloatbutton_Click(object sender, EventArgs e)
        {
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = true;
            cpucircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            debloatbutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            cpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
        }

        private void cpubutton_Click(object sender, EventArgs e)
        {
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            cpucircle.Visible = true;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            cpubutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
        }

        private void gpubutton_Click(object sender, EventArgs e)
        {
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            cpucircle.Visible = false;
            gpucircle.Visible = true;
            gamingcircle.Visible = false;
            infocircle.Visible = false;
            gpubutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            cpubutton.FillColor = Color.FromArgb(33, 33, 33);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
        }

        private void gamingbutton_Click(object sender, EventArgs e)
        {
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            cpucircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = true;
            infocircle.Visible = false;
            gamingbutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            cpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            homebutton.FillColor = Color.FromArgb(33, 33, 33);
            infobutton.FillColor = Color.FromArgb(33, 33, 33);
        }

        private void infobutton_Click(object sender, EventArgs e)
        {
            homecircle.Visible = false;
            wincircle.Visible = false;
            debloatcircle.Visible = false;
            cpucircle.Visible = false;
            gpucircle.Visible = false;
            gamingcircle.Visible = false;
            infocircle.Visible = true;
            infobutton.FillColor = Color.FromArgb(40, 40, 40);
            winbutton.FillColor = Color.FromArgb(33, 33, 33);
            debloatbutton.FillColor = Color.FromArgb(33, 33, 33);
            cpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gpubutton.FillColor = Color.FromArgb(33, 33, 33);
            gamingbutton.FillColor = Color.FromArgb(33, 33, 33);
            homebutton.FillColor = Color.FromArgb(33, 33, 33);
        }
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
    }
}
