using Guna.UI2.WinForms;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility.LoadingForms;

namespace ToX_Free_Utility.Tabs
{
    public partial class WindowsTweaksP4 : Form
    {
        public WindowsTweaksP4()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            CoreParking.Checked = Properties.Settings.Default.CoreParking;
            ClearDevices.Checked = Properties.Settings.Default.ClearDevices;
            BiosTweaks.Checked = Properties.Settings.Default.BiosTweaks;
            Mitigations.Checked = Properties.Settings.Default.Mitigations;
            AltTab.Checked = Properties.Settings.Default.AltTab;
            HyperV.Checked = Properties.Settings.Default.HyperV;
        }

        private void SaveState(Guna2ToggleSwitch Tswitch, string Tbool)
        {
            Properties.Settings.Default[Tbool] = Tswitch.Checked;
            Properties.Settings.Default.Save();
        }


        private void WindowsTweaks_Load(object sender, EventArgs e)
        {

        }

        private void guna2ImageButton2_Click(object sender, EventArgs e)
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

        private void CoreParking_CheckedChanged(object sender, EventArgs e) => SaveState(CoreParking, "CoreParking");
        private void ClearDevices_CheckedChanged(object sender, EventArgs e) => SaveState(ClearDevices, "ClearDevices");
        private void BiosTweaks_CheckedChanged(object sender, EventArgs e) => SaveState(BiosTweaks, "BiosTweaks");
        private void Mitigations_CheckedChanged(object sender, EventArgs e) => SaveState(Mitigations, "Mitigations");
        private void HyperV_CheckedChanged(object sender, EventArgs e) => SaveState(HyperV, "HyperV");
        private void AltTab_CheckedChanged(object sender, EventArgs e) => SaveState(AltTab, "AltTab");

        private async void CoreParking_Click(object sender, EventArgs e)
        {
            if (CoreParking.Checked) await RunWithLoadingScreenAsync(DisableCoreParking);
            else RevertCoreParking();
        }

        private async void ClearDevices_Click(object sender, EventArgs e)
        {
            if (ClearDevices.Checked) await RunWithLoadingScreenAsync(ClearDevicesa);
            else RevertClearDevices();
        }

        private async void BiosTweaks_Click(object sender, EventArgs e)
        {
            if (BiosTweaks.Checked) await RunWithLoadingScreenAsync(ApplyBiosTweaks);
            else RevertBiosTweaks();
        }

        private async void Mitigations_Click(object sender, EventArgs e)
        {
            if (Mitigations.Checked) await RunWithLoadingScreenAsync(ApplyMitigations);
            else RevertMitigations();
        }

        private async void HyperV_Click(object sender, EventArgs e)
        {
            if (HyperV.Checked) await RunWithLoadingScreenAsync(DisableHyperV);
            else EnableHyperV();
        }

        private async void AltTab_Click(object sender, EventArgs e)
        {
            if (AltTab.Checked) await RunWithLoadingScreenAsync(ApplyClassicAltTab);
            else RevertClassicAltTab();
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
        private void DisableCoreParking()
        {
            string batchCommands = @"
        powercfg -setacvalueindex scheme_current sub_processor CPMINCORES 100
        powercfg /setactive SCHEME_CURRENT
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void ClearDevicesa()
        {
            string batchCommands = @"
        POWERSHELL ""$Devices = Get-PnpDevice | ? Status -eq Unknown;foreach ($Device in $Devices) { &""pnputil"" /remove-device $Device.InstanceId }""
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void ApplyBiosTweaks()
        {
            string batchCommands = @"
        Reg.exe add ""HKLM\System\CurrentControlSet\Services\VxD\BIOS"" /v ""CPUPriority"" /t REG_DWORD /d ""1"" /f
        Reg.exe add ""HKLM\System\CurrentControlSet\Services\VxD\BIOS"" /v ""FastDRAM"" /t REG_DWORD /d ""1"" /f
        Reg.exe add ""HKLM\System\CurrentControlSet\Services\VxD\BIOS"" /v ""AGPConcur"" /t REG_DWORD /d ""1"" /f
        Reg.exe add ""HKLM\System\CurrentControlSet\Services\VxD\BIOS"" /v ""PCIConcur"" /t REG_DWORD /d ""1"" /f
        timeout /t 1 /nobreak > NUL
        bcdedit /set tscsyncpolicy legacy
        timeout /t 1 /nobreak > NUL
        bcdedit /set hypervisorlaunchtype off
        timeout /t 1 /nobreak > NUL
        bcdedit /set linearaddress57 OptOut
        bcdedit /set increaseuserva 268435328
        timeout /t 1 /nobreak > NUL
        bcdedit /set isolatedcontext No
        bcdedit /set allowedinmemorysettings 0x0
        timeout /t 1 /nobreak > NUL
        bcdedit /set vsmlaunchtype Off
        bcdedit /set vm No
        Reg.exe add ""HKLM\Software\Policies\Microsoft\FVE"" /v ""DisableExternalDMAUnderLock"" /t REG_DWORD /d ""0"" /f
        Reg.exe add ""HKLM\Software\Policies\Microsoft\Windows\DeviceGuard"" /v ""EnableVirtualizationBasedSecurity"" /t REG_DWORD /d ""0"" /f
        Reg.exe add ""HKLM\Software\Policies\Microsoft\Windows\DeviceGuard"" /v ""HVCIMATRequired"" /t REG_DWORD /d ""0"" /f
        bcdedit /set x2apicpolicy Enable
        bcdedit /set uselegacyapicmode No
        bcdedit /set configaccesspolicy Default
        bcdedit /set MSI Default
        bcdedit /set usephysicaldestination No
        bcdedit /set usefirmwarepcisettings No
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void ApplyMitigations()
        {
            string batchCommands = @"
        powershell set-ProcessMitigation -System -Disable DEP, EmulateAtlThunks, SEHOP, ForceRelocateImages, RequireInfo, BottomUp, HighEntropy, StrictHandle, DisableWin32kSystemCalls, AuditSystemCall, DisableExtensionPoints, BlockDynamicCode, AllowThreadsToOptOut, AuditDynamicCode, CFG, SuppressExports, StrictCFG, MicrosoftSignedOnly, AllowStoreSignedBinaries, AuditMicrosoftSigned, AuditStoreSigned, EnforceModuleDependencySigning, DisableNonSystemFonts, AuditFont, BlockRemoteImageLoads, BlockLowLabelImageLoads, PreferSystem32, AuditRemoteImageLoads, AuditLowLabelImageLoads, AuditPreferSystem32, EnableExportAddressFilter, AuditEnableExportAddressFilter, EnableExportAddressFilterPlus, AuditEnableExportAddressFilterPlus, EnableImportAddressFilter, AuditEnableImportAddressFilter, EnableRopStackPivot, AuditEnableRopStackPivot, EnableRopCallerCheck, AuditEnableRopCallerCheck, EnableRopSimExec, AuditEnableRopSimExec, SEHOP, AuditSEHOP, SEHOPTelemetry, TerminateOnError, DisallowChildProcessCreation, AuditChildProcess
        Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity"" /v ""Enabled"" /t REG_DWORD /d ""0"" /f
        Reg.exe add ""HKLM\Software\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe"" /v MitigationAuditOptions /t Reg_BINARY /d ""222222222222222222222222222222222222222222222222"" /f
        Reg.exe add ""HKLM\Software\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe"" /v MitigationOptions /t Reg_BINARY /d ""222222222222222222222222222222222222222222222222"" /f
        Reg.exe add ""HKLM\System\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableTsx"" /t REG_DWORD /d ""1"" /f
        Reg.exe add ""HKLM\Software\Microsoft\PolicyManager\default\DmaGuard\DeviceEnumerationPolicy"" /v ""value"" /t REG_DWORD /d ""2"" /f
        Reg.exe add ""HKLM\Software\Policies\Microsoft\Windows\DeviceGuard"" /v ""HVCIMATRequired"" /t REG_DWORD /d ""0"" /f
        Reg.exe add ""HKLM\Software\Policies\Microsoft\Windows\DeviceGuard"" /v ""EnableVirtualizationBasedSecurity"" /t REG_DWORD /d ""0"" /f
        Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableExceptionChainValidation"" /t REG_DWORD /d ""1"" /f
        Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""KernelSEHOPEnabled"" /t REG_DWORD /d ""0"" /f
        Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""MoveImages"" /t REG_DWORD /d ""0"" /f
        Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettings"" /t REG_DWORD /d ""1"" /f
        Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverride"" /t REG_DWORD /d ""3"" /f
        Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverrideMask"" /t REG_DWORD /d ""3"" /f
        Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""EnableCfg"" /t REG_DWORD /d ""0"" /f
        sc config SysMain start= auto
        sc start SysMain
        powershell ""ForEach($v in (Get-Command -Name ""Set-ProcessMitigation"").Parameters[""Disable""].Attributes.ValidValues){Set-ProcessMitigation -System -Disable $v.ToString() -ErrorAction SilentlyContinue}""
        powershell ""Remove-Item -Path ""HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\*"" -Recurse -ErrorAction SilentlyContinue""
        sc stop SysMain
        sc config SysMain start= disabled
        Reg.exe add ""HKLM\System\CurrentControlSet\Control\Session Manager"" /v ""ProtectionMode"" /t Reg_DWORD /d ""0"" /f
        Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""MitigationOptions"" /t REG_BINARY /d ""222222222222222222222222222222222222222222222222"" /f
        Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableExceptionChainValidation"" /t Reg_DWORD /d ""1"" /f
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void ApplyClassicAltTab()
        {
            string batchCommands = @"
        reg add ""HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer"" /v ""AltTabSettings"" /t REG_DWORD /d 1 /f
        dism.exe /Online /Disable-Feature:Microsoft-Hyper-V-All /Quiet /NoRestart
    ";
            ExecuteBatchCommands(batchCommands);
        }
        private void RevertCoreParking()
        {
            string batchCommands = @"
        powercfg -setacvalueindex scheme_current sub_processor CPMINCORES 0
        powercfg /setactive SCHEME_CURRENT
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void RevertClearDevices()
        {
            string batchCommands = @"
        POWERSHELL ""$Devices = Get-PnpDevice | ? Status -eq Unknown;foreach ($Device in $Devices) { &""pnputil"" /add-device $Device.InstanceId }""
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void RevertBiosTweaks()
        {
            string batchCommands = @"
        Reg.exe delete ""HKLM\System\CurrentControlSet\Services\VxD\BIOS"" /v ""CPUPriority"" /f
        Reg.exe delete ""HKLM\System\CurrentControlSet\Services\VxD\BIOS"" /v ""FastDRAM"" /f
        Reg.exe delete ""HKLM\System\CurrentControlSet\Services\VxD\BIOS"" /v ""AGPConcur"" /f
        Reg.exe delete ""HKLM\System\CurrentControlSet\Services\VxD\BIOS"" /v ""PCIConcur"" /f
        timeout /t 1 /nobreak > NUL
        bcdedit /set tscsyncpolicy legacy
        timeout /t 1 /nobreak > NUL
        bcdedit /set hypervisorlaunchtype off
        timeout /t 1 /nobreak > NUL
        bcdedit /set linearaddress57 OptOut
        bcdedit /set increaseuserva 268435328
        timeout /t 1 /nobreak > NUL
        bcdedit /set isolatedcontext No
        bcdedit /set allowedinmemorysettings 0x0
        timeout /t 1 /nobreak > NUL
        bcdedit /set vsmlaunchtype Off
        bcdedit /set vm No
        Reg.exe delete ""HKLM\Software\Policies\Microsoft\FVE"" /v ""DisableExternalDMAUnderLock"" /f
        Reg.exe delete ""HKLM\Software\Policies\Microsoft\Windows\DeviceGuard"" /v ""EnableVirtualizationBasedSecurity"" /f
        Reg.exe delete ""HKLM\Software\Policies\Microsoft\Windows\DeviceGuard"" /v ""HVCIMATRequired"" /f
        bcdedit /set x2apicpolicy Default
        bcdedit /set uselegacyapicmode No
        bcdedit /set configaccesspolicy Default
        bcdedit /set MSI Default
        bcdedit /set usephysicaldestination No
        bcdedit /set usefirmwarepcisettings No
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void RevertMitigations()
        {
            string batchCommands = @"
        powershell set-ProcessMitigation -System -Disable DEP, EmulateAtlThunks, SEHOP, ForceRelocateImages, RequireInfo, BottomUp, HighEntropy, StrictHandle, DisableWin32kSystemCalls, AuditSystemCall, DisableExtensionPoints, BlockDynamicCode, AllowThreadsToOptOut, AuditDynamicCode, CFG, SuppressExports, StrictCFG, MicrosoftSignedOnly, AllowStoreSignedBinaries, AuditMicrosoftSigned, AuditStoreSigned, EnforceModuleDependencySigning, DisableNonSystemFonts, AuditFont, BlockRemoteImageLoads, BlockLowLabelImageLoads, PreferSystem32, AuditRemoteImageLoads, AuditLowLabelImageLoads, AuditPreferSystem32, EnableExportAddressFilter, AuditEnableExportAddressFilter, EnableExportAddressFilterPlus, AuditEnableExportAddressFilterPlus, EnableImportAddressFilter, AuditEnableImportAddressFilter, EnableRopStackPivot, AuditEnableRopStackPivot, EnableRopCallerCheck, AuditEnableRopCallerCheck, EnableRopSimExec, AuditEnableRopSimExec, SEHOP, AuditSEHOP, SEHOPTelemetry, TerminateOnError, DisallowChildProcessCreation, AuditChildProcess
        Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity"" /v ""Enabled"" /f
        Reg.exe delete ""HKLM\Software\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe"" /v MitigationAuditOptions /f
        Reg.exe delete ""HKLM\Software\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe"" /v MitigationOptions /f
        Reg.exe delete ""HKLM\System\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableTsx"" /f
        Reg.exe delete ""HKLM\Software\Microsoft\PolicyManager\default\DmaGuard\DeviceEnumerationPolicy"" /v ""value"" /f
        Reg.exe delete ""HKLM\Software\Policies\Microsoft\Windows\DeviceGuard"" /v ""HVCIMATRequired"" /f
        Reg.exe delete ""HKLM\Software\Policies\Microsoft\Windows\DeviceGuard"" /v ""EnableVirtualizationBasedSecurity"" /f
        Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableExceptionChainValidation"" /f
        Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""KernelSEHOPEnabled"" /f
        Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""MoveImages"" /f
        Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettings"" /f
        Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverride"" /f
        Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverrideMask"" /f
        Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""EnableCfg"" /f
        sc stop SysMain
        sc config SysMain start= disabled
        powershell ""ForEach($v in (Get-Command -Name ""Set-ProcessMitigation"").Parameters[""Disable""].Attributes.ValidValues){Set-ProcessMitigation -System -Disable $v.ToString() -ErrorAction SilentlyContinue}"" 
        powershell ""Remove-Item -Path ""HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\*"" -Recurse -ErrorAction SilentlyContinue""
        sc config SysMain start= auto
        sc start SysMain
        Reg.exe delete ""HKLM\System\CurrentControlSet\Control\Session Manager"" /v ""ProtectionMode"" /f
        Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""MitigationOptions"" /f
        Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableExceptionChainValidation"" /f
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void RevertClassicAltTab()
        {
            string batchCommands = @"
        reg delete ""HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer"" /v ""AltTabSettings"" /f
        dism.exe /Online /Enable-Feature:Microsoft-Hyper-V-All /Quiet /NoRestart
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void DisableHyperV()
        {
            string batchCommands = @"
        dism.exe /Online /Disable-Feature:Microsoft-Hyper-V-All /Quiet /NoRestart
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void EnableHyperV()
        {
            string batchCommands = @"
        dism.exe /Online /Enable-Feature:Microsoft-Hyper-V-All /Quiet /NoRestart
    ";
            ExecuteBatchCommands(batchCommands);
        }

    }
}
