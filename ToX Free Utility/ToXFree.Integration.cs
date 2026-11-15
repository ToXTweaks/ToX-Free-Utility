using System;
using System.Drawing;
using System.Windows.Forms;
using ToX_Free_Utility_NEW_ERA.Pages;

namespace ToX_Free_Utility_NEW_ERA
{
    public partial class ToXFree
    {
        public dashboard dashboardPage;
        public windowstweaks1 tweaksPage;
        public peripherals peripheralsPage;
        public debloat debloatPage;
        public graphic graphicPage;
        public biostweaks biosPage;
        public gamebooster gameboosterPage;
        public advanced advancedPage;

        public Vortex.Ui.VortexLabel Welcomelbl => dashboardPage.Welcomelbl;
        public Vortex.Ui.VortexComboBox vortexComboBox1 => biosPage.vortexComboBox1;
        public Vortex.Ui.VortexCheckBox BingBloatware_CheckBox => debloatPage.BingBloatware_CheckBox;
        public Vortex.Ui.VortexSidebarButton installApps => debloatPage.installApps;
        public Vortex.Ui.VortexCheckBox M3D_Builder_CheckBox => debloatPage.M3D_Builder_CheckBox;
        public Vortex.Ui.VortexCheckBox Microsoft3DViewer_CheckBox => debloatPage.Microsoft3DViewer_CheckBox;
        public Vortex.Ui.VortexCheckBox MicrosoftEdge_CheckBox => debloatPage.MicrosoftEdge_CheckBox;
        public Vortex.Ui.VortexSidebarButton uninstallApps => debloatPage.uninstallApps;
        public Vortex.Ui.VortexCheckBox WindowsAlarms_CheckBox => debloatPage.WindowsAlarms_CheckBox;
        public Vortex.Ui.VortexCheckBox WindowsCamera_CheckBox => debloatPage.WindowsCamera_CheckBox;
        public Vortex.Ui.VortexCheckBox WindowsMaps_CheckBox => debloatPage.WindowsMaps_CheckBox;
        public Vortex.Ui.VortexCheckBox WindowsPhone_CheckBox => debloatPage.WindowsPhone_CheckBox;
        public Vortex.Ui.VortexCheckBox WindowsSoundRecorder_CheckBox => debloatPage.WindowsSoundRecorder_CheckBox;
        public Vortex.Ui.VortexCheckBox XboxApp_CheckBox => debloatPage.XboxApp_CheckBox;
        public Vortex.Ui.VortexPictureBox GameCurrent => gameboosterPage.GameCurrent;
        public System.Windows.Forms.Label GameName => gameboosterPage.GameName;
        public Vortex.Ui.VortexPictureBox GameNext => gameboosterPage.GameNext;
        public Vortex.Ui.VortexRoundTextBox gamePath => gameboosterPage.gamePath;
        public Vortex.Ui.VortexPictureBox GamePrev => gameboosterPage.GamePrev;
        public Vortex.Ui.VortexSidebarButton optimizeGame => gameboosterPage.optimizeGame;
        public Vortex.Ui.VortexRoundTextBox searchGame => gameboosterPage.searchGame;

        private void InitializePages()
        {
            SuspendLayout();

            dashboardPage = new dashboard();
            AddContentPage(dashboardPage, "dashboardPage");

            tweaksPage = new windowstweaks1();
            AddContentPage(tweaksPage, "tweaksPage");

            peripheralsPage = new peripherals();
            AddContentPage(peripheralsPage, "peripheralsPage");

            debloatPage = new debloat();
            AddContentPage(debloatPage, "debloatPage");

            graphicPage = new graphic();
            AddContentPage(graphicPage, "graphicPage");

            biosPage = new biostweaks();
            AddContentPage(biosPage, "biosPage");

            gameboosterPage = new gamebooster();
            AddContentPage(gameboosterPage, "gameboosterPage");

            advancedPage = new advanced();
            AddContentPage(advancedPage, "advancedPage");

            sidePanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            LayoutContentPages();
            ResumeLayout(false);
        }

        private void AddContentPage(UserControl page, string name)
        {
            page.Name = name;
            page.MaximumSize = Size.Empty;
            page.MinimumSize = new Size(640, 480);
            if (page.Controls.Count > 0)
                page.Controls[0].Dock = DockStyle.Fill;

            Controls.Add(page);
        }

        internal void LayoutContentPages()
        {
            if (dashboardPage == null || sidePanel == null)
                return;

            sidePanel.Height = ClientSize.Height;

            int x = sidePanel.Width + 10;
            int y = 24;
            int width = Math.Max(694, ClientSize.Width - x - 16);
            int height = Math.Max(480, ClientSize.Height - y - 24);
            var bounds = new Rectangle(x, y, width, height);

            dashboardPage.Bounds = bounds;
            tweaksPage.Bounds = bounds;
            peripheralsPage.Bounds = bounds;
            debloatPage.Bounds = bounds;
            graphicPage.Bounds = bounds;
            biosPage.Bounds = bounds;
            gameboosterPage.Bounds = bounds;
            advancedPage.Bounds = bounds;
        }
    }
}
