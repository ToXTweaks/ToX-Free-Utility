using System.Windows.Forms;
using ToX_Free_Utility_NEW_ERA.Controls;

namespace ToX_Free_Utility_NEW_ERA.Pages
{
    public partial class peripherals : UserControl
    {
        public peripherals()
        {
            InitializeComponent();
            TweakCardFactory.BuildPage(peripheralsPage,
                "Mouse and Keyboard",
                "Disable Mouse Acceleration",
                "Disable USB Selective Suspend",
                "Game Mode and HAGS",
                "Network Optimization");
        }
    }
}
