using Guna.UI2.WinForms;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility.LoadingForms;

namespace ToX_Free_Utility.Tabs
{
    public partial class WindowsTweaksP2 : Form
    {
        public WindowsTweaksP2()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            TweakCSRSS.Checked = Properties.Settings.Default.TweakCSRSS;
            TweakMMCSS.Checked = Properties.Settings.Default.TweakMMCSS;
            DisableDefender.Checked = Properties.Settings.Default.DisableDefender;
            FixJPEG.Checked = Properties.Settings.Default.FixJPEG;
            BasicServices.Checked = Properties.Settings.Default.BasicServices;
            MicrosoftEdge.Checked = Properties.Settings.Default.MicrosoftEdge;
        }

        private void SaveState(Guna2ToggleSwitch Tswitch, string Tbool)
        {
            Properties.Settings.Default[Tbool] = Tswitch.Checked;
            Properties.Settings.Default.Save();
        }

        private void TweakCSRSS_CheckedChanged(object sender, EventArgs e) => SaveState(TweakCSRSS, "TweakCSRSS");
        private void TweakMMCSS_CheckedChanged(object sender, EventArgs e) => SaveState(TweakMMCSS, "TweakMMCSS");
        private void DisableDefender_CheckedChanged(object sender, EventArgs e) => SaveState(DisableDefender, "DisableDefender");
        private void FixJPEG_CheckedChanged(object sender, EventArgs e) => SaveState(FixJPEG, "FixJPEG");
        private void BasicServices_CheckedChanged(object sender, EventArgs e) => SaveState(BasicServices, "BasicServices");
        private void MicrosoftEdge_CheckedChanged(object sender, EventArgs e) => SaveState(MicrosoftEdge, "MicrosoftEdge");

        private async void TweakCSRSS_Click(object sender, EventArgs e)
        {
            if (TweakCSRSS.Checked) await RunWithLoadingScreenAsync(TweakCSRSSAction);
            else RevertTweakCSRSSAction();
        }

        private async void TweakMMCSS_Click(object sender, EventArgs e)
        {
            if (TweakMMCSS.Checked) await RunWithLoadingScreenAsync(TweakMMCSSAction);
            else RevertTweakMMCSSAction();
        }

        private async void DisableDefender_Click(object sender, EventArgs e)
        {
            if (DisableDefender.Checked) await RunWithLoadingScreenAsync(DisableDefenderAction);
            else RevertDisableDefenderAction();
        }

        private async void FixJPEG_Click(object sender, EventArgs e)
        {
            if (FixJPEG.Checked) await RunWithLoadingScreenAsync(FixJPEGAction);
            else RevertFixJPEGAction();
        }

        private async void BasicServices_Click(object sender, EventArgs e)
        {
            if (BasicServices.Checked) await RunWithLoadingScreenAsync(BasicServicesAction);
            else RevertBasicServicesAction();
        }

        private async void MicrosoftEdge_Click(object sender, EventArgs e)
        {
            if (MicrosoftEdge.Checked) await RunWithLoadingScreenAsync(DisableMicrosoftEdge);
            else RevertDisableMicrosoftEdge();
        }

        private void TweakCSRSSAction()
        {
            string batchCommands = @"Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe\PerfOptions"" /v ""CpuPriorityClass"" /t REG_DWORD /d ""4"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe\PerfOptions"" /v ""IoPriority"" /t REG_DWORD /d ""3"" /f
";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertTweakCSRSSAction()
        {
            string batchCommands = @"reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe\PerfOptions"" /v ""CpuPriorityClass"" /f
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe\PerfOptions"" /v ""IoPriority"" /f
";

            ExecuteBatchCommands(batchCommands);
        }

        private void TweakMMCSSAction()
        {
            string batchCommands = @"
REG ADD ""HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v ""SystemResponsiveness"" /t REG_DWORD /d 0 /f
REG ADD ""HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v ""NetworkThrottlingIndex"" /t REG_DWORD /d ffffffff /f
REG ADD ""HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v ""NoLazyMode"" /t REG_DWORD /d 1 /f
REG ADD ""HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v ""AlwaysOn"" /t REG_DWORD /d 1 /f

REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""Affinity"" /t REG_DWORD /d 7 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""Background Only"" /t REG_SZ /d ""True"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""Clock Rate"" /t REG_DWORD /d 10000 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""GPU Priority"" /t REG_DWORD /d 8 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""Priority"" /t REG_DWORD /d 6 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""Scheduling Category"" /t REG_SZ /d ""Medium"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""SFIO Priority"" /t REG_SZ /d ""Normal"" /f

REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""Affinity"" /t REG_DWORD /d 0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""Background Only"" /t REG_SZ /d ""True"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""BackgroundPriority"" /t REG_DWORD /d 24 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""Clock Rate"" /t REG_DWORD /d 10000 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""GPU Priority"" /t REG_DWORD /d 18 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""Priority"" /t REG_DWORD /d 8 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""Scheduling Category"" /t REG_SZ /d ""High"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""SFIO Priority"" /t REG_SZ /d ""High"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""Latency Sensitive"" /t REG_SZ /d ""True"" /f

REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Affinity"" /t REG_DWORD /d 0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Background Only"" /t REG_SZ /d ""False"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Clock Rate"" /t REG_DWORD /d 10000 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""GPU Priority"" /t REG_DWORD /d 18 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Priority"" /t REG_DWORD /d 6 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Scheduling Category"" /t REG_SZ /d ""High"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""SFIO Priority"" /t REG_SZ /d ""High"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Latency Sensitive"" /t REG_SZ /d ""True"" /f
";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertTweakMMCSSAction()
        {
            string batchCommands = @"@echo off

REG ADD ""HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v ""SystemResponsiveness"" /t REG_DWORD /d 00000000 /f
REG ADD ""HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v ""NetworkThrottlingIndex"" /t REG_DWORD /d ffffffff /f
REG ADD ""HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v ""NoLazyMode"" /t REG_DWORD /d 00000000 /f
REG ADD ""HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v ""AlwaysOn"" /t REG_DWORD /d 00000000 /f

REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""Affinity"" /t REG_DWORD /d 00000000 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""Background Only"" /t REG_SZ /d ""False"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""Clock Rate"" /t REG_DWORD /d 00002710 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""GPU Priority"" /t REG_DWORD /d 00000008 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""Priority"" /t REG_DWORD /d 00000002 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""Scheduling Category"" /t REG_SZ /d ""Normal"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio"" /v ""SFIO Priority"" /t REG_SZ /d ""Normal"" /f

REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""Affinity"" /t REG_DWORD /d 00000000 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""Background Only"" /t REG_SZ /d ""False"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""BackgroundPriority"" /t REG_DWORD /d 00000008 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""Clock Rate"" /t REG_DWORD /d 00002710 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""GPU Priority"" /t REG_DWORD /d 00000008 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""Priority"" /t REG_DWORD /d 00000002 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""Scheduling Category"" /t REG_SZ /d ""Normal"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""SFIO Priority"" /t REG_SZ /d ""Normal"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\DisplayPostProcessing"" /v ""Latency Sensitive"" /t REG_SZ /d ""False"" /f

REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Affinity"" /t REG_DWORD /d 00000000 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Background Only"" /t REG_SZ /d ""False"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Clock Rate"" /t REG_DWORD /d 00002710 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""GPU Priority"" /t REG_DWORD /d 00000008 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Priority"" /t REG_DWORD /d 00000002 /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Scheduling Category"" /t REG_SZ /d ""Normal"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""SFIO Priority"" /t REG_SZ /d ""Normal"" /f
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Latency Sensitive"" /t REG_SZ /d ""False"" /f
";

            ExecuteBatchCommands(batchCommands);
        }

        private void DisableDefenderAction()
        {
            string batchCommands = @"reg add ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender"" /v DisableAntiSpyware /t REG_DWORD /d 1 /f
reg add ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableRealtimeMonitoring /t REG_DWORD /d 1 /f";

            ExecuteBatchCommands(batchCommands);

        }
        private void RevertDisableDefenderAction()
        {
            string batchCommands = @"reg delete ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender"" /v DisableAntiSpyware /f
reg delete ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"" /v DisableRealtimeMonitoring /f";

            ExecuteBatchCommands(batchCommands);
        }

        private void FixJPEGAction()
        {
            string batchCommands = @"Reg.exe add ""HKCU\Control Panel\Desktop"" /v ""JPEGImportQuality"" /t REG_DWORD /d ""100"" /f";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertFixJPEGAction()
        {
            string batchCommands = @"Reg.exe add ""HKCU\Control Panel\Desktop"" /v ""JPEGImportQuality"" /t REG_DWORD /d ""85"" /f";

            ExecuteBatchCommands(batchCommands);
        }

        private void BasicServicesAction()
        {
            string batchCommands = @"sc stop DoSvc > nul
sc config DoSvc start= disabled > nul

sc stop diagsvc > nul
sc config diagsvc start= disabled > nul

sc stop DPS > nul 
sc config DPS start= disabled > nul

sc stop dmwappushservice > nul
sc config dmwappushservice start= disabled > nul

sc stop MapsBroker > nul
sc config MapsBroker start= disabled > nul

sc stop lfsvc > nul
sc config lfsvc start= disabled > nul

sc stop CscService > nul
sc config CscService start= disabled > nul
 
sc stop SEMgrSvc > nul
sc config SEMgrSvc start= disabled > nul

sc stop PhoneSvc > nul
sc config PhoneSvc start= disabled > nul

sc stop RemoteRegistry > nul
sc config RemoteRegistry start= disabled > nul

sc stop RetailDemo > nul
sc config RetailDemo start= disabled > nul

sc stop SysMain > nul
sc config SysMain start= disabled > nul

sc stop WalletService > nul
sc config WalletService start= disabled > nul

sc stop WSearch > nul
sc config WSearch start= disabled > nul

sc stop W32Time > nul
sc config W32Time start= disabled > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MessagingService"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\WpnUserService"" /v Start /t REG_DWORD /d 00000004 /f > nul
";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertBasicServicesAction()
        {
            string batchCommands = @":: Re-enable previously disabled services
sc config DoSvc start= demand > nul
sc config diagsvc start= demand > nul
sc config DPS start= demand > nul
sc config dmwappushservice start= demand > nul
sc config MapsBroker start= demand > nul
sc config lfsvc start= demand > nul
sc config CscService start= demand > nul
sc config SEMgrSvc start= demand > nul
sc config PhoneSvc start= demand > nul
sc config RemoteRegistry start= demand > nul
sc config RetailDemo start= demand > nul
sc config SysMain start= demand > nul
sc config WalletService start= demand > nul
sc config WSearch start= demand > nul
sc config W32Time start= demand > nul

:: Delete registry changes (MessagingService and WpnUserService)
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MessagingService"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\WpnUserService"" /f > nul";

            ExecuteBatchCommands(batchCommands);
        }

        private void DisableMicrosoftEdge()
        {
            string batchCommands = @"cd /d ""%ProgramFiles(x86)%\Microsoft\Edge\Application""
rmdir /s /q ""%ProgramFiles(x86)%\Microsoft\Edge""";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertDisableMicrosoftEdge()
        {
            string batchCommands = @"powershell -Command ""Invoke-WebRequest -Uri 'https://github.com/ToXTweaks/Resources-Free/raw/refs/heads/main/MicrosoftEdgeSetup.exe' -OutFile '%TEMP%\MicrosoftEdgeSetup.exe'""

:: Install Microsoft Edge silently
start /wait """" ""%TEMP%\MicrosoftEdgeSetup.exe"" /silent /install
";

            ExecuteBatchCommands(batchCommands);
        }

        private async Task RunWithLoadingScreenAsync(Action tweakAction)
        {
            TweaksLoading loadingForm = new TweaksLoading
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true
            };

            Main mainForm = (Main)this.ParentForm;
            TweaksLoading.ShowModal(mainForm, loadingForm);

            await Task.Delay(2000);
            await Task.Run(tweakAction);

            loadingForm.CloseModal();

            TweaksSuccess successForm = new TweaksSuccess
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true
            };
            await TweaksSuccess.ShowModal(mainForm, successForm);
        }

        private void ExecuteBatchCommands(string batchCommands)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe")
            {
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas"
            };

            using (Process process = Process.Start(processInfo))
            {
                using (StreamWriter sw = process.StandardInput)
                {
                    sw.WriteLine(batchCommands);
                }
                process.WaitForExit();
            }
        }
        #region Pagination
        private void guna2ImageButton2_Click(object sender, EventArgs e)
        {
            if (Main.Instance != null)
            {
                // Create an instance of the WindowsTweaksP2 form
                WindowsTweaks myForm = new WindowsTweaks();
                myForm.TopLevel = false; // Set to false since it's being shown in a panel

                // Clear existing controls in the MainPanel
                Main.Instance.MainPanel.Controls.Clear();

                // Fade out the current content in the panel
                Main.Instance.guna2Transition1.HideSync(Main.Instance.MainPanel);

                // Show the new form and apply the transition after a short delay
                Task.Delay(1).ContinueWith(_ =>
                {
                    Main.Instance.MainPanel.Invoke((Action)(() =>
                    {
                        Main.Instance.MainPanel.Controls.Add(myForm); // Add the new form to the panel
                        myForm.Show(); // Show the new form
                        Main.Instance.guna2Transition1.ShowSync(Main.Instance.MainPanel); // Fade in the new content
                    }));
                });
            }
        }

        private void NextPage_Click(object sender, EventArgs e)
        {
            if (Main.Instance != null)
            {
                // Create an instance of the WindowsTweaksP2 form
                WindowsTweaksP3 myForm = new WindowsTweaksP3();
                myForm.TopLevel = false; // Set to false since it's being shown in a panel

                // Clear existing controls in the MainPanel
                Main.Instance.MainPanel.Controls.Clear();

                // Fade out the current content in the panel
                Main.Instance.guna2Transition1.HideSync(Main.Instance.MainPanel);

                // Show the new form and apply the transition after a short delay
                Task.Delay(1).ContinueWith(_ =>
                {
                    Main.Instance.MainPanel.Invoke((Action)(() =>
                    {
                        Main.Instance.MainPanel.Controls.Add(myForm); // Add the new form to the panel
                        myForm.Show(); // Show the new form
                        Main.Instance.guna2Transition1.ShowSync(Main.Instance.MainPanel); // Fade in the new content
                    }));
                });
            }
        }
        #endregion
    }
}
