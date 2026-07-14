using System;
using System.Diagnostics;
using System.Windows.Forms;
using ToX_Free_Utility_NEW_ERA.Tweaks;
using Vortex.Ui;

namespace ToX_Free_Utility_NEW_ERA.Pages
{
    public partial class biostweaks : UserControl
    {
        private const string PremiumUrl = "https://toxtweaks.com";

        public biostweaks()
        {
            InitializeComponent();
        }

        private void restartToBios_Click(object sender, EventArgs e)
        {
            if (!TweakExecutor.IsRunningAsAdministrator())
            {
                TweakExecutor.ShowMessage(
                    "Restarting into BIOS/UEFI requires administrator privileges. Restart the app as administrator.",
                    "Administrator Required");
                return;
            }

            DialogResult confirm = VortexMessageBox.Show(
                "Your PC will restart in 5 seconds and attempt to open the UEFI firmware settings.\n\nSave your work first.",
                "Restart to BIOS",
                VortexMessageBox.MessageBoxButtons.YesNo,
                true);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown.exe",
                    Arguments = "/r /fw /t 5",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                TweakExecutor.ShowMessage(
                    $"Could not schedule firmware restart.\n\n{ex.Message}\n\nEnter BIOS manually (Del, F2, or F12 during boot).",
                    "Restart Failed");
            }
        }

        private void goPremiumBios_Click(object sender, EventArgs e)
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
                TweakExecutor.ShowMessage($"Could not open premium page.\n\n{ex.Message}", "Browser Error");
            }
        }
    }
}
