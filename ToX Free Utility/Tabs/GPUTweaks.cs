using Guna.UI2.WinForms;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility.LoadingForms;

namespace ToX_Free_Utility.Tabs
{
    public partial class GPUTweaks : Form
    {
        public GPUTweaks()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            GeneralNvidia.Checked = Properties.Settings.Default.GeneralNvidia;
            NvHidden.Checked = Properties.Settings.Default.NvHidden;
            NviPstate.Checked = Properties.Settings.Default.NviPstate;
            NIP.Checked = Properties.Settings.Default.NIP;
            GeneralAMD.Checked = Properties.Settings.Default.GeneralAMD;
            AMDHidden.Checked = Properties.Settings.Default.AMDHidden;
            AMDPStates.Checked = Properties.Settings.Default.AMDPStates;
            AmdSC.Checked = Properties.Settings.Default.AmdSC;
        }

        private void SaveState(Guna2ToggleSwitch Tswitch, string Tbool)
        {
            Properties.Settings.Default[Tbool] = Tswitch.Checked;
            Properties.Settings.Default.Save();
        }

        private void GeneralNvidia_CheckedChanged(object sender, EventArgs e) => SaveState(GeneralNvidia, "GeneralNvidia");
        private void NvHidden_CheckedChanged(object sender, EventArgs e) => SaveState(NvHidden, "NvHidden");
        private void NviPstate_CheckedChanged(object sender, EventArgs e) => SaveState(NviPstate, "NviPstate");
        private void NIP_CheckedChanged(object sender, EventArgs e) => SaveState(NIP, "NIP");
        private void GeneralAMD_CheckedChanged(object sender, EventArgs e) => SaveState(GeneralAMD, "GeneralAMD");
        private void AMDHidden_CheckedChanged(object sender, EventArgs e) => SaveState(AMDHidden, "AMDHidden");
        private void AMDPStates_CheckedChanged(object sender, EventArgs e) => SaveState(AMDPStates, "AMDPStates");
        private void AmdSC_CheckedChanged(object sender, EventArgs e) => SaveState(AmdSC, "AmdSC");

        private async void GeneralNvidia_Click(object sender, EventArgs e)
        {
            if (GeneralNvidia.Checked) await RunWithLoadingScreenAsync(NvidiaGeneralTweaks);
            else RevertNvidiaGeneralTweaks();
        }

        private async void NvHidden_Click(object sender, EventArgs e)
        {
            if (NvHidden.Checked) await RunWithLoadingScreenAsync(NvidiaHidden);
            else RevertNvidiaHidden();
        }

        private async void NviPstate_Click(object sender, EventArgs e)
        {
            if (NviPstate.Checked) await RunWithLoadingScreenAsync(NvidiaPStates);
            else RevertNvidiaPStates();
        }

        private async void NIP_Click(object sender, EventArgs e)
        {
            if (NIP.Checked) await RunWithLoadingScreenAsync(TweakedNip);
            else RevertNip();
        }

        private async void GeneralAMD_Click(object sender, EventArgs e)
        {
            if (GeneralAMD.Checked) await RunWithLoadingScreenAsync(AMDGeneralTweaks);
            else RevertAMDGeneralTweaks();
        }

        private async void AMDHidden_Click(object sender, EventArgs e)
        {
            if (AMDHidden.Checked) await RunWithLoadingScreenAsync(AmdHidden);
            else RevertAmdHidden();
        }

        private async void AMDPStates_Click(object sender, EventArgs e)
        {
            if (AMDPStates.Checked) await RunWithLoadingScreenAsync(AMDPstates);
            else RevertAMDPstates();
        }

        private async void AmdSC_Click(object sender, EventArgs e)
        {
            if (AmdSC.Checked) await RunWithLoadingScreenAsync(AMDShaderCache);
            else RevertAMDShaderCache();
        }

        private async Task RunWithLoadingScreenAsync(Action tweakAction)
        {
            var loadingForm = new TweaksLoading
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true
            };

            var mainForm = (Main)this.ParentForm;
            TweaksLoading.ShowModal(mainForm, loadingForm);

            await Task.Delay(2000);
            await Task.Run(tweakAction);

            loadingForm.CloseModal();

            var successForm = new TweaksSuccess
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true
            };
            await TweaksSuccess.ShowModal(mainForm, successForm);
        }
        private void AMDGeneralTweaks()
        {
            string batchCommands = @"for /f %i in ('reg query ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"" /s /v ""DriverDesc""^| findstr ""HKEY AMD ATI""') do if /i ""%i"" neq ""DriverDesc"" (set ""REGPATH_AMD=%i"")
for /f %i in ('reg query ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"" /s /v ""DriverDesc""^| findstr ""HKEY AMD ATI""') do if /i ""%i"" neq ""DriverDesc"" (set ""REGPATH_AMD=%i"")
reg add ""%REGPATH_AMD%"" /v ""3D_Refresh_Rate_Override_DEF"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""3to2Pulldown_NA"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""AAF_NA"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""Adaptive De-interlacing"" /t Reg_DWORD /d ""1"" /f
reg add ""%REGPATH_AMD%"" /v ""AllowRSOverlay"" /t Reg_SZ /d ""false"" /f
reg add ""%REGPATH_AMD%"" /v ""AllowSkins"" /t Reg_SZ /d ""false"" /f
reg add ""%REGPATH_AMD%"" /v ""AllowSnapshot"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""AllowSubscription"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""AntiAlias_NA"" /t Reg_SZ /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""AreaAniso_NA"" /t Reg_SZ /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""ASTT_NA"" /t Reg_SZ /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""AutoColorDepthReduction_NA"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""DisableSAMUPowerGating"" /t Reg_DWORD /d ""1"" /f
reg add ""%REGPATH_AMD%"" /v ""DisableUVDPowerGatingDynamic"" /t Reg_DWORD /d ""1"" /f
reg add ""%REGPATH_AMD%"" /v ""DisableVCEPowerGating"" /t Reg_DWORD /d ""1"" /f
reg add ""%REGPATH_AMD%"" /v ""EnableAspmL0s"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""EnableAspmL1"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""EnableUlps"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""EnableUlps_NA"" /t Reg_SZ /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""KMD_DeLagEnabled"" /t Reg_DWORD /d ""1"" /f
reg add ""%REGPATH_AMD%"" /v ""KMD_FRTEnabled"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""DisableDMACopy"" /t Reg_DWORD /d ""1"" /f
reg add ""%REGPATH_AMD%"" /v ""DisableBlockWrite"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""StutterMode"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""EnableUlps"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""PP_SclkDeepSleepDisable"" /t Reg_DWORD /d ""1"" /f
reg add ""%REGPATH_AMD%"" /v ""PP_ThermalAutoThrottlingEnable"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%"" /v ""DisableDrmdmaPowerGating"" /t Reg_DWORD /d ""1"" /f
reg add ""%REGPATH_AMD%"" /v ""KMD_EnableComputePreemption"" /t Reg_DWORD /d ""0"" /f
reg add ""%REGPATH_AMD%\UMD"" /v ""Main3D_DEF"" /t Reg_SZ /d ""1"" /f
reg add ""%REGPATH_AMD%\UMD"" /v ""Main3D"" /t Reg_BINARY /d ""3100"" /f
reg add ""%REGPATH_AMD%\UMD"" /v ""FlipQueueSize"" /t Reg_BINARY /d ""3100"" /f
reg add ""%REGPATH_AMD%\UMD"" /v ""Tessellation_OPTION"" /t Reg_BINARY /d ""3200"" /f
reg add ""%REGPATH_AMD%\UMD"" /v ""Tessellation"" /t Reg_BINARY /d ""3100"" /f
reg add ""%REGPATH_AMD%\UMD"" /v ""VSyncControl"" /t Reg_BINARY /d ""3000"" /f
reg add ""%REGPATH_AMD%\UMD"" /v ""TFQ"" /t Reg_BINARY /d ""3200"" /f
reg add ""%REGPATH_AMD%\DAL2_DATA__2_0\DisplayPath_4\EDID_D109_78E9\Option"" /v ""ProtectionControl"" /t Reg_BINARY /d ""0100000001000000"" /f
";

            ExecuteBatchCommands(batchCommands);
        }

        private void AmdHidden()
        {
            string batchCommands = @"reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""3D_Refresh_Rate_Override_DEF"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AllowSnapshot"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AAF_NA"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AntiAlias_NA"" /t REG_SZ /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""ASTT_NA"" /t REG_SZ /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AllowSubscription"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AreaAniso_NA"" /t REG_SZ /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AllowRSOverlay"" /t REG_SZ /d ""false"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""Adaptive De-interlacing"" /t REG_DWORD /d ""1"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AllowSkins"" /t REG_SZ /d ""false"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AutoColorDepthReduction_NA"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisableSAMUPowerGating"" /t REG_DWORD /d ""1"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisableUVDPowerGatingDynamic"" /t REG_DWORD /d ""1"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisableVCEPowerGating"" /t REG_DWORD /d ""1"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisablePowerGating"" /t REG_DWORD /d ""1"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisableDrmdmaPowerGating"" /t REG_DWORD /d ""1"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""EnableVceSwClockGating"" /t REG_DWORD /d ""1"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""EnableUvdClockGating"" /t REG_DWORD /d ""1"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""EnableAspmL0s"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""EnableAspmL1"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""EnableUlps"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""EnableUlps_NA"" /t REG_SZ /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""KMD_DeLagEnabled"" /t REG_DWORD /d ""1"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""KMD_FRTEnabled"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisableDMACopy"" /t REG_DWORD /d ""1"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisableBlockWrite"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""StutterMode"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""PP_SclkDeepSleepDisable"" /t REG_DWORD /d ""1"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""PP_ThermalAutoThrottlingEnable"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""KMD_EnableComputePreemption"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000\UMD"" /v ""Main3D_DEF"" /t REG_SZ /d ""1"" /f
";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertAmdHidden()
        {
            string batchCommands = @"reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""3D_Refresh_Rate_Override_DEF"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AllowSnapshot"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AAF_NA"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AntiAlias_NA"" /t REG_SZ /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""ASTT_NA"" /t REG_SZ /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AllowSubscription"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AreaAniso_NA"" /t REG_SZ /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AllowRSOverlay"" /t REG_SZ /d ""false"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""Adaptive De-interlacing"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AllowSkins"" /t REG_SZ /d ""false"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""AutoColorDepthReduction_NA"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisableSAMUPowerGating"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisableUVDPowerGatingDynamic"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisableVCEPowerGating"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisablePowerGating"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisableDrmdmaPowerGating"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""EnableVceSwClockGating"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""EnableUvdClockGating"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""EnableAspmL0s"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""EnableAspmL1"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""EnableUlps"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""EnableUlps_NA"" /t REG_SZ /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""KMD_DeLagEnabled"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""KMD_FRTEnabled"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisableDMACopy"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DisableBlockWrite"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""StutterMode"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""PP_SclkDeepSleepDisable"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""PP_ThermalAutoThrottlingEnable"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""KMD_EnableComputePreemption"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000\UMD"" /v ""Main3D_DEF"" /t REG_SZ /d ""1"" /f
";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertAMDGeneralTweaks()
        {
            string batchCommands = @"for /f %i in ('reg query ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"" /s /v ""DriverDesc""^| findstr ""HKEY AMD ATI""') do if /i ""%i"" neq ""DriverDesc"" (set ""REGPATH_AMD=%i"")
for /f %i in ('reg query ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"" /s /v ""DriverDesc""^| findstr ""HKEY AMD ATI""') do if /i ""%i"" neq ""DriverDesc"" (set ""REGPATH_AMD=%i"")
reg delete ""%REGPATH_AMD%"" /v ""3D_Refresh_Rate_Override_DEF"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""3to2Pulldown_NA"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""AAF_NA"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""Adaptive De-interlacing"" /t Reg_DWORD /d ""1"" /f
reg delete ""%REGPATH_AMD%"" /v ""AllowRSOverlay"" /t Reg_SZ /d ""false"" /f
reg delete ""%REGPATH_AMD%"" /v ""AllowSkins"" /t Reg_SZ /d ""false"" /f
reg delete ""%REGPATH_AMD%"" /v ""AllowSnapshot"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""AllowSubscription"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""AntiAlias_NA"" /t Reg_SZ /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""AreaAniso_NA"" /t Reg_SZ /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""ASTT_NA"" /t Reg_SZ /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""AutoColorDepthReduction_NA"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""DisableSAMUPowerGating"" /t Reg_DWORD /d ""1"" /f
reg delete ""%REGPATH_AMD%"" /v ""DisableUVDPowerGatingDynamic"" /t Reg_DWORD /d ""1"" /f
reg delete ""%REGPATH_AMD%"" /v ""DisableVCEPowerGating"" /t Reg_DWORD /d ""1"" /f
reg delete ""%REGPATH_AMD%"" /v ""EnableAspmL0s"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""EnableAspmL1"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""EnableUlps"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""EnableUlps_NA"" /t Reg_SZ /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""KMD_DeLagEnabled"" /t Reg_DWORD /d ""1"" /f
reg delete ""%REGPATH_AMD%"" /v ""KMD_FRTEnabled"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""DisableDMACopy"" /t Reg_DWORD /d ""1"" /f
reg delete ""%REGPATH_AMD%"" /v ""DisableBlockWrite"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""StutterMode"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""EnableUlps"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""PP_SclkDeepSleepDisable"" /t Reg_DWORD /d ""1"" /f
reg delete ""%REGPATH_AMD%"" /v ""PP_ThermalAutoThrottlingEnable"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%"" /v ""DisableDrmdmaPowerGating"" /t Reg_DWORD /d ""1"" /f
reg delete ""%REGPATH_AMD%"" /v ""KMD_EnableComputePreemption"" /t Reg_DWORD /d ""0"" /f
reg delete ""%REGPATH_AMD%\UMD"" /v ""Main3D_DEF"" /t Reg_SZ /d ""1"" /f
reg delete ""%REGPATH_AMD%\UMD"" /v ""Main3D"" /t Reg_BINARY /d ""3100"" /f
reg delete ""%REGPATH_AMD%\UMD"" /v ""FlipQueueSize"" /t Reg_BINARY /d ""3100"" /f
reg delete ""%REGPATH_AMD%\UMD"" /v ""Tessellation_OPTION"" /t Reg_BINARY /d ""3200"" /f
reg delete ""%REGPATH_AMD%\UMD"" /v ""Tessellation"" /t Reg_BINARY /d ""3100"" /f
reg delete ""%REGPATH_AMD%\UMD"" /v ""VSyncControl"" /t Reg_BINARY /d ""3000"" /f
reg delete ""%REGPATH_AMD%\UMD"" /v ""TFQ"" /t Reg_BINARY /d ""3200"" /f
reg delete ""%REGPATH_AMD%\DAL2_DATA__2_0\DisplayPath_4\EDID_D109_78E9\Option"" /v ""ProtectionControl"" /t Reg_BINARY /d ""0100000001000000"" /f
";

            ExecuteBatchCommands(batchCommands);
        }
        private void AMDPstates()
        {
            string batchCommands = @"reg add ""HKLM\SOFTWARE\AMD\Energy\PowerState"" /v ""PStateControl"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SOFTWARE\AMD\Energy\PowerState"" /v ""PStateMode"" /t REG_DWORD /d ""0"" /f
reg add ""HKLM\SOFTWARE\AMD\Energy\PowerState"" /v ""EnableDynamicPower"" /t REG_DWORD /d ""0"" /f
";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertAMDPstates()
        {
            string batchCommands = @"reg add ""HKLM\SOFTWARE\AMD\Energy\PowerState"" /v ""PStateControl"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SOFTWARE\AMD\Energy\PowerState"" /v ""PStateMode"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SOFTWARE\AMD\Energy\PowerState"" /v ""EnableDynamicPower"" /t REG_DWORD /d ""0"" /f
";

            ExecuteBatchCommands(batchCommands);
        }
        private void AMDShaderCache()
        {
            string batchCommands = @"reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000\UMD"" /v ""ShaderCache"" /t REG_BINARY /d ""3200"" /f";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertAMDShaderCache()
        {
            string batchCommands = @"reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000\UMD"" /v ""ShaderCache"" /t REG_BINARY /d ""3200"" /f";

            ExecuteBatchCommands(batchCommands);
        }
        private void NvidiaGeneralTweaks()
        {
            string batchCommands = @"Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""TCCSupported"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKCU\SOFTWARE\NVIDIA Corporation\Global\NVTweak\Devices\509901423-0\Color"" /v ""NvCplUseColorCorrection"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""PlatformSupportMiracast"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\FTS"" /v EnableRID73779  /t REG_DWORD /d 1 /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\FTS"" /v EnableRID73780  /t REG_DWORD /d 1 /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\FTS"" /v EnableRID74361  /t REG_DWORD /d 1 /f
Reg.exe add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v EnableRID44231  /t REG_DWORD /d 0 /f
Reg.exe add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v EnableRID64640  /t REG_DWORD /d 0 /f
Reg.exe add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v EnableRID66610  /t REG_DWORD /d 0 /f
Reg.exe add ""HKLM\SOFTWARE\NVIDIA Corporation\NvControlPanel2\Client"" /v OptInOrOutPreference /t REG_DWORD /d 0 /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\services\NvTelemetryContainer"" /v Start /t REG_DWORD /d 4 /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\Startup"" /v SendTelemetryData /t REG_DWORD /d 0 /f
Reg.exe add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\Startup\SendTelemetryData"" /v 0 /t REG_DWORD /d 0 /f
Reg.exe add ""HKLM\SYSTEM\ControlSet001\Services\nvlddmkm"" /v ""EnableMidBufferPreemption"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler"" /v ""PlatformSupportMiracast"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""RmGpsPsEnablePerCpuCoreDpc"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Power"" /v ""RmGpsPsEnablePerCpuCoreDpc"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler"" /v ""RMDisablePostL2Compression"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler"" /v ""RmDisableRegistryCaching"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler"" /v ""RmGpsPsEnablePerCpuCoreDpc"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler"" /v ""PlatformSupportMiracast"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Throttle"" /v ""PerfEnablePackageIdle"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""0x112493bd"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""0x11e91a61"" /t REG_DWORD /d ""4294967295"" /f
Reg.exe add ""HKLM\SYSTEM\ControlSet001\Services\nvlddmkm"" /v ""DisableCudaContextPreemption"" /t REG_DWORD /d ""1"" /f
Reg.exe Add ""HKLM\SYSTEM\CurrentControlSet\Services\NvTelemetryContainer"" /v ""Start"" /t REG_DWORD /d ""4"" /f
Reg.exe Add ""HKLM\SOFTWARE\NVIDIA Corporation\NvControlPanel2\Client"" /v ""OptInOrOutPreference"" /t REG_DWORD /d ""0"" /f
Reg.exe Add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v ""EnableRID44231"" /t REG_DWORD /d ""0"" /f
Reg.exe Add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v ""EnableRID64640"" /t REG_DWORD /d ""0"" /f
Reg.exe Add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v ""EnableRID66610"" /t REG_DWORD /d ""0"" /f
Reg.exe Add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\FTS"" /v ""EnableRID44231"" /t REG_DWORD /d ""0"" /f
Reg.exe Add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\FTS"" /v ""EnableRID64640"" /t REG_DWORD /d ""0"" /f
Reg.exe Add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\FTS"" /v ""EnableRID66610"" /t REG_DWORD /d ""0"" /f
Reg.exe Delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"" /v ""NvBackend"" /f
";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertNvidiaGeneralTweaks()
        {
            string batchCommands = @"Reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\FTS"" /v ""EnableRID61684"" /t REG_DWORD /d ""1"" /f

Reg delete ""HKCU\Software\NVIDIA Corporation\Global\NVTweak\Devices\509901423-0\Color"" /v ""NvCplUseColorCorrection"" /t REG_DWORD /d ""0"" /f
Reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""PlatformSupportMiracast"" /t REG_DWORD /d ""0"" /f
Reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\NVTweak"" /v ""DisplayPowerSaving"" /t REG_DWORD /d ""0"" /f

reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""D3PCLatency"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""F1TransitionLatency"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""LOWLATENCY"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""Node3DLowLatency"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""PciLatencyTimerControl"" /t REG_DWORD /d ""20"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RMDeepL1EntryLatencyUsec"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RmGspcMaxFtuS"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RmGspcMinFtuS"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RmGspcPerioduS"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RMLpwrEiIdleThresholdUs"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RMLpwrGrIdleThresholdUs"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RMLpwrGrRgIdleThresholdUs"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RMLpwrMsIdleThresholdUs"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""VRDirectFlipDPCDelayUs"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""VRDirectFlipTimingMarginUs"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""VRDirectJITFlipMsHybridFlipDelayUs"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""vrrCursorMarginUs"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""vrrDeflickerMarginUs"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""vrrDeflickerMaxUs"" /t REG_DWORD /d ""1"" /f

reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RMHdcpKeyGlobZero"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""TCCSupported"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""PreferSystemMemoryContiguous"" /t REG_DWORD /d ""1"" /f

reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""RmGpsPsEnablePerCpuCoreDpc"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\NVAPI"" /v ""RmGpsPsEnablePerCpuCoreDpc"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\NVTweak"" /v ""RmGpsPsEnablePerCpuCoreDpc"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""RmGpsPsEnablePerCpuCoreDpc"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Power"" /v ""RmGpsPsEnablePerCpuCoreDpc"" /t REG_DWORD /d ""1"" /f

reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RmCacheLoc"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""TdrLevel"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""TdrDelay"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""TdrDdiDelay"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""TdrDebugMode"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""TdrLimitCount"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""TdrLimitTime"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""TdrTestMode"" /t REG_DWORD /d ""0"" /f

reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RmFbsrPagedDMA"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""Acceleration.Level"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""DisablePreemption"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""DisableCudaContextPreemption"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""EnableCEPreemption"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""DisablePreemptionOnS3S4"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""ComputePreemption"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""DisableWriteCombining"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DesktopStereoShortcuts"" /t REG_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""FeatureControl"" /t REG_DWORD /d ""4"" /f
";

            ExecuteBatchCommands(batchCommands);
        }
        private void NvidiaHidden()
        {
            string batchCommands = @"Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""TCCSupported"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKCU\SOFTWARE\NVIDIA Corporation\Global\NVTweak\Devices\509901423-0\Color"" /v ""NvCplUseColorCorrection"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""PlatformSupportMiracast"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\FTS"" /v EnableRID73779  /t REG_DWORD /d 1 /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\FTS"" /v EnableRID73780  /t REG_DWORD /d 1 /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\FTS"" /v EnableRID74361  /t REG_DWORD /d 1 /f
Reg.exe add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v EnableRID44231  /t REG_DWORD /d 0 /f
Reg.exe add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v EnableRID64640  /t REG_DWORD /d 0 /f
Reg.exe add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v EnableRID66610  /t REG_DWORD /d 0 /f
Reg.exe add ""HKLM\SOFTWARE\NVIDIA Corporation\NvControlPanel2\Client"" /v OptInOrOutPreference /t REG_DWORD /d 0 /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\services\NvTelemetryContainer"" /v Start /t REG_DWORD /d 4 /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\Startup"" /v SendTelemetryData /t REG_DWORD /d 0 /f
Reg.exe add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\Startup\SendTelemetryData"" /v 0 /t REG_DWORD /d 0 /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\Windows\Dwm"" /v ""OverlayTestMode"" /t REG_DWORD /d ""5"" /f
Reg.exe add ""HKLM\SYSTEM\ControlSet001\Services\nvlddmkm"" /v ""EnableMidBufferPreemption"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler"" /v ""PlatformSupportMiracast"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""RmGpsPsEnablePerCpuCoreDpc"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Power"" /v ""RmGpsPsEnablePerCpuCoreDpc"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler"" /v ""RMDisablePostL2Compression"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler"" /v ""RmDisableRegistryCaching"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler"" /v ""RmGpsPsEnablePerCpuCoreDpc"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""DesktopStereoShortcuts"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""FeatureControl"" /t REG_DWORD /d ""4"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""NVDeviceSupportKFilter"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RmCacheLoc"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RmDisableInst2Sys"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RmFbsrPagedDMA"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RMGpuId"" /t REG_DWORD /d ""256"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""RmProfilingAdminOnly"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""TCCSupported"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""TrackResetEngine"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""UseBestResolution"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000"" /v ""ValidateBlitSubRects"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""MaxIAverageGraphicsLatencyInOneBucket"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\Scheduler"" /v ""PlatformSupportMiracast"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""CsEnabled"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""PerfCalculateActualUtilization"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""SleepReliabilityDetailedDiagnostics"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""EventProcessorEnabled"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""QosManagesIdleProcessors"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""DisableVsyncLatencyUpdate"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""DisableSensorWatchdog"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""InterruptSteeringDisabled"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\intelppm\Parameters"" /v ""AcpiFirmwareWatchDog"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\intelppm\Parameters"" /v ""AmliWatchdogAction"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\intelppm\Parameters"" /v ""AmliWatchdogTimeout"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\intelppm\Parameters"" /v ""WatchdogTimeout"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Throttle"" /v ""PerfEnablePackageIdle"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""0x112493bd"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""0x11e91a61"" /t REG_DWORD /d ""4294967295"" /f
Reg.exe add ""HKLM\SYSTEM\ControlSet001\Services\nvlddmkm"" /v ""DisableCudaContextPreemption"" /t REG_DWORD /d ""1"" /f
";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertNvidiaHidden()
        {
            string batchCommands = @"set cdCache=%cd%
cd ""%SystemDrive%\Program Files\NVIDIA Corporation\NVSMI\""
start """" /I /WAIT /B ""nvidia-smi"" -acp 0
cd %cdCache%

reg delete ""HKCU\Software\NVIDIA Corporation\Global\NVTweak\Devices\509901423-0\Color"" /v ""NvCplUseColorCorrection"" /t Reg_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""PlatformSupportMiracast"" /t Reg_DWORD /d ""0"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\NVTweak"" /v ""DisplayPowerSaving"" /t Reg_DWORD /d ""0"" /f

cd ""%SYSTEMDRIVE%\Program Files\NVIDIA Corporation\NVSMI\""
nvidia-smi -acp UNRESTRICTED
nvidia-smi -acp DEFAULT

for /f %a in ('reg query ""HKLM\System\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"" /t REG_SZ /s /e /f ""NVIDIA"" ^| findstr ""HKEY""') do (
    echo !S_MAGENTA! Reset Tiled Display!S_YELLOW! 
timeout /t 1 /nobreak > NUL
    reg delete ""%a"" /v ""EnableTiledDisplay"" /f
    echo !S_MAGENTA! Reset TCC!S_YELLOW! 
timeout /t 1 /nobreak > NUL
    reg delete ""%a"" /v ""TCCSupported"" /f
)


for /f %i in ('reg query ""HKLM\System\ControlSet001\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"" /t REG_SZ /s /e /f ""NVIDIA"" ^| findstr ""HKEY""') do (
  Reg delete ""%a"" /v ""PowerMizerEnable"" /t REG_DWORD /d ""1"" /f
  Reg delete ""%a"" /v ""PowerMizerLevel"" /t REG_DWORD /d ""1"" /f
  Reg delete ""%a"" /v ""PowerMizerLevelAC"" /t REG_DWORD /d ""1"" /f
  Reg delete ""%a"" /v ""PerfLevelSrc"" /t REG_DWORD /d ""8738"" /f
)

reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\FTS"" /v ""EnableRID61684"" /t REG_DWORD /d ""1"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""DisableWriteCombining"" /t Reg_DWORD /d ""1"" /f

reg delete ""HKLM\SOFTWARE\NVIDIA Corporation\NvControlPanel2\Client"" /v ""OptInOrOutPreference"" /t REG_DWORD /d 0 /f
reg delete ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v ""EnableRID44231"" /t REG_DWORD /d 0 /f
reg delete ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v ""EnableRID64640"" /t REG_DWORD /d 0 /f
reg delete ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v ""EnableRID66610"" /t REG_DWORD /d 0 /f
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"" /v ""NvBackend"" /f
schtasks /change /disable /tn ""NvTmRep_CrashReport1_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}""
schtasks /change /disable /tn ""NvTmRep_CrashReport2_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}""
schtasks /change /disable /tn ""NvTmRep_CrashReport3_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}""
schtasks /change /disable /tn ""NvTmRep_CrashReport4_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}""
sc config NvTelemetyContainer start=disabled
sc stop NvTelemetyContainer
if exist ""%systmedrive%\Program Files\NVIDIA Corporation\Installer2\InstallerCore\NVI2.DLL"" (rundll32 ""%systmedrive%\Program Files\NVIDIA Corporation\Installer2\InstallerCore\NVI2.DLL"",UninstallPackage NvTelemetryContainer)

Reg.exe delete ""HKLM\SYSTEM\ControlSet001\Services\nvlddmkm"" /v ""LogWarningEntries"" /t REG_DWORD /d ""0"" /f
Reg.exe delete ""HKLM\SYSTEM\ControlSet001\Services\nvlddmkm"" /v ""LogPagingEntries"" /t REG_DWORD /d ""0"" /f
Reg.exe delete ""HKLM\SYSTEM\ControlSet001\Services\nvlddmkm"" /v ""LogEventEntries"" /t REG_DWORD /d ""0"" /f
Reg.exe delete ""HKLM\SYSTEM\ControlSet001\Services\nvlddmkm"" /v ""LogErrorEntries"" /t REG_DWORD /d ""0"" /f

sc config NVDisplay.ContainerLocalSystem start= auto
sc start NVDisplay.ContainerLocalSystem
";

            ExecuteBatchCommands(batchCommands);
        }
        private void NvidiaPStates()
        {
            string batchCommands = @"for /f %f in ('wmic path Win32_VideoController get PNPDeviceID^| findstr /L ""PCI\VEN_""') do (
	for /f ""tokens=3"" %b in ('reg query ""HKLM\SYSTEM\ControlSet001\Enum\%f"" /v ""Driver""') do (
		for /f %f in ('echo %b ^| findstr ""{""') do (
		     Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Class\%f"" /v ""DisableDynamicPstate"" /t REG_DWORD /d ""1"" /f
                   )
                )
             )
			 for /f %m in ('wmic path Win32_VideoController get PNPDeviceID^| findstr /L ""PCI\VEN_""') do (
	for /f ""tokens=3"" %a in ('reg query ""HKLM\SYSTEM\ControlSet001\Enum\%m"" /v ""Driver""') do (
		for /f %m in ('echo %a ^| findstr ""{""') do (
		     Reg.exe add ""HKLM\System\ControlSet001\Control\Class\%m"" /v ""RMHdcpKeyglobZero"" /t REG_DWORD /d ""1"" /f
                   )
                )
             )
";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertNvidiaPStates()
        {
            string batchCommands = @"for /f %i in ('reg query ""HKLM\System\ControlSet001\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"" /t REG_SZ /s /e /f ""NVIDIA"" ^| findstr ""HKEY""') do (
  reg add ""%i"" /v ""DisableDynamicPstate"" /t REG_DWORD /d ""0"" /f
)
";

            ExecuteBatchCommands(batchCommands);
        }
        private void TweakedNip()
        {
            string batchCommands = @"if not exist ""C:\ToX\"" ( mkdir ""C:\ToX"" >nul 2>&1 )
if not exist ""C:\ToX\Resources\"" ( mkdir ""C:\ToX\Resources\"" >nul 2>&1 )
curl -g -k -L -# -o ""%tmp%\nvidiaProfileInspector.zip"" ""https://github.com/Orbmu2k/nvidiaProfileInspector/releases/latest/download/nvidiaProfileInspector.zip"">nul 2>&1
powershell -NoProfile Expand-Archive '%tmp%\nvidiaProfileInspector.zip' -DestinationPath 'C:\ToX\Resources\'>nul 2>&1
curl -g -k -L -# -o ""C:\ToX\Resources\ToXFree.nip"" ""https://raw.githubusercontent.com/ToXTweaks/Resources-Free/refs/heads/main/ToXFree.nip"" >nul 2>&1
start """" /D ""C:\ToX\Resources"" nvidiaProfileInspector.exe ToXFree.nip";

            ExecuteBatchCommands(batchCommands);
        }
        private void RevertNip()
        {
            string batchCommands = @"if not exist ""C:\ToX\"" ( mkdir ""C:\ToX"" >nul 2>&1 )
if not exist ""C:\ToX\Resources\"" ( mkdir ""C:\ToX\Resources\"" >nul 2>&1 )
curl -g -k -L -# -o ""%tmp%\nvidiaProfileInspector.zip"" ""https://github.com/Orbmu2k/nvidiaProfileInspector/releases/latest/download/nvidiaProfileInspector.zip"">nul 2>&1
powershell -NoProfile Expand-Archive '%tmp%\nvidiaProfileInspector.zip' -DestinationPath 'C:\ToX\Resources\'>nul 2>&1
curl -g -k -L -# -o ""C:\ToX\Resources\Basic.nip"" ""https://www.dropbox.com/scl/fi/jjd47lkw1zyh0znp8lheh/Basics.nip?rlkey=fdnxdmt5gw7xdxgyb2l9w5wed&st=2yz2eids&dl=1"" >nul 2>&1
start """" /D ""C:\ToX\Resources"" nvidiaProfileInspector.exe Basic.nip";

            ExecuteBatchCommands(batchCommands);
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
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine(batchCommands);
                    }
                }
                process.WaitForExit();
            }
        }

    }
}