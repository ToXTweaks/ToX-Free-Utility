using System.Collections.Generic;

namespace ToX_Free_Utility_NEW_ERA.Tweaks
{
    public static class WindowsTweaks
    {
        public static List<EmbeddedTweak> All => new List<EmbeddedTweak>
        {
            new EmbeddedTweak
            {
                Key = "Debloat_Windows",
                Name = "Remove Bloatwares & Unnecessary Apps",
                Tags = "SafeToUse",
                Description = "Removes bloatwares and unnecessary apps from the system.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
:: Remove OneDrive
taskkill.exe /F /IM ""explorer.exe"" >nul 2>&1
taskkill.exe /F /IM ""OneDrive.exe"" >nul 2>&1

winget uninstall --silent --accept-source-agreements Microsoft.OneDrive >nul 2>&1

reg add ""HKCR\CLSID\{{018D5C66-4533-4307-9B53-224DE2ED1FE6}}"" /v ""System.IsPinnedToNameSpaceTree"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKCR\Wow6432Node\CLSID\{{018D5C66-4533-4307-9B53-224DE2ED1FE6}}"" /v ""System.IsPinnedToNameSpaceTree"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg load ""hku\Default"" ""C:\Users\Default\NTUSER.DAT"" >nul 2>&1
reg delete ""HKEY_USERS\Default\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"" /v ""OneDriveSetup"" /f >nul 2>&1
reg unload ""hku\Default"" >nul 2>&1
del /f /q ""%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\OneDrive.lnk"" >nul 2>&1

start explorer.exe >nul 2>&1

:: Remove Cortana
takeown /f ""%windir%\SystemApps\Microsoft.Windows.Cortana_cw5n1h2txyewy"" /r /a
icacls ""%windir%\SystemApps\Microsoft.Windows.Cortana_cw5n1h2txyewy"" /grant administrators:F /t
ren ""%windir%\SystemApps\Microsoft.Windows.Cortana_cw5n1h2txyewy"" Microsoft.Windows.Cortana_cw5n1h2txyewy.old
Get-AppxPackage *CortanaUI* ^| Remove-AppxPackage; ^
Get-AppxPackage *CortanaUIFullTrustApp* ^| Remove-AppxPackage; ^

:: Remove Edge
taskkill /f /im MicrosoftEdgeUpdate.exe >nul 2>&1
taskkill /f /im msedge.exe >nul 2>&1
taskkill /f /im MicrosoftEdge*.exe >nul 2>&1

sc stop edgeupdate >nul 2>&1
sc delete edgeupdate >nul 2>&1
sc stop edgeupdatem >nul 2>&1
sc delete edgeupdatem >nul 2>&1
sc stop MicrosoftEdgeElevationService >nul 2>&1
sc delete MicrosoftEdgeElevationService >nul 2>&1

del /f /q ""%PUBLIC%\Desktop\Microsoft Edge.lnk"" >nul 2>&1
del /f /q ""%ProgramData%\Microsoft\Windows\Start Menu\Programs\Microsoft Edge.lnk"" >nul 2>&1

powershell -NoP -EP Bypass ^
 ""Get-AppxPackage -AllUsers Microsoft.MicrosoftEdge.Stable* | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue""
powershell -NoP -EP Bypass ^
 ""Get-AppxPackage -AllUsers Microsoft.MicrosoftEdgeDevToolsClient* | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue""

reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v TaskbarMigratedBrowserPin /f
reg delete ""HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge"" /f
reg delete ""HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Microsoft Edge Update"" /f
reg delete ""HKCR\MSEdgeHTM"" /f
reg delete ""HKCR\MSEdgePDF"" /f
reg delete ""HKCR\MSEdgeMHT"" /f
reg delete ""HKLM\SOFTWARE\Clients\StartMenuInternet\Microsoft Edge"" /f
reg delete ""HKLM\SOFTWARE\RegisteredApplications"" /v ""Microsoft Edge"" /f
reg delete ""HKCU\SOFTWARE\RegisteredApplications"" /v ""Microsoft Edge"" /f
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\msedge.exe"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Edge"" /f

reg add ""HKLM\SOFTWARE\Policies\Microsoft\EdgeUpdate"" /v InstallDefault /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\EdgeUpdate"" /v UpdateDefault /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\EdgeUpdate"" /v CreateDesktopShortcutDefault /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Microsoft\EdgeUpdate"" /v DoNotUpdateToEdgeWithChromium /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\OEM\MicrosoftEdge"" /v AutoLaunchOnLogonBehavior /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\EdgeUI"" /v DisableMFUTracking /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\MicrosoftEdge\Main"" /v AllowPrelaunch /t REG_DWORD /d 0 /f

takeown /f ""%ProgramFiles(x86)%\Microsoft\Edge"" /r /d y >nul 2>&1
icacls ""%ProgramFiles(x86)%\Microsoft\Edge"" /grant Administrators:F /t >nul 2>&1
rmdir /s /q ""%ProgramFiles(x86)%\Microsoft\Edge""

takeown /f ""%ProgramFiles(x86)%\Microsoft\EdgeCore"" /r /d y >nul 2>&1
icacls ""%ProgramFiles(x86)%\Microsoft\EdgeCore"" /grant Administrators:F /t >nul 2>&1
rmdir /s /q ""%ProgramFiles(x86)%\Microsoft\EdgeCore""

takeown /f ""%ProgramFiles(x86)%\Microsoft\EdgeUpdate"" /r /d y >nul 2>&1
icacls ""%ProgramFiles(x86)%\Microsoft\EdgeUpdate"" /grant Administrators:F /t >nul 2>&1
rmdir /s /q ""%ProgramFiles(x86)%\Microsoft\EdgeUpdate""

del /f /q ""%windir%\system32\MicrosoftEdgeBCHost.exe"" >nul 2>&1
del /f /q ""%windir%\system32\MicrosoftEdgeCP.exe"" >nul 2>&1
del /f /q ""%windir%\system32\MicrosoftEdgeDevTools.exe"" >nul 2>&1
del /f /q ""%windir%\system32\MicrosoftEdgeSH.exe"" >nul 2>&1

:: Kill Teams
taskkill /f /im msteams.exe >nul 2>&1
taskkill /f /im ms-teams.exe >nul 2>&1

:: Remove Bloatwares
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
""Get-AppxPackage *Clipchamp.Clipchamp* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.ApplicationCompatibilityEnhancements* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.AV1VideoExtension* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.AVCEncoderVideoExtension* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.BingNews* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.BingSearch* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.BingWeather* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.Copilot* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.Edge.GameAssist* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.Family* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.GetHelp* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.Getstarted* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.HEIFImageExtension* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.HEVCVideoExtension* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.MicrosoftEdge.Stable* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.MicrosoftOfficeHub* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.MicrosoftSolitaireCollection* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.MicrosoftStickyNotes* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.MPEG2VideoExtension* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.OneDriveSync* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.OutlookForWindows* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.Paint* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.People* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.PowerAutomateDesktop* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.RawImageExtension* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.ScreenSketch* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.Todos* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.VP9VideoExtensions* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.WebMediaExtensions* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.WebpImageExtension* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.WindowsAlarms* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.WindowsCalculator* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.WindowsCamera* ^| Remove-AppxPackage; ^
Get-AppxPackage *microsoft.windowscommunicationsapps* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.Windows.DevHome* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.WindowsFeedbackHub* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.WindowsMaps* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.WindowsNotepad* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.Windows.Photos* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.WindowsSoundRecorder* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.WindowsTerminal* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.Xbox.TCUI* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.XboxGameOverlay* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.XboxGamingOverlay* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.XboxSpeechToTextOverlay* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.YourPhone* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.ZuneMusic* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.ZuneVideo* ^| Remove-AppxPackage; ^
Get-AppxPackage *MicrosoftCorporationII.MicrosoftFamily* ^| Remove-AppxPackage; ^
Get-AppxPackage *MicrosoftCorporationII.QuickAssist* ^| Remove-AppxPackage; ^
Get-AppxPackage *MicrosoftWindows.CrossDevice* ^| Remove-AppxPackage; ^
Get-AppxPackage *MicrosoftWindows.LKG.TwinSxS* ^| Remove-AppxPackage; ^
Get-AppxPackage *Microsoft.WidgetsPlatformRuntime* ^| Remove-AppxPackage; ^
Get-AppxPackage *MSTeams* ^| Remove-AppxPackage; ^
Get-AppxPackage *MicrosoftWindows.Client.WebExperience* ^| Remove-AppxPackage""",
                RevertBatch = $@"@echo off"
            },

            new EmbeddedTweak
            {
                Key = "Applies Power Tweaks",
                Tags = "PowerHungry",
                Description = "Tunes Windows power settings for optimal performance.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off

:: Power tuning
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Processor"" /v ""DisableAsserts"" /t REG_DWORD /d 1 /f >nul
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""AllowAudioToEnableExecutionRequiredPowerRequests"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""MSDisabled"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""HibernateChecksummingEnabled"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""DisableVsyncLatencyUpdate"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""CoalescingTimerInterval"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""PerfCalculateActualUtilization"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""EnergyEstimationEnabled"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""IpiLastClockOwnerDisable"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""HibernateEnabledDefault"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""EventProcessorEnabled"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""AllowHibernate"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""PlatformRoleOverride"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""AlwaysComputeQosHints"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""SleepstudyAccountingEnabled"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""PlatformAoAcOverride"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""DisableDisplayBurstOnPowerSourceChange"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""FxAccountingTelemetryDisabled"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""AllowSystemRequiredPowerRequests"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""CoalescingFlushInterval"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""HiberbootEnabled"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""PoFxSystemIrpWaitForReportDevicePowered"" /t REG_DWORD /d 0 /f

reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power"" /v ""SleepStudyDisabled"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power"" /v ""SleepStudyDeviceAccountingLevel"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power"" /v ""WatchdogSleepTimeout"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power"" /v ""WatchdogResumeTimeout"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power"" /v ""IdleScanInterval"" /t REG_DWORD /d 0 /f

:: Power Plan
powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61 11111111-1111-1111-1111-111111111111 >NUL 
powercfg /setactive 11111111-1111-1111-1111-111111111111
powercfg -delete 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c
powercfg -delete a1841308-3541-4fab-bc81-f71556f20b4a
powercfg -changename 11111111-1111-1111-1111-111111111111 ""ToX Free"" ""Optimized for Performance - toxtweaks.com""
powercfg -setacvalueindex scheme_current sub_processor PERFINCPOL 2 >NUL 2>nul
powercfg -setacvalueindex scheme_current sub_processor PERFDECPOL 1 >NUL 2>nul
powercfg -setacvalueindex scheme_current sub_processor PERFINCTHRESHOLD 10 >NUL 2>nul
powercfg -setacvalueindex scheme_current sub_processor PERFDECTHRESHOLD 8 >NUL 2>nul
powercfg -setacvalueindex scheme_current 2a737441-1930-4402-8d77-b2bebba308a3 d4e98f31-5ffe-4ce1-be31-1b38b384c009 0 >nul
powercfg -setacvalueindex scheme_current 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0 >nul
powercfg -setacvalueindex scheme_current SUB_PCIEXPRESS ASPM 0 >nul
powercfg -setacvalueindex scheme_current sub_none DEVICEIDLE 0 >nul
powercfg -setacvalueindex scheme_current SUB_SLEEP AWAYMODE 0 >nul
powercfg -setacvalueindex scheme_current SUB_SLEEP ALLOWSTANDBY 0 >nul
powercfg -setacvalueindex scheme_current SUB_SLEEP HYBRIDSLEEP 0 >nul
powercfg -setacvalueindex scheme_current SUB_VIDEO VIDEOIDLE 0 >nul
powercfg -setacvalueindex scheme_current SUB_GRAPHICS GPUPREFERENCEPOLICY 0 >nul
powercfg -setacvalueindex scheme_current SUB_SLEEP RTCWAKE 0 >nul
powercfg -setacvalueindex scheme_current SUB_IR DEEPSLEEP 0 >nul
powercfg -setacvalueindex scheme_current SUB_PROCESSOR PROCTHROTTLEMIN 30 >nul
powercfg -setacvalueindex scheme_current SUB_PROCESSOR PROCTHROTTLEMAX 100 >nul
powercfg -setacvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 9943e905-9a30-4ec1-9b99-44dd3b76f7a2 1 >nul
powercfg -setacvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 465e1f50-b610-473a-ab58-00d1077dc419 2 >nul
powercfg -setacvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 45bcc044-d885-43e2-8605-ee0ec6e96b59 100 >nul

powercfg /setacvalueindex SCHEME_CURRENT fea3413e-7e05-4911-9a71-700331f1c294 0e796bdb-100d-47d6-a2d5-f7d2daa51f51 1
powercfg /setacvalueindex SCHEME_CURRENT fea3413e-7e05-4911-9a71-700331f1c294 245d8541-3943-4422-b025-13a784f679b7 1
powercfg /setacvalueindex SCHEME_CURRENT fea3413e-7e05-4911-9a71-700331f1c294 4faab71a-92e5-4726-b531-224559672d19 0
powercfg /setacvalueindex SCHEME_CURRENT fea3413e-7e05-4911-9a71-700331f1c294 68afb2d9-ee95-47a8-8f50-4115088073b1 0
powercfg /setacvalueindex SCHEME_CURRENT fea3413e-7e05-4911-9a71-700331f1c294 f15576e8-98b7-4186-b944-eafa664402d9 1
powercfg /setacvalueindex SCHEME_CURRENT 0012ee47-9041-4b5d-9b77-535fba8b1442 0b2d69d7-a2a1-449c-9680-f91c70521c60 0
powercfg /setacvalueindex SCHEME_CURRENT 0012ee47-9041-4b5d-9b77-535fba8b1442 51dea550-bb38-4bc4-991b-eacf37be5ec8 100
powercfg /setacvalueindex SCHEME_CURRENT 0012ee47-9041-4b5d-9b77-535fba8b1442 6738e2c4-e8a5-4a42-b16a-e040e769756e 0
powercfg /setacvalueindex SCHEME_CURRENT 0012ee47-9041-4b5d-9b77-535fba8b1442 80e3c60e-bb94-4ad8-bbe0-0d3195efc663 0
powercfg /setacvalueindex SCHEME_CURRENT 0012ee47-9041-4b5d-9b77-535fba8b1442 d3d55efd-c1ff-424e-9dc3-441be7833010 60000
powercfg /setacvalueindex SCHEME_CURRENT 0012ee47-9041-4b5d-9b77-535fba8b1442 d639518a-e56d-4345-8af2-b9f32fb26109 60000
powercfg /setacvalueindex SCHEME_CURRENT 0012ee47-9041-4b5d-9b77-535fba8b1442 dbc9e238-6de9-49e3-92cd-8c2b4946b472 0
powercfg /setacvalueindex SCHEME_CURRENT 0012ee47-9041-4b5d-9b77-535fba8b1442 fc7372b6-ab2d-43ee-8797-15e9841f2cca 1
powercfg /setacvalueindex SCHEME_CURRENT 0012ee47-9041-4b5d-9b77-535fba8b1442 fc95af4d-40e7-4b6d-835a-56d131dbc80e 0
powercfg /setacvalueindex SCHEME_CURRENT 0d7dbae2-4294-402a-ba8e-26777e8488cd 309dce9b-bef4-4119-9921-a851fb12f0f4 1
powercfg /setacvalueindex SCHEME_CURRENT 19cbb8fa-5279-450e-9fac-8a3d5fedd0c1 12bbebe6-58d6-4636-95bb-3217ef867c1a 0
powercfg /setacvalueindex SCHEME_CURRENT 238c9fa8-0aad-41ed-83f4-97be242c8f20 1a34bdc3-7e6b-442e-a9d0-64b6ef378e84 0
powercfg /setacvalueindex SCHEME_CURRENT 238c9fa8-0aad-41ed-83f4-97be242c8f20 25dfa149-5dd1-4736-b5ab-e8a37b5b8187 0
powercfg /setacvalueindex SCHEME_CURRENT 238c9fa8-0aad-41ed-83f4-97be242c8f20 29f6c1db-86da-48c5-9fdb-f2b67b1f44da 0
powercfg /setacvalueindex SCHEME_CURRENT 238c9fa8-0aad-41ed-83f4-97be242c8f20 7bc4a2f9-d8fc-4469-b07b-33eb785aaca0 0
powercfg /setacvalueindex SCHEME_CURRENT 238c9fa8-0aad-41ed-83f4-97be242c8f20 94ac6d29-73ce-41a6-809f-6363ba21b47e 0
powercfg /setacvalueindex SCHEME_CURRENT 238c9fa8-0aad-41ed-83f4-97be242c8f20 9d7815a6-7ee4-497e-8888-515a05f02364 0
powercfg /setacvalueindex SCHEME_CURRENT 238c9fa8-0aad-41ed-83f4-97be242c8f20 bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d 0
powercfg /setacvalueindex SCHEME_CURRENT 238c9fa8-0aad-41ed-83f4-97be242c8f20 d4c1d4c8-d5cc-43d3-b83e-fc51215cb04d 0
powercfg /setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 0853a681-27c8-4100-a2fd-82013e970683 0
powercfg /setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 498c044a-201b-4631-a522-5c744ed4e678 1

rem To apply changes
powercfg -setactive scheme_current >nul 2>nul",
                RevertBatch = $@"@echo off
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Processor"" /v ""DisableAsserts"" /f

reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""AllowAudioToEnableExecutionRequiredPowerRequests"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""MSDisabled"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""HibernateChecksummingEnabled"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""DisableVsyncLatencyUpdate"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""CoalescingTimerInterval"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""PerfCalculateActualUtilization"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""EnergyEstimationEnabled"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""IpiLastClockOwnerDisable"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""EventProcessorEnabled"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""PlatformRoleOverride"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""AlwaysComputeQosHints"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""SleepstudyAccountingEnabled"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""PlatformAoAcOverride"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""DisableDisplayBurstOnPowerSourceChange"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""FxAccountingTelemetryDisabled"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""AllowSystemRequiredPowerRequests"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""CoalescingFlushInterval"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""PoFxSystemIrpWaitForReportDevicePowered"" /f

reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power"" /v ""SleepStudyDisabled"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power"" /v ""SleepStudyDeviceAccountingLevel"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power"" /v ""WatchdogSleepTimeout"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power"" /f /v ""WatchdogResumeTimeout"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power"" /v ""IdleScanInterval"" /f

reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""HibernateEnabledDefault"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""AllowHibernate"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Power"" /v ""HiberbootEnabled"" /t REG_DWORD /d 1 /f

powercfg -restoredefaultschemes
powercfg /setactive 381b4222-f694-41f0-9685-ff5bb260df2e"
            },

            new EmbeddedTweak
            {
                Key = "Telemetry Tweaks",
                Name = "Disable Telemetry",
                Tags = "Finished,FeatureBreaking",
                Description = "Disables telemetry and data collection for improved system performance.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off

:: Disabling Telemetry
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection"" /v ""AllowTelemetry"" /t REG_DWORD /d ""1"" /f >nul 2>&1
reg add ""HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\DataCollection"" /v ""AllowTelemetry"" /t REG_DWORD /d 00000000 /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection"" /v ""AllowDeviceNameInTelemetry"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsConsumerFeatures"" /t REG_DWORD /d ""1"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""EnableActivityFeed"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""PublishUserActivities"" /t REG_DWORD /d ""0"" /f >nul 2>&1 
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""UploadUserActivities"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Sensor\Overrides\{{BFA794E4-F964-4FDB-90F6-51056BFE4B44}}"" /v ""SensorPermissionState"" /t REG_DWORD /d ""0"" /f >nul 2>&1 
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\lfsvc\Service\Configuration"" /v ""Status"" /t REG_DWORD /d ""0"" /f >nul 2>&1 
reg add ""HKLM\SYSTEM\Maps"" /v ""AutoUpdateEnabled"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Microsoft\Windows\Windows Error Reporting"" /v ""Disabled"" /t REG_DWORD /d ""1"" /f >nul 2>&1

:: Disabling Driver Searching
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Device Installer"" /v ""DisableCoInstallers"" /t REG_DWORD /d ""1"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching"" /v ""SearchOrderConfig"" /t REG_DWORD /d ""0"" /f >nul 2>&1

:: Disabling Network Telemetry
reg add ""HKLM\Software\Microsoft\PolicyManager\default\WiFi\AllowWiFiHotSpotReporting"" /v ""Value"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKLM\Software\Microsoft\PolicyManager\default\WiFi\AllowAutoConnectToWiFiSenseHotspots"" /v ""Value"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Remote Assistance"" /v ""fAllowToGetHelp"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config"" /v ""DODownloadMode"" /t REG_DWORD /d ""1"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization"" /v ""DODownloadMode"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\NetworkConnectivityStatusIndicator"" /v ""NoActiveProbe"" /t REG_DWORD /d ""1"" /f >nul 2>&1

:: Disabling Windows Feedback Prompts
reg add ""HKCU\SOFTWARE\Microsoft\Siuf\Rules"" /v ""NumberOfSIUFInPeriod"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg delete ""HKCU\SOFTWARE\Microsoft\Siuf\Rules"" /v ""PeriodInNanoSeconds"" /f >nul 2>&1

:: Disabling Windows Content Delivery Settings
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""ContentDeliveryAllowed"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""OemPreInstalledAppsEnabled"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""PreInstalledAppsEnabled"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""PreInstalledAppsEverEnabled"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SilentInstalledAppsEnabled"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-338387Enabled"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-338388Enabled"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-338389Enabled"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-353698Enabled"" /t REG_DWORD /d ""0"" /f >nul 2>&1 
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SystemPaneSuggestionsEnabled"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement"" /v ""ScoobeSystemSettingEnabled"" /t REG_DWORD /d ""0"" /f >nul 2>&1
reg add ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableTailoredExperiencesWithDiagnosticData"" /t REG_DWORD /d ""1"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsConsumerFeatures"" /t REG_DWORD /d ""1"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo"" /v ""DisabledByGroupPolicy"" /t REG_DWORD /d ""1"" /f >nul 2>&1

:: Disabling Windows Error Reporting
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""Disabled"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""DontShowUI"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""DontSendAdditionalData"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""LoggingDisabled"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting\Consent"" /v ""value"" /t REG_SZ /d ""0"" /f
reg add ""HKCU\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""Disabled"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""DontSendAdditionalData"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""LoggingDisabled"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\PCHealth\ErrorReporting"" /v ""DoReport"" /t REG_DWORD /d 0 /f

:: Disabling Input Telemetry
reg add ""HKCU\SOFTWARE\Microsoft\Input\Settings"" /v ""InsightsEnabled"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Input\TIPC"" /v ""Enabled"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\TextInput"" /v ""AllowLinguisticDataCollection"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Microsoft\InputPersonalization"" /v ""RestrictImplicitTextCollection"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Microsoft\InputPersonalization"" /v ""RestrictImplicitInkCollection"" /t REG_DWORD /d 1 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\CPSS\Store\InkingAndTypingPersonalization"" /v ""Value"" /t REG_DWORD /d 0 /f
reg add ""HKLM\Software\Microsoft\Personalization\Settings"" /v ""AcceptedPrivacyPolicy"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Microsoft\Personalization\Settings"" /v ""AcceptedPrivacyPolicy"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Microsoft\InputPersonalization\TrainedDataStore"" /v ""HarvestContacts"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft"" /v ""InputPersonalization"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Policies\Microsoft"" /v ""InputPersonalization"" /t REG_DWORD /d 0 /f

:: Disabling Sync
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableSettingSync /t REG_DWORD /d 2 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableSettingSyncUserOverride /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableSyncOnPaidNetwork /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableWindowsSettingSync /t REG_DWORD /d 2 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableApplicationSettingSync /t REG_DWORD /d 2 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableApplicationSettingSyncUserOverride /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableWebBrowserSettingSync /t REG_DWORD /d 2 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableWebBrowserSettingSyncUserOverride /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableDesktopThemeSettingSync /t REG_DWORD /d 2 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableDesktopThemeSettingSyncUserOverride /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableCredentialsSettingSync /t REG_DWORD /d 2 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableCredentialsSettingSyncUserOverride /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisablePersonalizationSettingSync /t REG_DWORD /d 2 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisablePersonalizationSettingSyncUserOverride /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableStartLayoutSettingSync /t REG_DWORD /d 2 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v DisableStartLayoutSettingSyncUserOverride /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization"" /v Enabled /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings"" /v Enabled /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials"" /v Enabled /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility"" /v Enabled /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows"" /v Enabled /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync"" /v SyncPolicy /t REG_DWORD /d 5 /f

:: Disable AI Data Analysis
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsAI"" /v DisableAIDataAnalysis /t REG_DWORD /d 1 /f

:: Disabling Location & Sensors
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors"" /v DisableSensors /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Search"" /v ""BackgroundAppGlobalToggle"" /t REG_DWORD /d 0 /f

for %L in (
""Cellcore""
""CimFSUnionFS-Filter""
""Circular Kernel Context Logger""
""CloudExperienceHostOOBE""
""DefenderApiLogger""
""DefenderAuditLogger""
""Diaglog""
""Diagtrack-Listener""
""HolographicDevice""
""LwtNetLog""
""Mellanox-Kernel""
""Microsoft-Windows-AssignedAccess-Trace""
""Microsoft-Windows-Rdp-Graphics-RdpIdd-Trace""
""Microsoft-Windows-Setup""
""NBSMLOGGER""
""NetCore""
""NtfsLog""
""PEAuthLog""
""RadioMgr""
""RdrLog""
""ReadyBoot""
""ReFSLog""
""SetupPlatform""
""SetupPlatformTel""
""SpoolerLogger""
""SQMLogger""
""TCPIPLOGGER""
""TileStore""
""Tpm""
""UBPM""
""WdiContextLog""
""WFP-IPsec Trace""
""WiFiDriverHVSession""
""WiFiDriverHVSessionRepro""
""WiFiDriverIHVSession""
""WiFiDriverIHVSessionRepro""
""WiFiSession""
""WMI_Traces""
) do (
    reg query ""HKLM\SYSTEM\CurrentControlSet\Control\WMI\Autologger\%~L"" >nul 2>&1 && (
        reg add ""HKLM\SYSTEM\CurrentControlSet\Control\WMI\Autologger\%~L"" /v Start /t REG_DWORD /d 0 /f >nul
    )
)

reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\EventLog\Setup"" /v Enabled /t REG_DWORD /d 0 /f >nul
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\EventLog\ProtectedEventLogging"" /v EnableProtectedEventLogging /t REG_DWORD /d 0 /f >nul

setx DOTNET_CLI_TELEMETRY_OPTOUT 1 >nul
setx DOTNET_TRY_CLI_TELEMETRY_OPTOUT 1 >nul
setx CLOUDSDK_CORE_DISABLE_PROMPTS 1 >nul
setx DOCKER_CLI_TELEMETRY_OPTOUT 1 >nul
setx npm_config_loglevel silent >nul
setx VS_TELEMETRY_OPT_OUT 1 >nul

:: Restrict Anonymous Access
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\LanManServer\Parameters"" /v ""RestrictNullSessAccess"" /t REG_DWORD /d 1 /f

:: Disable Online Speech Recognition
reg add ""HKCU\SOFTWARE\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy"" /v ""HasAccepted"" /t REG_DWORD /d 0 /f

:: Disable Online DRM
reg add ""HKLM\SOFTWARE\Policies\Microsoft\WMDRM"" /v ""DisableOnline"" /t REG_DWORD /d 1 /f

:: Disable Lock Screen Camera
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization"" /v ""NoLockScreenCamera"" /t REG_DWORD /d 1 /f

:: Disable Website Access to Language List
reg add ""HKCU\Control Panel\International\User Profile"" /v ""HttpAcceptLanguageOptOut"" /t REG_DWORD /d 1 /f

:: Prevent Device Metadata Retrieval
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Device Metadata"" /v ""PreventDeviceMetadataFromNetwork"" /t REG_DWORD /d 1 /f

:: Disallow Microsoft Account Sign-in
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"" /v ""NoConnectedUser"" /t REG_DWORD /d 1 /f

:: Disable Message Service Cloud Sync
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Messaging"" /v ""AllowMessageSync"" /t REG_DWORD /d 0 /f

:: Disable Performance Track
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\WDI\{{9c5a40da-b965-4fc3-8781-88dd50a6299d}}"" /v ""ScenarioExecutionEnabled"" /t REG_DWORD /d 0 /f

:: Disable Shared Experiences
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\CDP\SettingsPage"" /v ""BluetoothLastDisabledNearShare"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\CDP"" /v ""NearShareChannelUserAuthzPolicy"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\CDP"" /v ""CdpSessionUserAuthzPolicy"" /t REG_DWORD /d 1 /f

:: Disable Tailored Experiences
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableTailoredExperiencesWithDiagnosticData"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableTailoredExperiencesWithDiagnosticData"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy"" /v ""TailoredExperiencesWithDiagnosticDataEnabled"" /t REG_DWORD /d 0 /f

:: Disable Windows Spotlight
reg add ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsSpotlightFeatures"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsSpotlightWindowsWelcomeExperience"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsSpotlightOnActionCenter"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsSpotlightOnSettings"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableThirdPartySuggestions"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableSpotlightCollectionOnDesktop"" /t REG_DWORD /d 1 /f

:: Disable RSoP Logging
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""RSoPLogging"" /t REG_DWORD /d 0 /f

:: Remove Accessibility Tool Shortcut
reg add ""HKCU\Control Panel\Accessibility\SlateLaunch"" /v ""LaunchAT"" /t REG_DWORD /d 0 /f

:: Disable Advertising ID
reg add ""HKCU\SOFTWARE\Classes\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo"" /v ""Enabled"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo"" /v ""DisabledByGroupPolicy"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo"" /v ""DisableGraphRecentItems"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo"" /v ""Enabled"" /t REG_DWORD /d 0 /f

:: Disable Recent Apps
reg add ""HKCU\SOFTWARE\Policies\Microsoft\Windows\EdgeUI"" /v ""DisableRecentApps"" /t REG_DWORD /d 1 /f

:: Disable Content Delivery Manager
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""FeatureManagementEnabled"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContentEnabled"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-310093Enabled"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-338393Enabled"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-353694Enabled"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-353696Enabled"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-338387Enabled"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SoftLandingEnabled"" /t REG_DWORD /d 0 /f

:: Disable Account Notifications
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\SystemSettings\AccountNotifications"" /v ""EnableAccountNotifications"" /t REG_DWORD /d 0 /f

:: Disable Storage Sense
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\StorageSense"" /v ""AllowStorageSenseGlobal"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy"" /v ""04"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy"" /v ""01"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Classes\SOFTWARE\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy"" /v ""01"" /t REG_DWORD /d 0 /f

:: Disable Background Apps
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications"" /v ""GlobalUserDisabled"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Search"" /v ""BackgroundAppGlobalToggle"" /t REG_DWORD /d 0 /f

:: Disable SMB Bandwidth Throttling
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters"" /v ""DisableBandwidthThrottling"" /t REG_DWORD /d 1 /f

:: Disable News and Interests / Widgets
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Feeds"" /v ""EnableFeeds"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Dsh"" /v ""AllowNewsAndInterests"" /t REG_DWORD /d 0 /f

:: Disable Sharing Wizard
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""SharingWizardOn"" /t REG_DWORD /d 0 /f

:: Disable Tablet Mode
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ImmersiveShell"" /v ""SignInMode"" /t REG_DWORD /d 1 /f

:: Disable Windows Chat
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Chat"" /v ""ChatIcon"" /t REG_DWORD /d 3 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""TaskbarMn"" /t REG_DWORD /d 0 /f

:: Add End Task to Taskbar
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings"" /v ""TaskbarEndTask"" /t REG_DWORD /d 1 /f

:: Hide Meet Now
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer"" /v ""HideSCAMeetNow"" /t REG_DWORD /d 1 /f

:: Disable Tablet Mode

reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\ImmersiveShell"" /v ""ConvertibleSlateModePromptPreference"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\ImmersiveShell"" /v ""SignInMode"" /t REG_DWORD /d 1 /f

:: Disable Timeline

reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""EnableActivityFeed"" /t REG_DWORD /d 0 /f

:: Disable Nearby Sharing

reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\CDP"" /v ""CdpSessionUserAuthzPolicy"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\CDP"" /v ""RomeSdkChannelUserAuthzPolicy"" /t REG_DWORD /d 0 /f

:: Disable Clipboard History
reg add ""HKCU\Software\Microsoft\Clipboard"" /v ""EnableClipboardHistory"" /t REG_DWORD /d 0 /f

:: Disable Autocorrect, Spellcheck & Text Prediction
reg add ""HKCU\SOFTWARE\Microsoft\TabletTip\1.7"" /v ""EnableAutocorrection"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\TabletTip\1.7"" /v ""EnableDoubleTapSpace"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\TabletTip\1.7"" /v ""EnablePredictionSpaceInsertion"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\TabletTip\1.7"" /v ""EnableSpellchecking"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\TabletTip\1.7"" /v ""EnableTextPrediction"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Input\Settings"" /v ""InsightsEnabled"" /t REG_DWORD /d 0 /f

:: Remove Chat / Microsoft Teams button
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""TaskbarMn"" /t REG_DWORD /d 0 /f

:: Remove Copilot button
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""ShowCopilotButton"" /t REG_DWORD /d 0 /f

:: Disable Windows Copilot
reg add ""HKCU\Software\Policies\Microsoft\Windows\WindowsCopilot"" /v ""TurnOffWindowsCopilot"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot"" /v ""TurnOffWindowsCopilot"" /t REG_DWORD /d 1 /f

:: Remove Meet Now icon
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"" /v ""HideSCAMeetNow"" /t REG_DWORD /d 1 /f
reg add ""HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"" /v ""HideSCAMeetNow"" /t REG_DWORD /d 1 /f

:: Maps
reg add ""HKLM\SYSTEM\Maps"" /v ""AutoUpdateEnabled"" /t REG_DWORD /d 0 /f

:: Clear Startup Registry Keys
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"" /f >nul 2>&1
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"" /f >nul
reg delete ""HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run"" /f >nul
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\RunNotification"" /f >nul 2>&1
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\RunNotification"" /f >nul

:: REMOVE UPDATE TASKS / ACTIVE SETUP ENTRIES
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree\MicrosoftEdgeUpdateTaskMachineCore"" /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree\MicrosoftEdgeUpdateTaskMachineUA"" /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Microsoft\Active Setup\Installed Components\{{8A69D345-D564-463C-AFF1-A69D9E530F96}}"" /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Microsoft\Active Setup\Installed Components\{{9459C573-B17A-45AE-9F64-1857B5D58CEE}}"" /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree\GoogleUpdateTaskMachineCore{{47162628-41C4-4A76-A11F-33E566CE2A71}}"" /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree\GoogleUpdateTaskMachineUA{{5917433A-B42A-4304-AFA7-FC45BA4A446E}}"" /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree\OneDrive Reporting Task-S-1-5-21-2390651398-2060614119-1221944134-500"" /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects\{{1FD49718-1D00-4B19-AF5F-070AF6D5D54C}}"" /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\Browser Helper Objects\{{1FD49718-1D00-4B19-AF5F-070AF6D5D54C}}"" /f >nul 2>&1

:: Disable Magnifier Settings
reg add ""HKCU\SOFTWARE\Microsoft\ScreenMagnifier"" /v ""FollowCaret"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\ScreenMagnifier"" /v ""FollowNarrator"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\ScreenMagnifier"" /v ""FollowMouse"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\ScreenMagnifier"" /v ""FollowFocus"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\ScreenMagnifier"" /v ""UseBitmapSmoothing"" /t REG_DWORD /d 0 /f

:: Disable Narrator Settings
reg add ""HKCU\SOFTWARE\Microsoft\Narrator"" /v ""IntonationPause"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator"" /v ""ReadHints"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator"" /v ""ErrorNotificationType"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator"" /v ""EchoChars"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator"" /v ""EchoWords"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator\NarratorHome"" /v ""MinimizeType"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator\NarratorHome"" /v ""AutoStart"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator\NoRoam"" /v ""EchoToggleKeys"" /t REG_DWORD /d 0 /f",
                RevertBatch = $@"@echo off
:: Telemetry
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection"" /v ""AllowTelemetry"" /f
reg delete ""HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\DataCollection"" /v ""AllowTelemetry"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection"" /v ""AllowDeviceNameInTelemetry"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsConsumerFeatures"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""EnableActivityFeed"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""PublishUserActivities"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""UploadUserActivities"" /f

reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Sensor\Overrides\{{BFA794E4-F964-4FDB-90F6-51056BFE4B44}}"" /v ""SensorPermissionState"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\lfsvc\Service\Configuration"" /v ""Status"" /f
reg delete ""HKLM\SYSTEM\Maps"" /v ""AutoUpdateEnabled"" /f
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\Windows Error Reporting"" /v ""Disabled"" /f

:: Driver Searching
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Device Installer"" /v ""DisableCoInstallers"" /f
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching"" /v ""SearchOrderConfig"" /f

:: Network
reg delete ""HKLM\Software\Microsoft\PolicyManager\default\WiFi\AllowWiFiHotSpotReporting"" /v ""Value"" /f
reg delete ""HKLM\Software\Microsoft\PolicyManager\default\WiFi\AllowAutoConnectToWiFiSenseHotspots"" /v ""Value"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Remote Assistance"" /v ""fAllowToGetHelp"" /f
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config"" /v ""DODownloadMode"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization"" /v ""DODownloadMode"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\NetworkConnectivityStatusIndicator"" /v ""NoActiveProbe"" /f
reg delete ""HKLM\System\CurrentControlSet\Services\NlaSvc\Parameters\Internet"" /v ""EnableActiveProbing"" /f

:: Feedback
reg delete ""HKCU\SOFTWARE\Microsoft\Siuf\Rules"" /v ""NumberOfSIUFInPeriod"" /f

:: Content Delivery Manager
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""ContentDeliveryAllowed"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""OemPreInstalledAppsEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""PreInstalledAppsEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""PreInstalledAppsEverEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SilentInstalledAppsEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-338387Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-338388Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-338389Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-353698Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SystemPaneSuggestionsEnabled"" /f

reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement"" /v ""ScoobeSystemSettingEnabled"" /f
reg delete ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableTailoredExperiencesWithDiagnosticData"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo"" /v ""DisabledByGroupPolicy"" /f

:: Windows Error Reporting
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""Disabled"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""DontShowUI"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""DontSendAdditionalData"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""LoggingDisabled"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting\Consent"" /v ""value"" /f

reg delete ""HKCU\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""Disabled"" /f
reg delete ""HKCU\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""DontSendAdditionalData"" /f
reg delete ""HKCU\SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting"" /v ""LoggingDisabled"" /f

reg delete ""HKLM\SOFTWARE\Policies\Microsoft\PCHealth\ErrorReporting"" /v ""DoReport"" /f

:: Input
reg delete ""HKCU\SOFTWARE\Microsoft\Input\Settings"" /v ""InsightsEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Input\TIPC"" /v ""Enabled"" /f
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\TextInput"" /v ""AllowLinguisticDataCollection"" /f
reg delete ""HKLM\SOFTWARE\Microsoft\InputPersonalization"" /v ""RestrictImplicitTextCollection"" /f
reg delete ""HKLM\SOFTWARE\Microsoft\InputPersonalization"" /v ""RestrictImplicitInkCollection"" /f
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\CPSS\Store\InkingAndTypingPersonalization"" /v ""Value"" /f
reg delete ""HKLM\Software\Microsoft\Personalization\Settings"" /v ""AcceptedPrivacyPolicy"" /f
reg delete ""HKCU\Software\Microsoft\Personalization\Settings"" /v ""AcceptedPrivacyPolicy"" /f
reg delete ""HKCU\Software\Microsoft\InputPersonalization\TrainedDataStore"" /v ""HarvestContacts"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft"" /v ""InputPersonalization"" /f
reg delete ""HKCU\SOFTWARE\Policies\Microsoft"" /v ""InputPersonalization"" /f

:: Setting Sync
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableSettingSync"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableSettingSyncUserOverride"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableSyncOnPaidNetwork"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableWindowsSettingSync"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableApplicationSettingSync"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableApplicationSettingSyncUserOverride"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableWebBrowserSettingSync"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableWebBrowserSettingSyncUserOverride"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableDesktopThemeSettingSync"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableDesktopThemeSettingSyncUserOverride"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableCredentialsSettingSync"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableCredentialsSettingSyncUserOverride"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisablePersonalizationSettingSync"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisablePersonalizationSettingSyncUserOverride"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableStartLayoutSettingSync"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\SettingSync"" /v ""DisableStartLayoutSettingSyncUserOverride"" /f

reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Personalization"" /v ""Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\BrowserSettings"" /v ""Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Credentials"" /v ""Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Accessibility"" /v ""Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync\Groups\Windows"" /v ""Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\SettingSync"" /v ""SyncPolicy"" /f

reg delete ""HKLM\SOFTWARE\Policies\Microsoft\FindMyDevice"" /v ""AllowFindMyDevice"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\FindMyDevice"" /v ""LocationSyncEnabled"" /f""

reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Search"" /v ""BackgroundAppGlobalToggle"" /f

for %L in (
""Cellcore""
""CimFSUnionFS-Filter""
""Circular Kernel Context Logger""
""CloudExperienceHostOOBE""
""DefenderApiLogger""
""DefenderAuditLogger""
""Diaglog""
""Diagtrack-Listener""
""HolographicDevice""
""LwtNetLog""
""Mellanox-Kernel""
""Microsoft-Windows-AssignedAccess-Trace""
""Microsoft-Windows-Rdp-Graphics-RdpIdd-Trace""
""Microsoft-Windows-Setup""
""NBSMLOGGER""
""NetCore""
""NtfsLog""
""PEAuthLog""
""RadioMgr""
""RdrLog""
""ReadyBoot""
""ReFSLog""
""SetupPlatform""
""SetupPlatformTel""
""SpoolerLogger""
""SQMLogger""
""TCPIPLOGGER""
""TileStore""
""Tpm""
""UBPM""
""WdiContextLog""
""WFP-IPsec Trace""
""WiFiDriverHVSession""
""WiFiDriverHVSessionRepro""
""WiFiDriverIHVSession""
""WiFiDriverIHVSessionRepro""
""WiFiSession""
""WMI_Traces""
) do (
    reg query ""HKLM\SYSTEM\CurrentControlSet\Control\WMI\Autologger\%~L"" >nul 2>&1 && (
        reg add ""HKLM\SYSTEM\CurrentControlSet\Control\WMI\Autologger\%~L"" /v Start /t REG_DWORD /d 1 /f >nul
    )
)

reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\EventLog\Setup"" /v Enabled /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\EventLog\ProtectedEventLogging"" /v EnableProtectedEventLogging /f >nul 2>&1

reg delete ""HKCU\Environment"" /v DOTNET_CLI_TELEMETRY_OPTOUT /f >nul 2>&1
reg delete ""HKCU\Environment"" /v DOTNET_TRY_CLI_TELEMETRY_OPTOUT /f >nul 2>&1
reg delete ""HKCU\Environment"" /v CLOUDSDK_CORE_DISABLE_PROMPTS /f >nul 2>&1
reg delete ""HKCU\Environment"" /v DOCKER_CLI_TELEMETRY_OPTOUT /f >nul 2>&1
reg delete ""HKCU\Environment"" /v npm_config_loglevel /f >nul 2>&1
reg delete ""HKCU\Environment"" /v VS_TELEMETRY_OPT_OUT /f >nul 2>&1

:: Restrict Anonymous Access
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\LanManServer\Parameters"" /v ""RestrictNullSessAccess"" /f

:: Online Speech Recognition
reg delete ""HKCU\SOFTWARE\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy"" /v ""HasAccepted"" /f

:: Online DRM
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\WMDRM"" /v ""DisableOnline"" /f

:: Lock Screen Camera
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization"" /v ""NoLockScreenCamera"" /f

:: Website Language List
reg delete ""HKCU\Control Panel\International\User Profile"" /v ""HttpAcceptLanguageOptOut"" /f

:: Device Metadata
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Device Metadata"" /v ""PreventDeviceMetadataFromNetwork"" /f

:: Microsoft Account Sign-in
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"" /v ""NoConnectedUser"" /f

:: Message Service Cloud Sync
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Messaging"" /v ""AllowMessageSync"" /f

:: Performance Track
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\WDI\{{9c5a40da-b965-4fc3-8781-88dd50a6299d}}"" /v ""ScenarioExecutionEnabled"" /f

:: Windows Feedback
reg delete ""HKCU\SOFTWARE\Microsoft\Siuf\Rules"" /v ""NumberOfSIUFInPeriod"" /f
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection"" /v ""DoNotShowFeedbackNotifications"" /f

:: Shared Experiences
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\CDP\SettingsPage"" /v ""BluetoothLastDisabledNearShare"" /f
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\CDP"" /v ""NearShareChannelUserAuthzPolicy"" /f
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\CDP"" /v ""CdpSessionUserAuthzPolicy"" /f

:: Tailored Experiences
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableTailoredExperiencesWithDiagnosticData"" /f
reg delete ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableTailoredExperiencesWithDiagnosticData"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy"" /v ""TailoredExperiencesWithDiagnosticDataEnabled"" /f

:: Windows Spotlight
reg delete ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsSpotlightFeatures"" /f
reg delete ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsSpotlightWindowsWelcomeExperience"" /f
reg delete ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsSpotlightOnActionCenter"" /f
reg delete ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsSpotlightOnSettings"" /f
reg delete ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableThirdPartySuggestions"" /f
reg delete ""HKCU\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableSpotlightCollectionOnDesktop"" /f

:: AI Logging
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsAI"" /v ""DisableAIDataAnalysis"" /f

:: RSoP Logging
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""RSoPLogging"" /f

:: Accessibility Tool Shortcut
reg delete ""HKCU\Control Panel\Accessibility\SlateLaunch"" /v ""LaunchAT"" /f

:: Advertising ID
reg delete ""HKCU\SOFTWARE\Classes\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo"" /v ""Enabled"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo"" /v ""DisabledByGroupPolicy"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo"" /v ""DisableGraphRecentItems"" /f
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo"" /v ""Enabled"" /f

:: Recent Apps
reg delete ""HKCU\SOFTWARE\Policies\Microsoft\Windows\EdgeUI"" /v ""DisableRecentApps"" /f

:: Content Delivery Manager
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""ContentDeliveryAllowed"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""FeatureManagementEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContentEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""OemPreInstalledAppsEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""PreInstalledAppsEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""PreInstalledAppsEverEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SilentInstalledAppsEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-310093Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-338393Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-353694Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-353696Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SystemPaneSuggestionsEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-338387Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""RotatingLockScreenOverlayEnabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-338388Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SubscribedContent-338389Enabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v ""SoftLandingEnabled"" /f

:: Account Notifications
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\SystemSettings\AccountNotifications"" /v ""EnableAccountNotifications"" /f

:: Storage Sense
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\StorageSense"" /v ""AllowStorageSenseGlobal"" /f
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy"" /v ""04"" /f
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy"" /v ""01"" /f
reg delete ""HKCU\Software\Classes\SOFTWARE\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy"" /v ""01"" /f

:: Background Apps
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications"" /v ""GlobalUserDisabled"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Search"" /v ""BackgroundAppGlobalToggle"" /f

:: SMB Bandwidth Throttling
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters"" /v ""DisableBandwidthThrottling"" /f

:: News and Interests / Widgets
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Feeds"" /v ""EnableFeeds"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Dsh"" /v ""AllowNewsAndInterests"" /f

echo Reverting Explorer, Taskbar and CrashControl tweaks...

:: Restore Sharing Wizard
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""SharingWizardOn"" /t REG_DWORD /d 1 /f

:: Restore Tablet Mode
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\ImmersiveShell"" /v ""SignInMode"" /f

:: Restore Windows Chat
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Chat"" /v ""ChatIcon"" /f
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""TaskbarMn"" /f

:: Remove End Task option
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings"" /v ""TaskbarEndTask"" /f

:: Restore Meet Now
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer"" /v ""HideSCAMeetNow"" /f

:: Enable Tablet Mode
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\ImmersiveShell"" /v ""ConvertibleSlateModePromptPreference"" /t REG_DWORD /d 1 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\ImmersiveShell"" /v ""SignInMode"" /t REG_DWORD /d 0 /f

:: Enable Timeline
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""EnableActivityFeed"" /f

:: Enable Nearby Sharing
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\CDP"" /v ""CdpSessionUserAuthzPolicy"" /t REG_DWORD /d 1 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\CDP"" /v ""RomeSdkChannelUserAuthzPolicy"" /t REG_DWORD /d 1 /f

:: Enable Clipboard History
reg add ""HKCU\Software\Microsoft\Clipboard"" /v ""EnableClipboardHistory"" /t REG_DWORD /d 1 /f

:: Enable Autocorrect, Spellcheck & Text Prediction
reg add ""HKCU\SOFTWARE\Microsoft\TabletTip\1.7"" /v ""EnableAutocorrection"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\TabletTip\1.7"" /v ""EnableDoubleTapSpace"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\TabletTip\1.7"" /v ""EnablePredictionSpaceInsertion"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\TabletTip\1.7"" /v ""EnableSpellchecking"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\TabletTip\1.7"" /v ""EnableTextPrediction"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Input\Settings"" /v ""InsightsEnabled"" /t REG_DWORD /d 1 /f

:: Restore Chat / Microsoft Teams button
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""TaskbarMn"" /t REG_DWORD /d 1 /f

:: Restore Copilot button
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""ShowCopilotButton"" /t REG_DWORD /d 1 /f

:: Enable Windows Copilot
reg delete ""HKCU\Software\Policies\Microsoft\Windows\WindowsCopilot"" /v ""TurnOffWindowsCopilot"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot"" /v ""TurnOffWindowsCopilot"" /f

:: Restore Meet Now icon
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"" /v ""HideSCAMeetNow"" /t REG_DWORD /d 0 /f
reg add ""HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"" /v ""HideSCAMeetNow"" /t REG_DWORD /d 0 /f

:: Maps AutoUpdate
reg add ""HKLM\SYSTEM\Maps"" /v ""AutoUpdateEnabled"" /t REG_DWORD /d 1 /f

:: Restore Startup Registry Keys
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"" /f
reg add ""HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run"" /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\RunNotification"" /f

:: Enable Magnifier Settings
reg add ""HKCU\SOFTWARE\Microsoft\ScreenMagnifier"" /v ""FollowCaret"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\ScreenMagnifier"" /v ""FollowNarrator"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\ScreenMagnifier"" /v ""FollowMouse"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\ScreenMagnifier"" /v ""FollowFocus"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\ScreenMagnifier"" /v ""UseBitmapSmoothing"" /t REG_DWORD /d 1 /f

:: Enable Narrator Settings
reg add ""HKCU\SOFTWARE\Microsoft\Narrator"" /v ""IntonationPause"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator"" /v ""ReadHints"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator"" /v ""ErrorNotificationType"" /t REG_DWORD /d 4 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator"" /v ""EchoChars"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator"" /v ""EchoWords"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator\NarratorHome"" /v ""MinimizeType"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator\NarratorHome"" /v ""AutoStart"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Narrator\NoRoam"" /v ""EchoToggleKeys"" /t REG_DWORD /d 1 /f"
            },

            new EmbeddedTweak
            {
                Key = "Disable Power Saving Features",
                Name = "Disable Power Saving Features",
                Tags = "Finished,PowerHungry",
                Description = "Disables power saving features to improve performance.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
for %i in (
	""EnhancedPowerManagementEnabled""
	""AllowIdleIrpInD3""
	""DeviceSelectiveSuspended""
	""DeviceResetNotificationEnabled""
	""SelectiveSuspendEnabled""
    ""EnableSelectiveSuspend""
	""DeviceSelectiveSuspended""
	""SelectiveSuspendEnabled""
	""SelectiveSuspendOn""
    ""EnumerationRetryCount""
	""WaitWakeEnabled""
	""D3ColdSupported""
	""DisableD3Cold""
	""WdfDirectedPowerTransitionEnable""
	""EnableIdlePowerManagement""
	""IdleInWorkingState""
	""IdleTimeoutInMS""
	""MinimumIdleTimeoutInMS""
	""EnableHIPM""
	""EnableHDDParking""
	""EnableDIPM""
    ""DmaRemappingCompatible""
	""DmaRemappingCompatibleSelfhost""
) do (
    for /f ""delims="" %k in ('reg query ""HKLM\SYSTEM\CurrentControlSet\Enum"" /s /f ""%~i"" ^| findstr ""HK""') do (
        reg add ""%k"" /v ""%~i"" /t reg_dword /d 0 /f > nul 2>&1
    )
)

for %i in (
	""DisableIdlePowerManagement""
	""DisableRuntimePowerManagement""
) do (
    for /f ""delims="" %k in ('reg query ""HKLM\SYSTEM\CurrentControlSet\Enum"" /s /f ""%~i"" ^| findstr ""HK""') do (
        reg add ""%k"" /v ""%~i"" /t reg_dword /d 1 /f > nul 2>&1
    )",
                RevertBatch = $@"@echo off
for %i in (
    ""EnhancedPowerManagementEnabled""
    ""AllowIdleIrpInD3""
    ""DeviceSelectiveSuspended""
    ""DeviceResetNotificationEnabled""
    ""SelectiveSuspendEnabled""
    ""EnableSelectiveSuspend""
    ""SelectiveSuspendOn""
    ""EnumerationRetryCount""
    ""WaitWakeEnabled""
    ""D3ColdSupported""
    ""DisableD3Cold""
    ""WdfDirectedPowerTransitionEnable""
    ""EnableIdlePowerManagement""
    ""IdleInWorkingState""
    ""IdleTimeoutInMS""
    ""MinimumIdleTimeoutInMS""
    ""EnableHIPM""
    ""EnableHDDParking""
    ""EnableDIPM""
    ""DmaRemappingCompatible""
    ""DmaRemappingCompatibleSelfhost""
    ""DisableIdlePowerManagement""
    ""DisableRuntimePowerManagement""
) do (
    for /f ""delims="" %k in ('reg query ""HKLM\SYSTEM\CurrentControlSet\Enum"" /s /f ""%~i"" ^| findstr ""HK""') do (
        reg delete ""%k"" /v ""%~i"" /f >nul 2>&1
    )"
            },

            new EmbeddedTweak
            {
                Key = "General Windows Tweaks",
                Name = "Apply General Windows Tweaks",
                Tags = "Finished,SafeToUse",
                Description = "Applies a set of general Windows tweaks to enhance system performance and user experience, optimizing and improving system responsiveness.",
                RequiresAdmin = false,
                ApplyBatch = $@"@echo off
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""EnableWerUserReporting"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""SerializeTimerExpiration"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""TimerCheckFlags"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""InterruptSteeringFlags"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableExceptionChainValidation"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""EnablePerCpuClockTickScheduling"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableControlFlowGuardXfg"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableControlFlowGuardExportSuppression"" /t REG_DWORD /d 1 /f

reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\I/O System"" /v ""DisableDiskCounters"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\I/O System"" /v ""RequireDeviceAccessCheck"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\I/O System"" /v ""IoAllowLoadCrashDumpDriver"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager"" /v ""ProtectionMode"" /t REG_DWORD /d 0 /f

reg add ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v ""SystemResponsiveness"" /t REG_DWORD /d 0xa /f
reg add ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v ""NetworkThrottlingIndex"" /t REG_DWORD /d 0xa /f

reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Diagnostics\Performance"" /v ""DisableDiagnosticTracing"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\FVE"" /v ""DisableExternalDMAUnderLock"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DeviceGuard"" /v ""EnableVirtualizationBasedSecurity"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control"" /v ""WaitToKillServiceTimeout"" /t REG_SZ /d ""2000"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control"" /v ""SystemStartOptions"" /t REG_SZ /d "" NOEXECUTE=OPTIN  VSMLAUNCHTYPE=OFF  DISABLEDYNAMICTICK  FVEBOOT=2666496"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\CI\Config"" /v ""VulnerableDriverBlocklistEnable"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard"" /v ""RequirePlatformSecurityFeatures"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity"" /v ""Enabled"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\KernelShadowStacks"" /v ""Enabled"" /t REG_DWORD /d 0 /f

reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Wdf"" /v ""WdfGlobalSleepStudyDisabled"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Wdf"" /v ""WdfGlobalLogsDisabled"" /t REG_DWORD /d 1 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Dsh"" /v ""PackageRepairCompleted"" /t REG_DWORD /d 1 /f

reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Executive"" /v ""AdditionalCriticalWorkerThreads"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Executive"" /v ""AdditionalDelayedWorkerThreads"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Executive"" /v ""TickcountRolloverDelay"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Executive"" /v ""KernelWorkerTestFlags"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Executive"" /v ""ForceEnableMutantAutoboost"" /t REG_DWORD /d 1 /f

reg add ""HKLM\SOFTWARE\Microsoft\SQMClient\Windows"" /v CEIPEnable /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\SQMClient\Windows"" /v CEIPEnable /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Wow6432Node\Microsoft\VSCommon\15.0\SQM"" /v OptIn /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment"" /v POWERSHELL_TELEMETRY_OPTOUT /t REG_DWORD /d 1 /f

reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WTDS\Components"" /v ServiceEnabled /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\CI\Policy"" /v VerifiedAndReputablePolicyState /t REG_DWORD /d 0 /f

reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows"" /v TimerCoalescing /t REG_BINARY /d 0000000000000000000000000000000000000000000000000000000000000 /f
reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows"" /v RITdemonTimerPowerSaveElapse /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows"" /v RITdemonTimerPowerSaveCoalescing /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows"" /v ProcessDCEsInUseThreshold /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows"" /v DesktopHeapLogging /t REG_DWORD /d 0 /f

:: Optimize Video Quality on Battery
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\VideoSettings"" /v ""VideoQualityOnBattery"" /t REG_DWORD /d 1 /f

reg add ""HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Power\EnergyEstimation\TaggedEnergy"" /v DisableTaggedEnergyLogging /t reg_dword /d 1 /f >nul",
                RevertBatch = $@"@echo off
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""EnableWerUserReporting"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""SerializeTimerExpiration"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""TimerCheckFlags"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""InterruptSteeringFlags"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableExceptionChainValidation"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""EnablePerCpuClockTickScheduling"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableControlFlowGuardXfg"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableControlFlowGuardExportSuppression"" /f
reg add ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v ""SystemResponsiveness"" /t REG_DWORD /d 20 /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\I/O System"" /v ""DisableDiskCounters"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\I/O System"" /v ""RequireDeviceAccessCheck"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\I/O System"" /v ""IoAllowLoadCrashDumpDriver"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager"" /v ""ProtectionMode"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\FVE"" /v ""DisableExternalDMAUnderLock"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DeviceGuard"" /v ""EnableVirtualizationBasedSecurity"" /f

:: Restore WaitToKillServiceTimeout (Windows default)
reg add ""HKLM\SYSTEM\CurrentControlSet\Control"" /v ""WaitToKillServiceTimeout"" /t REG_SZ /d ""5000"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control"" /v ""SystemStartOptions"" /t REG_SZ /d ""NOEXECUTE=OPTIN"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\CI\Config"" /v ""VulnerableDriverBlocklistEnable"" /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard"" /v ""RequirePlatformSecurityFeatures"" /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity"" /v ""Enabled"" /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\KernelShadowStacks"" /v ""Enabled"" /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Wdf"" /v ""WdfGlobalSleepStudyDisabled"" /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Wdf"" /v ""WdfGlobalLogsDisabled"" /f >nul 2>&1
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Dsh"" /v ""PackageRepairCompleted"" /f

reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Executive"" /v ""AdditionalCriticalWorkerThreads"" /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Executive"" /v ""AdditionalDelayedWorkerThreads"" /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Executive"" /v ""TickcountRolloverDelay"" /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Executive"" /v ""KernelWorkerTestFlags"" /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Executive"" /v ""ForceEnableMutantAutoboost"" /f >nul 2>&1

reg delete ""HKLM\SOFTWARE\Microsoft\SQMClient\Windows"" /v CEIPEnable /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\SQMClient\Windows"" /v CEIPEnable /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Wow6432Node\Microsoft\VSCommon\15.0\SQM"" /v OptIn /f >nul 2>&1

reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment"" /v POWERSHELL_TELEMETRY_OPTOUT /f >nul 2>&1

reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WTDS\Components"" /v ServiceEnabled /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\CI\Policy"" /v VerifiedAndReputablePolicyState /f >nul 2>&1


reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows"" /v TimerCoalescing /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows"" /v RITdemonTimerPowerSaveElapse /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows"" /v RITdemonTimerPowerSaveCoalescing /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows"" /v ProcessDCEsInUseThreshold /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows"" /v DesktopHeapLogging /f >nul 2>&1

reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\VideoSettings"" /v VideoQualityOnBattery /t REG_DWORD /d 0 /f >nul 2>&1

reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Power\EnergyEstimation\TaggedEnergy"" /v DisableTaggedEnergyLogging /f >nul 2>&1"
            },

            new EmbeddedTweak
            {
                Key = "Enable HAGS",
                Name = "Enable HAGS",
                Tags = "SafeToUse,Finished",
                Description = "Enables HAGS (Hardware-Accelerated GPU Scheduling) to improve graphics performance and reduce CPU overhead.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""HwSchMode"" /t REG_DWORD /d 2 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""HwSchTreatExperimentalAsStable"" /t REG_DWORD /d 1 /f",
                RevertBatch = $@"@echo off
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""HwSchMode"" /t REG_DWORD /d 0 /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v ""HwSchTreatExperimentalAsStable"" /f"
            },

            new EmbeddedTweak
            {
                Key = "Disable Game Mode",
                Name = "Disable Game Mode",
                Tags = "SafeToUse,Finished",
                Description = "Disables Game Mode to improve system performance.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
reg add ""HKCU\Software\Microsoft\GameBar"" /v AllowAutoGameMode /t REG_DWORD /d 0 /f >nul
reg add ""HKCU\Software\Microsoft\GameBar"" /v AutoGameModeEnabled /t REG_DWORD /d 0 /f >nul",
                RevertBatch = $@"@echo off
reg add ""HKCU\Software\Microsoft\GameBar"" /v AllowAutoGameMode /t REG_DWORD /d 1 /f
reg add ""HKCU\Software\Microsoft\GameBar"" /v AutoGameModeEnabled /t REG_DWORD /d 1 /f >nul"
            },

            new EmbeddedTweak
            {
                Key = "Disable Mitigations",
                Name = "Disable Mitigations",
                Tags = "SafeToUse,Finished",
                Description = "Disables various mitigations to optimize system performance and reduce latency.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
for %%a in (
	fontdrvhost.exe
	dwm.exe
	lsass.exe
	svchost.exe
	WmiPrvSE.exe
	winlogon.exe
	csrss.exe
	audiodg.exe
	ntoskrnl.exe
	services.exe
) do (
	reg add ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\%%a"" /v ""MitigationOptions"" /t REG_BINARY /d !mitigation_mask! /f >nul 2>&1
	reg add ""HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\%%a"" /v ""MitigationAuditOptions"" /t REG_BINARY /d !mitigation_mask! /f >nul 2>&1
)
reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""MitigationOptions"" /t REG_BINARY /d !mitigation_mask! /f >nul 2>&1
reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""MitigationAuditOptions"" /t REG_BINARY /d !mitigation_mask! /f >nul 2>&1
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager"" /v ""ProtectionMode"" /t REG_DWORD /d 0 /f >nul 2>&1
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableExceptionChainValidation"" /t REG_DWORD /d 1 /f >nul 2>&1
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableControlFlowGuardXfg"" /t REG_DWORD /d 1 /f >nul 2>&1
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableControlFlowGuardExportSuppression"" /t REG_DWORD /d 1 /f >nul 2>&1
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverride"" /t REG_DWORD /d 3 /f >nul 2>&1
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverrideMask"" /t REG_DWORD /d 3 /f >nul 2>&1
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettings"" /t REG_DWORD /d 1 /f >nul 2>&1
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\SCMConfig"" /v ""EnableSvchostMitigationsPolicy"" /t REG_DWORD /d 0 /f >nul 2>&1
reg add ""HKLM\SYSTEM\ControlSet001\Control\SCMConfig"" /v ""EnableSvchostMitigationsPolicy"" /t REG_DWORD /d 0 /f >nul 2>&1",
                RevertBatch = $@"@echo off
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverride"" /f > nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverrideMask"" /f > nul 2>&1
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettings"" /t REG_DWORD /d 0 /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableExceptionChainValidation"" /f > nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableControlFlowGuardXfg"" /f > nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""DisableControlFlowGuardExportSuppression"" /f > nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""MitigationAuditOptions"" /f > nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel"" /v ""MitigationOptions"" /f > nul 2>&1
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager"" /v ""ProtectionMode"" /t REG_DWORD /d 1 /f > nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\SCMConfig"" /v ""EnableSvchostMitigationsPolicy"" /f > nul 2>&1
reg delete ""HKLM\SYSTEM\ControlSet001\Control\SCMConfig"" /v ""EnableSvchostMitigationsPolicy"" /f > nul 2>&1"
            },

            new EmbeddedTweak
            {
                Key = "Optimize BCDEdit",
                Name = "Basic BCDEdit Optimization",
                Tags = "SafeToUse,Finished",
                Description = "Optimizes system performance by adjusting BCDEdit settings for faster boot times and improved responsiveness, disabling unnecessary features.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
bcdedit /deletevalue useplatformclock >nul 2>&1
bcdedit /deletevalue useplatformtick >nul 2>&1
bcdedit /set bootmenupolicy Legacy >nul 2>&1
bcdedit /set quietboot Yes >nul 2>&1
bcdedit /set bootux disabled >nul 2>&1
bcdedit /set quietboot Yes >nul 2>&1
bcdedit /set bootux disabled >nul 2>&1
bcdedit /set disableelamdrivers Yes >nul 2>&1
bcdedit /set hypervisorlaunchtype off >nul 2>&1
bcdedit /set vm no >nul 2>&1
bcdedit /set vsmlaunchtype Off >nul 2>&1
bcdedit /timeout 5 >nul 2>&1",
                RevertBatch = $@"@echo off
bcdedit /deletevalue useplatformclock >nul 2>&1
bcdedit /deletevalue useplatformtick >nul 2>&1
bcdedit /set bootmenupolicy Standard >nul 2>&1
bcdedit /set quietboot No >nul 2>&1
bcdedit /set bootux enabled >nul 2>&1
bcdedit /deletevalue quietboot >nul 2>&1
bcdedit /deletevalue bootux >nul 2>&1
bcdedit /set disableelamdrivers No >nul 2>&1
bcdedit /set hypervisorlaunchtype Auto >nul 2>&1
bcdedit /deletevalue vm >nul 2>&1
bcdedit /set vsmlaunchtype Auto >nul 2>&1
bcdedit /timeout 30 >nul 2>&1"
            },

            new EmbeddedTweak
            {
                Key = "Memory Optimization",
                Name = "Basic Memory Optimization",
                Tags = "Finished,SafeToUse",
                Description = "Applying basic memory optimizations to enhance system performance.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""DisablePageCombining"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettings"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverride"" /t REG_DWORD /d 3 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverrideMask"" /t REG_DWORD /d 3 /f",
                RevertBatch = $@"@echo off
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""DisablePageCombining"" /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettings"" /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverride"" /f >nul 2>&1
reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management"" /v ""FeatureSettingsOverrideMask"" /f >nul 2>&1"
            },

            new EmbeddedTweak
            {
                Key = "Disable DMA Remapping",
                Name = "Disable DMA Remapping",
                Tags = "FeatureBreaking",
                Description = "This script disables DMA remapping, which can improve performance in certain scenarios.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
for /f ""delims="" %i in ('reg query ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services"" /s /f ""DmaRemappingCompatible"" ^| findstr /i ""^HKEY_""') do (
    reg delete ""%i"" /v ""DmaRemappingCompatible"" /f >nul 2>&1
)

:: NVMe and SATA DMA Remapping Disable
reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\storahci\Parameters"" /v ""DmaRemappingCompatible"" /t REG_DWORD /d 0 /f",
                RevertBatch = $@"@echo off
for /f ""delims="" %i in ('reg query ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services"" /s /f ""DmaRemappingCompatible"" ^| findstr /i ""^HKEY_""') do (
    reg delete ""%i"" /v ""DmaRemappingCompatible"" /f >nul 2>&1
)

:: NVMe and SATA DMA Remapping Enable
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\storahci\Parameters"" /v ""DmaRemappingCompatible"" /f >nul 2>&1"
            },

            new EmbeddedTweak
            {
                Key = "Disable Virtualization-Based Security",
                Name = "Disable Virtualization-Based Security",
                Tags = "FeatureBreaking,Finished",
                Description = "This script disables virtualization-based security, which can improve performance in certain scenarios.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
:: Disable services/drivers
for %S in (
	vmbushid
	HvHost
	vmickvpexchange
	storflt
	Vid
	hvservice
	vmbus
	HyperVideo
	gencounter
	vmicguestinterface
	vmicshutdown
	hyperkbd
	bttflt
	vmgid
	vmicheartbeat
	vmicvmsession
	vmicrdv
	vmictimesync
	vmicvss
	vpci
	umbus
	storqosflt
	ndisvirtualbus
) do (
    sc config ""%S"" start= disabled >nul 2>&1
)

:: Disable Hyper-V boot components
bcdedit /set hypervisorlaunchtype off
bcdedit /set vm no
bcdedit /set vsmlaunchtype Off

:: Disable Hyper-V Windows Feature
DISM.exe /Online /Disable-Feature /FeatureName:Microsoft-Hyper-V-All /NoRestart

:: Disable related PnP devices
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
""Get-PnpDevice -FriendlyName 'NDIS Virtual Network Adapter Enumerator' -ErrorAction Ignore | Disable-PnpDevice -Confirm:`$false -ErrorAction Ignore""

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
""Get-PnpDevice -FriendlyName 'UMBus Root Bus Enumerator' -ErrorAction Ignore | Disable-PnpDevice -Confirm:`$false -ErrorAction Ignore""

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
""Get-PnpDevice -FriendlyName 'Microsoft Hyper-V Virtualization Infrastructure Driver' -ErrorAction Ignore | Disable-PnpDevice -Confirm:`$false -ErrorAction Ignore""

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
""Get-PnpDevice -FriendlyName 'Microsoft Hypervisor Service' -ErrorAction Ignore | Disable-PnpDevice -Confirm:`$false -ErrorAction Ignore""",
                RevertBatch = $@"@echo off

:: Re-enabling Hyper-V...

:: Restore services/drivers to System start
for %S in (
	vmbushid
	HvHost
	vmickvpexchange
	storflt
	Vid
	hvservice
	vmbus
	HyperVideo
	gencounter
	vmicguestinterface
	vmicshutdown
	hyperkbd
	bttflt
	vmgid
	vmicheartbeat
	vmicvmsession
	vmicrdv
	vmictimesync
	vmicvss
	vpci
	umbus
	storqosflt
	ndisvirtualbus
) do (
    sc config ""%S"" start= system >nul 2>&1
)

:: Restore Hyper-V boot settings
bcdedit /set hypervisorlaunchtype auto
bcdedit /deletevalue vm >nul 2>&1
bcdedit /set vsmlaunchtype Auto

:: Re-enable Hyper-V Windows Feature
DISM.exe /Online /Enable-Feature /FeatureName:Microsoft-Hyper-V-All /All /NoRestart

:: Re-enable related PnP devices
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
""Get-PnpDevice -FriendlyName 'NDIS Virtual Network Adapter Enumerator' -ErrorAction Ignore | Enable-PnpDevice -Confirm:`$false -ErrorAction Ignore""

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
""Get-PnpDevice -FriendlyName 'UMBus Root Bus Enumerator' -ErrorAction Ignore | Enable-PnpDevice -Confirm:`$false -ErrorAction Ignore""

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
""Get-PnpDevice -FriendlyName 'Microsoft Hyper-V Virtualization Infrastructure Driver' -ErrorAction Ignore | Enable-PnpDevice -Confirm:`$false -ErrorAction Ignore""

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
""Get-PnpDevice -FriendlyName 'Microsoft Hypervisor Service' -ErrorAction Ignore | Enable-PnpDevice -Confirm:`$false -ErrorAction Ignore"""
            },

            new EmbeddedTweak
            {
                Key = "Disable FTH",
                Name = "Disable FTH",
                Tags = "FeatureBreaking,Finished",
                Description = "This script disables FTH, which can improve performance in certain scenarios.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
powershell -Command ""Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\FTH' -Name 'Enabled' -Value 0 -Type DWORD; rundll32.exe fthsvc.dll,FthSysprepSpecialize""",
                RevertBatch = $@"@echo off
powershell -Command ""Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\FTH' -Name 'Enabled' -Value 1 -Type DWORD; rundll32.exe fthsvc.dll,FthSysprepSpecialize"""
            },

            new EmbeddedTweak
            {
                Key = "Game bar and Game DVR",
                Name = "Disable Game bar and Game DVR",
                Tags = "FeatureBreaking,Finished",
                Description = "This script disables the Game bar, which can improve performance in certain scenarios.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
reg add ""HKCU\System\GameConfigStore"" /v ""GameDVR_Enabled"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR"" /v ""AppCaptureEnabled"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\GameBar"" /v ""GamePanelStartupTipIndex"" /t REG_DWORD /d 3 /f
reg add ""HKCU\SOFTWARE\Microsoft\GameBar"" /v ""ShowStartupPanel"" /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\GameBar"" /v ""UseNexusForGameBarEnabled"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Microsoft\WindowsRuntime\ActivatableClassId\Windows.Gaming.GameBar.PresenceServer.Internal.PresenceWriter"" /v ""ActivationType"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR"" /v ""AllowGameDVR"" /t REG_DWORD /d 0 /f
reg add ""HKCR\ms-gamebar"" /ve /t REG_SZ /d ""URL:ms-gamebar"" /f
reg add ""HKCR\ms-gamebar"" /v ""URL Protocol"" /t REG_SZ /d "" /f
reg add ""HKCR\ms-gamebar"" /v ""NoOpenWith"" /t REG_SZ /d "" /f
reg add ""HKCR\ms-gamebar\shell\open\command"" /ve /t REG_EXPAND_SZ /d ""%SystemRoot%%\System32\systray.exe"" /f

reg add ""HKCU\System\GameConfigStore"" /v ""GameDVR_DSEBehavior"" /t REG_DWORD /d 2 /f
reg add ""HKCU\System\GameConfigStore"" /v ""GameDVR_DXGIHonorFSEWindowsCompatible"" /t REG_DWORD /d 0 /f
reg add ""HKCU\System\GameConfigStore"" /v ""GameDVR_FSEBehavior"" /t REG_DWORD /d 2 /f
reg add ""HKCU\System\GameConfigStore"" /v ""GameDVR_FSEBehaviorMode"" /t REG_DWORD /d 1 /f",
                RevertBatch = $@"@echo off
:: Restore Game DVR defaults
reg add ""HKCU\System\GameConfigStore"" /v ""GameDVR_Enabled"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR"" /v ""AppCaptureEnabled"" /t REG_DWORD /d 1 /f

:: Remove Game Bar customizations
reg delete ""HKCU\SOFTWARE\Microsoft\GameBar"" /v ""GamePanelStartupTipIndex"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\GameBar"" /v ""ShowStartupPanel"" /f
reg delete ""HKCU\SOFTWARE\Microsoft\GameBar"" /v ""UseNexusForGameBarEnabled"" /f

:: Restore Presence Server activation
reg delete ""HKLM\SOFTWARE\Microsoft\WindowsRuntime\ActivatableClassId\Windows.Gaming.GameBar.PresenceServer.Internal.PresenceWriter"" /v ""ActivationType"" /f

:: Remove GameDVR policy
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR"" /v ""AllowGameDVR"" /f

:: Remove custom protocol registration
reg delete ""HKCR\ms-gamebar"" /f

reg delete ""HKCU\System\GameConfigStore"" /v ""GameDVR_DSEBehavior"" /f
reg delete ""HKCU\System\GameConfigStore"" /v ""GameDVR_DXGIHonorFSEWindowsCompatible"" /f
reg delete ""HKCU\System\GameConfigStore"" /v ""GameDVR_FSEBehavior"" /f
reg delete ""HKCU\System\GameConfigStore"" /v ""GameDVR_FSEBehaviorMode"" /f"
            },

            new EmbeddedTweak
            {
                Key = "Disable Scheduled Tasks",
                Name = "Disable Scheduled Tasks",
                Tags = "FeatureBreaking,Finished",
                Description = "This script disables some useless Scheduled Tasks , which can improve performance in certain scenarios.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
schtasks /change /tn ""\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Application Experience\ProgramDataUpdater"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Application Experience\StartupAppTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Customer Experience Improvement Program\Consolidator"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Customer Experience Improvement Program\KernelCeipTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Customer Experience Improvement Program\Uploader"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Autochk\Proxy"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\CloudExperienceHost\CreateObjectTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\DiskFootprint\Diagnostics"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\EDP\StorageCardEncryption Task"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\BitLocker\BitLocker Encrypt All Drives"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\BitLocker\BitLocker MDM policy Refresh"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\ApplicationData\DsSvcCleanup"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\International\Synchronize Language Settings"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Application Experience\SdbinstMergeDbTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\PI\SecureBootEncodeUEFI"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\PI\Sqm-Tasks"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Registry\RegIdleBackup"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Shell\ThemesSyncedImageDownload"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\ApplicationData\appuriverifierdaily"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\ApplicationData\appuriverifierinstall"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\AppxDeploymentClient\Pre-staged app cleanup"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\DUSM\dusmtask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Diagnosis\Scheduled"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\DiskCleanup\SilentCleanup"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\DiskFootprint\StorageSense"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Location\WindowsActionDialog"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Management\Provisioning\Logon"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\NlaSvc\WiFiTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\RetailDemo\CleanupOfflineContent"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Shell\FamilySafetyRefreshTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Shell\IndexerAutomaticMaintenance"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\SpacePort\SpaceAgentTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Speech\SpeechModelDownloadTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WCM\WiFiTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WDI\ResolutionHost"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WOF\WIM-Hash-Management"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WOF\WIM-Hash-Validation"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Windows Filtering Platform\BfeOnServiceStartTypeChange"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\MemoryDiagnostic\ProcessMemoryDiagnosticEvents"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Ras\MobilityManager"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\PushToInstall\LoginCheck"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\PushToInstall\Registration"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\UPnP\UPnPHostConfig"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WaaSMedic\PerformRemediation"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\UpdateOrchestrator\StartOobeAppsScanAfterUpdate"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\UpdateOrchestrator\StartOobeAppsScan_LicenseAccepted"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\UpdateOrchestrator\Start Oobe Expedite Work"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Diagnosis\RecommendedTroubleshootingScanner"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\OneSettings\RefreshCache"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Power Efficiency Diagnostics\AnalyzeSystem"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Network Connectivity Status Indicator\NcsiIdentifyUserProxies"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Multimedia\SystemSoundsService"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Active Directory Rights Management Services Client\AD RMS Rights Policy Template Management (Manual)"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Application Experience\MareBackup"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser Exp"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Application Experience\PcaPatchDbTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\AppListBackup\Backup"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\AppListBackup\BackupNonMaintenance"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\AppxDeploymentClient\UCPD velocity"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\CloudRestore\Backup"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\CloudRestore\Restore"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Diagnosis\UnexpectedCodepath"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticResolver"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Feedback\Siuf\DmClientOnScenarioDownload"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Feedback\Siuf\DmClient"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\FileHistory\File History (maintenance mode)"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\BootstrapUsageDataReporting"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\GovernedFeatureUsageProcessing"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\ReconcileConfigs"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\ReconcileFeatures"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\UsageDataFlushing"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\UsageDataReceiver"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\UsageDataReporting"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\InputSettingsRestoreDataAvailable"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\LocalUserSyncDataAvailable"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\MouseSyncDataAvailable"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\PenSyncDataAvailable"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\RemoteMouseSyncDataAvailable"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\RemotePenSyncDataAvailable"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\RemoteTouchpadSyncDataAvailable"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\syncpensettings"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\TouchpadSyncDataAvailable"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Maintenance\WinSAT"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Management\Provisioning\Cellular"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Maps\MapsToastTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\MemoryDiagnostic\AutomaticOfflineMemoryDiagnostic"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\PerformanceTrace\RequestTrace"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\PerformanceTrace\WhesvcToast"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\RemoteAssistance\RemoteAssistanceTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Servicing\OOBEFodSetup"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Setup\PITRTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Shell\FamilySafetyMonitor"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Shell\UpdateUserPictureTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Shell\UpdateUserPictureTaskContained"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Sustainability\PowerGridForecastTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Sustainability\SustainabilityTelemetry"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\UpdateOrchestrator\USO_UxBroker"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\UsageAndQualityInsights\UsageAndQualityInsights-MaintenanceTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WindowsAI\Recall\InitialConfiguration"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WindowsAI\Recall\PolicyConfiguration"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WindowsAI\Settings\InitialConfiguration"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WlanSvc\CDSSync"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Work Folders\Work Folders Logon Synchronization"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Work Folders\Work Folders Maintenance Work"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WwanSvc\NotificationTask"" /disable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WwanSvc\OobeDiscovery"" /disable >nul 2>&1
schtasks /change /tn ""MicrosoftEdgeUpdateTaskMachineCore"" /disable >nul 2>&1
schtasks /change /tn ""MicrosoftEdgeUpdateTaskMachineUA"" /disable >nul 2>&1",
                RevertBatch = $@"@echo off
schtasks /change /tn ""\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Application Experience\ProgramDataUpdater"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Application Experience\StartupAppTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Customer Experience Improvement Program\Consolidator"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Customer Experience Improvement Program\KernelCeipTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Customer Experience Improvement Program\UsbCeip"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Customer Experience Improvement Program\Uploader"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Autochk\Proxy"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\CloudExperienceHost\CreateObjectTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\DiskFootprint\Diagnostics"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\EDP\StorageCardEncryption Task"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\BitLocker\BitLocker Encrypt All Drives"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\BitLocker\BitLocker MDM policy Refresh"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\ApplicationData\DsSvcCleanup"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\International\Synchronize Language Settings"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Application Experience\SdbinstMergeDbTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\PI\SecureBootEncodeUEFI"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\PI\Sqm-Tasks"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Registry\RegIdleBackup"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Shell\ThemesSyncedImageDownload"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\ApplicationData\appuriverifierdaily"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\ApplicationData\appuriverifierinstall"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\AppxDeploymentClient\Pre-staged app cleanup"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\DUSM\dusmtask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Diagnosis\Scheduled"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\DiskCleanup\SilentCleanup"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\DiskFootprint\StorageSense"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Location\WindowsActionDialog"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Management\Provisioning\Logon"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\NlaSvc\WiFiTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\RetailDemo\CleanupOfflineContent"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Shell\FamilySafetyRefreshTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Shell\IndexerAutomaticMaintenance"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\SpacePort\SpaceAgentTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Speech\SpeechModelDownloadTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WCM\WiFiTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WDI\ResolutionHost"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WOF\WIM-Hash-Management"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WOF\WIM-Hash-Validation"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Windows Filtering Platform\BfeOnServiceStartTypeChange"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\MemoryDiagnostic\ProcessMemoryDiagnosticEvents"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Ras\MobilityManager"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\PushToInstall\LoginCheck"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\PushToInstall\Registration"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\UPnP\UPnPHostConfig"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WaaSMedic\PerformRemediation"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\UpdateOrchestrator\StartOobeAppsScanAfterUpdate"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\UpdateOrchestrator\StartOobeAppsScan_LicenseAccepted"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\UpdateOrchestrator\Start Oobe Expedite Work"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Diagnosis\RecommendedTroubleshootingScanner"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\OneSettings\RefreshCache"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Power Efficiency Diagnostics\AnalyzeSystem"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Network Connectivity Status Indicator\NcsiIdentifyUserProxies"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Multimedia\SystemSoundsService"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Active Directory Rights Management Services Client\AD RMS Rights Policy Template Management (Manual)"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Application Experience\MareBackup"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser Exp"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Application Experience\PcaPatchDbTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\AppListBackup\Backup"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\AppListBackup\BackupNonMaintenance"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\AppxDeploymentClient\UCPD velocity"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\CloudRestore\Backup"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\CloudRestore\Restore"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Diagnosis\UnexpectedCodepath"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticResolver"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Feedback\Siuf\DmClientOnScenarioDownload"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Feedback\Siuf\DmClient"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\FileHistory\File History (maintenance mode)"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\BootstrapUsageDataReporting"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\GovernedFeatureUsageProcessing"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\ReconcileConfigs"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\ReconcileFeatures"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\UsageDataFlushing"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\UsageDataReceiver"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Flighting\FeatureConfig\UsageDataReporting"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\InputSettingsRestoreDataAvailable"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\LocalUserSyncDataAvailable"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\MouseSyncDataAvailable"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\PenSyncDataAvailable"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\RemoteMouseSyncDataAvailable"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\RemotePenSyncDataAvailable"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\RemoteTouchpadSyncDataAvailable"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\syncpensettings"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\input\TouchpadSyncDataAvailable"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Maintenance\WinSAT"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Management\Provisioning\Cellular"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Maps\MapsToastTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\MemoryDiagnostic\AutomaticOfflineMemoryDiagnostic"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\PerformanceTrace\RequestTrace"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\PerformanceTrace\WhesvcToast"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\RemoteAssistance\RemoteAssistanceTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Servicing\OOBEFodSetup"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Setup\PITRTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Shell\FamilySafetyMonitor"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Shell\UpdateUserPictureTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Shell\UpdateUserPictureTaskContained"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Sustainability\PowerGridForecastTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Sustainability\SustainabilityTelemetry"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\UpdateOrchestrator\USO_UxBroker"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\UsageAndQualityInsights\UsageAndQualityInsights-MaintenanceTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WindowsAI\Recall\InitialConfiguration"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WindowsAI\Recall\PolicyConfiguration"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WindowsAI\Settings\InitialConfiguration"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WlanSvc\CDSSync"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Work Folders\Work Folders Logon Synchronization"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\Work Folders\Work Folders Maintenance Work"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WwanSvc\NotificationTask"" /enable >nul 2>&1
schtasks /change /tn ""\Microsoft\Windows\WwanSvc\OobeDiscovery"" /enable >nul 2>&1
schtasks /change /tn ""MicrosoftEdgeUpdateTaskMachineCore"" /enable >nul 2>&1
schtasks /change /tn ""MicrosoftEdgeUpdateTaskMachineUA"" /enable >nul 2>&1"
            },

            new EmbeddedTweak
            {
                Key = "Disable Firewall",
                Name = "Disable Windows Firewall",
                Tags = "FeatureBreaking,Finished",
                Description = "This script disables the Windows Firewall, which can improve performance, but lessens security.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\FirewallRules"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\FirewallRules"" /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile"" /v ""DisableNotifications"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile"" /v ""DoNotAllowExceptions"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile"" /v ""EnableFirewall"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile"" /v ""DisableNotifications"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile"" /v ""DoNotAllowExceptions"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile"" /v ""EnableFirewall"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile"" /v ""DisableNotifications"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile"" /v ""DoNotAllowExceptions"" /t REG_DWORD /d 1 /f",
                RevertBatch = $@"@echo off
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile"" /v ""DisableNotifications"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile"" /v ""DoNotAllowExceptions"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile"" /v ""EnableFirewall"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile"" /v ""DisableNotifications"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile"" /v ""DoNotAllowExceptions"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile"" /v ""EnableFirewall"" /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile"" /v ""DisableNotifications"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile"" /v ""DoNotAllowExceptions"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile"" /v ""EnableFirewall"" /t REG_DWORD /d 1 /f
netsh advfirewall set allprofiles state on"
            },

            new EmbeddedTweak
            {
                Key = "Disable Features",
                Name = "Disable Windows Useless Features",
                Tags = "FeatureBreaking,Finished",
                Description = "This disables unnecessary Windows features.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
:: Remove Steps Recorder
DISM.exe /Online /Remove-Capability /CapabilityName:""App.StepsRecorder~~~~0.0.1.0"" /NoRestart

:: Disable Windows Media Player
DISM.exe /Online /Disable-Feature /FeatureName:""WindowsMediaPlayer"" /NoRestart
DISM.exe /Online /Remove-Capability /CapabilityName:""Media.WindowsMediaPlayer~~~~0.0.12.0"" /NoRestart

:: Disable Media Playback
DISM.exe /Online /Disable-Feature /FeatureName:""MediaPlayback"" /NoRestart

:: Disable SMB Direct
DISM.exe /Online /Disable-Feature /FeatureName:""SMBDirect"" /NoRestart

:: Disable Remote Desktop Client Infrastructure
DISM.exe /Online /Disable-Feature /FeatureName:""MSRDC-Infrastructure"" /NoRestart

:: Disable Work Folders Client
DISM.exe /Online /Disable-Feature /FeatureName:""Workfolders-client"" /NoRestart

:: Disable WCF Services
DISM.exe /Online /Disable-Feature /FeatureName:""WCF-Services45"" /NoRestart
DISM.exe /Online /Disable-Feature /FeatureName:""WCF-TCP-PortSharing45"" /NoRestart

:: Disable Microsoft Print to PDF
DISM.exe /Online /Disable-Feature /FeatureName:""Printing-PrintToPDFServices-Features"" /NoRestart

:: Disable Printing Foundation
DISM.exe /Online /Disable-Feature /FeatureName:""Printing-Foundation-Features"" /NoRestart
DISM.exe /Online /Disable-Feature /FeatureName:""Printing-Foundation-InternetPrinting-Client"" /NoRestart

:: Disable Reserved Storage
DISM.exe /Online /Set-ReservedStorageState /State:Disabled

reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager"" /v ""0"" /t REG_DWORD /d 0 /f",
                RevertBatch = $@"@echo off
:: Reinstall Steps Recorder
DISM.exe /Online /Add-Capability /CapabilityName:""App.StepsRecorder~~~~0.0.1.0""

:: Enable Windows Media Player
DISM.exe /Online /Enable-Feature /FeatureName:""WindowsMediaPlayer"" /All /NoRestart
DISM.exe /Online /Add-Capability /CapabilityName:""Media.WindowsMediaPlayer~~~~0.0.12.0""

:: Enable Media Playback
DISM.exe /Online /Enable-Feature /FeatureName:""MediaPlayback"" /All /NoRestart

:: Enable SMB Direct
DISM.exe /Online /Enable-Feature /FeatureName:""SMBDirect"" /All /NoRestart

:: Enable Remote Desktop Client Infrastructure
DISM.exe /Online /Enable-Feature /FeatureName:""MSRDC-Infrastructure"" /All /NoRestart

:: Enable Work Folders Client
DISM.exe /Online /Enable-Feature /FeatureName:""Workfolders-client"" /All /NoRestart

:: Enable WCF Services
DISM.exe /Online /Enable-Feature /FeatureName:""WCF-Services45"" /All /NoRestart
DISM.exe /Online /Enable-Feature /FeatureName:""WCF-TCP-PortSharing45"" /All /NoRestart

:: Enable Windows Search
DISM.exe /Online /Enable-Feature /FeatureName:""SearchEngine-Client-Package"" /All /NoRestart

:: Enable Microsoft Print to PDF
DISM.exe /Online /Enable-Feature /FeatureName:""Printing-PrintToPDFServices-Features"" /All /NoRestart

:: Enable Printing Foundation
DISM.exe /Online /Enable-Feature /FeatureName:""Printing-Foundation-Features"" /All /NoRestart
DISM.exe /Online /Enable-Feature /FeatureName:""Printing-Foundation-InternetPrinting-Client"" /All /NoRestart

:: Restore Reserved Storage
DISM.exe /Online /Set-ReservedStorageState /State:Enabled

:: Remove custom ReserveManager value
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\ReserveManager"" /v ""0"" /f"
            },

            new EmbeddedTweak
            {
                Key = "Disable Useless Devices",
                Name = "Disable Useless Devices",
                Tags = "FeatureBreaking,Finished",
                Description = "This script disables useless devices, which can improve performance, but may reduce functionality.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI Processor Aggregator' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI Wake Alarm' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Microsoft Windows Management Interface for ACPI' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Direct memory access controller' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Virtual Device Enumerator' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'System speaker' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'UMBus Root Bus Enumerator' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Microsoft GS Wavetable Synth' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'AMD PSP' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Base System Device' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Composite Bus Enumerator' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'High Precision Event Timer' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Microsoft Kernel Debug Network Adapter' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Microsoft Virtual Drive Enumerator' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Microsoft RRAS Root Enumerator' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'NDIS Virtual Network Adapter Enumerator' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Numeric Data Processor' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'PCI Encryption/Decryption Controller' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'PCI Memory Controller' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'PCI standard ISA bridge' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Unknown Device' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI\\PNP0C0C\\AA' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI\\PNP0C14\\ASUSLEDCONTROLWMI' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI\\PNP0C14\\ASUSMBSWINTERFACE' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI\\PNP0C14\\AOD' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI\\PNP0C14\\AWW' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'SWD\\RADIO{{3DB5895D-CC28-44B3-AD3D-6F01A782B8D2}}' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ROOT\\SPACEPORT\\0000' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'NVIDIA Virtual Audio Device (Wave Extensible) (WDM)' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'NvModuleTracker Device' -ErrorAction Ignore | Disable-PnpDevice -Confirm:\$False -ErrorAction Ignore""",
                RevertBatch = $@"@echo off
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI Processor Aggregator' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI Wake Alarm' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Microsoft Windows Management Interface for ACPI' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Direct memory access controller' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Virtual Device Enumerator' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'System speaker' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'UMBus Root Bus Enumerator' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Microsoft GS Wavetable Synth' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'AMD PSP' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Base System Device' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Composite Bus Enumerator' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'High Precision Event Timer' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Microsoft Kernel Debug Network Adapter' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Microsoft Virtual Drive Enumerator' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Microsoft RRAS Root Enumerator' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'NDIS Virtual Network Adapter Enumerator' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Numeric Data Processor' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'PCI Encryption/Decryption Controller' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'PCI Memory Controller' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'PCI standard ISA bridge' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'Unknown Device' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI\\PNP0C0C\\AA' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI\\PNP0C14\\ASUSLEDCONTROLWMI' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI\\PNP0C14\\ASUSMBSWINTERFACE' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI\\PNP0C14\\AOD' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ACPI\\PNP0C14\\AWW' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'SWD\\RADIO{{3DB5895D-CC28-44B3-AD3D-6F01A782B8D2}}' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'ROOT\\SPACEPORT\\0000' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'NVIDIA Virtual Audio Device (Wave Extensible) (WDM)' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore""
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-PnpDevice -FriendlyName 'NvModuleTracker Device' -ErrorAction Ignore | Enable-PnpDevice -Confirm:\$False -ErrorAction Ignore"""
            },

            new EmbeddedTweak
            {
                Key = "Optimize Sound",
                Name = "Optimize Sound Settings",
                Tags = "FeatureBreaking,Finished",
                Description = "This script optimizes sound settings for better performance.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
set aud_key=HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Audio
set maud_key=HKCU\Software\Microsoft\Multimedia\Audio

::disable BCMStartupLatency
reg add ""%aud_key%"" /v ""DisableExemptionForBCMStartupLatency"" /t reg_dword /d 1 /f >nul

::disable audio capture monitoring
reg add ""%aud_key%"" /v ""EnableCaptureMonitor"" /t reg_dword /d 0 /f >nul

::disable audio notification policy
reg add ""%aud_key%"" /v ""DisableToastPolicy"" /t reg_dword /d 1 /f >nul

::acoustic echo cancellation history
reg add ""%aud_key%"" /v ""DisableAecStateHistory"" /t reg_dword /d 1 /f >nul

::disable pump backup timer
reg add ""%aud_key%"" /v ""DisablePumpBackupTimer"" /t reg_dword /d 1 /f >nul

::disable ducking preference
reg add ""%maud_key%"" /v ""UserDuckingPreference"" /t reg_dword /d 3 /f >nul
reg add ""%maud_key%"" /v ""ScreenReaderDuckingPreference"" /t reg_dword /d 0 /f >nul

::disable audio mono
reg add ""%maud_key%"" /v ""AccessibilityMonoMixState"" /t reg_dword /d 0 /f >nul",
                RevertBatch = $@"@echo off
set aud_key=HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Audio
set maud_key=HKCU\Software\Microsoft\Multimedia\Audio

:: Restore BCM Startup Latency
reg delete ""%aud_key%"" /v ""DisableExemptionForBCMStartupLatency"" /f >nul 2>&1

:: Restore Audio Capture Monitor
reg delete ""%aud_key%"" /v ""EnableCaptureMonitor"" /f >nul 2>&1

:: Restore Toast Policy
reg delete ""%aud_key%"" /v ""DisableToastPolicy"" /f >nul 2>&1

:: Restore AEC State History
reg delete ""%aud_key%"" /v ""DisableAecStateHistory"" /f >nul 2>&1

:: Restore Pump Backup Timer
reg delete ""%aud_key%"" /v ""DisablePumpBackupTimer"" /f >nul 2>&1

:: Restore Ducking Preferences
reg delete ""%maud_key%"" /v ""UserDuckingPreference"" /f >nul 2>&1
reg delete ""%maud_key%"" /v ""ScreenReaderDuckingPreference"" /f >nul 2>&1

:: Restore Mono Audio
reg delete ""%maud_key%"" /v ""AccessibilityMonoMixState"" /f >nul 2>&1"
            },

            new EmbeddedTweak
            {
                Key = "Cleaner System and Application Caches",
                Name = "Cleaning System and Application Caches",
                Tags = "FeatureBreaking,Finished",
                Description = "This script cleans up unnecessary files & app's caches. It doesn't need to be reverted.",
                RequiresAdmin = true,
                ApplyBatch = @"@echo off
setlocal EnableDelayedExpansion

echo =========================================
echo Cleaning System and Application Caches
echo =========================================

echo [1/9] Cleaning TEMP Dirs...
call :EmptyDir ""C:\Windows\Temp""
call :EmptyDir ""%TEMP%""
call :EmptyDir ""%LOCALAPPDATA%\Temp""

echo [2/9] Cleaning Recycle Bin...
powershell -NoProfile -Command ""Clear-RecycleBin -Force -ErrorAction SilentlyContinue"" >nul 2>&1

echo [3/9] Cleaning Windows Update...
net stop wuauserv >nul 2>&1
call :EmptyDir ""C:\Windows\SoftwareDistribution\Download""
net start wuauserv >nul 2>&1

echo [4/9] Cleaning Windows Dumps...
del /q /f ""C:\Windows\MEMORY.DMP"" >nul 2>&1
call :EmptyDir ""C:\Windows\Minidump""

echo [5/9] Cleaning Windows Extra...
call :EmptyDir ""%LOCALAPPDATA%\CrashDumps""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Windows\INetCache""
for /d %%d in (""%LOCALAPPDATA%\cache\qtshadercache*"") do call :EmptyDir ""%%d""
call :EmptyDir ""%LOCALAPPDATA%\CrashReportClient\Saved\Logs""
del /q /f /s ""%LOCALAPPDATA%\CEF\User Data\CrashpadMetrics*.pma"" >nul 2>&1
call :EmptyDir ""%LOCALAPPDATA%\HD-Player\cache\qmlcache""
for /d %%d in (""%LOCALAPPDATA%\HD-Player\cache\qtpipelinecache-*"") do call :EmptyDir ""%%d""
del /q /f ""C:\Windows\debug\WIA\wiatrace.txt"" >nul 2>&1
call :EmptyDir ""C:\Windows\Logs\CBS""
call :EmptyDir ""C:\Windows\Logs\DISM""
call :EmptyDir ""C:\Windows\Logs\WindowsUpdate""
call :EmptyDir ""C:\Windows\Logs\waasmedic""
call :EmptyDir ""C:\Windows\Logs\SIH""
del /q /f ""C:\Windows\Logs\WinREAgent\setup*.txt"" >nul 2>&1
call :EmptyDir ""C:\Windows\Logs\MeasuredBoot""
del /q /f ""C:\Windows\Logs\DirectX.txt"" >nul 2>&1
del /q /f ""C:\Windows\Logs\StorGroupPolicy.txt"" >nul 2>&1
call :EmptyDir ""C:\Windows\Logs\NetSetup""
call :EmptyDir ""C:\Windows\Logs\MoSetup""
del /q /f ""C:\Windows\SystemTemp\*.txt"" >nul 2>&1
del /q /f ""C:\Windows\SystemTemp\*.tmp"" >nul 2>&1

echo [6/9] Cleaning Web Browsers Cache...
taskkill /F /IM chrome.exe /T >nul 2>&1
taskkill /F /IM msedge.exe /T >nul 2>&1
taskkill /F /IM brave.exe /T >nul 2>&1

REM 1.1 Google Chrome Root Folder
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\Crashpad""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\component_crx_cache""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\GraphiteDawnCache""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\GrShaderCache""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\optimization_guide_model_store""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\screen_ai""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\ShaderCache""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\WasmTtsEngine""

REM 1.2 Google Chrome Default Profile Cache
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\Default\Cache""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\Default\Code Cache""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\Default\DawnGraphiteCache""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\Default\DawnWebGPUCache""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\Default\GPUCache""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\Default\image_cache""

REM 1.3 Google Chrome Other Profiles Cache
for /d %%p in (""%LOCALAPPDATA%\Google\Chrome\User Data\Profile*"") do (
    call :EmptyDir ""%%p\Cache""
    call :EmptyDir ""%%p\Code Cache""
    call :EmptyDir ""%%p\DawnGraphiteCache""
    call :EmptyDir ""%%p\DawnWebGPUCache""
    call :EmptyDir ""%%p\GPUCache""
    call :EmptyDir ""%%p\image_cache""
)

REM 2.1 Microsoft Edge Root Folder
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\component_crx_cache""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\Crashpad""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\GPUPersistentCache""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\GraphiteDawnCache""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\GrShaderCache""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\ProvenanceData""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\ProvenanceDataAllowList""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\ProvenanceDataTensors""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\ShaderCache""

REM 2.2 Microsoft Edge Default Profile Cache
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Cache""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Code Cache""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\DawnGraphiteCache""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\DawnWebGPUCache""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\GPUCache""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\image_cache""

REM 3.1 Brave Root Folder
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\component_crx_cache""
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Crashpad""
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\GPUPersistentCache""
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\GrShaderCache""
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\ShaderCache""

REM 3.2 Brave Default Profile Cache
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\Cache""
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\Code Cache""
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\DawnGraphiteCache""
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\DawnWebGPUCache""
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\GPUCache""
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\JumpListIconsRecentClosed""

echo [7/9] Cleaning Steam Cache...
taskkill /F /IM steam.exe /T >nul 2>&1
taskkill /F /IM steamwebhelper.exe /T >nul 2>&1
taskkill /F /IM GameOverlayUI.exe /T >nul 2>&1

call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\GraphiteDawnCache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\GrShaderCache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\ShaderCache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\blob_storage""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\Cache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\Code Cache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\DawnGraphiteCache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\DawnWebGPUCache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\GPUCache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\GraphiteDawnCache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\GrShaderCache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\ShaderCache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\VideoDecodeStats""

del /q /f ""%LOCALAPPDATA%\Steam\htmlcache\Default\History.*"" >nul 2>&1
del /q /f ""%LOCALAPPDATA%\Steam\htmlcache\Default\InterestGroups.*"" >nul 2>&1
del /q /f ""%LOCALAPPDATA%\Steam\htmlcache\Default\Visited Links.*"" >nul 2>&1
del /q /f ""%LOCALAPPDATA%\Steam\htmlcache\Default\Web Data.*"" >nul 2>&1

echo [8/9] Cleaning EA Cache...
taskkill /F /IM EADesktop.exe /T >nul 2>&1
taskkill /F /IM EABackgroundService.exe /T >nul 2>&1
taskkill /F /IM EACefSubProcess.exe /T >nul 2>&1
taskkill /F /IM EALocalHostSvc.exe /T >nul 2>&1

call :EmptyDir ""%LOCALAPPDATA%\EALaunchHelper\cache\qmlcache""
call :EmptyDir ""%LOCALAPPDATA%\EADesktop\cache\qmlcache""
call :EmptyDir ""%LOCALAPPDATA%\Electronic Arts\EA Desktop\CEF""
call :EmptyDir ""%LOCALAPPDATA%\Electronic Arts\EA Desktop\Logs""
call :EmptyDir ""%LOCALAPPDATA%\Electronic Arts\EA Desktop\IGOCache""
call :EmptyDir ""%LOCALAPPDATA%\Electronic Arts\EA Desktop\UIC""
del /q /f ""%LOCALAPPDATA%\Electronic Arts\EA Desktop\telemetry.ini"" >nul 2>&1

echo [9/9] Cleaning Epic Games ^& Discord Caches...
taskkill /F /IM EpicGamesLauncher.exe /T >nul 2>&1
taskkill /F /IM EpicOnlineServicesUserHelper.exe /T >nul 2>&1
taskkill /F /IM EpicWebHelper.exe /T >nul 2>&1
taskkill /F /IM Discord.exe /T >nul 2>&1
taskkill /F /IM DiscordSystemHelper.exe /T >nul 2>&1

REM Epic Games
call :EmptyDir ""%LOCALAPPDATA%\Epic Games\Epic Online Services\Bootstrapper\Logs""
call :EmptyDir ""%LOCALAPPDATA%\Epic Games\Epic Online Services\EpicOnlineServicesHost\Logs""
call :EmptyDir ""%LOCALAPPDATA%\Epic Games\Epic Online Services\InstallHelper\Logs""
call :EmptyDir ""%LOCALAPPDATA%\Epic Games\Epic Online Services\InstallHelper\CrashLogs\Saved\Logs""
call :EmptyDir ""%LOCALAPPDATA%\Epic Games\Epic Online Services\MainService\Logs""
call :EmptyDir ""%LOCALAPPDATA%\Epic Games\Epic Online Services\MainService\Crashes""
call :EmptyDir ""%LOCALAPPDATA%\EpicGamesLauncher\Saved\Logs""

for /d %%p in (""%LOCALAPPDATA%\EpicGamesLauncher\Saved\webcache_*"") do (
    call :EmptyDir ""%%p\Cache""
    call :EmptyDir ""%%p\Code Cache""
    call :EmptyDir ""%%p\blob_storage""
    call :EmptyDir ""%%p\VideoDecodeStats""
    call :EmptyDir ""%%p\shared_proto_db""
)

REM Discord
for /d %%p in (""%LOCALAPPDATA%\Discord\packages\Discord-*"") do call :EmptyDir ""%%p""
call :EmptyDir ""%APPDATA%\discord\Cache\Cache_Data""
call :EmptyDir ""%APPDATA%\discord\component_crx_cache""
call :EmptyDir ""%APPDATA%\discord\blob_storage""
call :EmptyDir ""%APPDATA%\discord\Code Cache""
call :EmptyDir ""%APPDATA%\discord\Crashpad\attachments""
call :EmptyDir ""%APPDATA%\discord\Crashpad\reports""
call :EmptyDir ""%APPDATA%\discord\DawnGraphiteCache""
call :EmptyDir ""%APPDATA%\discord\DawnWebGPUCache""
call :EmptyDir ""%APPDATA%\discord\GPUCache""
call :EmptyDir ""%APPDATA%\discord\logs""

echo =========================================
echo Cleanup Complete.

endlocal
exit

REM --------------------------------------------------------
REM Subroutine: Empties a directory without deleting the root folder itself.
REM --------------------------------------------------------
:EmptyDir
if exist ""%~1"" (
    del /q /f /s ""%~1\*.*"" >nul 2>&1
    for /d %%x in (""%~1\*"") do rd /s /q ""%%x"" >nul 2>&1
)",
                RevertBatch = $@"@echo off"
            },

            new EmbeddedTweak
            {
                Key = "Privacy Optimization",
                Name = "Optimizing Privacy",
                Tags = "FeatureBreaking,Finished",
                Description = "This script heavily hardens Windows and major web browsers.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
setlocal EnableDelayedExpansion
REM Ensure admin privileges (requires running as Administrator)
echo =========================================
echo Privacy Optimization
echo =========================================

:: ===================================================
:: Edge specific privacy
:: ===================================================

reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /f >nul 2>&1

:: Disable Edge Background Apps and Extensions
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""BackgroundModeEnabled"" /t REG_DWORD /d 0 /f

:: Disable Edge Startup Boost
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""StartupBoostEnabled"" /t REG_DWORD /d 0 /f

:: Disable the First Run Experience & Splash Screens
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""HideFirstRunExperience"" /t REG_DWORD /d 1 /f

:: Prevent Automatic Desktop Shortcuts
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""CreateDesktopShortcutDefault"" /t REG_DWORD /d 0 /f

:: ===================================================
:: User data tracking privacy
:: ===================================================

reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection"" /f >nul 2>&1
reg add ""HKCU\Software\Policies\Microsoft\Windows\DataCollection"" /f >nul 2>&1
reg add ""HKCU\Software\Microsoft\InputPersonalization"" /f >nul 2>&1
reg add ""HKCU\Software\Microsoft\InputPersonalization\TrainedDataStore"" /f >nul 2>&1
reg add ""HKCU\Software\Policies\Microsoft\Windows\CloudContent"" /f >nul 2>&1

:: Prevents apps from using a unique hardware ID to track you and serve targeted ads
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo"" /v ""Enabled"" /t REG_DWORD /d 0 /f

:: Disable activity history
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""PublishUserActivities"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""EnableActivityFeed"" /t REG_DWORD /d 0 /f

:: Disable telemetry
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection"" /v ""AllowTelemetry"" /t REG_DWORD /d 0 /f

:: Disable feedback notifications
reg add ""HKCU\Software\Policies\Microsoft\Windows\DataCollection"" /v ""DoNotShowFeedbackNotifications"" /t REG_DWORD /d 1 /f

:: Disable typing and handwriting data collection
reg add ""HKCU\Software\Microsoft\InputPersonalization"" /v ""RestrictImplicitTextCollection"" /t REG_DWORD /d 1 /f
reg add ""HKCU\Software\Microsoft\InputPersonalization"" /v ""RestrictImplicitInkCollection"" /t REG_DWORD /d 1 /f
reg add ""HKCU\Software\Microsoft\InputPersonalization\TrainedDataStore"" /v ""HarvestContacts"" /t REG_DWORD /d 0 /f

:: Disable personalized tips and ads
reg add ""HKCU\Software\Policies\Microsoft\Windows\CloudContent"" /v ""DisableTailoredExperiencesWithDiagnosticData"" /t REG_DWORD /d 1 /f

:: Disable Powershell 7+ tracking
setx POWERSHELL_TELEMETRY_OPTOUT ""1"" >nul 2>&1

:: ===================================================
:: Browsers privacy
:: ===================================================

taskkill /IM chrome.exe /F >nul 2>&1
taskkill /IM msedge.exe /F >nul 2>&1
taskkill /IM brave.exe /F >nul 2>&1

reg add ""HKLM\SOFTWARE\Policies\Google\Chrome"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /f >nul 2>&1

:: Disable Google Chrome services & tasks
for /f ""tokens=1"" %S in ('sc query state^= all ^| findstr /I ""GoogleUpdaterService""') do net stop ""%S"" >nul 2>&1
for /f ""tokens=1"" %S in ('sc query state^= all ^| findstr /I ""GoogleUpdaterInternalService""') do net stop ""%S"" >nul 2>&1
net stop ""GoogleChromeElevationService"" >nul 2>&1
for /f ""tokens=1"" %S in ('sc query state^= all ^| findstr /I ""GoogleUpdaterService""') do sc config ""%S"" start= disabled >nul 2>&1
for /f ""tokens=1"" %S in ('sc query state^= all ^| findstr /I ""GoogleUpdaterInternalService""') do sc config ""%S"" start= disabled >nul 2>&1
sc config ""GoogleChromeElevationService"" start= disabled >nul 2>&1
for /f ""tokens=*"" %T in ('schtasks /query /fo csv ^| findstr /I ""GoogleUpdaterTaskSystem""') do (
    for /f ""tokens=1 delims=,"" %N in (""%T"") do schtasks /change /tn %~N /disable >nul 2>&1
)

:: Disable Edge services & tasks
net stop ""edgeupdate"" >nul 2>&1
net stop ""edgeupdatem"" >nul 2>&1
net stop ""MicrosoftEdgeElevationService"" >nul 2>&1
sc config ""edgeupdate"" start= disabled >nul 2>&1
sc config ""edgeupdatem"" start= disabled >nul 2>&1
sc config ""MicrosoftEdgeElevationService"" start= disabled >nul 2>&1
for /f ""tokens=*"" %T in ('schtasks /query /fo csv ^| findstr /I ""MicrosoftEdgeUpdateTaskMachine""') do (
    for /f ""tokens=1 delims=,"" %N in (""%T"") do schtasks /change /tn %~N /disable >nul 2>&1
)

:: Disable Brave services & tasks
net stop ""brave"" >nul 2>&1
net stop ""bravem"" >nul 2>&1
net stop ""BraveElevationService"" >nul 2>&1
sc config ""brave"" start= disabled >nul 2>&1
sc config ""bravem"" start= disabled >nul 2>&1
sc config ""BraveElevationService"" start= disabled >nul 2>&1
for /f ""tokens=*"" %T in ('schtasks /query /fo csv ^| findstr /I ""BraveSoftwareUpdateTaskMachine""') do (
    for /f ""tokens=1 delims=,"" %N in (""%T"") do schtasks /change /tn %~N /disable >nul 2>&1
)

:: Disable access to camera Google Chrome
reg add ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultVideoCaptureSetting"" /t REG_DWORD /d 2 /f

:: Disable access to microphone Google Chrome
reg add ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultAudioCaptureSetting"" /t REG_DWORD /d 2 /f

:: Disable access to clipboard Google Chrome
reg add ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultClipboardSetting"" /t REG_DWORD /d 2 /f

:: Disable access to location Google Chrome
reg add ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultGeolocationSetting"" /t REG_DWORD /d 2 /f

:: Disable access to notifications Google Chrome
reg add ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultNotificationsSetting"" /t REG_DWORD /d 2 /f

:: Disable access to background sync Google Chrome
reg add ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultBackgroundSyncSetting"" /t REG_DWORD /d 2 /f

:: Disable access to idle detection api Google Chrome
reg add ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultIdleDetectionSetting"" /t REG_DWORD /d 2 /f

:: Disable access to usb api Google Chrome
reg add ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultWebUsbGuardSetting"" /t REG_DWORD /d 2 /f

:: Disable access to camera Edge
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultVideoCaptureSetting"" /t REG_DWORD /d 2 /f

:: Disable access to microphone Edge
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultAudioCaptureSetting"" /t REG_DWORD /d 2 /f

:: Disable access to clipboard Edge
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultClipboardSetting"" /t REG_DWORD /d 2 /f

:: Disable access to location Edge
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultGeolocationSetting"" /t REG_DWORD /d 2 /f

:: Disable access to notifications Edge
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultNotificationsSetting"" /t REG_DWORD /d 2 /f

:: Disable access to background sync Edge
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultBackgroundSyncSetting"" /t REG_DWORD /d 2 /f

:: Disable access to idle detection api Edge
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultIdleDetectionSetting"" /t REG_DWORD /d 2 /f

:: Disable access to usb api Edge
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultWebUsbGuardSetting"" /t REG_DWORD /d 2 /f

:: Disable access to camera Brave
reg add ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultVideoCaptureSetting"" /t REG_DWORD /d 2 /f

:: Disable access to microphone Brave
reg add ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultAudioCaptureSetting"" /t REG_DWORD /d 2 /f

:: Disable access to clipboard Brave
reg add ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultClipboardSetting"" /t REG_DWORD /d 2 /f

:: Disable access to location Brave
reg add ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultGeolocationSetting"" /t REG_DWORD /d 2 /f

:: Disable access to notifications Brave
reg add ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultNotificationsSetting"" /t REG_DWORD /d 2 /f

:: Disable access to background sync Brave
reg add ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultBackgroundSyncSetting"" /t REG_DWORD /d 2 /f

:: Disable access to idle detection api Brave
reg add ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultIdleDetectionSetting"" /t REG_DWORD /d 2 /f

:: Disable access to usb api Brave
reg add ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultWebUsbGuardSetting"" /t REG_DWORD /d 2 /f

:: ===================================================
:: Windows bloatwares privacy
:: ===================================================

reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy"" /f >nul 2>&1

:: Disable Windows Consumer Features
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsConsumerFeatures"" /t REG_DWORD /d 1 /f

:: Disable UWP apps to run quietly in the background
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy"" /v ""LetAppsRunInBackground"" /t REG_DWORD /d 2 /f

:: ===================================================
:: Notifications privacy
:: ===================================================

reg add ""HKCU\Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"" /f >nul 2>&1
reg add ""HKCU\Software\Policies\Microsoft\Windows\Explorer"" /f >nul 2>&1
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\PushNotifications"" /f >nul 2>&1
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings"" /f >nul 2>&1

:: Disable Toast Notifications and Application Notifications via Policy
reg add ""HKCU\Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"" /v ""NoToastNotification"" /t REG_DWORD /d 1 /f
reg add ""HKCU\Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"" /v ""NoToastApplicationNotification"" /t REG_DWORD /d 1 /f

:: Disable the Action Center / Notification Center panel
reg add ""HKCU\Software\Policies\Microsoft\Windows\Explorer"" /v ""DisableNotificationCenter"" /t REG_DWORD /d 1 /f

:: Disable standard Toast popups
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\PushNotifications"" /v ""ToastEnabled"" /t REG_DWORD /d 0 /f

:: Disable global toasts and forces ""Do Not Disturb"" mode ON
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings"" /v ""NOC_GLOBAL_SETTING_TOASTS_ENABLED"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings"" /v ""NOC_GLOBAL_SETTING_DND"" /t REG_DWORD /d 1 /f

:: Restart the WPN to apply instantly
net stop WpnService >nul 2>&1
net start WpnService >nul 2>&1
for /f ""tokens=1"" %S in ('sc query state^= all ^| findstr /I ""WpnUserService""') do (
    net stop ""%S"" >nul 2>&1
    net start ""%S"" >nul 2>&1
)

:: ===================================================
:: Start-up search privacy
:: ===================================================

reg add ""HKCU\Software\Policies\Microsoft\Windows\Explorer"" /f >nul 2>&1
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Search"" /f >nul 2>&1

:: Disabling Bing web search
reg add ""HKCU\Software\Policies\Microsoft\Windows\Explorer"" /v ""DisableSearchBoxSuggestions"" /t REG_DWORD /d 1 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Search"" /v ""BingSearchEnabled"" /t REG_DWORD /d 0 /f

:: ===================================================
:: UWP / Store apps app privacy
:: ===================================================

for %K in (location userAccountInformation graphicsCaptureProgrammatic radios activity camera contacts appointments chat phoneCall downloadsFolder broadFileSystemAccess picturesLibrary videosLibrary musicLibrary documentsLibrary savedPictures savedVideos) do (
    reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\%K"" /f >nul 2>&1
    reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\%K"" /v ""Value"" /t REG_SZ /d ""Deny"" /f
)

:: ===================================================
:: Classic desktop Win32 apps privacy
:: ===================================================

for %K in (location userAccountInformation graphicsCaptureProgrammatic radios activity camera contacts appointments chat phoneCall downloadsFolder broadFileSystemAccess picturesLibrary videosLibrary musicLibrary documentsLibrary savedPictures savedVideos) do (
    reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\%K\NonPackaged"" /f >nul 2>&1
    reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\%K\NonPackaged"" /v ""Value"" /t REG_SZ /d ""Deny"" /f
)

:: App Diagnostics
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics"" /v ""Value"" /t REG_SZ /d ""Deny"" /f

:: Broad File System Access
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\broadFileSystemAccess"" /v ""Value"" /t REG_SZ /d ""Deny"" /f

:: Graphics Capture Without Border
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\graphicsCaptureWithoutBorder"" /v ""Value"" /t REG_SZ /d ""Deny"" /f

:: Programmatic Graphics Capture
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\graphicsCaptureProgrammatic"" /v ""Value"" /t REG_SZ /d ""Deny"" /f

echo =========================================
echo Done.

endlocal",
                RevertBatch = $@"@echo off
setlocal EnableDelayedExpansion

REM Ensure admin privileges (requires running as Administrator)
echo =========================================
echo Privacy revert
echo =========================================

:: ===================================================
:: Edge specific privacy
:: ===================================================

reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""BackgroundModeEnabled"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""StartupBoostEnabled"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""HideFirstRunExperience"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""CreateDesktopShortcutDefault"" /f

:: ===================================================
:: User data tracking privacy
:: ===================================================

reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo"" /v ""Enabled"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""PublishUserActivities"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\System"" /v ""EnableActivityFeed"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection"" /v ""AllowTelemetry"" /f
reg delete ""HKCU\Software\Policies\Microsoft\Windows\DataCollection"" /v ""DoNotShowFeedbackNotifications"" /f
reg delete ""HKCU\Software\Microsoft\InputPersonalization"" /v ""RestrictImplicitTextCollection"" /f
reg delete ""HKCU\Software\Microsoft\InputPersonalization"" /v ""RestrictImplicitInkCollection"" /f
reg delete ""HKCU\Software\Microsoft\InputPersonalization\TrainedDataStore"" /v ""HarvestContacts"" /f
reg delete ""HKCU\Software\Policies\Microsoft\Windows\CloudContent"" /v ""DisableTailoredExperiencesWithDiagnosticData"" /f

setx POWERSHELL_TELEMETRY_OPTOUT "" >nul 2>&1

:: ===================================================
:: Browsers privacy
:: ===================================================

taskkill /IM chrome.exe /F >nul 2>&1
taskkill /IM msedge.exe /F >nul 2>&1
taskkill /IM brave.exe /F >nul 2>&1

for /f ""tokens=1"" %S in ('sc query state^= all ^| findstr /I ""GoogleUpdaterService""') do net start ""%S"" >nul 2>&1
for /f ""tokens=1"" %S in ('sc query state^= all ^| findstr /I ""GoogleUpdaterInternalService""') do net start ""%S"" >nul 2>&1
net start ""GoogleChromeElevationService"" >nul 2>&1
for /f ""tokens=1"" %S in ('sc query state^= all ^| findstr /I ""GoogleUpdaterService""') do sc config ""%S"" start= auto >nul 2>&1
for /f ""tokens=1"" %S in ('sc query state^= all ^| findstr /I ""GoogleUpdaterInternalService""') do sc config ""%S"" start= auto >nul 2>&1
sc config ""GoogleChromeElevationService"" start= auto >nul 2>&1
for /f ""tokens=*"" %T in ('schtasks /query /fo csv ^| findstr /I ""GoogleUpdaterTaskSystem""') do (
    for /f ""tokens=1 delims=,"" %N in (""%T"") do schtasks /change /tn %~N /enable >nul 2>&1
)

net start ""edgeupdate"" >nul 2>&1
net start ""edgeupdatem"" >nul 2>&1
net start ""MicrosoftEdgeElevationService"" >nul 2>&1
sc config ""edgeupdate"" start= auto >nul 2>&1
sc config ""edgeupdatem"" start= auto >nul 2>&1
sc config ""MicrosoftEdgeElevationService"" start= auto >nul 2>&1
for /f ""tokens=*"" %T in ('schtasks /query /fo csv ^| findstr /I ""MicrosoftEdgeUpdateTaskMachine""') do (
    for /f ""tokens=1 delims=,"" %N in (""%T"") do schtasks /change /tn %~N /enable >nul 2>&1
)

net start ""brave"" >nul 2>&1
net start ""bravem"" >nul 2>&1
net start ""BraveElevationService"" >nul 2>&1
sc config ""brave"" start= auto >nul 2>&1
sc config ""bravem"" start= auto >nul 2>&1
sc config ""BraveElevationService"" start= auto >nul 2>&1
for /f ""tokens=*"" %T in ('schtasks /query /fo csv ^| findstr /I ""BraveSoftwareUpdateTaskMachine""') do (
    for /f ""tokens=1 delims=,"" %N in (""%T"") do schtasks /change /tn %~N /enable >nul 2>&1
)

reg delete ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultVideoCaptureSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultAudioCaptureSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultClipboardSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultGeolocationSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultNotificationsSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultBackgroundSyncSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultIdleDetectionSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Google\Chrome"" /v ""DefaultWebUsbGuardSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultVideoCaptureSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultAudioCaptureSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultClipboardSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultGeolocationSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultNotificationsSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultBackgroundSyncSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultIdleDetectionSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Edge"" /v ""DefaultWebUsbGuardSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultVideoCaptureSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultAudioCaptureSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultClipboardSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultGeolocationSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultNotificationsSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultBackgroundSyncSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultIdleDetectionSetting"" /f
reg delete ""HKLM\SOFTWARE\Policies\BraveSoftware\Brave"" /v ""DefaultWebUsbGuardSetting"" /f

:: ===================================================
:: Windows bloatwares privacy
:: ===================================================

reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent"" /v ""DisableWindowsConsumerFeatures"" /f >nul 2>&1
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy"" /v ""LetAppsRunInBackground"" /f >nul 2>&1

:: ===================================================
:: Notifications privacy
:: ===================================================

reg delete ""HKCU\Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"" /v ""NoToastNotification"" /f >nul 2>&1
reg delete ""HKCU\Software\Policies\Microsoft\Windows\CurrentVersion\PushNotifications"" /v ""NoToastApplicationNotification"" /f >nul 2>&1
reg delete ""HKCU\Software\Policies\Microsoft\Windows\Explorer"" /v ""DisableNotificationCenter"" /f >nul 2>&1
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\PushNotifications"" /v ""ToastEnabled"" /f >nul 2>&1
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings"" /v ""NOC_GLOBAL_SETTING_TOASTS_ENABLED"" /f >nul 2>&1
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Notifications\Settings"" /v ""NOC_GLOBAL_SETTING_DND"" /f >nul 2>&1

net stop WpnService >nul 2>&1
net start WpnService >nul 2>&1
for /f ""tokens=1"" %S in ('sc query state^= all ^| findstr /I ""WpnUserService""') do (
    net stop ""%S"" >nul 2>&1
    net start ""%S"" >nul 2>&1
)

:: ===================================================
:: Start-up search privacy
:: ===================================================

reg delete ""HKCU\Software\Policies\Microsoft\Windows\Explorer"" /v ""DisableSearchBoxSuggestions"" /f >nul 2>&1
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Search"" /v ""BingSearchEnabled"" /f >nul 2>&1

:: ===================================================
:: UWP / Store apps app privacy
:: ===================================================

for %K in (location userAccountInformation graphicsCaptureProgrammatic radios activity camera contacts appointments chat phoneCall downloadsFolder broadFileSystemAccess picturesLibrary videosLibrary musicLibrary documentsLibrary savedPictures savedVideos) do (
    reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\%K"" /f >nul 2>&1
    reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\%K"" /v ""Value"" /t REG_SZ /d ""Allow"" /f
)

:: ===================================================
:: Classic desktop Win32 apps privacy
:: ===================================================

for %K in (location userAccountInformation graphicsCaptureProgrammatic radios activity camera contacts appointments chat phoneCall downloadsFolder broadFileSystemAccess picturesLibrary videosLibrary musicLibrary documentsLibrary savedPictures savedVideos) do (
    reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\%K\NonPackaged"" /f >nul 2>&1
    reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\%K\NonPackaged"" /v ""Value"" /t REG_SZ /d ""Allow"" /f
)

:: App Diagnostics
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics"" /v ""Value"" /t REG_SZ /d ""Allow"" /f

:: Broad File System Access
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\broadFileSystemAccess"" /v ""Value"" /t REG_SZ /d ""Allow"" /f

:: Graphics Capture Without Border
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\graphicsCaptureWithoutBorder"" /v ""Value"" /t REG_SZ /d ""Allow"" /f

:: Programmatic Graphics Capture
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\graphicsCaptureProgrammatic"" /v ""Value"" /t REG_SZ /d ""Allow"" /f

echo =========================================
echo done

endlocal"
            },

            new EmbeddedTweak
            {
                Key = "Services Optimization",
                Name = "Disabling Unnecessary Services and Executables",
                Tags = "FeatureBreaking,Finished",
                Description = "This script stops and disables Windows system, user, and third-party related services, per-user services. It also terminates a list of running applications related to Windows.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
setlocal EnableDelayedExpansion

REM Ensure admin privileges (requires running as Administrator)
echo =========================================
echo Services and executables optimization
echo =========================================

for %i in (	
	WerSvc
	GpuEnergyDrv
	IntelPMT
	kdnic
	Vid
	usbprint
	Themes
	vdrvroot
	Telemetry
	troubleshootingsvc
	luafv
	ndu
	PRM
	DPS
	diagnosticshub.standardcollector.service
	dmwappushservice
	WdiServiceHost
	WdiSystemHost
	TrkWks
    RetailDemo 
 	WiaRpc 
 	DusmSvc 
 	DsSvc 
 	SSDPSRV 
 	StiSvc 
 	TapiSrv 
 	TextInputManagementService 
 	InventorySvc 
 	RemoteAccess 
 	RasAuto 
 	ssh-agent 
 	EntAppSvc 
 	LanmanServer 
 	LanmanWorkstation 
 	RemoteRegistry 
 	TermService 
 	SessionEnv 
 	PeerDistSvc 
 	PNRPAutoReg 
 	PNRPsvc 
 	p2pimsvc 
 	p2psvc 
 	WMPNetworkSvc 
 	WFDSConMgrSvc 
 	workfolderssvc 
 	WPDBusEnum 
 	midisrv 
 	seclogon 
 	wuqisvc 
 	ZTHELPER 
 	wlpasvc 
 	AarSvc 			 
 	ConsentUxUserSvc 			 
 	VSS 
 	wbengine 
 	fhsvc 
 	HvHost 
 	P9RdrService 
 	vmicguestinterface 
 	vmicheartbeat 
 	vmickvpexchange 
 	vmicrdv 
 	vmicshutdown 
 	vmictimesync 
 	vmicvmsession 
 	vmicvss 
 	vmvss 
 	nvagent 
 	vmcompute 
 	vmms 
 	perceptionsimulation 
 	vds 
 	HgClientService 
 	SCPolicySvc 
 	SCardSvr 
 	ScDeviceEnum 
 	MsKeyboardFilter 
 	Browser 
 	IpxlatCfgSvc 
 	AssignedAccessManagerSvc 
 	DialogBlockingService 
 	PenService 			 
 	RmSvc 
 	wuauserv 
 	UsoSvc 
 	W32Time 
 	wisvc 
 	WSearch 
 	lfsvc 
 	WpcMonSvc 
 	tzautoupdate 
 	TimeBrokerSvc 
 	FontCache3 			 
 	PcaSvc 
 	QWAVE 
 	wcncsvc 
 	AppXSvc 
 	StorSvc 
 	AppIDSvc 
 	CloudBackupRestoreSvc 			 
 	cloudidsvc 
 	CscService 
 	EapHost 
 	VaultSvc 
 	DPS 
 	Wecsvc 
 	WinRm 
 	WdiServiceHost 
 	WdiSystemHost 
 	DiagTrack 
 	WerSvc 
 	TroubleshootingSvc 
 	wercplsupport 
 	pla 
 	ADPSvc 
 	CryptSvc 
 	BDESVC 
 	EFS 
 	Spooler 
 	PrintScanBrokerDevice 
 	PrintScanBrokerService 
 	PrintNotify 
 	PrintDeviceConfigurationService 
 	PrintWorkflowUserSvc 			 
 	McpManagementService 
 	GoogleChromeElevationService 
 	GoogleUpdaterInternalService 			 
 	GoogleUpdaterService 			 
 	MicrosoftEdgeElevationService 
 	edgeupdate 
 	edgeupdatem 
 	SEMgrSvc 
 	WalletService 
 	PhoneSvc 
 	SmsRouter 
 	WpnService 
 	WpnUserService 			 
 	PushNotificationService 
 	BITS 
 	DevQueryBroker 
 	DevicePickerUserSvc 			 
 	DevicesFlowUserSvc 			 
 	DmEnrollmentSvc 
 	NcbService 
 	CDPSvc 
 	CDPUserSvc 			 
 	shpamsvc 
 	TrkWks 
 	fdPHost 
 	FDResPub  
 	InstallService 
 	BcastDVRUserService 
 	CaptureService 			 
 	RasMan 
 	SstpSvc 
 	bthserv 
 	BTAGService 
 	BluetoothUserService 			 
 	DeviceAssociationService 
 	BthAvctpSvc 
 	SensorDataService 
 	SensorService 
 	SensrSvc 
 	PimIndexMaintenanceSvc_ 			 
 	DisplayEnhancementService 
 	WSLService 
 	webthreatdefsvc 
 	lmhosts 
 	NgcCtnrSvc 
 	NgcSvc 
 	whesvc
) do (
	echo [%i] - disabling
	sc stop %i >nul 2>&1
	sc config %i start= disabled >nul 2>&1
)

taskkill /F /T /IM msedgewebview2.exe >nul 2>&1
taskkill /F /T /IM smartscreen.exe >nul 2>&1
taskkill /F /T /IM SnippingTool.exe >nul 2>&1
taskkill /F /T /IM PhoneExperienceHost.exe >nul 2>&1
taskkill /F /T /IM msedge.exe >nul 2>&1
taskkill /F /T /IM SearchIndexer.exe >nul 2>&1
taskkill /F /T /IM SearchProtocolHost.exe >nul 2>&1
taskkill /F /T /IM SearchFilterHost.exe >nul 2>&1
taskkill /F /T /IM SearchHost.exe >nul 2>&1
taskkill /F /T /IM StartMenuExperienceHost.exe >nul 2>&1
taskkill /F /T /IM MoNotificationUx.exe >nul 2>&1
taskkill /F /T /IM ctfmon.exe >nul 2>&1
taskkill /F /T /IM wmpnetwk.exe >nul 2>&1
taskkill /F /T /IM UserOOBEBroker.exe >nul 2>&1
taskkill /F /T /IM vmms.exe >nul 2>&1
taskkill /F /T /IM vmcompute.exe >nul 2>&1
taskkill /F /T /IM LockApp.exe >nul 2>&1
taskkill /F /T /IM Copilot.exe >nul 2>&1
taskkill /F /T /IM MicrosoftStartFeedProvider.exe >nul 2>&1
taskkill /F /T /IM gamingservices.exe >nul 2>&1
taskkill /F /T /IM CrossDeviceResume.exe >nul 2>&1
taskkill /F /T /IM WidgetService.exe >nul 2>&1
taskkill /F /T /IM PerceptionSimulationService.exe >nul 2>&1
taskkill /F /T /IM LsaIso.exe >nul 2>&1
taskkill /F /T /IM SecurityHealthService.exe >nul 2>&1
taskkill /F /T /IM OneDrive.exe >nul 2>&1
taskkill /F /T /IM OneDrive.Sync.Service.exe >nul 2>&1
taskkill /F /T /IM OneDriveStandaloneUpdater.exe >nul 2>&1
taskkill /F /T /IM XboxPcApp.exe >nul 2>&1
taskkill /F /T /IM XboxPcTray.exe >nul 2>&1
taskkill /F /T /IM XboxPcAppFT.exe >nul 2>&1
taskkill /F /T /IM WidgetBoard.exe >nul 2>&1
taskkill /F /T /IM GameInputSvc.exe >nul 2>&1
taskkill /F /T /IM gamingservicesnet.exe >nul 2>&1
taskkill /F /T /IM Locator.exe >nul 2>&1
taskkill /F /T /IM msiexec.exe >nul 2>&1
taskkill /F /T /IM WinRAR.exe >nul 2>&1

echo =========================================
echo Done.

endlocal",
            RevertBatch = $@"@echo off
setlocal EnableDelayedExpansion

REM Ensure admin privileges (requires running as Administrator)
echo =========================================
echo Services and executables revert
echo =========================================

for %i in (	
	WerSvc
	GpuEnergyDrv
	IntelPMT
	kdnic
	Vid
	usbprint
	Themes
	vdrvroot
	Telemetry
	troubleshootingsvc
	luafv
	ndu
	PRM
	DPS
	diagnosticshub.standardcollector.service
	dmwappushservice
	WdiServiceHost
	WdiSystemHost
	TrkWks
    RetailDemo 
 	WiaRpc 
 	DusmSvc 
 	DsSvc 
 	SSDPSRV 
 	StiSvc 
 	TapiSrv 
 	TextInputManagementService 
 	InventorySvc 
 	RemoteAccess 
 	RasAuto 
 	ssh-agent 
 	EntAppSvc 
 	LanmanServer 
 	LanmanWorkstation 
 	RemoteRegistry 
 	TermService 
 	SessionEnv 
 	PeerDistSvc 
 	PNRPAutoReg 
 	PNRPsvc 
 	p2pimsvc 
 	p2psvc 
 	WMPNetworkSvc 
 	WFDSConMgrSvc 
 	workfolderssvc 
 	WPDBusEnum 
 	midisrv 
 	seclogon 
 	wuqisvc 
 	ZTHELPER 
 	wlpasvc 
 	AarSvc 			 
 	ConsentUxUserSvc 			 
 	wbengine 
 	fhsvc 
 	HvHost 
 	P9RdrService 
 	vmicguestinterface 
 	vmicheartbeat 
 	vmickvpexchange 
 	vmicrdv 
 	vmicshutdown 
 	vmictimesync 
 	vmicvmsession 
 	vmicvss 
 	vmvss 
 	nvagent 
 	vmcompute 
 	vmms 
 	perceptionsimulation 
 	vds 
 	HgClientService 
 	SCPolicySvc 
 	SCardSvr 
 	ScDeviceEnum 
 	MsKeyboardFilter 
 	Browser 
 	IpxlatCfgSvc 
 	AssignedAccessManagerSvc 
 	DialogBlockingService 
 	PenService 			 
 	RmSvc 
 	wuauserv 
 	UsoSvc 
 	W32Time 
 	wisvc 
 	WSearch 
 	lfsvc 
 	WpcMonSvc 
 	tzautoupdate 
 	TimeBrokerSvc 
 	FontCache3 			 
 	PcaSvc 
 	QWAVE 
 	wcncsvc 
 	AppXSvc 
 	StorSvc 
 	AppIDSvc 
 	CloudBackupRestoreSvc 			 
 	cloudidsvc 
 	CscService 
 	EapHost 
 	VaultSvc 
 	DPS 
 	Wecsvc 
 	WinRm 
 	WdiServiceHost 
 	WdiSystemHost 
 	DiagTrack 
 	WerSvc 
 	TroubleshootingSvc 
 	wercplsupport 
 	pla 
 	ADPSvc 
 	BDESVC 
 	EFS 
 	Spooler 
 	PrintScanBrokerDevice 
 	PrintScanBrokerService 
 	PrintNotify 
 	PrintDeviceConfigurationService 
 	PrintWorkflowUserSvc 			 
 	McpManagementService 
 	GoogleChromeElevationService 
 	GoogleUpdaterInternalService 			 
 	GoogleUpdaterService 			 
 	MicrosoftEdgeElevationService 
 	edgeupdate 
 	edgeupdatem 
 	SEMgrSvc 
 	WalletService 
 	PhoneSvc 
 	SmsRouter 
 	WpnService 
 	WpnUserService 			 
 	PushNotificationService 
 	BITS 
 	DevQueryBroker 
 	DevicePickerUserSvc 			 
 	DevicesFlowUserSvc 			 
 	DmEnrollmentSvc 
 	NcbService 
 	CDPSvc 
 	CDPUserSvc 			 
 	shpamsvc 
 	TrkWks 
 	fdPHost 
 	FDResPub  
 	InstallService 
 	BcastDVRUserService 
 	CaptureService 			 
 	RasMan 
 	SstpSvc 
 	bthserv 
 	BTAGService 
 	BluetoothUserService 			 
 	DeviceAssociationService 
 	BthAvctpSvc 
 	SensorDataService 
 	SensorService 
 	SensrSvc 
 	PimIndexMaintenanceSvc_ 			 
 	DisplayEnhancementService 
 	WSLService 
 	webthreatdefsvc 
 	lmhosts 
 	NgcCtnrSvc 
 	NgcSvc 
 	whesvc
) do (
	echo [%i] - starting
        sc config %i start= auto >nul 2>&1
	sc start %i >nul 2>&1
)

echo =========================================
echo done

endlocal"
            },

            new EmbeddedTweak
            {
                Key = "Storage Optimization",
                Name = "Optimizing Storage",
                Tags = "FeatureBreaking,Finished",
                Description = "This script cleans up peripherals noise and optimize SSD volumes. It doesn't need to be reverted.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
setlocal EnableDelayedExpansion

REM Ensure admin privileges (requires running as Administrator)
echo =========================================
echo Storage Optimization
echo =========================================

REM --------------------------------------------------------
REM Starting defrag service
REM --------------------------------------------------------
echo [1/6] Restarting defrag service...
net stop defragsvc >nul 2>&1
net start defragsvc >nul 2>&1

REM --------------------------------------------------------
REM Optimizing volumes (Retrim)
REM --------------------------------------------------------
echo [2/6] Optimizing volumes (Retrim)...
REM /C performs the operation on all volumes. /L performs a Retrim.
defrag /C /L >nul 2>&1

REM --------------------------------------------------------
REM Stopping defrag service
REM --------------------------------------------------------
echo [3/6] Stopping defrag service...
net stop defragsvc >nul 2>&1
sc config defragsvc start= disabled >nul 2>&1

REM --------------------------------------------------------
REM Restore storage service to custom volumes features
REM --------------------------------------------------------
echo [4/6] Restoring storage service (SysMain)...
sc config SysMain start= auto >nul 2>&1
net start SysMain >nul 2>&1

REM --------------------------------------------------------
REM Disabling volumes features
REM --------------------------------------------------------
echo [5/6] Disabling specific volume features...
fsutil behavior set disablelastaccess 1 >nul 2>&1
fsutil behavior set disable8dot3 1 >nul 2>&1
fsutil behavior set encryptpagingfile 0 >nul 2>&1
powershell -NoProfile -Command ""Disable-MMAgent -PageCombining -ErrorAction SilentlyContinue"" >nul 2>&1
powershell -NoProfile -Command ""Disable-MMAgent -ApplicationPreLaunch -ErrorAction SilentlyContinue"" >nul 2>&1

REM --------------------------------------------------------
REM Stop storage service
REM --------------------------------------------------------
echo [6/6] Stopping and disabling storage service (SysMain)...
net stop SysMain >nul 2>&1
sc config SysMain start= disabled >nul 2>&1

echo =========================================
echo Done.

endlocal",
        RevertBatch = $@"@echo off
setlocal EnableDelayedExpansion

REM Ensure admin privileges (requires running as Administrator)
echo =========================================
echo Storage revert
echo =========================================

echo [1/3] Restarting defrag service...
sc config defragsvc start= auto >nul 2>&1
net start defragsvc >nul 2>&1

echo [2/3] Restoring storage service (SysMain)...
sc config SysMain start= auto >nul 2>&1
net start SysMain >nul 2>&1

echo [3/3] Disabling specific volume features...
fsutil behavior set disablelastaccess 0 >nul 2>&1
fsutil behavior set disable8dot3 0 >nul 2>&1
fsutil behavior set encryptpagingfile 1 >nul 2>&1
powershell -NoProfile -Command ""Enable-MMAgent -PageCombining -ErrorAction SilentlyContinue"" >nul 2>&1
powershell -NoProfile -Command ""Enable-MMAgent -ApplicationPreLaunch -ErrorAction SilentlyContinue"" >nul 2>&1

echo =========================================
echo Done.

endlocal"
            },

            new EmbeddedTweak
            {
                Key = "Quality Of Life Optimization",
                Name = "Optimizing Quality Of Life",
                Tags = "FeatureBreaking,Finished",
                Description = "This script optimizes user quality of life on the Windows system.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
setlocal EnableDelayedExpansion

REM Ensure admin privileges (requires running as Administrator)
echo =========================================
echo Quality of life
echo =========================================

:: Disable Animations
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects"" /v ""VisualFXSetting"" /t REG_DWORD /d 3 /f
reg add ""HKCU\Control Panel\Desktop"" /v ""UserPreferencesMask"" /t REG_BINARY /d 9812078010000000 /f
reg add ""HKCU\Control Panel\Desktop\WindowMetrics"" /v ""MinAnimate"" /t REG_SZ /d ""0"" /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""TaskbarAnimations"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Microsoft\Windows\DWM"" /v ""TaskbarAnimations"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Control Panel\Desktop"" /v ""FontSmoothing"" /t REG_SZ /d ""2"" /f

:: Disable Transparency
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"" /v ""EnableTransparency"" /t REG_DWORD /d 0 /f

:: Dark Theme
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"" /v ""AppsUseLightTheme"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"" /v ""SystemUsesLightTheme"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"" /v ""AppsUseLightTheme"" /t REG_DWORD /d 0 /f

:: Hide start menu recommended section / category view / app list
reg add ""HKLM\Software\Policies\Microsoft\Windows\Explorer"" /v ""HideRecommendedSection"" /t REG_DWORD /d 1 /f
reg add ""HKLM\Software\Policies\Microsoft\Windows\Explorer"" /v ""HideCategoryView"" /t REG_DWORD /d 1 /f
reg add ""HKLM\Software\Policies\Microsoft\Windows\Explorer"" /v ""HideAppList"" /t REG_DWORD /d 1 /f

:: Show everytime file extensions
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""HideFileExt"" /t REG_DWORD /d 0 /f

:: Show everytime hidden files & folders
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""Hidden"" /t REG_DWORD /d 1 /f

:: Hide task view button on taskbar
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""ShowTaskViewButton"" /t REG_DWORD /d 0 /f

:: Open file explorer to this pc instead of quick access weird s
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""LaunchTo"" /t REG_DWORD /d 1 /f

:: Disable cortana integration
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Search"" /v ""CortanaConsent"" /t REG_DWORD /d 0 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Search"" /v ""AllowCortana"" /t REG_DWORD /d 0 /f

echo =========================================
echo Done.

endlocal",
            RevertBatch = $@"@echo off
setlocal EnableDelayedExpansion

REM Ensure admin privileges (requires running as Administrator)
echo =========================================
echo Quality of life revert
echo =========================================

:: Restore Visual Effects
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects"" /v ""VisualFXSetting"" /t REG_DWORD /d 0 /f

:: Restore default UserPreferencesMask
reg add ""HKCU\Control Panel\Desktop"" /v ""UserPreferencesMask"" /t REG_BINARY /d 9E1E078012000000 /f

:: Enable window minimize/maximize animations
reg add ""HKCU\Control Panel\Desktop\WindowMetrics"" /v ""MinAnimate"" /t REG_SZ /d ""1"" /f

:: Enable taskbar animations
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""TaskbarAnimations"" /t REG_DWORD /d 1 /f

:: Remove the DWM TaskbarAnimations override
reg delete ""HKCU\Software\Microsoft\Windows\DWM"" /v ""TaskbarAnimations"" /f

:: Keep ClearType font smoothing enabled
reg add ""HKCU\Control Panel\Desktop"" /v ""FontSmoothing"" /t REG_SZ /d ""2"" /f

:: Enable transparency effects
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"" /v ""EnableTransparency"" /t REG_DWORD /d 1 /f

reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"" /v ""AppsUseLightTheme"" /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"" /v ""SystemUsesLightTheme"" /t REG_DWORD /d 1 /f

reg add ""HKLM\Software\Policies\Microsoft\Windows\Explorer"" /v ""HideRecommendedSection"" /t REG_DWORD /d 0 /f
reg add ""HKLM\Software\Policies\Microsoft\Windows\Explorer"" /v ""HideCategoryView"" /t REG_DWORD /d 0 /f
reg add ""HKLM\Software\Policies\Microsoft\Windows\Explorer"" /v ""HideAppList"" /t REG_DWORD /d 0 /f

reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""HideFileExt"" /t REG_DWORD /d 1 /f

reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""Hidden"" /t REG_DWORD /d 0 /f

reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""ShowTaskViewButton"" /t REG_DWORD /d 1 /f

reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced"" /v ""LaunchTo"" /t REG_DWORD /d 0 /f

reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Search"" /v ""CortanaConsent"" /t REG_DWORD /d 1 /f
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Search"" /v ""AllowCortana"" /t REG_DWORD /d 1 /f

echo =========================================
echo Done.

endlocal
exit"
            },

            new EmbeddedTweak
            {
                Key = "Disable ETW",
                Name = "Optimizing Event Tracing",
                Tags = "FeatureBreaking,Finished",
                Description = "This script disables event tracing for Windows and third-party applications.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
logman stop ""8696EAC4-1288-4288-A4EE-49EE431B0AD9"" -ets >nul 2>&1
logman stop ""1DSListener"" -ets >nul 2>&1
logman stop ""AppModel"" -ets >nul 2>&1
logman stop ""Circular Kernel Context Logger"" -ets >nul 2>&1
logman stop ""CimFSUnionFS-Filter"" -ets >nul 2>&1
logman stop ""DefenderApiLogger"" -ets >nul 2>&1
logman stop ""DefenderAuditLogger"" -ets >nul 2>&1
logman stop ""iclsClient"" -ets >nul 2>&1
logman stop ""iclsProxy"" -ets >nul 2>&1
logman stop ""FaceRecoTel"" -ets >nul 2>&1
logman stop ""FaceUnlock"" -ets >nul 2>&1
logman stop ""FilterMgr-Logger"" -ets >nul 2>&1
logman stop ""LwtNetLog"" -ets >nul 2>&1
logman stop ""Microsoft-Windows-Rdp-Graphics-RdpIdd-Trace"" -ets >nul 2>&1
logman stop ""IntelRST"" -ets >nul 2>&1
logman stop ""NetCfgTrace"" -ets >nul 2>&1
logman stop ""NOCAT"" -ets >nul 2>&1
logman stop ""NtfsLog"" -ets >nul 2>&1
logman stop ""RadioMgr"" -ets >nul 2>&1
logman stop ""ReFSLog"" -ets >nul 2>&1
logman stop ""PlatformLicenseManagerService"" -ets >nul 2>&1
logman stop ""ScreenOnPowerStudyTraceSession"" -ets >nul 2>&1
logman stop ""TileStore"" -ets >nul 2>&1
logman stop ""UserNotPresentTraceSession"" -ets >nul 2>&1
logman stop ""WdiContextLog"" -ets >nul 2>&1
logman stop ""WiFiSession"" -ets >nul 2>&1
logman stop ""WindowsUpdate_trace_log"" -ets >nul 2>&1
logman stop ""NVIDIA-NVTOPPS-NOCAT"" -ets >nul 2>&1
logman stop ""NVIDIA-NVTOPPS-FILTER"" -ets >nul 2>&1
logman stop ""SleepStudyTraceSession"" -ets >nul 2>&1
logman stop ""Admin_PS_Provider"" -ets >nul 2>&1
logman stop ""UpdateSessionOrchestration"" -ets >nul 2>&1
logman stop ""MoUxCoreWorker"" -ets >nul 2>&1
logman stop ""WiFiDriverIHVSession"" -ets >nul 2>&1
logman stop ""UBPM"" -ets >nul 2>&1
logman stop ""Steam Event Tracing"" -ets >nul 2>&1
logman stop ""MpWppCoreTracing-20250422-153720-00000003-100000000"" -ets >nul 2>&1
logman stop ""SHS-04222025-153736-7-1ff"" -ets >nul 2>&1
logman stop ""dfa2c640-651d-488d-a479-2fd7a7ca6e29"" -ets >nul 2>&1
logman stop ""MpWppTracing-20250422-133720-00000003-fffffffeffffffff"" -ets >nul 2>&1
logman stop ""TraceEventSession"" -ets >nul 2>&1
logman stop ""CldFltLog"" -ets >nul 2>&1
logman stop ""MSDTC_TRACE_SESSION"" -ets >nul 2>&1
logman stop ""Terminal-Services-LSM-ApplicationLag-5456"" -ets >nul 2>&1",
                RevertBatch = $@"@echo off
logman start ""8696EAC4-1288-4288-A4EE-49EE431B0AD9"" -ets >nul 2>&1
logman start ""1DSListener"" -ets >nul 2>&1
logman start ""AppModel"" -ets >nul 2>&1
logman start ""Circular Kernel Context Logger"" -ets >nul 2>&1
logman start ""CimFSUnionFS-Filter"" -ets >nul 2>&1
logman start ""DefenderApiLogger"" -ets >nul 2>&1
logman start ""DefenderAuditLogger"" -ets >nul 2>&1
logman start ""iclsClient"" -ets >nul 2>&1
logman start ""iclsProxy"" -ets >nul 2>&1
logman start ""FaceRecoTel"" -ets >nul 2>&1
logman start ""FaceUnlock"" -ets >nul 2>&1
logman start ""FilterMgr-Logger"" -ets >nul 2>&1
logman start ""LwtNetLog"" -ets >nul 2>&1
logman start ""Microsoft-Windows-Rdp-Graphics-RdpIdd-Trace"" -ets >nul 2>&1
logman start ""IntelRST"" -ets >nul 2>&1
logman start ""NetCfgTrace"" -ets >nul 2>&1
logman start ""NOCAT"" -ets >nul 2>&1
logman start ""NtfsLog"" -ets >nul 2>&1
logman start ""RadioMgr"" -ets >nul 2>&1
logman start ""ReFSLog"" -ets >nul 2>&1
logman start ""PlatformLicenseManagerService"" -ets >nul 2>&1
logman start ""ScreenOnPowerStudyTraceSession"" -ets >nul 2>&1
logman start ""TileStore"" -ets >nul 2>&1
logman start ""UserNotPresentTraceSession"" -ets >nul 2>&1
logman start ""WdiContextLog"" -ets >nul 2>&1
logman start ""WiFiSession"" -ets >nul 2>&1
logman start ""WindowsUpdate_trace_log"" -ets >nul 2>&1
logman start ""NVIDIA-NVTOPPS-NOCAT"" -ets >nul 2>&1
logman start ""NVIDIA-NVTOPPS-FILTER"" -ets >nul 2>&1
logman start ""SleepStudyTraceSession"" -ets >nul 2>&1
logman start ""Admin_PS_Provider"" -ets >nul 2>&1
logman start ""UpdateSessionOrchestration"" -ets >nul 2>&1
logman start ""MoUxCoreWorker"" -ets >nul 2>&1
logman start ""WiFiDriverIHVSession"" -ets >nul 2>&1
logman start ""UBPM"" -ets >nul 2>&1
logman start ""Steam Event Tracing"" -ets >nul 2>&1
logman start ""MpWppCoreTracing-20250422-153720-00000003-100000000"" -ets >nul 2>&1
logman start ""SHS-04222025-153736-7-1ff"" -ets >nul 2>&1
logman start ""dfa2c640-651d-488d-a479-2fd7a7ca6e29"" -ets >nul 2>&1
logman start ""MpWppTracing-20250422-133720-00000003-fffffffeffffffff"" -ets >nul 2>&1
logman start ""TraceEventSession"" -ets >nul 2>&1
logman start ""CldFltLog"" -ets >nul 2>&1
logman start ""MSDTC_TRACE_SESSION"" -ets >nul 2>&1
logman start ""Terminal-Services-LSM-ApplicationLag-5456"" -ets >nul 2>&1"
            },

            new EmbeddedTweak
            {
                Key = "Disable GameBarPresenceWriter",
                Name = "Disabling GameBarPresenceWriter",
                Tags = "FeatureBreaking,Finished",
                Description = "This script disables GameBarPresenceWriter.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
@REM takeown /f ""C:\Windows\System32\GameBarPresenceWriter.exe"" >NUL 2>&1
@REM icacls ""C:\Windows\System32\GameBarPresenceWriter.exe"" /grant "":F"" >NUL 2>&1
@REM icacls ""C:\Windows\System32\GameBarPresenceWriter.exe"" /grant ""%USERNAME%:F"" >NUL 2>&1
@REM move C:\Windows\System32\GameBarPresenceWriter.exe C:\Windows >NUL 2>&1
@REM subinacl /regkey HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\WindowsRuntime\ActivatableClassId\Windows.Gaming.GameBar.PresenceServer.Internal.PresenceWriter /setowner=%Username%
powerrun /SW:0 cmd.exe /k ""reg add ""HKLM\SOFTWARE\Microsoft\WindowsRuntime\ActivatableClassId\Windows.Gaming.GameBar.PresenceServer.Internal.PresenceWriter"" /v ""ActivationType"" /t REG_DWORD /d ""0"" /f >NUL 2>&1 & exit""",
                RevertBatch = $@"@echo off
powerrun /SW:0 cmd.exe /k ""reg add ""HKLM\SOFTWARE\Microsoft\WindowsRuntime\ActivatableClassId\Windows.Gaming.GameBar.PresenceServer.Internal.PresenceWriter"" /v ""ActivationType"" /t REG_DWORD /d ""1"" /f >NUL 2>&1 & exit"""
            }
        };
    }
}
