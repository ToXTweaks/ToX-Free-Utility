using Guna.UI2.WinForms;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility.LoadingForms;

namespace ToX_Free_Utility.Tabs
{
    public partial class WindowsTweaksP3 : Form
    {
        public WindowsTweaksP3()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            BCDedit.Checked = Properties.Settings.Default.BCDedit;
            USBPSavings.Checked = Properties.Settings.Default.USBPSavings;
            SetSVCHost.Checked = Properties.Settings.Default.SetSVCHost;
            Network.Checked = Properties.Settings.Default.Network;
            Memory.Checked = Properties.Settings.Default.Memory;
            DUAC.Checked = Properties.Settings.Default.DUAC;
        }

        private void SaveState(Guna2ToggleSwitch Tswitch, string Tbool)
        {
            Properties.Settings.Default[Tbool] = Tswitch.Checked;
            Properties.Settings.Default.Save();
        }

        private void BCDedit_CheckedChanged(object sender, EventArgs e) => SaveState(BCDedit, "BCDedit");

        private void USBPSavings_CheckedChanged(object sender, EventArgs e) => SaveState(USBPSavings, "USBPSavings");

        private void SetSVCHost_CheckedChanged(object sender, EventArgs e) => SaveState(SetSVCHost, "SetSVCHost");

        private void Network_CheckedChanged(object sender, EventArgs e) => SaveState(Network, "Network");

        private void Memory_CheckedChanged(object sender, EventArgs e) => SaveState(Memory, "Memory");

        private void DUAC_CheckedChanged(object sender, EventArgs e) => SaveState(DUAC, "DUAC");

        private async void BCDedit_Click(object sender, EventArgs e)
        {
            if (BCDedit.Checked) await RunWithLoadingScreenAsync(BCDeditAction);
            else RevertBCDeditAction();
        }

        private async void USBPSavings_Click(object sender, EventArgs e)
        {
            if (USBPSavings.Checked) await RunWithLoadingScreenAsync(USBPSavingsAction);
            else RevertUSBPSavingsAction();
        }

        private async void SetSVCHost_Click(object sender, EventArgs e)
        {
            if (SetSVCHost.Checked) await RunWithLoadingScreenAsync(SetSVCHostAction);
            else RevertSetSVCHostAction();
        }

        private async void Network_Click(object sender, EventArgs e)
        {
            if (Network.Checked) await RunWithLoadingScreenAsync(NetworkAction);
            else RevertNetworkAction();
        }

        private async void Memory_Click(object sender, EventArgs e)
        {
            if (Memory.Checked) await RunWithLoadingScreenAsync(MemoryAction);
            else RevertMemoryAction();
        }

        private async void DUAC_Click(object sender, EventArgs e)
        {
            if (DUAC.Checked) await RunWithLoadingScreenAsync(DUACAction);
            else RevertDUACAction();
        }

        private void BCDeditAction()
        {
            string batchCommands = @"bcdedit /set configaccesspolicy default
bcdedit /set MSI default
bcdedit /set usephysicaldestination no
bcdedit /set usefirmwarepcisettings no
bcdedit /deletevalue useplatformtick >nul 2>&1 
bcdedit /deletevalue useplatformclockJ >nul 2>&1 
bcdedit /set disabledynamictick yes
bcdedit /set tscsyncpolicy legacy
bcdedit /set x2apicpolicy enable
bcdedit /set ems no
bcdedit /set bootems no
bcdedit /set vm no
bcdedit /set sos no
bcdedit /set quietboot yes
bcdedit /set integrityservices disable
bcdedit /set bootux disabled
bcdedit /set bootlog no
bcdedit /set tpmbootentropy ForceDisable
bcdedit /set disableelamdrivers yes
bcdedit /set hypervisorlaunchtype off
bcdedit /set isolatedcontext no
bcdedit /set pae ForceDisable
bcdedit /set vsmlaunchtype off";

            ExecuteBatchCommands(batchCommands);
        }

        private void RevertBCDeditAction()
        {
            string batchCommands = @"bcdedit /deletevalue configaccesspolicy >nul 2>&1
bcdedit /deletevalue MSI >nul 2>&1
bcdedit /deletevalue usephysicaldestination >nul 2>&1
bcdedit /deletevalue usefirmwarepcisettings >nul 2>&1
bcdedit /deletevalue useplatformtick >nul 2>&1 
bcdedit /deletevalue useplatformclockJ >nul 2>&1 
bcdedit /deletevalue disabledynamictick >nul 2>&1
bcdedit /deletevalue tscsyncpolicy >nul 2>&1
bcdedit /deletevalue x2apicpolicy >nul 2>&1
bcdedit /deletevalue ems >nul 2>&1
bcdedit /deletevalue bootems >nul 2>&1
bcdedit /deletevalue vm >nul 2>&1
bcdedit /deletevalue sos >nul 2>&1
bcdedit /deletevalue quietboot >nul 2>&1
bcdedit /deletevalue integrityservices >nul 2>&1
bcdedit /deletevalue bootux >nul 2>&1
bcdedit /deletevalue bootlog >nul 2>&1
bcdedit /deletevalue tpmbootentropy >nul 2>&1
bcdedit /deletevalue disableelamdrivers >nul 2>&1
bcdedit /deletevalue hypervisorlaunchtype >nul 2>&1
bcdedit /deletevalue isolatedcontext >nul 2>&1
bcdedit /deletevalue pae >nul 2>&1
bcdedit /deletevalue vsmlaunchtype >nul 2>&1";

            ExecuteBatchCommands(batchCommands);

        }

        private void USBPSavingsAction()
        {
            string batchCommands = @"for /f %i in ('wmic path Win32_USBController get PNPDeviceID') do (
    reg add ""HKLM\System\CurrentControlSet\Enum\%i\Device Parameters\Interrupt Management\Affinity Policy"" /v ""DevicePriority"" /f
    reg add ""HKLM\System\CurrentControlSet\Enum\%i\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties"" /v ""MSISupported"" /t REG_DWORD /d ""1"" /f
)

echo !S_MAGENTA!USB Power Management!S_YELLOW! 
timeout /t 1 /nobreak > NUL
for /f %u in ('wmic path Win32_USBController get PNPDeviceID^| findstr /l ""PCI\VEN_""') do (
    set REG_PATH=""HKLM\SYSTEM\CurrentControlSet\Enum\%u\Device Parameters""
    
    reg add !REG_PATH! /v SelectiveSuspendOn /t REG_DWORD /d 0 /f
    reg add !REG_PATH! /v SelectiveSuspendEnabled /t REG_BINARY /d 00 /f
    reg add !REG_PATH! /v EnhancedPowerManagementEnabled /t REG_DWORD /d 0 /f
    reg add !REG_PATH! /v AllowIdleIrpInD3 /t REG_DWORD /d 0 /f
    reg add !REG_PATH!\WDF /v IdleInWorkingState /t REG_DWORD /d 0 /f
)

for /f %i in ('wmic path Win32_USBController get PNPDeviceID^| findstr /l ""PCI\VEN_""') do (
	reg add ""HKLM\SYSTEM\CurrentControlSet\Enum\%i\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties"" /v ""MSISupported"" /t REG_DWORD /d ""1"" /f
	reg add ""HKLM\SYSTEM\CurrentControlSet\Enum\%i\Device Parameters\Interrupt Management\Affinity Policy"" /v ""DevicePriority"" /t REG_DWORD /d ""0"" /f

echo !S_MAGENTA!Disable USB PowerSavings!S_YELLOW! 
timeout /t 1 /nobreak > NUL
	reg add ""HKLM\SYSTEM\CurrentControlSet\Enum\%i\Device Parameters"" /v ""AllowIdleIrpInD3"" /t REG_DWORD /d ""0"" /f
	reg add ""HKLM\SYSTEM\CurrentControlSet\Enum\%i\Device Parameters"" /v ""D3ColdSupported"" /t REG_DWORD /d ""0"" /f
	reg add ""HKLM\SYSTEM\CurrentControlSet\Enum\%i\Device Parameters"" /v ""DeviceSelectiveSuspended"" /t REG_DWORD /d ""0"" /f
	reg add ""HKLM\SYSTEM\CurrentControlSet\Enum\%i\Device Parameters"" /v ""EnableSelectiveSuspend"" /t REG_DWORD /d ""0"" /f
	reg add ""HKLM\SYSTEM\CurrentControlSet\Enum\%i\Device Parameters"" /v ""EnhancedPowerManagementEnabled"" /t REG_DWORD /d ""0"" /f
	reg add ""HKLM\SYSTEM\CurrentControlSet\Enum\%i\Device Parameters"" /v ""SelectiveSuspendEnabled"" /t REG_DWORD /d ""0"" /f
	reg add ""HKLM\SYSTEM\CurrentControlSet\Enum\%i\Device Parameters"" /v ""SelectiveSuspendOn"" /t REG_DWORD /d ""0"" /f
)

for %a in (
	EnhancedPowerManagementEnabled
	AllowIdleIrpInD3
	EnableSelectiveSuspend
	DeviceSelectiveSuspended
	SelectiveSuspendEnabled
	SelectiveSuspendOn
	EnumerationRetryCount
	ExtPropDescSemaphore
	WaitWakeEnabled
	D3ColdSupported
	WdfDirectedPowerTransitionEnable
	EnableIdlePowerManagement
	IdleInWorkingState
	IoLatencyCap
	Dmaecho !S_MAGENTA!appingCompatible
	Dmaecho !S_MAGENTA!appingCompatibleSelfhost
) do for /f ""delims="" %b in ('reg query ""HKLM\SYSTEM\CurrentControlSet\Enum"" /s /f ""%a"" ^| findstr ""HKEY""') do reg add ""%b"" /v ""%a"" /t REG_DWORD /d ""0"" /f
";

            ExecuteBatchCommands(batchCommands);
        }

        private void RevertUSBPSavingsAction()
        {
            string batchCommands = @"@echo off
setlocal enabledelayedexpansion

rem Resetting USB Interrupt Management Affinity Policy and MessageSignaledInterruptProperties to default
for /f %%i in ('wmic path Win32_USBController get PNPDeviceID') do (
    reg delete ""HKLM\System\CurrentControlSet\Enum\%%i\Device Parameters\Interrupt Management\Affinity Policy"" /f > nul 2>&1
    reg delete ""HKLM\System\CurrentControlSet\Enum\%%i\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties"" /f > nul 2>&1
)

echo !S_MAGENTA!Reset USB Power Management to default values!S_YELLOW!
timeout /t 1 /nobreak > NUL

rem Resetting USB Power Management values for specific devices
for /f %%u in ('wmic path Win32_USBController get PNPDeviceID^| findstr /l ""PCI\VEN_""') do (
    set REG_PATH=""HKLM\SYSTEM\CurrentControlSet\Enum\%%u\Device Parameters""

    reg delete !REG_PATH! /v SelectiveSuspendOn /f > nul 2>&1
    reg delete !REG_PATH! /v SelectiveSuspendEnabled /f > nul 2>&1
    reg delete !REG_PATH! /v EnhancedPowerManagementEnabled /f > nul 2>&1
    reg delete !REG_PATH! /v AllowIdleIrpInD3 /f > nul 2>&1
    reg delete !REG_PATH!\WDF /v IdleInWorkingState /f > nul 2>&1
)

rem Resetting MSISupported and DevicePriority for USB Interrupt Management
for /f %%i in ('wmic path Win32_USBController get PNPDeviceID^| findstr /l ""PCI\VEN_""') do (
    reg delete ""HKLM\SYSTEM\CurrentControlSet\Enum\%%i\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties"" /v ""MSISupported"" /f > nul 2>&1
    reg delete ""HKLM\SYSTEM\CurrentControlSet\Enum\%%i\Device Parameters\Interrupt Management\Affinity Policy"" /v ""DevicePriority"" /f > nul 2>&1
)

rem Resetting USB Power Savings related settings
for /f %%i in ('wmic path Win32_USBController get PNPDeviceID^| findstr /l ""PCI\VEN_""') do (
    reg delete ""HKLM\SYSTEM\CurrentControlSet\Enum\%%i\Device Parameters"" /v ""AllowIdleIrpInD3"" /f > nul 2>&1
    reg delete ""HKLM\SYSTEM\CurrentControlSet\Enum\%%i\Device Parameters"" /v ""D3ColdSupported"" /f > nul 2>&1
    reg delete ""HKLM\SYSTEM\CurrentControlSet\Enum\%%i\Device Parameters"" /v ""DeviceSelectiveSuspended"" /f > nul 2>&1
    reg delete ""HKLM\SYSTEM\CurrentControlSet\Enum\%%i\Device Parameters"" /v ""EnableSelectiveSuspend"" /f > nul 2>&1
    reg delete ""HKLM\SYSTEM\CurrentControlSet\Enum\%%i\Device Parameters"" /v ""EnhancedPowerManagementEnabled"" /f > nul 2>&1
    reg delete ""HKLM\SYSTEM\CurrentControlSet\Enum\%%i\Device Parameters"" /v ""SelectiveSuspendEnabled"" /f > nul 2>&1
    reg delete ""HKLM\SYSTEM\CurrentControlSet\Enum\%%i\Device Parameters"" /v ""SelectiveSuspendOn"" /f > nul 2>&1
)

rem Resetting all the additional power management keys to default (setting values to 0)
for %%a in (
    EnhancedPowerManagementEnabled
    AllowIdleIrpInD3
    EnableSelectiveSuspend
    DeviceSelectiveSuspended
    SelectiveSuspendEnabled
    SelectiveSuspendOn
    EnumerationRetryCount
    ExtPropDescSemaphore
    WaitWakeEnabled
    D3ColdSupported
    WdfDirectedPowerTransitionEnable
    EnableIdlePowerManagement
    IdleInWorkingState
    IoLatencyCap
) do for /f ""delims="" %%b in ('reg query ""HKLM\SYSTEM\CurrentControlSet\Enum"" /s /f ""%%a"" ^| findstr ""HKEY""') do reg delete ""%%b"" /v ""%%a"" /f > nul 2>&1

echo USB power management and interrupt settings have been reset to default.
endlocal
";

            ExecuteBatchCommands(batchCommands);
        }

        private void SetSVCHostAction()
        {
            string batchCommands = @"for /f ""tokens=2 delims=="" %i in ('wmic os get TotalVisibleMemorySize /value') do set /a mem=%i + 1024000
reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control"" /v ""SvcHostSplitThresholdInKB"" /t REG_DWORD /d %mem% /f";

            ExecuteBatchCommands(batchCommands);
        }

        private void RevertSetSVCHostAction()
        {
            string batchCommands = @"reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control"" /v ""SvcHostSplitThresholdInKB"" /t REG_DWORD /d 1024 /f
";

            ExecuteBatchCommands(batchCommands);
        }

        private void NetworkAction()
        {
            string batchCommands = @"PowerShell Disable-NetAdapterLso -Name ""*""

powershell ""ForEach($adapter In Get-NetAdapter){Disable-NetAdapterPowerManagement -Name $adapter.Name -ErrorAction SilentlyContinue}""

powershell ""ForEach($adapter In Get-NetAdapter){Disable-NetAdapterLso -Name $adapter.Name -ErrorAction SilentlyContinue}""

POWERSHELL Disable-NetAdapterPowerManagement -Name ""*"" -ErrorAction SilentlyContinue

:: Get the Sub ID of the Network Adapter
for /f %n in ('Reg query ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}"" /v ""*SpeedDuplex"" /s ^| findstr  ""HKEY""') do (

:: Disable NIC Power Savings
echo Disabling Hidden Power Saving Network Features and nic power saving features
Reg.exe add ""%n"" /v ""AutoPowerSaveModeEnabled"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""AutoDisableGigabit"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""AdvancedEEE"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""DisableDelayedPowerUp"" /t REG_SZ /d ""2"" /f
Reg.exe add ""%n"" /v ""*EEE"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""EEE"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""EnablePME"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""EEELinkAdvertisement"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""EnableGreenEthernet"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""EnableSavePowerNow"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""EnablePowerManagement"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""EnableDynamicPowerGating"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""EnableConnectedPowerGating"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""EnableWakeOnLan"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""GigaLite"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""NicAutoPowerSaver"" /t REG_SZ /d ""2"" /f
Reg.exe add ""%n"" /v ""PowerDownPll"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""PowerSavingMode"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""ReduceSpeedOnPowerDown"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""S5NicKeepOverrideMacAddrV2"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""S5WakeOnLan"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""ULPMode"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""WakeOnDisconnect"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""*WakeOnMagicPacket"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""*WakeOnPattern"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""WakeOnLink"" /t REG_SZ /d ""0"" /f
Reg.exe add ""%n"" /v ""WolShutdownLinkSpeed"" /t REG_SZ /d ""2"" /f
)";

            ExecuteBatchCommands(batchCommands);
        }

        private void RevertNetworkAction()
        {
            string batchCommands = @"PowerShell Disable-NetAdapterLso -Name ""*""

powershell ""ForEach($adapter In Get-NetAdapter){Disable-NetAdapterPowerManagement -Name $adapter.Name -ErrorAction SilentlyContinue}""

powershell ""ForEach($adapter In Get-NetAdapter){Disable-NetAdapterLso -Name $adapter.Name -ErrorAction SilentlyContinue}""

POWERSHELL Disable-NetAdapterPowerManagement -Name ""*"" -ErrorAction SilentlyContinue

:: Reset the NIC Power Saving settings and other adjustments to default
for /f %n in ('Reg query ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}"" /v ""*SpeedDuplex"" /s ^| findstr  ""HKEY""') do (

    :: Restore default values for hidden power saving network features
    Reg.exe delete ""%n"" /v ""AutoPowerSaveModeEnabled"" /f
    Reg.exe delete ""%n"" /v ""AutoDisableGigabit"" /f
    Reg.exe delete ""%n"" /v ""AdvancedEEE"" /f
    Reg.exe delete ""%n"" /v ""DisableDelayedPowerUp"" /f
    Reg.exe delete ""%n"" /v ""*EEE"" /f
    Reg.exe delete ""%n"" /v ""EEE"" /f
    Reg.exe delete ""%n"" /v ""EnablePME"" /f
    Reg.exe delete ""%n"" /v ""EEELinkAdvertisement"" /f
    Reg.exe delete ""%n"" /v ""EnableGreenEthernet"" /f
    Reg.exe delete ""%n"" /v ""EnableSavePowerNow"" /f
    Reg.exe delete ""%n"" /v ""EnablePowerManagement"" /f
    Reg.exe delete ""%n"" /v ""EnableDynamicPowerGating"" /f
    Reg.exe delete ""%n"" /v ""EnableConnectedPowerGating"" /f
    Reg.exe delete ""%n"" /v ""EnableWakeOnLan"" /f
    Reg.exe delete ""%n"" /v ""GigaLite"" /f
    Reg.exe delete ""%n"" /v ""NicAutoPowerSaver"" /f
    Reg.exe delete ""%n"" /v ""PowerDownPll"" /f
    Reg.exe delete ""%n"" /v ""PowerSavingMode"" /f
    Reg.exe delete ""%n"" /v ""ReduceSpeedOnPowerDown"" /f
    Reg.exe delete ""%n"" /v ""S5NicKeepOverrideMacAddrV2"" /f
    Reg.exe delete ""%n"" /v ""S5WakeOnLan"" /f
    Reg.exe delete ""%n"" /v ""ULPMode"" /f
    Reg.exe delete ""%n"" /v ""WakeOnDisconnect"" /f
    Reg.exe delete ""%n"" /v ""*WakeOnMagicPacket"" /f
    Reg.exe delete ""%n"" /v ""*WakeOnPattern"" /f
    Reg.exe delete ""%n"" /v ""WakeOnLink"" /f
    Reg.exe delete ""%n"" /v ""WolShutdownLinkSpeed"" /f
)
";

            ExecuteBatchCommands(batchCommands);
        }

        private void MemoryAction()
        {
            string batchCommands = @"
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""ClearPageFileAtShutdown"" /t REG_DWORD /d 00000000 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""DisablePagingExecutive"" /t REG_DWORD /d 00000001 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""LargeSystemCache"" /t REG_DWORD /d 00000000 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""NonPagedPoolQuota"" /t REG_DWORD /d 00000000 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""NonPagedPoolSize"" /t REG_DWORD /d 00000000 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""PagedPoolQuota"" /t REG_DWORD /d 00000000 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""PagedPoolSize"" /t REG_DWORD /d 000000c0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""SecondLevelDataCache"" /t REG_DWORD /d 00000400 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""SessionPoolSize"" /t REG_DWORD /d 000000c0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""SessionViewSize"" /t REG_DWORD /d 000000c0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""SystemPages"" /t REG_DWORD /d ffffffff /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""PhysicalAddressExtension"" /t REG_DWORD /d 00000001 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettings"" /t REG_DWORD /d 00000001 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverride"" /t REG_DWORD /d 00000003 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverrideMask"" /t REG_DWORD /d 00000003 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""IoPageLockLimit"" /t REG_DWORD /d 00fefc00 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""PoolUsageMaximum"" /t REG_DWORD /d 00000060 /f

REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters"" /v ""EnablePrefetcher"" /t REG_DWORD /d 00000003 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters"" /v ""EnableSuperfetch"" /t REG_DWORD /d 00000003 /f
";

            ExecuteBatchCommands(batchCommands);
        }

        private void RevertMemoryAction()
        {
            string batchCommands = @"
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""ClearPageFileAtShutdown"" /t REG_DWORD /d 0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""DisablePagingExecutive"" /t REG_DWORD /d 0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""LargeSystemCache"" /t REG_DWORD /d 1 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""NonPagedPoolQuota"" /t REG_DWORD /d 0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""NonPagedPoolSize"" /t REG_DWORD /d 0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""PagedPoolQuota"" /t REG_DWORD /d 0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""PagedPoolSize"" /t REG_DWORD /d 0xC0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""SecondLevelDataCache"" /t REG_DWORD /d 0x400 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""SessionPoolSize"" /t REG_DWORD /d 0xC0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""SessionViewSize"" /t REG_DWORD /d 0xC0 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""SystemPages"" /t REG_DWORD /d 0xFFFFFFFF /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""PhysicalAddressExtension"" /t REG_DWORD /d 1 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettings"" /t REG_DWORD /d 1 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverride"" /t REG_DWORD /d 3 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverrideMask"" /t REG_DWORD /d 3 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""IoPageLockLimit"" /t REG_DWORD /d 0xFefc00 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""PoolUsageMaximum"" /t REG_DWORD /d 0x60 /f

REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters"" /v ""EnablePrefetcher"" /t REG_DWORD /d 3 /f
REG ADD ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters"" /v ""EnableSuperfetch"" /t REG_DWORD /d 3 /f
";

            ExecuteBatchCommands(batchCommands);
        }

        private void DUACAction()
        {
            string batchCommands = @"
REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"" /v ""EnableLUA"" /t REG_DWORD /d 0 /f
";

            ExecuteBatchCommands(batchCommands);
        }

        private void RevertDUACAction()
        {
            string batchCommands = @"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"" /v ""EnableLUA"" /t REG_DWORD /d 1 /f";


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

        private void guna2ImageButton2_Click(object sender, EventArgs e)
        {
            if (Main.Instance != null)
            {
                WindowsTweaksP2 myForm = new WindowsTweaksP2();
                myForm.TopLevel = false;
                Main.Instance.MainPanel.Controls.Clear();
                Main.Instance.guna2Transition1.HideSync(Main.Instance.MainPanel);
                Task.Delay(1).ContinueWith(_ =>
                {
                    Main.Instance.MainPanel.Invoke((Action)(() =>
                    {
                        Main.Instance.MainPanel.Controls.Add(myForm);
                        myForm.Show();
                        Main.Instance.guna2Transition1.ShowSync(Main.Instance.MainPanel);
                    }));
                });
            }
        }

        private void NextPage_Click_1(object sender, EventArgs e)
        {
            if (Main.Instance != null)
            {
                WindowsTweaksP4 myForm = new WindowsTweaksP4();
                myForm.TopLevel = false;
                Main.Instance.MainPanel.Controls.Clear();
                Main.Instance.guna2Transition1.HideSync(Main.Instance.MainPanel);
                Task.Delay(1).ContinueWith(_ =>
                {
                    Main.Instance.MainPanel.Invoke((Action)(() =>
                    {
                        Main.Instance.MainPanel.Controls.Add(myForm);
                        myForm.Show();
                        Main.Instance.guna2Transition1.ShowSync(Main.Instance.MainPanel);
                    }));
                });
            }
        }
    }
}