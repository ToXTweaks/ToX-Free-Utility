using Guna.UI2.WinForms;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility.LoadingForms;

namespace ToX_Free_Utility.Tabs
{
    public partial class WindowsTweaksP5 : Form
    {
        public WindowsTweaksP5()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            Services.Checked = Properties.Settings.Default.Services;
            Kernel.Checked = Properties.Settings.Default.Kernel;
            Power.Checked = Properties.Settings.Default.Power;
            FSE.Checked = Properties.Settings.Default.FSE;
            SSD.Checked = Properties.Settings.Default.SSD;
            Hiber.Checked = Properties.Settings.Default.Hiber;
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
                WindowsTweaksP4 myForm = new WindowsTweaksP4();
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

        private void CoreParking_CheckedChanged(object sender, EventArgs e) => SaveState(Services, "Services");
        private void ClearDevices_CheckedChanged(object sender, EventArgs e) => SaveState(Kernel, "Kernel");
        private void BiosTweaks_CheckedChanged(object sender, EventArgs e) => SaveState(Power, "Power");
        private void Mitigations_CheckedChanged(object sender, EventArgs e) => SaveState(FSE, "FSE");
        private void HyperV_CheckedChanged(object sender, EventArgs e) => SaveState(Hiber, "Hiber");
        private void AltTab_CheckedChanged(object sender, EventArgs e) => SaveState(SSD, "SSD");

        private async void CoreParking_Click(object sender, EventArgs e)
        {
            if (Services.Checked) await RunWithLoadingScreenAsync(ADServices);
            else RevertADServices();
        }

        private async void ClearDevices_Click(object sender, EventArgs e)
        {
            if (Kernel.Checked) await RunWithLoadingScreenAsync(KernelTweaks);
            else RevertKernelTweaks();
        }

        private async void BiosTweaks_Click(object sender, EventArgs e)
        {
            if (Power.Checked) await RunWithLoadingScreenAsync(PowerTweaks);
            else RevertPowerTweaks();
        }

        private async void Mitigations_Click(object sender, EventArgs e)
        {
            if (FSE.Checked) await RunWithLoadingScreenAsync(FSETweaks);
            else RevertFSETweaks();
        }

        private async void HyperV_Click(object sender, EventArgs e)
        {
            if (Hiber.Checked) await RunWithLoadingScreenAsync(Hibernation);
            else RevertHibernation();
        }

        private async void AltTab_Click(object sender, EventArgs e)
        {
            if (SSD.Checked) await RunWithLoadingScreenAsync(SSDTweaks);
            else RevertSSDTweaks();
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
        private void ADServices()
        {
            string batchCommands = @"
sc stop AJRouter > nul
sc config AJRouter start= disabled > nul

sc stop AppXSvc > nul
sc config AppXSvc start= disabled > nul

sc stop ALG > nul
sc config ALG start= disabled > nul

sc stop AppMgmt > nul
sc config AppMgmt start= disabled > nul

sc stop tzautoupdate > nul
sc config tzautoupdate start= disabled > nul

sc stop AssignedAccessManagerSvc > nul 
sc config AssignedAccessManagerSvc start= disabled > nul

sc stop BITS > nul
sc config BITS start= disabled > nul

sc stop BDESVC > nul
sc config BDESVC start= disabled > nul

sc stop wbengine > nul
sc config wbengine start= disabled > nul

sc stop BTAGService > nul
sc config BTAGService start= disabled > nul

sc stop bthserv > nul
sc config bthserv start= disabled > nul

sc stop BthHFSrv > nul
sc config BthHFSrv start= disabled > nul

sc stop PeerDistSvc > nul
sc config PeerDistSvc start= disabled > nul

sc stop CertPropSvc > nul 
sc config CertPropSvc start= disabled > nul

sc stop ClipSVC > nul
sc config ClipSVC start= disabled > nul

sc stop DiagTrack > nul
sc config DiagTrack start= disabled > nul

sc stop VaultSvc > nul
sc config VaultSvc start= disabled > nul

sc stop CDPSvc > nul 
sc config CDPSvc start= disabled > nul 

sc stop DusmSvc > nul
sc config DusmSvc start= disabled > nul
 
sc stop DoSvc > nul
sc config DoSvc start= disabled > nul

sc stop diagsvc > nul
sc config diagsvc start= disabled > nul

sc stop DPS > nul
sc config DPS start= disabled > nul

sc stop WdiServiceHost > nul
sc config WdiServiceHost start= disabled > nul

sc stop WdiSystemHost > nul
sc config WdiSystemHost start= disabled > nul

sc stop TrkWks > nul
sc config TrkWks start= disabled > nul

sc stop MSDTC > nul
sc config MSDTC start= disabled > nul

sc stop dmwappushservice > nul 
sc config dmwappushservice start= disabled > nul

sc stop DisplayEnhancementService > nul
sc config DisplayEnhancementService start= disabled > nul

sc stop MapsBroker > nul
sc config MapsBroker start= disabled > nul

sc stop fdPHost > nul
sc config fdPHost start= disabled > nul

sc stop FDResPub > nul
sc config FDResPub start= disabled > nul

sc stop EFS > nul
sc config EFS start= disabled > nul

sc stop EntAppSvc > nul
sc config EntAppSvc start= disabled > nul

sc stop fhsvc > nul
sc config fhsvc start= disabled > nul

sc stop lfsvc > nul
sc config lfsvc start= disabled > nul

sc stop HomeGroupListener > nul
sc config HomeGroupListener start= disabled > nul

sc stop HomeGroupProvider > nul
sc config HomeGroupProvider start= disabled > nul

sc stop HvHost > nul
sc config HvHost start= disabled > nul

sc stop hns > nul
sc config hns start= disabled > nul

sc stop vmickvpexchange > nul
sc config vmickvpexchange start= disabled > nul

sc stop vmicguestinterface > nul
sc config vmicguestinterface start= disabled > nul

sc stop vmicshutdown > nul
sc config vmicshutdown start= disabled > nul

sc stop vmicheartbeat > nul
sc config vmicheartbeat start= disabled > nul

sc stop vmicvmsession > nul
sc config vmicvmsession start= disabled > nul
 
sc stop vmicrdv > nul
sc config vmicrdv start= disabled > nul

sc stop vmictimesync > nul
sc config vmictimesync start= disabled > nul

sc stop vmicvss > nul
sc config vmicvss start= disabled > nul

sc stop IEEtwCollectorService > nul
sc config IEEtwCollectorService start= disabled > nul

sc stop iphlpsvc > nul
sc config iphlpsvc start= disabled > nul 

sc stop IpxlatCfgSvc > nul
sc config IpxlatCfgSvc start= disabled > nul
 
sc stop PolicyAgent > nul
sc config PolicyAgent start= disabled > nul

sc stop irmon > nul
sc config irmon start= disabled > nul

sc stop SharedAccess > nul
sc config SharedAccess start= disabled > nul

sc stop lltdsvc > nul
sc config lltdsvc start= disabled > nul

sc stop diagnosticshub.standardcollector.service > nul
sc config diagnosticshub.standardcollector.service start= disabled > nul

sc stop wlidsvc > nul
sc config wlidsvc start= disabled > nul

sc stop AppVClient > nul
sc config AppVClient start= disabled > nul

sc stop smphost > nul
sc config smphost start= disabled > nul

sc stop InstallService > nul
sc config InstallService start= disabled > nul
  
sc stop SmsRouter > nul
sc config SmsRouter start= disabled > nul

sc stop MSiSCSI > nul
sc config MSiSCSI start= disabled > nul

sc stop NaturalAuthentication > nul
sc config NaturalAuthentication start= disabled > nul

sc stop CscService > nul
sc config CscService start= disabled > nul

sc stop defragsvc > nul
sc config defragsvc start= disabled > nul

sc stop SEMgrSvc > nul
sc config SEMgrSvc start= disabled > nul

sc stop PNRPsvc > nul
sc config PNRPsvc start= disabled > nul

sc stop p2psvc > nul
sc config p2psvc start= disabled > nul

sc stop p2pimsvc > nul
sc config p2pimsvc start= disabled > nul

sc stop pla > nul
sc config pla start= disabled > nul

sc stop PhoneSvc > nul
sc config PhoneSvc start= disabled > nul

sc stop WPDBusEnum > nul
sc config WPDBusEnum start= disabled > nul

sc stop Spooler > nul
sc config Spooler start= disabled > nul

sc stop PrintNotify > nul
sc config PrintNotify start= disabled > nul

sc stop PcaSvc > nul
sc config PcaSvc start= disabled > nul

sc stop WpcMonSvc > nul
sc config WpcMonSvc start= disabled > nul

sc stop QWAVE > nul
sc config QWAVE start= disabled > nul

sc stop RasAuto > nul
sc config RasAuto start= disabled > nul
 
sc stop RasMan > nul
sc config RasMan start= disabled > nul

sc stop SessionEnv > nul
sc config SessionEnv start= disabled > nul

sc stop TermService > nul
sc config TermService start= disabled > nul

sc stop UmRdpService > nul 
sc config UmRdpService start= disabled > nul

sc stop RpcLocator > nul
sc config RpcLocator start= disabled > nul

sc stop RemoteRegistry > nul
sc config RemoteRegistry start= disabled > nul

sc stop RetailDemo > nul
sc config RetailDemo start= disabled > nul

sc stop RemoteAccess > nul
sc config RemoteAccess start= disabled > nul
 
sc stop RmSvc > nul 
sc config RmSvc start= disabled > nul

sc stop SNMPTRAP > nul
sc config SNMPTRAP start= disabled > nul

sc stop seclogon > nul
sc config seclogon start= disabled > nul

sc stop wscsvc > nul
sc config wscsvc start= disabled > nul

sc stop SamSs > nul
sc config SamSs start= disabled > nul

sc stop SensorDataService > nul
sc config SensorDataService start= disabled > nul

sc stop SensrSvc > nul
sc config SensrSvc start= disabled > nul

sc stop SensorService > nul
sc config SensorService start= disabled > nul

sc stop LanmanServer > nul
sc config LanmanServer start= disabled > nul

sc stop shpamsvc > nul
sc config shpamsvc start= disabled > nul

sc stop ShellHWDetection > nul
sc config ShellHWDetection start= disabled > nul

sc stop SCardSvr > nul
sc config SCardSvr start= disabled > nul

sc stop ScDeviceEnum > nul
sc config ScDeviceEnum start= disabled > nul

sc stop SCPolicySvc > nul
sc config SCPolicySvc start= disabled > nul

sc stop SharedRealitySvc > nul
sc config SharedRealitySvc start= disabled > nul

sc stop StorSvc > nul
sc config StorSvc start= disabled > nul

sc stop TieringEngineService > nul
sc config TieringEngineService start= disabled > nul

sc stop SysMain > nul
sc config SysMain start= disabled > nul

sc stop SgrmBroker > nul
sc config SgrmBroker start= disabled > nul

sc stop lmhosts > nul
sc config lmhosts start= disabled > nul

sc stop TapiSrv > nul
sc config TapiSrv start= disabled > nul

sc stop Themes > nul
sc config Themes start= disabled > nul

sc stop tiledatamodelsvc > nul
sc config tiledatamodelsvc start= disabled > nul

sc stop TabletInputService > nul
sc config TabletInputService start= disabled > nul

sc stop UsoSvc > nul
sc config UsoSvc start= disabled > nul

sc stop UevAgentService > nul
sc config UevAgentService start= disabled > nul

sc stop WalletService > nul
sc config WalletService start= disabled > nul

sc stop wmiApSrv > nul
sc config wmiApSrv start= disabled > nul

sc stop TokenBroker > nul
sc config TokenBroker start= disabled > nul

sc stop WebClient > nul
sc config WebClient start= disabled > nul

sc stop WFDSConMgrSvc > nul
sc config WFDSConMgrSvc start= disabled > nul

sc stop SDRSVC > nul
sc config SDRSVC start= disabled > nul
 
sc stop WbioSrvc > nul
sc config WbioSrvc start= disabled > nul

sc stop FrameServer > nul
sc config FrameServer start= disabled > nul
 
sc stop wcncsvc > nul
sc config wcncsvc start= disabled > nul

sc stop Sense > nul
sc config Sense start= disabled > nul

sc stop WdNisSvc > nul
sc config WdNisSvc start= disabled > nul

sc stop WinDefend > nul
sc config WinDefend start= disabled > nul

sc stop SecurityHealthService > nul
sc config SecurityHealthService start= disabled > nul

sc stop WEPHOSTSVC > nul
sc config WEPHOSTSVC start= disabled > nul

sc stop WerSvc > nul
sc config WerSvc start= disabled > nul

sc stop Wecsvc > nul
sc config Wecsvc start= disabled > nul

sc stop FontCache > nul
sc config FontCache start= disabled > nul

sc stop StiSvc > nul
sc config StiSvc start= disabled > nul

sc stop wisvc > nul
sc config wisvc start= disabled > nul

sc stop LicenseManager > nul
sc config LicenseManager start= disabled > nul

sc stop icssvc > nul
sc config icssvc start= disabled > nul

sc stop WMPNetworkSvc > nul
sc config WMPNetworkSvc start= disabled > nul

sc stop FontCache3.0.0.0 > nul
sc config FontCache3.0.0.0 start= disabled > nul

sc stop WpnService > nul
sc config WpnService start= disabled > nul

sc stop perceptionsimulation > nul
sc config perceptionsimulation start= disabled > nul

sc stop spectrum > nul 
sc config spectrum start= disabled > nul
 
sc stop WinRM > nul
sc config WinRM start= disabled > nul

sc stop WSearch > nul
sc config WSearch start= disabled > nul

sc stop SecurityHealthService > nul
sc config SecurityHealthService start= disabled > nul

sc stop W32Time > nul
sc config W32Time start= disabled > nul

sc stop wuauserv > nul
sc config wuauserv start= disabled > nul

sc stop WaaSMedicSvc > nul
sc config WaaSMedicSvc start= disabled > nul

sc stop LanmanWorkstation > nul
sc config LanmanWorkstation start= disabled > nul

sc stop XboxGipSvc > nul
sc config XboxGipSvc start= disabled > nul

sc stop xbgm > nul
sc config xbgm start= disabled > nul

sc stop XblAuthManager > nul
sc config XblAuthManager start= disabled > nul

sc stop XblGameSave > nul
sc config XblGameSave start= disabled > nul
  
sc stop XboxNetApiSvc > nul
sc config XboxNetApiSvc start= disabled > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BluetoothUserService"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CDPUserSvc"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CaptureService"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\ConsentUxUserSvc"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PimIndexMaintenanceSvc"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DevicePickerUserSvc"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DevicesFlowUserSvc"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BcastDVRUserService"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MessagingService"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PrintWorkflowUserSvc"" /v Start /t REG_DWORD /d 00000004 /f > nul 

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\OneSyncSvc"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UserDataSvc"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UnistoreSvc"" /v Start /t REG_DWORD /d 00000004 /f > nul

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\WpnUserService"" /v Start /t REG_DWORD /d 00000004 /f > nul";
            ExecuteBatchCommands(batchCommands);
        }

        private void KernelTweaks()
        {
            string batchCommands = @"Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\DXGKrnl"" /v ""MonitorLatencyTolerance"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\DXGKrnl"" /v ""MonitorRefreshLatencyTolerance"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\DXGKrnl"" /v ""MonitorLatencyTolerance"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Services\DXGKrnl"" /v ""MonitorRefreshLatencyTolerance"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DpcWatchdogProfileOffset"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableExceptionChainValidation"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""KernelSEHOPEnabled"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableAutoBoost"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DpcTimeout"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""ThreadDpcEnable"" /t REG_DWORD /d ""1"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DpcWatchdogPeriod"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""InterruptSteeringDisabled"" /t REG_DWORD /d ""1"" /f

    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void PowerTweaks()
        {
            string batchCommands = @"
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""CoalescingTimerInterval"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""PlatformAoAcOverride"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""EnergyEstimationEnabled"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""EventProcessorEnabled"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""CsEnabled"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling"" /v ""PowerThrottlingOff"" /t REG_DWORD /d ""1"" /f
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void FSETweaks()
        {
            string batchCommands = @"
Reg.exe add ""HKLM\SOFTWARE\Microsoft\PolicyManager\default\ApplicationManagement\AllowGameDVR"" /v ""value"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKCU\System\GameConfigStore"" /v ""GameDVR_Enabled"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKCU\System\GameConfigStore"" /v ""GameDVR_FSEBehaviorMode"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKCU\System\GameConfigStore"" /v ""GameDVR_HonorUserFSEBehaviorMode"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKCU\System\GameConfigStore"" /v ""GameDVR_DXGIHonorFSEWindowsCompatible"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKCU\System\GameConfigStore"" /v ""GameDVR_EFSEFeatureFlags"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR"" /v ""AllowGameDVR"" /t REG_SZ /d ""0"" /f
Reg.exe add ""HKLM\SOFTWARE\Microsoft\PolicyManager\default\ApplicationManagement\AllowGameDVR"" /v ""value"" /t REG_SZ /d ""00000000"" /f
Reg.exe add ""HKCU\Software\Microsoft\GameBar"" /v ""UseNexusForGameBarEnabled"" /t REG_DWORD /d ""0"" /f
Reg.exe add ""HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR"" /v ""AppCaptureEnabled"" /t REG_DWORD /d ""0"" /f

    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void SSDTweaks()
        {
            string batchCommands = @"
        Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SD\IdleState\1"" /v ""IdleExitEnergyMicroJoules"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SD\IdleState\1"" /v ""IdleExitLatencyMs"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SD\IdleState\1"" /v ""IdlePowerMw"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SD\IdleState\1"" /v ""IdleTimeLengthMs"" /t REG_DWORD /d ""4294967295"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\1"" /v ""IdleExitEnergyMicroJoules"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\1"" /v ""IdleExitLatencyMs"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\1"" /v ""IdlePowerMw"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\1"" /v ""IdleTimeLengthMs"" /t REG_DWORD /d ""4294967295"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\2"" /v ""IdleExitEnergyMicroJoules"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\2"" /v ""IdleExitLatencyMs"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\2"" /v ""IdlePowerMw"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\2"" /v ""IdleTimeLengthMs"" /t REG_DWORD /d ""4294967295"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\3"" /v ""IdleExitEnergyMicroJoules"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\3"" /v ""IdleExitLatencyMs"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\3"" /v ""IdlePowerMw"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\3"" /v ""IdleTimeLengthMs"" /t REG_DWORD /d ""4294967295"" /f 
    ";
            ExecuteBatchCommands(batchCommands);
        }
        private void RevertADServices()
        {
            string batchCommands = @"@
:: Re-enable previously disabled advanced services
sc config AJRouter start= demand > nul
sc config AppXSvc start= demand > nul
sc config ALG start= demand > nul
sc config AppMgmt start= demand > nul
sc config tzautoupdate start= demand > nul
sc config AssignedAccessManagerSvc start= demand > nul
sc config BITS start= demand > nul
sc config BDESVC start= demand > nul
sc config wbengine start= demand > nul
sc config BTAGService start= demand > nul
sc config bthserv start= demand > nul
sc config BthHFSrv start= demand > nul
sc config PeerDistSvc start= demand > nul
sc config CertPropSvc start= demand > nul
sc config ClipSVC start= demand > nul
sc config DiagTrack start= demand > nul
sc config VaultSvc start= demand > nul
sc config CDPSvc start= demand > nul
sc config DusmSvc start= demand > nul
sc config DoSvc start= demand > nul
sc config diagsvc start= demand > nul
sc config DPS start= demand > nul
sc config WdiServiceHost start= demand > nul
sc config WdiSystemHost start= demand > nul
sc config TrkWks start= demand > nul
sc config MSDTC start= demand > nul
sc config dmwappushservice start= demand > nul
sc config DisplayEnhancementService start= demand > nul
sc config MapsBroker start= demand > nul
sc config fdPHost start= demand > nul
sc config FDResPub start= demand > nul
sc config EFS start= demand > nul
sc config EntAppSvc start= demand > nul
sc config fhsvc start= demand > nul
sc config lfsvc start= demand > nul
sc config HomeGroupListener start= demand > nul
sc config HomeGroupProvider start= demand > nul
sc config HvHost start= demand > nul
sc config hns start= demand > nul
sc config vmickvpexchange start= demand > nul
sc config vmicguestinterface start= demand > nul
sc config vmicshutdown start= demand > nul
sc config vmicheartbeat start= demand > nul
sc config vmicvmsession start= demand > nul
sc config vmicrdv start= demand > nul
sc config vmictimesync start= demand > nul
sc config vmicvss start= demand > nul
sc config IEEtwCollectorService start= demand > nul
sc config iphlpsvc start= demand > nul 
sc config IpxlatCfgSvc start= demand > nul
sc config PolicyAgent start= demand > nul
sc config irmon start= demand > nul
sc config SharedAccess start= demand > nul
sc config lltdsvc start= demand > nul
sc config diagnosticshub.standardcollector.service start= demand > nul
sc config wlidsvc start= demand > nul
sc config AppVClient start= demand > nul
sc config smphost start= demand > nul
sc config InstallService start= demand > nul
sc config SmsRouter start= demand > nul
sc config MSiSCSI start= demand > nul
sc config NaturalAuthentication start= demand > nul
sc config CscService start= demand > nul
sc config defragsvc start= demand > nul
sc config SEMgrSvc start= demand > nul
sc config PNRPsvc start= demand > nul
sc config p2psvc start= demand > nul
sc config p2pimsvc start= demand > nul
sc config pla start= demand > nul
sc config PhoneSvc start= demand > nul
sc config WPDBusEnum start= demand > nul
sc config Spooler start= demand > nul
sc config PrintNotify start= demand > nul
sc config PcaSvc start= demand > nul
sc config WpcMonSvc start= demand > nul
sc config QWAVE start= demand > nul
sc config RasAuto start= demand > nul
sc config RasMan start= demand > nul
sc config SessionEnv start= demand > nul
sc config TermService start= demand > nul
sc config UmRdpService start= demand > nul
sc config RpcLocator start= demand > nul
sc config RemoteRegistry start= demand > nul
sc config RetailDemo start= demand > nul
sc config RemoteAccess start= demand > nul
sc config RmSvc start= demand > nul 
sc config SNMPTRAP start= demand > nul
sc config seclogon start= demand > nul
sc config wscsvc start= demand > nul
sc config SamSs start= demand > nul
sc config SensorDataService start= demand > nul
sc config SensrSvc start= demand > nul
sc config SensorService start= demand > nul
sc config LanmanServer start= demand > nul
sc config shpamsvc start= demand > nul
sc config ShellHWDetection start= demand > nul
sc config SCardSvr start= demand > nul
sc config ScDeviceEnum start= demand > nul
sc config SCPolicySvc start= demand > nul
sc config SharedRealitySvc start= demand > nul
sc config StorSvc start= demand > nul
sc config TieringEngineService start= demand > nul
sc config SysMain start= demand > nul
sc config SgrmBroker start= demand > nul
sc config lmhosts start= demand > nul
sc config TapiSrv start= demand > nul
sc config Themes start= demand > nul
sc config tiledatamodelsvc start= demand > nul
sc config TabletInputService start= demand > nul
sc config UsoSvc start= demand > nul
sc config UevAgentService start= demand > nul
sc config WalletService start= demand > nul
sc config wmiApSrv start= demand > nul
sc config TokenBroker start= demand > nul
sc config WebClient start= demand > nul
sc config WFDSConMgrSvc start= demand > nul
sc config SDRSVC start= demand > nul
sc config WbioSrvc start= demand > nul
sc config FrameServer start= demand > nul
sc config wcncsvc start= demand > nul
sc config Sense start= demand > nul
sc config WdNisSvc start= demand > nul
sc config WinDefend start= demand > nul
sc config SecurityHealthService start= demand > nul
sc config WEPHOSTSVC start= demand > nul
sc config WerSvc start= demand > nul
sc config Wecsvc start= demand > nul
sc config FontCache start= demand > nul
sc config StiSvc start= demand > nul
sc config wisvc start= demand > nul
sc config LicenseManager start= demand > nul
sc config icssvc start= demand > nul
sc config WMPNetworkSvc start= demand > nul
sc config FontCache3.0.0.0 start= demand > nul
sc config WpnService start= demand > nul
sc config perceptionsimulation start= demand > nul
sc config spectrum start= demand > nul
sc config WinRM start= demand > nul
sc config WSearch start= demand > nul
sc config SecurityHealthService start= demand > nul
sc config W32Time start= demand > nul
sc config wuauserv start= demand > nul
sc config WaaSMedicSvc start= demand > nul
sc config LanmanWorkstation start= demand > nul
sc config XboxGipSvc start= demand > nul
sc config xbgm start= demand > nul
sc config XblAuthManager start= demand > nul
sc config XblGameSave start= demand > nul
sc config XboxNetApiSvc start= demand > nul

:: Delete registry changes made in the previous script
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BluetoothUserService"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CDPUserSvc"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\CaptureService"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\ConsentUxUserSvc"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PimIndexMaintenanceSvc"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DevicePickerUserSvc"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\DevicesFlowUserSvc"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\BcastDVRUserService"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MessagingService"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\PrintWorkflowUserSvc"" /f > nul 
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\OneSyncSvc"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UserDataSvc"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UnistoreSvc"" /f > nul
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\WpnUserService"" /f > nul
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void RevertKernelTweaks()
        {
            string batchCommands = @"
next update lol
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void RevertPowerTweaks()
        {
            string batchCommands = @"Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""CoalescingTimerInterval"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""PlatformAoAcOverride"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""EnergyEstimationEnabled"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""EventProcessorEnabled"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""CsEnabled"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling"" /v ""PowerThrottlingOff"" /f

    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void RevertFSETweaks()
        {
            string batchCommands = @"Reg.exe Delete ""HKEY_LOCAL_MACHINE\System\GameConfigStore"" /f >Nul 2>&1
Reg.exe Delete ""HKEY_USERS\.Default\System\GameConfigStore"" /f >Nul 2>&1
Reg.exe Delete ""HKEY_USERS\S-1-5-19\System\GameConfigStore"" /f >Nul 2>&1
Reg.exe Delete ""HKEY_USERS\S-1-5-20\System\GameConfigStore"" /f >Nul 2>&1
Reg.exe Add ""HKEY_CURRENT_USER\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""DirectXUserGlobalSettings"" /t REG_SZ /d ""VRROptimizeEnable=0;SwapEffectUpgradeEnable=1;"" /f >Nul 2>&1
Reg.exe Add ""HKEY_CURRENT_USER\Software\Microsoft\DirectX\GraphicsSettings"" /v ""SwapEffectUpgradeCache"" /t REG_DWORD /d ""1"" /f >Nul 2>&1
Reg.exe Add ""HKEY_CURRENT_USER\System\GameConfigStore"" /v ""GameDVR_DXGIHonorFSEWindowsCompatible"" /t REG_DWORD /d ""1"" /f >Nul 2>&1
Reg.exe Delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment"" /v ""__COMPAT_LAYER"" /f >Nul 2>&1
Reg.exe Add ""HKEY_CURRENT_USER\System\GameConfigStore"" /v ""GameDVR_HonorUserFSEBehaviorMode"" /t REG_DWORD /d ""1"" /f >Nul 2>&1
Reg.exe Add ""HKEY_CURRENT_USER\System\GameConfigStore"" /v ""GameDVR_EFSEFeatureFlags"" /t REG_DWORD /d ""0"" /f >Nul 2>&1
Reg.exe Add ""HKEY_CURRENT_USER\System\GameConfigStore"" /v ""GameDVR_FSEBehaviorMode"" /t REG_DWORD /d ""2"" /f >Nul 2>&1
Reg.exe Add ""HKEY_CURRENT_USER\System\GameConfigStore"" /v ""GameDVR_FSEBehavior"" /t REG_DWORD /d ""2"" /f >Nul 2>&1
Reg.exe Add ""HKEY_CURRENT_USER\System\GameConfigStore"" /v ""GameDVR_DSEBehavior"" /t REG_DWORD /d ""2"" /f >Nul 2>&1
Reg.exe Add ""HKEY_CURRENT_USER\System\GameConfigStore"" /v ""GameDVR_Enabled"" /t REG_DWORD /d ""0"" /f >Nul 2>&1
Reg.exe Delete ""HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\Dwm"" /v ""OverlayTestMode"" /f >Nul 2>&1
    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void RevertSSDTweaks()
        {
            string batchCommands = @"
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SD\IdleState\1"" /v ""IdleExitEnergyMicroJoules"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SD\IdleState\1"" /v ""IdleExitLatencyMs"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SD\IdleState\1"" /v ""IdlePowerMw"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SD\IdleState\1"" /v ""IdleTimeLengthMs"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\1"" /v ""IdleExitEnergyMicroJoules"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\1"" /v ""IdleExitLatencyMs"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\1"" /v ""IdlePowerMw"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\1"" /v ""IdleTimeLengthMs"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\2"" /v ""IdleExitEnergyMicroJoules"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\2"" /v ""IdleExitLatencyMs"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\2"" /v ""IdlePowerMw"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\2"" /v ""IdleTimeLengthMs"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\3"" /v ""IdleExitEnergyMicroJoules"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\3"" /v ""IdleExitLatencyMs"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\3"" /v ""IdlePowerMw"" /f
Reg.exe delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\Storage\SSD\IdleState\3"" /v ""IdleTimeLengthMs"" /f

    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void Hibernation()
        {
            string batchCommands = @"
        powercfg /h off
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""HibernateEnabled"" /t REG_DWORD /d ""0"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""SleepReliabilityDetailedDiagnostics"" /t REG_DWORD /d ""0"" /f 

    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void RevertHibernation()
        {
            string batchCommands = @"
        powercfg /h off
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""HibernateEnabled"" /t REG_DWORD /d ""1"" /f 
Reg.exe add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""SleepReliabilityDetailedDiagnostics"" /t REG_DWORD /d ""1"" /f 

    ";
            ExecuteBatchCommands(batchCommands);
        }

        private void NextPage_Click(object sender, EventArgs e)
        {
            if (Main.Instance != null)
            {
                WindowsTweaksP6 myForm = new WindowsTweaksP6();
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
