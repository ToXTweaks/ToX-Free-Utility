using System.Collections.Generic;

namespace ToX_Free_Utility_NEW_ERA.Tweaks
{
  public static class GraphicTweaks
  {
    public static List<EmbeddedTweak> All => new List<EmbeddedTweak>
      {
        new EmbeddedTweak
        {
          Key = "General Nvidia GPU Tweaks",
          Name = "Optimize Nvidia GPU",
          Tags = "Finished,PowerHungry,FeatureBreaking",
          Description = "Optimizes Nvidia GPU settings for improved performance, latency, and speed. This tweak may increase temperatures on poorly cooled PCs.",
          RequiresAdmin = false,
          ApplyBatch = $@"@echo off
for /f ""tokens=*"" %%c in ('reg query ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{{4d36e968-e325-11ce-bfc1-08002be10318}}"" /f ""Nvidia"" /t REG_SZ /s 2^>nul ^| findstr /i ""HKEY_""') do (
    set ""gpu_key=%%c""
)

:: disabling HDCP
reg add ""%Gpu_key%"" /v ""RmDisableHdcp22"" /t REG_DWORD /d 1 /f
reg add ""%Gpu_key%"" /v ""RMHdcpKeyglobZero"" /t REG_DWORD /d 1 /f
reg add ""%Gpu_key%"" /v ""RMSkipHdcp22Init"" /t REG_DWORD /d 1 /f
reg add ""%Gpu_key%"" /v ""RMHdcpOffload"" /t REG_DWORD /d 3 /f

:: disabling async p-states
reg add ""%Gpu_key%"" /v ""DisableAsyncPstates"" /t REG_DWORD /d 1 /f
reg add ""%Gpu_key%"" /v ""RMEnableOverclockingAllPstates"" /t REG_DWORD /d 1 /f

:: disabling pre-os apps
reg add ""%Gpu_key%"" /v ""RmDisablePreosapps"" /t REG_DWORD /d 1 /f

:: disabling logging
reg add ""HKLM\SYSTEM\CurrentControlSet\services\nvlddmkm\Parameters"" /v ""LogWarningEntries"" /t REG_DWORD /d 0 /f 
reg add ""HKLM\SYSTEM\CurrentControlSet\services\nvlddmkm\Parameters"" /v ""LogPagingEntries"" /t REG_DWORD /d 0 /f 
reg add ""HKLM\SYSTEM\CurrentControlSet\services\nvlddmkm\Parameters"" /v ""LogEventEntries"" /t REG_DWORD /d 0 /f 
reg add ""HKLM\SYSTEM\CurrentControlSet\services\nvlddmkm\Parameters"" /v ""LogErrorEntries"" /t REG_DWORD /d 0 /f 

:: disabling HDAudio D3 Cold
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""EnableHDAudioD3Cold"" /t REG_DWORD /d 0 /f

:: disabling Dlss
reg add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\NGXCore"" /v ""ShowDlssIndicator"" /t REG_DWORD /d 0 /f

:: disabling display PS
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\NVTweak"" /v ""DisplayPowerSaving"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\ControlSet001\Services\nvlddmkm\Global\NVTweak"" /v ""DisplayPowerSaving"" /t REG_DWORD /d 0 /f
reg add ""HKLM\Software\NVIDIA Corporation\Global\NVTweak"" /v ""DisplayPowerSaving"" /t REG_DWORD /d 0 /f

:: disabling telemetry
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\Startup"" /v ""SendTelemetryData"" /t REG_DWORD /d 0 /f 
reg add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v EnableRID44231 /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v EnableRID64640 /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v EnableRID66610 /t REG_DWORD /d 0 /f
reg delete ""HKLM\System\CurrentControlSet\Services\nvlddmkm\NvCamera"" /f
sc config NvTelemetryContainer start=disabled 
reg add ""HKLM\Software\NVIDIA Corporation\NvControlPanel2\Client"" /v ""OptInOrOutPreference"" /t REG_DWORD /d 0 /f 
For %%i in (NvTmRep_CrashReport1 NvTmRep_CrashReport2 NvTmRep_CrashReport3 NvTmRep_CrashReport4) Do Schtasks /Change /Disable /Tn ""%%i_{{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}}"" 
For %%i in (NvTmMon NvTmRep NvProfile NvNodeLauncher NvDriverUpdateCheckDaily NvBatteryBoostCheckOnLogon ""NVIDIA GeForce Experience SelfUpdate"") Do Schtasks /Change /Tn ""%%i"" /Disable 

for /d %%D in (""C:\Windows\System32\DriverStore\FileRepository\nv_dispsi.inf_amd64_*"") do (
    takeown /f ""%%D\NvTelemetry64.dll""
    icacls ""%%D\NvTelemetry64.dll"" /grant Administrators:F
    del /f /q ""%%D\NvTelemetry64.dll""
)
for /d %%D in (""C:\Windows\System32\DriverStore\FileRepository\nv_dispsi.inf_amd64_*"") do (
    takeown /f ""%%D\NvCamera"" /r /d y
    icacls ""%%D\NvCamera"" /grant Administrators:F /t
    rd /s /q ""%%D\NvCamera""
)
for /r ""C:\Windows\System32\DriverStore\FileRepository"" %%F in (_DisplayDriverRAS.dll) do (
    if exist ""%%F"" (
        takeown /f ""%%F""
        icacls ""%%F"" /grant Administrators:F
        del /f /q ""%%F""
    )
)
reg add ""HKLM\System\CurrentControlSet\Services\nvlddmkm\Global\Startup"" /v ""SendTelemetryData"" /t REG_DWORD /D 0 /f 

:: disabling notifs
reg add ""HKCU\SOFTWARE\NVIDIA Corporation\Global\GFExperience"" /v ""NotifyNewDisplayUpdates"" /t REG_DWORD /d 0 /f

:: disabling tray icon
reg add ""HKLM\SOFTWARE\NVIDIA Corporation\NvTray"" /v ""StartOnLogin"" /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\NVTweak"" /v ""HideXGpuTrayIcon"" /t REG_DWORD /d 1 /f",
          RevertBatch = $@"@echo off
echo Locating NVIDIA registry key...

for /f ""tokens=*"" %%c in ('reg query ""HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{{4d36e968-e325-11ce-bfc1-08002be10318}}"" /f ""NVIDIA"" /t REG_SZ /s 2^>nul ^| findstr /i ""HKEY_""') do (
    set ""Gpu_key=%%c""
)

if not defined Gpu_key (
    echo NVIDIA registry key not found.
    goto :DriverSettings
)

echo Reverting driver registry tweaks...

:: HDCP
reg delete ""%Gpu_key%"" /v ""RmDisableHdcp22"" /f
reg delete ""%Gpu_key%"" /v ""RMHdcpKeyglobZero"" /f
reg delete ""%Gpu_key%"" /v ""RMSkipHdcp22Init"" /f
reg delete ""%Gpu_key%"" /v ""RMHdcpOffload"" /f

:: Async P-States
reg delete ""%Gpu_key%"" /v ""DisableAsyncPstates"" /f
reg delete ""%Gpu_key%"" /v ""RMEnableOverclockingAllPstates"" /f

:: PreOS Apps
reg delete ""%Gpu_key%"" /v ""RmDisablePreosapps"" /f

:DriverSettings

:: Logging
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Parameters"" /v ""LogWarningEntries"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Parameters"" /v ""LogPagingEntries"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Parameters"" /v ""LogEventEntries"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Parameters"" /v ""LogErrorEntries"" /f

:: HD Audio D3 Cold
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm"" /v ""EnableHDAudioD3Cold"" /f

:: DLSS Indicator
reg delete ""HKLM\SOFTWARE\NVIDIA Corporation\Global\NGXCore"" /v ""ShowDlssIndicator"" /f

:: Display Power Saving
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\NVTweak"" /v ""DisplayPowerSaving"" /f
reg delete ""HKLM\SYSTEM\ControlSet001\Services\nvlddmkm\Global\NVTweak"" /v ""DisplayPowerSaving"" /f
reg delete ""HKLM\SOFTWARE\NVIDIA Corporation\Global\NVTweak"" /v ""DisplayPowerSaving"" /f

:: Telemetry registry
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\Startup"" /v ""SendTelemetryData"" /f
reg delete ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v ""EnableRID44231"" /f
reg delete ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v ""EnableRID64640"" /f
reg delete ""HKLM\SOFTWARE\NVIDIA Corporation\Global\FTS"" /v ""EnableRID66610"" /f

:: NVIDIA Control Panel
reg delete ""HKLM\SOFTWARE\NVIDIA Corporation\NvControlPanel2\Client"" /v ""OptInOrOutPreference"" /f

:: Notifications
reg delete ""HKCU\SOFTWARE\NVIDIA Corporation\Global\GFExperience"" /v ""NotifyNewDisplayUpdates"" /f

:: Tray icon
reg delete ""HKLM\SOFTWARE\NVIDIA Corporation\NvTray"" /v ""StartOnLogin"" /f
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\NVTweak"" /v ""HideXGpuTrayIcon"" /f

echo.

echo Restoring NVIDIA services...

sc config NvTelemetryContainer start= demand >nul 2>&1

echo Restoring scheduled tasks...

for %%i in (
    ""NvTmRep_CrashReport1""
    ""NvTmRep_CrashReport2""
    ""NvTmRep_CrashReport3""
    ""NvTmRep_CrashReport4""
) do (
    schtasks /Change /Enable /TN ""%%i_{{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}}"" >nul 2>&1
)

for %%i in (
    ""NvTmMon""
    ""NvTmRep""
    ""NvProfile""
    ""NvNodeLauncher""
    ""NvDriverUpdateCheckDaily""
    ""NvBatteryBoostCheckOnLogon""
    ""NVIDIA GeForce Experience SelfUpdate""
) do (
    schtasks /Change /Enable /TN %%i >nul 2>&1
)"

        },
          
          new EmbeddedTweak
          {
            Key = "Strip Nvidia Driver",
            Name = "Strips Nvidia Driver",
            Tags = "Finished,FeatureBreaking",
            Description = "Strips the Nvidia driver of unnecessary features, telemetry, and bloatware.",
            RequiresAdmin = true,
            ApplyBatch = $@"@echo off
sc config NVDisplay.ContainerLocalSystem start= disabled
sc stop NVDisplay.ContainerLocalSystem

reg add ""HKCU\SOFTWARE\NVIDIA Corporation\NVControlPanel2\Client"" /v ""OptInOrOutPreference"" /t REG_DWORD /d 0 /f

reg add ""HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Global\Startup\SendTelemetryData"" /ve /t REG_DWORD /d 0 /f


for /d %%D in (""C:\Windows\System32\DriverStore\FileRepository\nv_dispsi.inf_amd64_*"") do if exist ""%%D"" (

    takeown /r /d Y /f ""%%D"" >nul 2>&1
    icacls ""%%D"" /grant Administrators:F /t >nul 2>&1

del /f /q ""%%D\nvFBC.dll""
del /f /q ""%%D\nvFBC64.dll""
del /f /q ""%%D\nvIFR.dll""
del /f /q ""%%D\nvIFR64.dll""
del /f /q ""%%D\MCU.exe""
del /f /q ""%%D\nvdispco64.exe""
del /f /q ""%%D\nvsvs.dll""
del /f /q ""%%D\nvsvsr.dll""
del /f /q ""%%D\nvsvc64.dll""
del /f /q ""%%D\nvsvcr.dll""
del /f /q ""%%D\nvwss.dll""
del /f /q ""%%D\nvwssr.dll""
del /f /q ""%%D\nvdebugdump.exe""
del /f /q ""%%D\nvcudadebugger.dll""
del /f /q ""%%D\libcudadebugger.so.1""
del /f /q ""%%D\nvsmartmaxapp64.exe""
del /f /q ""%%D\nvsmartmax64.dll""
del /f /q ""%%D\nvsmartmaxapp.exe""
del /f /q ""%%D\nvsmartmax.dll""
del /f /q ""%%D\nvIccAdvancedColorIdentity.icm""
del /f /q ""%%D\nvEncMFThevcx.dll""
del /f /q ""%%D\nvEncMFTH264x.dll""
del /f /q ""%%D\nvEncMFThevc.dll""
del /f /q ""%%D\nvEncMFTH264.dll.dll""
del /f /q ""%%D\nvEncMFTav1x.dll""
del /f /q ""%%D\nvEncMFTav1.dll""
del /f /q ""%%D\nvDecMFTMjpegx.dll.dll""
del /f /q ""%%D\nvDecMFTMjpeg.dll""
del /f /q ""%%D\nvOSC.exe""
del /f /q ""%%D\nvppex.dll""
del /f /q ""%%D\nvppe.dll""
del /f /q ""%%D\nvAIVPx.dll""
del /f /q ""%%D\nvAIHDRx.dll""
del /f /q ""%%D\nvAIHDR.dll""
del /f /q ""%%D\nvAIDVCx.dll""
del /f /q ""%%D\nvAIDVC.dll""
del /f /q ""%%D\NvPresent64.dll""
del /f /q ""%%D\libnvwgf2umx.so""
del /f /q ""%%D\libnvoptix_loader.so.1""
del /f /q ""%%D\libnvidia-ptxjitcompiler.so.1""
del /f /q ""%%D\libnvidia-opticalflow.so.1""
del /f /q ""%%D\libnvidia-nvvm70.so.4""
del /f /q ""%%D\libnvidia-nvvm.so.4""
del /f /q ""%%D\libnvidia-ngx.so.1""
del /f /q ""%%D\libnvidia-ml_loader.so""
del /f /q ""%%D\libnvidia-ml.so.1""
del /f /q ""%%D\libnvdxgdmal.so.1""
del /f /q ""%%D\libnvidia-gpucomp.so""
del /f /q ""%%D\libnvidia-encode.so.1""
del /f /q ""%%D\libnvdxdlkernels.so""
del /f /q ""%%D\libnvcuvid.so.1""
del /f /q ""%%D\libcuda_loader.so""
del /f /q ""%%D\libcudadebugger.so.1""
del /f /q ""%%D\libcuda.so.1.1""


)


if exist ""%%D\Display.NvContainer"" (

    takeown /r /d Y /f ""%%D\Display.NvContainer"" >nul 2>&1
    icacls ""%%D\Display.NvContainer"" /grant Administrators:F /t >nul 2>&1

    del /s /f /q ""%%D\Display.NvContainer\NvContainerRecovery.bat""
    del /s /f /q ""%%D\Display.NvContainer\NvMessageBus.dll""
    del /s /f /q ""%%D\Display.NvContainer\nvgwls.exe""

    if exist ""%%D\Display.NvContainer\plugins\LocalSystem"" (
        del /s /f /q ""%%D\Display.NvContainer\plugins\LocalSystem\_NvMsgBusBroadcast.dll""
    )

    if exist ""%%D\Display.NvContainer\plugins\Session"" (
        del /s /f /q ""%%D\Display.NvContainer\plugins\Session\_NvGSTPlugin.dll""
        del /s /f /q ""%%D\Display.NvContainer\plugins\Session\nvprofileupdaterplugin.dll""
    )
)


if exist ""%%D\NVWMI"" (
    takeown /f ""%%D\NVWMI"" /R /D Y >nul 2>&1
    icacls ""%%D\NVWMI"" /grant Administrators:F /t >nul 2>&1
    rmdir /s /q ""%%D\NVWMI""
)


if exist ""%%D\NvCamera"" (
    takeown /f ""%%D\NvCamera"" /R /D Y >nul 2>&1
    icacls ""%%D\NvCamera"" /grant Administrators:F /t >nul 2>&1
    rmdir /s /q ""%%D\NvCamera""
)


del /a /s /f ""C:\Windows\System32\DriverStore\FileRepository\NvTelemetry64.dll""


reg delete ""HKLM\System\ControlSet001\Services\nvlddmkm\NvCamera"" /f


sc config NVDisplay.ContainerLocalSystem start= auto
sc start NVDisplay.ContainerLocalSystem",
          RevertBatch = $@"@echo off"
        },

        new EmbeddedTweak
            {
              Key = "General Graphics Tweaks",
              Name = "Tweaks your GPU",
              Tags = "Finished",
              Description = "These basic tweaks improve GPU performance and stability.",
              RequiresAdmin = true,
              ApplyBatch = $@"@echo off
set ""graphics_key=HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers""

reg add ""%graphics_key%"" /v ""TdrLevel""     /t REG_DWORD /d 0 /f >nul 2>&1
reg add ""%graphics_key%"" /v ""TdrDebugMode"" /t REG_DWORD /d 1 /f >nul 2>&1
reg add ""%graphics_key%"" /v ""DisableBadDriverCheckForHwProtection"" /t REG_DWORD /d 1 /f >nul 2>&1

reg add ""%graphics_key%"" /v EnableRuntimePowerManagement /t reg_dword /d 0 /f >nul
reg add ""%graphics_key%"" /v DisableBoostedVSyncVirtualization /t reg_dword /d 1 /f >nul",
              RevertBatch = $@"@echo off
set ""graphics_key=HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers""

reg delete ""%graphics_key%"" /v ""TdrLevel"" /f >nul 2>&1
reg delete ""%graphics_key%"" /v ""TdrDebugMode"" /f >nul 2>&1
reg delete ""%graphics_key%"" /v ""DisableBadDriverCheckForHwProtection"" /f >nul 2>&1

reg delete ""%graphics_key%"" /v ""EnableRuntimePowerManagement"" /f >nul 2>&1
reg delete ""%graphics_key%"" /v ""DisableBoostedVSyncVirtualization"" /f >nul 2>&1"
            },

            new EmbeddedTweak
            {
                Key = "AMD GPU Optimization",
                Name = "Optimize AMD GPU",
                Tags = "Finished",
                Description = "Optimizes AMD GPU settings for improved performance, latency, and speed. This tweak may increase temperatures on poorly cooled PCs.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
for /f ""tokens=*"" %c in (
	'reg query ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{{4d36e968-e325-11ce-bfc1-08002be10318}}"" /f ""Radeon"" /t REG_SZ /s 2^>nul ^| findstr /l ""}}""
) do (
	set gpu_key=%c
)

:: Disable Sleep
reg add ""%gpu_key%"" /v ""DisableGfxCoarseGrainLightSleep"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGfxCpLightSleep"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGfxMediumGrainLightSleep"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGfxRlcLightSleep"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableDrmLightSleep"" /t REG_DWORD /d 1 /f > nul 2>&1

:: Disable ULPS
reg add ""%gpu_key%"" /v ""PP_EnableCFULPSPolicy"" /t reg_dword /d 0 /f >nul
reg add ""%gpu_key%"" /v ""EnableUlps"" /t reg_dword /d 0 /f >nul

:: Disable Radeon Boost
reg add ""%gpu_key%"" /v ""KMD_RadeonBoostEnabled"" /t reg_dword /d 0 /f > nul 2>&1

:: Disable Clock Gating
reg add ""%gpu_key%"" /v ""DisableGfx3DCGLS"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGfxCGTS"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGfxCGTS_LS"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGfxMGCGPerfMon"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableDrmdmaMGCG"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableDrmMGCG"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGfx3DCGCG"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""EnableUvdClockGating"" /t reg_dword /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""EnableVceSwClockGating"" /t reg_dword /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""EnableGfxClockGatingThruSmu"" /t reg_dword /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""EnableSysClockGatingThruSmu"" /t reg_dword /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""IRQMgrDisableIHClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""swGcClockGatingMask"" /t reg_dword /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""swGcClockGatingOverride"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableRomMediumGrainClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableRomMGCGClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableSamuClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableSysClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableVceClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGfxCoarseGrainClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGfxMediumGrainClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableMcMediumGrainClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableNbioMediumGrainClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGfxClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DalDisableClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DalFineGrainClockGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableAllClockGating"" /t reg_dword /d 1 /f > nul 2>&1

:: Disable Power Gating
reg add ""%gpu_key%"" /v ""DisableGfxPGCondClearStateWA"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableCpPowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableAcpPowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableDrmdmaPowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableDynamicGfxMGPowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGDSPowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGfxCGPowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableGFXPipelinePowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableUVDPowerGatingDynamic"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisablePowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableQuickGfxMGPowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableSAMUPowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableStaticGfxMGPowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableUVDPowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableVCEPowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableXdmaPowerGating"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableXdmaSclkGating"" /t reg_dword /d 1 /f > nul 2>&1

:: Disable Powerdown
reg add ""%gpu_key%"" /v ""DalPSRSkipCRTCPowerDown"" /t reg_dword /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""PP_GPUPowerDownEnabled"" /t reg_dword /d 0 /f > nul 2>&1

:: Disable ASPM
reg add ""%gpu_key%"" /v ""DisableAspmSWL1"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableAspmL0s"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""DisableAspmL1"" /t reg_dword /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""EnableAspmL0s"" /t reg_dword /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""EnableAspmL1"" /t reg_dword /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""EnableAspmL1SS"" /t reg_dword /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""AspmL0sTimeout"" /t reg_dword /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""AspmL1Timeout"" /t reg_dword /d 0 /f > nul 2>&1

:: Enable Performance Mode
reg add ""%gpu_key%"" /v ""PP_Force3DPerformanceMode"" /t REG_DWORD /d 1 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""PP_ForceHighDPMLevel"" /t REG_DWORD /d 1 /f > nul 2>&1

:: Disable ClkReq
reg add ""%gpu_key%"" /v ""DisableClkReqSupport"" /t REG_DWORD /d 1 /f > nul 2>&1

:: Disable FBC
reg add ""%gpu_key%"" /v ""DisableFBCSupport"" /t REG_DWORD /d 1 /f > nul 2>&1

:: Disable UvD
reg add ""%gpu_key%"" /v ""DisableForceUvdToSclk"" /t REG_DWORD /d 1 /f > nul 2>&1

:: Configure Downgrade
reg add ""%gpu_key%"" /v ""PipeTilingDowngrade"" /t REG_DWORD /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""GroupSizeDowngrade"" /t REG_DWORD /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""RowTilingDowngrade"" /t REG_DWORD /d 0 /f > nul 2>&1
reg add ""%gpu_key%"" /v ""SampleSplitDowngrade"" /t REG_DWORD /d 0 /f > nul 2>&1

:: Disable Services
reg add ""HKLM\System\CurrentControlSet\Services\amdwddmg"" /v ""ChillEnabled"" /t REG_DWORD /d 0 /f > nul 2>&1
reg add ""HKLM\System\CurrentControlSet\Services\AMD Crash Defender Service"" /v ""Start"" /t REG_DWORD /d 4 /f > nul 2>&1
reg add ""HKLM\System\CurrentControlSet\Services\AMD External Events Utility"" /v ""Start"" /t REG_DWORD /d 4 /f > nul 2>&1
reg add ""HKLM\System\CurrentControlSet\Services\amdfendr"" /v ""Start"" /t REG_DWORD /d 4 /f > nul 2>&1
reg add ""HKLM\System\CurrentControlSet\Services\amdfendrmgr"" /v ""Start"" /t REG_DWORD /d 4 /f > nul 2>&1
reg add ""HKLM\System\CurrentControlSet\Services\amdlog"" /v ""Start"" /t REG_DWORD /d 4 /f > nul 2>&1

:: Disable Auto Update
reg add ""HKCU\Software\AMD\CN"" /v ""AutoUpdateTriggered"" /t REG_DWORD /d 0 /f > nul 2>&1
reg add ""HKCU\Software\AMD\CN"" /v ""AutoUpdate"" /t REG_DWORD /d 0 /f > nul 2>&1

:: Disable Animations
reg add ""HKCU\Software\AMD\CN"" /v ""AnimationEffect"" /t REG_SZ /d ""false"" /f > nul 2>&1",
              RevertBatch = $@"@echo off
for /f ""tokens=*"" %c in ('
    reg query ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{{4d36e968-e325-11ce-bfc1-08002be10318}}"" /f ""Radeon"" /t REG_SZ /s 2^>nul ^| findstr /l ""}}""
') do (
    set ""gpu_key=%c""
)

for %V in (
    EnableRuntimePowerManagement
    DisableGfxCoarseGrainLightSleep
    DisableGfxCpLightSleep
    DisableGfxMediumGrainLightSleep
    DisableGfxRlcLightSleep
    DisableDrmLightSleep
    PP_EnableCFULPSPolicy
    EnableUlps
    KMD_RadeonBoostEnabled
    DisableGfx3DCGLS
    DisableGfxCGTS
    DisableGfxCGTS_LS
    DisableGfxMGCGPerfMon
    DisableDrmdmaMGCG
    DisableDrmMGCG
    DisableGfx3DCGCG
    EnableUvdClockGating
    EnableVceSwClockGating
    EnableGfxClockGatingThruSmu
    EnableSysClockGatingThruSmu
    IRQMgrDisableIHClockGating
    swGcClockGatingMask
    swGcClockGatingOverride
    DisableRomMediumGrainClockGating
    DisableRomMGCGClockGating
    DisableSamuClockGating
    DisableSysClockGating
    DisableVceClockGating
    DisableGfxCoarseGrainClockGating
    DisableGfxMediumGrainClockGating
    DisableMcMediumGrainClockGating
    DisableNbioMediumGrainClockGating
    DisableGfxClockGating
    DalDisableClockGating
    DalFineGrainClockGating
    DisableAllClockGating
    DisableGfxPGCondClearStateWA
    DisableCpPowerGating
    DisableAcpPowerGating
    DisableDrmdmaPowerGating
    DisableDynamicGfxMGPowerGating
    DisableGDSPowerGating
    DisableGfxCGPowerGating
    DisableGFXPipelinePowerGating
    DisableUVDPowerGatingDynamic
    DisablePowerGating
    DisableQuickGfxMGPowerGating
    DisableSAMUPowerGating
    DisableStaticGfxMGPowerGating
    DisableUVDPowerGating
    DisableVCEPowerGating
    DisableXdmaPowerGating
    DisableXdmaSclkGating
    DalPSRSkipCRTCPowerDown
    PP_GPUPowerDownEnabled
    DisableAspmSWL1
    DisableAspmL0s
    DisableAspmL1
    EnableAspmL0s
    EnableAspmL1
    EnableAspmL1SS
    AspmL0sTimeout
    AspmL1Timeout
    PP_Force3DPerformanceMode
    PP_ForceHighDPMLevel
    DisableClkReqSupport
    DisableFBCSupport
    DisableForceUvdToSclk
    PipeTilingDowngrade
    GroupSizeDowngrade
    RowTilingDowngrade
    SampleSplitDowngrade
) do (
    reg delete ""%gpu_key%"" /v ""%V"" /f >nul 2>&1
)

:: Restore AMD Adrenalin settings
reg delete ""HKCU\Software\AMD\CN"" /v AutoUpdateTriggered /f >nul 2>&1
reg delete ""HKCU\Software\AMD\CN"" /v AutoUpdate /f >nul 2>&1
reg delete ""HKCU\Software\AMD\CN"" /v AnimationEffect /f >nul 2>&1

:: Restore driver parameter
reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\amdwddmg"" /v ChillEnabled /f >nul 2>&1

:SERVICES
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\AMD Crash Defender Service"" /v Start /t REG_DWORD /d 3 /f >nul
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\AMD External Events Utility"" /v Start /t REG_DWORD /d 2 /f >nul
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\amdfendr"" /v Start /t REG_DWORD /d 3 /f >nul
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\amdfendrmgr"" /v Start /t REG_DWORD /d 2 /f >nul
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\amdlog"" /v Start /t REG_DWORD /d 3 /f >nul
"
            },

            new EmbeddedTweak
            {
                Key = "DWM Optimization",
                Name = "DWM Optimization",
                Tags = "Finished",
                Description = "Optimizes DWM settings for better performance.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
set dwm_key=HKCU\SOFTWARE\Microsoft\Windows\Dwm
reg add ""%dwm_key%"" /v ""EnableAeroPeek"" /t reg_dword /d 0 /f >nul
reg add ""%dwm_key%"" /v ""UseDPIScaling"" /t reg_dword /d 0 /f >nul
reg add ""%dwm_key%"" /v ""AnimationsShiftKey"" /t reg_dword /d 0 /f >nul
reg add ""%dwm_key%"" /v ""ParallelModePolicy"" /t reg_dword /d 1 /f >nul
reg add ""%dwm_key%"" /v ""ResampleModeOverride"" /t reg_dword /d 1 /f >nul
reg add ""%dwm_key%"" /v ""EnableShadow"" /t reg_dword /d 0 /f >nul
reg add ""%dwm_key%"" /v ""EnableCpuClipping"" /t reg_dword /d 0 /f >nul
reg add ""%dwm_key%"" /v ""EnablePrimitiveReordering"" /t reg_dword /d 0 /f >nul",
                RevertBatch = $@"@echo off
set dwm_key=HKCU\SOFTWARE\Microsoft\Windows\Dwm
reg delete ""%dwm_key%"" /v ""EnableAeroPeek"" /t reg_dword /d 0 /f >nul
reg delete ""%dwm_key%"" /v ""UseDPIScaling"" /f >nul
reg delete ""%dwm_key%"" /v ""AnimationsShiftKey"" /f >nul
reg delete ""%dwm_key%"" /v ""ParallelModePolicy"" /f >nul
reg delete ""%dwm_key%"" /v ""EnableCpuClipping"" /f >nul
reg delete ""%dwm_key%"" /v ""EnablePrimitiveReordering"" /f >nul
reg delete ""%dwm_key%"" /v ""ResampleModeOverride"" /f >nul
reg delete ""%dwm_key%"" /v ""EnableShadow"" /f >nul"
            },

            new EmbeddedTweak
            {
                Key = "GPU Preferences Optimization",
            Tags = "Finished",
            Description = "Allocate effectively GPU ressources for system.",
            RequiresAdmin = true,
            ApplyBatch = $@"@echo off
EnableDelayedExpansion

reg add ""HKCU\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""C:\Windows\System32\dwm.exe"" /t REG_SZ /d ""GpuPreference=2;SwapEffectUpgradeEnable=1;"" /f

reg add ""HKCU\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""C:\Windows\explorer.exe"" /t REG_SZ /d ""GpuPreference=1;"" /f

for %%P in (
""C:\Program Files (x86)\Microsoft\EdgeWebView\Application""
""C:\Program Files\Microsoft\EdgeWebView\Application""
) do (
    for /d %%G in (""%%P\*"") do (
        if exist ""%%G\msedgewebview2.exe"" (
            reg add ""HKCU\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""%%G\msedgewebview2.exe"" /t REG_SZ /d ""GpuPreference=1;"" /f
        )
    )
)

for /d %%G in (""C:\Windows\SystemApps\Microsoft.Windows.StartMenuExperienceHost_*"") do (
    if exist ""%%G\StartMenuExperienceHost.exe"" (
        reg add ""HKCU\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""%%G\StartMenuExperienceHost.exe"" /t REG_SZ /d ""GpuPreference=1;"" /f
    )
)

for /d %%G in (""C:\Windows\SystemApps\MicrosoftWindows.Client.CBS_*"") do (
    if exist ""%%G\MicrosoftWindows.Client.CBS.exe"" (
        reg add ""HKCU\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""%%G\MicrosoftWindows.Client.CBS.exe"" /t REG_SZ /d ""GpuPreference=1;"" /f
    )
)",
            RevertBatch = $@"@echo off
:: Revert DWM
reg delete ""HKCU\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""C:\Windows\System32\dwm.exe"" /f

:: Revert Explorer
reg delete ""HKCU\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""C:\Windows\explorer.exe"" /f

:: Revert Edge WebView2
for %%P in (
""C:\Program Files (x86)\Microsoft\EdgeWebView\Application""
""C:\Program Files\Microsoft\EdgeWebView\Application""
) do (
    for /d %%G in (""%%P\*"") do (
        if exist ""%%G\msedgewebview2.exe"" (
            reg delete ""HKCU\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""%%G\msedgewebview2.exe"" /f
        )
    )
)

:: Revert Start Menu Experience Host
for /d %%G in (""C:\Windows\SystemApps\Microsoft.Windows.StartMenuExperienceHost_*"") do (
    if exist ""%%G\StartMenuExperienceHost.exe"" (
        reg delete ""HKCU\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""%%G\StartMenuExperienceHost.exe"" /f
    )
)

:: Revert Windows Client CBS
for /d %%G in (""C:\Windows\SystemApps\MicrosoftWindows.Client.CBS_*"") do (
    if exist ""%%G\MicrosoftWindows.Client.CBS.exe"" (
        reg delete ""HKCU\Software\Microsoft\DirectX\UserGpuPreferences"" /v ""%%G\MicrosoftWindows.Client.CBS.exe"" /f
    )
)"
           }
        };
    }
}
