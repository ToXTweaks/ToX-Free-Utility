using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility_NEW_ERA.Control_Pages;
using ToX_Free_Utility_NEW_ERA.Tweaks;
using Vortex.Ui;
using PageNav = ToX_Free_Utility_NEW_ERA.Control_Pages.Pages;

namespace ToX_Free_Utility_NEW_ERA
{
    public partial class ToXFree : Form
    {
        public ToXFree()
        {
            InitializeComponent();
            InitializePages();
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            Welcomelbl.Text = "Bonjour, " + Environment.UserName;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            vortexComboBox1.SelectedIndex = 0;
            PageNav.ShowExclusive(dashboardPage);
        }
        #region Pages
        private async void gotoDashboard_Click(object sender, EventArgs e)
        {
           await PageNav.GoToPage(this, sidePanel, dashboardPage, (VortexSidebarButton)sender);
        }
        private async void gotoTweaks_Click(object sender, EventArgs e)
        {
            await PageNav.GoToPage(this, sidePanel, tweaksPage, (VortexSidebarButton)sender);
        }
        private async void gotoPeripherals_Click(object sender, EventArgs e)
        {
            await PageNav.GoToPage(this, sidePanel, peripheralsPage, (VortexSidebarButton)sender);
        }
        private async void gotoDebloat_Click(object sender, EventArgs e)
        {
           await PageNav.GoToPage(this, sidePanel, debloatPage, (VortexSidebarButton)sender);
        }
        private async void gotoGraphicTweaks_Click(object sender, EventArgs e)
        {
            await PageNav.GoToPage(this, sidePanel, graphicPage, (VortexSidebarButton)sender);
        }
        private async void gotoBiosTweaks_Click(object sender, EventArgs e)
        {
            await PageNav.GoToPage(this, sidePanel, biosPage, (VortexSidebarButton)sender);
        }
        private async void gotoGameBooster_Click(object sender, EventArgs e)
        {
            await PageNav.GoToPage(this, sidePanel, gameboosterPage, (VortexSidebarButton)sender);
        }
        private async void gotoAdvanced_Click(object sender, EventArgs e)
        {
            await PageNav.GoToPage(this, sidePanel, advancedPage, (VortexSidebarButton)sender);
        }
        #endregion
        #region Control box
        private void endApp_Click(object sender, EventArgs e)
        {
            Environment.Exit(-1);
        }
        private void maxApp_Click(object sender, EventArgs e)
        {
            //WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }
        private void minApp_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
        #endregion
    }
}
