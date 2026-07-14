using System.Windows.Forms;
using ToX_Free_Utility_NEW_ERA.Controls;

namespace ToX_Free_Utility_NEW_ERA.Pages
{
    public partial class graphic : UserControl
    {
        public graphic()
        {
            InitializeComponent();
            TweakCardFactory.FillPanel(nvidiaPage,
                "General Nvidia GPU Tweaks",
                "Strip Nvidia Driver",
                "General Graphics Tweaks",
                "DWM Optimization",
                "GPU Preferences Optimization");
            TweakCardFactory.FillPanel(amdPage,
                "AMD GPU Optimization");
        }
    }
}
