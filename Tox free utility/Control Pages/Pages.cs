using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vortex.Ui;

namespace ToX_Free_Utility_NEW_ERA.Control_Pages
{
    internal static class Pages
    {
        private static readonly Color ActiveColor = Color.FromArgb(4, 120, 251);

        public static void ShowExclusive(Control page)
        {
            page.BringToFront();
        }

        public static Task GoToPage(Form form, Control container, Control page, VortexSidebarButton clickedButton)
        {
            foreach (VortexSidebarButton button in container.Controls.OfType<VortexSidebarButton>())
            {
                bool active = button == clickedButton;
                button.ShowSeparator = active;
                button.TextColor = active ? ActiveColor : Color.White;
                button.IconColor = active ? ActiveColor : Color.White;

                if (active)
                    ShowExclusive(page);
            }

            return Task.CompletedTask;
        }
    }
}
