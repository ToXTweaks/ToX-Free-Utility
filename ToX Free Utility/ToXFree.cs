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
        private const string PremiumUrl = "https://toxtweaks.com/premium";

        public ToXFree()
        {
            InitializeComponent();
            InitializePages();
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            Text = "ToX Free Utility V3.1";
            MinimumSize = new Size(1010, 700);
            MaximizeBox = true;
            Welcomelbl.Text = "Hello, " + Environment.UserName;
            Welcomelbl.Size = new Size(280, 18);
            Resize += ToXFree_Resize;
        }

        private void ToggleMaximize()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                formRounder1.Rounding = 20;
                return;
            }

            MaximizedBounds = Screen.FromControl(this).WorkingArea;
            formRounder1.Rounding = 0;
            WindowState = FormWindowState.Maximized;
        }

        private void ToXFree_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Maximized)
                MaximizedBounds = Screen.FromControl(this).WorkingArea;

            LayoutContentPages();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            GameBoosterTweaks.RepairAntiCheatLaunchBlocks();
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
            ToggleMaximize();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTCLIENT = 1;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            if (m.Msg == WM_NCHITTEST && WindowState != FormWindowState.Maximized)
            {
                base.WndProc(ref m);
                if ((int)m.Result == HTCLIENT)
                {
                    int packed = m.LParam.ToInt32();
                    int screenX = (short)(packed & 0xFFFF);
                    int screenY = (short)((packed >> 16) & 0xFFFF);
                    Point cursor = PointToClient(new Point(screenX, screenY));
                    const int grip = 6;
                    bool left = cursor.X <= grip;
                    bool right = cursor.X >= ClientSize.Width - grip;
                    bool top = cursor.Y <= grip;
                    bool bottom = cursor.Y >= ClientSize.Height - grip;

                    if (left && top) m.Result = (IntPtr)HTTOPLEFT;
                    else if (right && top) m.Result = (IntPtr)HTTOPRIGHT;
                    else if (left && bottom) m.Result = (IntPtr)HTBOTTOMLEFT;
                    else if (right && bottom) m.Result = (IntPtr)HTBOTTOMRIGHT;
                    else if (left) m.Result = (IntPtr)HTLEFT;
                    else if (right) m.Result = (IntPtr)HTRIGHT;
                    else if (top) m.Result = (IntPtr)HTTOP;
                    else if (bottom) m.Result = (IntPtr)HTBOTTOM;
                }

                return;
            }

            base.WndProc(ref m);
        }
        private void minApp_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
        private void upgradeNow_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = PremiumUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                TweakExecutor.ShowMessage(
                    $"Could not open the premium page.\n\n{ex.Message}",
                    "Browser Error");
            }
        }
        #endregion
    }
}
