using System.Windows.Forms;
using ToX_Free_Utility_NEW_ERA.Controls;
using ToX_Free_Utility_NEW_ERA.Tweaks;

namespace ToX_Free_Utility_NEW_ERA.Pages
{
    public partial class windowstweaks1 : UserControl
    {
        public windowstweaks1()
        {
            InitializeComponent();
            TweakCardFactory.BuildCategoryPage(tweaksPage1, TweakConfigType.WindowsTweaks);
        }
    }
}
