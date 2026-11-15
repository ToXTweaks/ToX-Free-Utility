using System.Collections.Generic;

namespace ToX_Free_Utility_NEW_ERA.Tweaks
{
    public static class PeripheralsTweaks
    {
        public static List<EmbeddedTweak> All => new List<EmbeddedTweak>
        {
            new EmbeddedTweak
            {
                Key = "Mouse and Keyboard",
                Name = "Tweak Mouse and Keyboard",
                Tags = "Finished,SafeToUse",
                Description = "Optimizes mouse and keyboard settings for faster input response and lower latency. Disables mouse acceleration, reduces menu/hover delays to 0ms, sets fastest keyboard repeat rate, disables mouse trails and USB idle power management. Note: Mouse will feel faster and more direct. May take time to adjust if you are used to acceleration.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
reg add ""HKCU\Control Panel\Mouse"" /v MouseSpeed /t REG_SZ /d ""0"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseThreshold1 /t REG_SZ /d ""0"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseThreshold2 /t REG_SZ /d ""0"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseSpeed /t REG_DWORD /d 0 /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseThreshold1 /t REG_DWORD /d 0 /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseThreshold2 /t REG_DWORD /d 0 /f

reg add ""HKCU\Control Panel\Mouse"" /v MouseHoverTime /t REG_SZ /d ""0"" /f
reg add ""HKCU\Control Panel\Desktop"" /v MenuShowDelay /t REG_SZ /d ""0"" /f

reg add ""HKCU\Control Panel\Keyboard"" /v InitialKeyboardIndicators /t REG_SZ /d ""2"" /f

reg add ""HKCU\Control Panel\Accessibility\Keyboard Response"" /v DelayBeforeAcceptance /t REG_SZ /d ""0"" /f
reg add ""HKCU\Control Panel\Accessibility\StickyKeys"" /v Flags /t REG_SZ /d ""506"" /f

reg add ""HKEY_CURRENT_USER\Control Panel\Keyboard"" /v InitialKeyboardIndicators /t REG_SZ /d ""2"" /f
reg add ""HKEY_CURRENT_USER\Control Panel\Keyboard"" /v KeyboardDelay /t REG_SZ /d ""0"" /f
reg add ""HKEY_CURRENT_USER\Control Panel\Keyboard"" /v KeyboardSpeed /t REG_SZ /d ""31"" /f

reg add ""HKEY_CURRENT_USER\Control Panel\Mouse"" /v ActiveWindowTracking /t REG_DWORD /d 0 /f
reg add ""HKEY_CURRENT_USER\Control Panel\Mouse"" /v Beep /t REG_SZ /d ""No"" /f
reg add ""HKEY_CURRENT_USER\Control Panel\Mouse"" /v DoubleClickHeight /t REG_SZ /d ""4"" /f
reg add ""HKEY_CURRENT_USER\Control Panel\Mouse"" /v DoubleClickSpeed /t REG_SZ /d ""550"" /f
reg add ""HKEY_CURRENT_USER\Control Panel\Mouse"" /v DoubleClickWidth /t REG_SZ /d ""4"" /f
reg add ""HKEY_CURRENT_USER\Control Panel\Mouse"" /v ExtendedSounds /t REG_SZ /d ""No"" /f
reg add ""HKEY_CURRENT_USER\Control Panel\Mouse"" /v MouseHoverHeight /t REG_SZ /d ""4"" /f
reg add ""HKEY_CURRENT_USER\Control Panel\Mouse"" /v MouseHoverWidth /t REG_SZ /d ""4"" /f
reg add ""HKEY_CURRENT_USER\Control Panel\Mouse"" /v MouseSensitivity /t REG_SZ /d ""10"" /f
reg add ""HKEY_CURRENT_USER\Control Panel\Mouse"" /v MouseTrails /t REG_SZ /d ""0"" /f
reg add ""HKEY_CURRENT_USER\Control Panel\Mouse"" /v SnapToDefaultButton /t REG_SZ /d ""0"" /f
reg add ""HKEY_CURRENT_USER\Control Panel\Mouse"" /v SwapMouseButtons /t REG_SZ /d ""0"" /f

reg add ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\usbflags"" /v DisableHCS0Idle /t REG_DWORD /d 1 /f",
                RevertBatch = $@"@echo off
reg add ""HKCU\Control Panel\Mouse"" /v MouseSpeed /t REG_SZ /d ""1"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseThreshold1 /t REG_SZ /d ""6"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseThreshold2 /t REG_SZ /d ""10"" /f

reg delete ""HKCU\Control Panel\Mouse"" /v MouseSpeed /f
reg delete ""HKCU\Control Panel\Mouse"" /v MouseThreshold1 /f
reg delete ""HKCU\Control Panel\Mouse"" /v MouseThreshold2 /f

reg add ""HKCU\Control Panel\Mouse"" /v MouseHoverTime /t REG_SZ /d ""400"" /f
reg add ""HKCU\Control Panel\Desktop"" /v MenuShowDelay /t REG_SZ /d ""400"" /f

reg add ""HKCU\Control Panel\Keyboard"" /v InitialKeyboardIndicators /t REG_SZ /d ""0"" /f
reg add ""HKCU\Control Panel\Keyboard"" /v KeyboardDelay /t REG_SZ /d ""1"" /f
reg add ""HKCU\Control Panel\Keyboard"" /v KeyboardSpeed /t REG_SZ /d ""31"" /f

reg add ""HKCU\Control Panel\Accessibility\Keyboard Response"" /v DelayBeforeAcceptance /t REG_SZ /d ""1000"" /f
reg add ""HKCU\Control Panel\Accessibility\StickyKeys"" /v Flags /t REG_SZ /d ""510"" /f

reg add ""HKCU\Control Panel\Mouse"" /v ActiveWindowTracking /t REG_DWORD /d 0 /f
reg add ""HKCU\Control Panel\Mouse"" /v Beep /t REG_SZ /d ""Yes"" /f
reg add ""HKCU\Control Panel\Mouse"" /v DoubleClickHeight /t REG_SZ /d ""4"" /f
reg add ""HKCU\Control Panel\Mouse"" /v DoubleClickSpeed /t REG_SZ /d ""500"" /f
reg add ""HKCU\Control Panel\Mouse"" /v DoubleClickWidth /t REG_SZ /d ""4"" /f
reg add ""HKCU\Control Panel\Mouse"" /v ExtendedSounds /t REG_SZ /d ""Yes"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseHoverHeight /t REG_SZ /d ""4"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseHoverTime /t REG_SZ /d ""400"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseHoverWidth /t REG_SZ /d ""4"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseSensitivity /t REG_SZ /d ""10"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseSpeed /t REG_SZ /d ""1"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseThreshold1 /t REG_SZ /d ""6"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseThreshold2 /t REG_SZ /d ""10"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseTrails /t REG_SZ /d ""0"" /f
reg add ""HKCU\Control Panel\Mouse"" /v SnapToDefaultButton /t REG_SZ /d ""0"" /f
reg add ""HKCU\Control Panel\Mouse"" /v SwapMouseButtons /t REG_SZ /d ""0"" /f
reg delete ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\usbflags"" /v DisableHCS0Idle /f"
            },

            new EmbeddedTweak
            {
                Key = "Network Optimization",
                Name = "Optimizing Network",
                Tags = "FeatureBreaking,Finished",
                Description = "Aggressively optimizes your active network adapter for lower latency gaming traffic. Disables: IPv6, LLDP, Hyper-V bindings, NetBIOS, power saving on NIC, Energy Efficient Ethernet, Green Ethernet, flow control, wake-on-LAN. Tunes TCP settings and Intel/Realtek driver buffers. Briefly restarts your adapter. Warning: Disables IPv6, network discovery, and wake-on-LAN. May break Hyper-V networking, printer sharing, and some corporate networks. Your internet will disconnect briefly during apply.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
setlocal EnableDelayedExpansion

powershell -NoProfile -Command ""Get-WmiObject MSPower_DeviceEnable -Namespace root\wmi | ForEach-Object {{ $$_.Enable = $$false; $$_.psbase.put() }}"" >nul 2>&1

set ""ActiveNIC=""
for /f ""delims="" %%I in ('powershell -NoProfile -Command ""(Get-NetAdapter | Where-Object Status -eq 'Up' | Select-Object -First 1).Name""') do set ""ActiveNIC=%%I""

if ""!ActiveNIC!""=="""" goto :done

powershell -NoProfile -ExecutionPolicy Bypass -Command ""$$nic='!ActiveNIC!'; $$ids=@('ms_lldp','ms_rspndr','ms_lltdio','ms_hyperv','vmware_bridge','ms_msclient','ms_server','ms_tcpip6','ms_implat'); foreach($$id in $$ids){{ Disable-NetAdapterBinding -Name $$nic -ComponentID $$id -ErrorAction SilentlyContinue }}""

netsh interface ip delete arpcache >nul 2>&1
nbtstat -R >nul 2>&1
ipconfig /flushdns >nul 2>&1

netsh int tcp set global autotuninglevel=normal >nul 2>&1
netsh int tcp set global rss=enabled >nul 2>&1
netsh int tcp set global rsc=enabled >nul 2>&1
netsh int tcp set global prr=enabled >nul 2>&1
netsh int tcp set global hystart=enabled >nul 2>&1
netsh int tcp set global fastopen=enabled >nul 2>&1
netsh int tcp set global timestamps=disabled >nul 2>&1
netsh int tcp set global nonsackrttresiliency=disabled >nul 2>&1
netsh int tcp set global pacingprofile=disabled >nul 2>&1
netsh int tcp set global ecncapability=disabled >nul 2>&1

set ""adapterKey=""
for /f ""delims="" %%K in ('reg query ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{{4d36e972-e325-11ce-bfc1-08002be10318}}"" /s ^| findstr /r ""\\[0-9][0-9][0-9][0-9]$""') do (
    reg query ""%%K"" /v DriverDesc 2>nul | find /i ""Intel"" >nul && (
        set ""adapterKey=%%K""
        goto :intel
    )
    reg query ""%%K"" /v DriverDesc 2>nul | find /i ""Realtek"" >nul && (
        set ""adapterKey=%%K""
        goto :realtek
    )
)

goto :done

:intel
reg add ""!adapterKey!"" /v ""*SpeedDuplex"" /t REG_SZ /d ""6"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*RSS"" /t REG_SZ /d ""1"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*NumRssQueues"" /t REG_SZ /d ""2"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*ReceiveBuffers"" /t REG_SZ /d ""512"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*TransmitBuffers"" /t REG_SZ /d ""512"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*DeviceSleepOnDisconnect"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*FlowControl"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*IPChecksumOffloadIPv4"" /t REG_SZ /d ""3"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*InterruptModeration"" /t REG_SZ /d ""1"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*TCPChecksumOffloadIPv4"" /t REG_SZ /d ""3"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*TCPChecksumOffloadIPv6"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*UDPChecksumOffloadIPv4"" /t REG_SZ /d ""3"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*UDPChecksumOffloadIPv6"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*WakeOnMagicPacket"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*WakeOnPattern"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""SipsEnabled"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""WakeOnFastStartup"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""SSIdleTimeoutMS"" /t REG_SZ /d ""0"" /f >nul 2>&1
goto :done

:realtek
reg add ""!adapterKey!"" /v ""*SpeedDuplex"" /t REG_SZ /d ""6"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*RSS"" /t REG_SZ /d ""1"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*NumRssQueues"" /t REG_SZ /d ""2"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*ReceiveBuffers"" /t REG_SZ /d ""512"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*TransmitBuffers"" /t REG_SZ /d ""512"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*DeviceSleepOnDisconnect"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*FlowControl"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*IPChecksumOffloadIPv4"" /t REG_SZ /d ""3"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*InterruptModeration"" /t REG_SZ /d ""1"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*TCPChecksumOffloadIPv4"" /t REG_SZ /d ""3"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*TCPChecksumOffloadIPv6"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*UDPChecksumOffloadIPv4"" /t REG_SZ /d ""3"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*UDPChecksumOffloadIPv6"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*WakeOnMagicPacket"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""*WakeOnPattern"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""ASPM"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""CLKREQ"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""EEEPlus"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""EnableAspm"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""DynamicLTR"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""EnableGreenEthernet"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""GigaLite"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""PowerDownPll"" /t REG_SZ /d ""0"" /f >nul 2>&1
reg add ""!adapterKey!"" /v ""PowerSavingMode"" /t REG_SZ /d ""0"" /f >nul 2>&1

:done
if not ""!ActiveNIC!""=="""" (
    netsh interface set interface ""!ActiveNIC!"" admin=disable >nul 2>&1
    netsh interface set interface ""!ActiveNIC!"" admin=enable >nul 2>&1
)",
            RevertBatch = $@"@echo off
setlocal EnableDelayedExpansion

powershell -NoProfile -Command ""Get-WmiObject MSPower_DeviceEnable -Namespace root\wmi | ForEach-Object {{ $$_.Enable = $$true; $$_.psbase.Put() }}"" >nul 2>&1

set ""ActiveNIC=""
for /f ""delims="" %%I in ('powershell -NoProfile -Command ""(Get-NetAdapter | Where-Object Status -eq 'Up' | Select-Object -First 1).Name""') do set ""ActiveNIC=%%I""

if ""!ActiveNIC!""=="""" goto :restart

powershell -NoProfile -ExecutionPolicy Bypass -Command ""$$nic='!ActiveNIC!'; $$ids=@('ms_lldp','ms_rspndr','ms_lltdio','ms_hyperv','vmware_bridge','ms_msclient','ms_server','ms_tcpip6','ms_implat'); foreach($$id in $$ids){{ Enable-NetAdapterBinding -Name $$nic -ComponentID $$id -ErrorAction SilentlyContinue }}""

netsh int tcp set global autotuninglevel=normal >nul 2>&1
netsh int tcp set global rss=enabled >nul 2>&1
netsh int tcp set global rsc=enabled >nul 2>&1
netsh int tcp set global prr=enabled >nul 2>&1
netsh int tcp set global hystart=enabled >nul 2>&1
netsh int tcp set global fastopen=enabled >nul 2>&1
netsh int tcp set global timestamps=disabled >nul 2>&1
netsh int tcp set global nonsackrttresiliency=disabled >nul 2>&1
netsh int tcp set global pacingprofile=off >nul 2>&1
netsh int tcp set global ecncapability=disabled >nul 2>&1

set ""adapterKey=""
for /f ""delims="" %%K in ('reg query ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{{4d36e972-e325-11ce-bfc1-08002be10318}}"" /s ^| findstr /r ""\\[0-9][0-9][0-9][0-9]$""') do (
    reg query ""%%K"" /v DriverDesc 2>nul | find /i ""Intel"" >nul && (
        set ""adapterKey=%%K""
        goto :found
    )
    reg query ""%%K"" /v DriverDesc 2>nul | find /i ""Realtek"" >nul && (
        set ""adapterKey=%%K""
        goto :found
    )
)

goto :restart

:found
for %%V in (
""*SpeedDuplex""
""*RSS""
""*ReceiveBuffers""
""*TransmitBuffers""
""*DeviceSleepOnDisconnect""
""*FlowControl""
""*IPChecksumOffloadIPv4""
""*InterruptModeration""
""*NumRssQueues""
""*TCPChecksumOffloadIPv4""
""*TCPChecksumOffloadIPv6""
""*UDPChecksumOffloadIPv4""
""*UDPChecksumOffloadIPv6""
""*WakeOnMagicPacket""
""*WakeOnPattern""
""SipsEnabled""
""WakeOnFastStartup""
""SSIdleTimeoutMS""
""ASPM""
""CLKREQ""
""EEEPlus""
""EnableAspm""
""DynamicLTR""
""EnableGreenEthernet""
""GigaLite""
""PowerDownPll""
""PowerSavingMode""
) do (
    reg delete ""!adapterKey!"" /v %%~V /f >nul 2>&1
)

:restart
if not ""!ActiveNIC!""=="""" (
    netsh interface set interface ""!ActiveNIC!"" admin=disable >nul 2>&1
    timeout /t 2 >nul 2>&1
    netsh interface set interface ""!ActiveNIC!"" admin=enable >nul 2>&1
)"
           },

            new EmbeddedTweak
            {
                Key = "Disable Mouse Acceleration",
                Name = "Disable Mouse Acceleration",
                Tags = "Finished,SafeToUse",
                Description = "Turns off Enhance Pointer Precision so mouse movement stays 1:1. Disables MouseSpeed and both mouse thresholds. Note: The pointer will feel more direct. Your DPI and in-game sensitivity stay the same.",
                RequiresAdmin = true,
                ApplyBatch = @"@echo off
reg add ""HKCU\Control Panel\Mouse"" /v MouseSpeed /t REG_SZ /d ""0"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseThreshold1 /t REG_SZ /d ""0"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseThreshold2 /t REG_SZ /d ""0"" /f
exit /b 0",
                RevertBatch = @"@echo off
reg add ""HKCU\Control Panel\Mouse"" /v MouseSpeed /t REG_SZ /d ""1"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseThreshold1 /t REG_SZ /d ""6"" /f
reg add ""HKCU\Control Panel\Mouse"" /v MouseThreshold2 /t REG_SZ /d ""10"" /f
exit /b 0"
            },

            new EmbeddedTweak
            {
                Key = "Disable USB Selective Suspend",
                Name = "Disable USB Selective Suspend",
                Tags = "Finished,SafeToUse",
                Description = "Stops Windows from sleeping USB ports on battery and on AC power. Disables USB selective suspend in the active power plan and sets DisableSelectiveSuspend. Note: Mice, keyboards, and audio interfaces stay awake. Laptop battery use can increase slightly.",
                RequiresAdmin = true,
                ApplyBatch = @"@echo off
powercfg /setacvalueindex scheme_current 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0
powercfg /setdcvalueindex scheme_current 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0
powercfg /setactive scheme_current
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\USB"" /v DisableSelectiveSuspend /t REG_DWORD /d 1 /f
exit /b 0",
                RevertBatch = @"@echo off
powercfg /setacvalueindex scheme_current 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 1
powercfg /setdcvalueindex scheme_current 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 1
powercfg /setactive scheme_current
reg add ""HKLM\SYSTEM\CurrentControlSet\Services\USB"" /v DisableSelectiveSuspend /t REG_DWORD /d 0 /f
exit /b 0"
            },

            new EmbeddedTweak
            {
                Key = "Game Mode and HAGS",
                Name = "Game Mode and HAGS",
                Tags = "Finished,SafeToUse",
                Description = "Turns on Windows Game Mode and Hardware-accelerated GPU scheduling. Enables AutoGameMode and sets HwSchMode to 2. Note: Restart Windows before the GPU scheduling change takes effect. Turn it off again if an older GPU stutters.",
                RequiresAdmin = true,
                ApplyBatch = @"@echo off
reg add ""HKCU\SOFTWARE\Microsoft\GameBar"" /v AutoGameModeEnabled /t REG_DWORD /d 1 /f
reg add ""HKCU\SOFTWARE\Microsoft\GameBar"" /v AllowAutoGameMode /t REG_DWORD /d 1 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v HwSchMode /t REG_DWORD /d 2 /f
exit /b 0",
                RevertBatch = @"@echo off
reg add ""HKCU\SOFTWARE\Microsoft\GameBar"" /v AutoGameModeEnabled /t REG_DWORD /d 0 /f
reg add ""HKCU\SOFTWARE\Microsoft\GameBar"" /v AllowAutoGameMode /t REG_DWORD /d 0 /f
reg add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v HwSchMode /t REG_DWORD /d 1 /f
exit /b 0"
            }
        };
    }
}
