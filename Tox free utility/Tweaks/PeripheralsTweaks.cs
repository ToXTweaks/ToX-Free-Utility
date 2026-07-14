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
                Description = "Tweaks mouse and keyboard settings to improve responsiveness",
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
                Description = "This script aggressively optimize network adapter.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
:: disabling nic features

powershell -NoProfile -Command ""Get-WmiObject MSPower_DeviceEnable -Namespace root\wmi | ForEach-Object {{ $_.enable = $false; $_.psbase.put(); }}"" >nul 2>&1

setlocal EnableDelayedExpansion

set ""ActiveNIC=""

for /f ""delims="" %%I in ('powershell -NoProfile -Command ""(Get-NetAdapter | Where-Object Status -eq 'Up' | Select-Object -First 1).Name""') do (
    set ""ActiveNIC=%%I""
)

if ""!ActiveNIC!""=="" (
    echo No active network adapter found!
    echo =========================================
    pause
    exit /b
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
""Disable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_lldp -ErrorAction SilentlyContinue; ^
Disable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_rspndr -ErrorAction SilentlyContinue; ^
Disable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_lltdio -ErrorAction SilentlyContinue; ^
Disable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_hyperv -ErrorAction SilentlyContinue; ^
Disable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID vmware_bridge -ErrorAction SilentlyContinue; ^
Disable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_msclient -ErrorAction SilentlyContinue; ^
Disable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_server -ErrorAction SilentlyContinue; ^
Disable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_tcpip6 -ErrorAction SilentlyContinue; ^
Disable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_implat -ErrorAction SilentlyContinue""


:: cache things

netsh interface ip delete arpcache
nbtstat -R
ipconfig /flushdns

:: tcp customization

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

::nic
for /f ""delims="" %%K in ('
reg query ""HKLM\SYSTEM\CurrentControlSet\Control\Class\{{4d36e972-e325-11ce-bfc1-08002be10318}}"" /s ^| findstr /r ""\\[0-9][0-9][0-9][0-9]$""
') do (
    reg query ""%%K"" /v DriverDesc 2>nul | find /i ""Intel"" >nul && (
        set ""adapterKey=%%K""
        goto :intel
    )
    reg query ""%%K"" /v DriverDesc 2>nul | find /i ""Realtek"" >nul && (
        set ""adapterKey=%%K""
        goto :realtek
    )
)

echo No Intel or Realtek adapter found.
goto :done

:intel
echo Intel key:
echo %adapterKey%

reg add ""%adapterKey%"" /v ""*SpeedDuplex"" /t REG_SZ /d ""6"" /f
reg add ""%adapterKey%"" /v ""*RSS"" /t REG_SZ /d ""1"" /f
reg add ""%adapterKey%"" /v ""*NumRssQueues"" /t REG_SZ /d ""2"" /f
reg add ""%adapterKey%"" /v ""*ReceiveBuffers"" /t REG_SZ /d ""512"" /f
reg add ""%adapterKey%"" /v ""*TransmitBuffers"" /t REG_SZ /d ""512"" /f
reg add ""%adapterKey%"" /v ""*DeviceSleepOnDisconnect"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""*FlowControl"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""*IPChecksumOffloadIPv4"" /t REG_SZ /d ""3"" /f
reg add ""%adapterKey%"" /v ""*InterruptModeration"" /t REG_SZ /d ""1"" /f
reg add ""%adapterKey%"" /v ""*TCPChecksumOffloadIPv4"" /t REG_SZ /d ""3"" /f
reg add ""%adapterKey%"" /v ""*TCPChecksumOffloadIPv6"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""*UDPChecksumOffloadIPv4"" /t REG_SZ /d ""3"" /f
reg add ""%adapterKey%"" /v ""*UDPChecksumOffloadIPv6"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""*WakeOnMagicPacket"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""*WakeOnPattern"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""SipsEnabled"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""WakeOnFastStartup"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""SSIdleTimeoutMS"" /t REG_SZ /d ""0"" /f

goto :done

:realtek
echo Realtek key:
echo %adapterKey%

reg add ""%adapterKey%"" /v ""*SpeedDuplex"" /t REG_SZ /d ""6"" /f
reg add ""%adapterKey%"" /v ""*RSS"" /t REG_SZ /d ""1"" /f
reg add ""%adapterKey%"" /v ""*NumRssQueues"" /t REG_SZ /d ""2"" /f
reg add ""%adapterKey%"" /v ""*ReceiveBuffers"" /t REG_SZ /d ""512"" /f
reg add ""%adapterKey%"" /v ""*TransmitBuffers"" /t REG_SZ /d ""512"" /f
reg add ""%adapterKey%"" /v ""*DeviceSleepOnDisconnect"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""*FlowControl"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""*IPChecksumOffloadIPv4"" /t REG_SZ /d ""3"" /f
reg add ""%adapterKey%"" /v ""*InterruptModeration"" /t REG_SZ /d ""1"" /f
reg add ""%adapterKey%"" /v ""*TCPChecksumOffloadIPv4"" /t REG_SZ /d ""3"" /f
reg add ""%adapterKey%"" /v ""*TCPChecksumOffloadIPv6"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""*UDPChecksumOffloadIPv4"" /t REG_SZ /d ""3"" /f
reg add ""%adapterKey%"" /v ""*UDPChecksumOffloadIPv6"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""*WakeOnMagicPacket"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""*WakeOnPattern"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""ASPM"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""CLKREQ"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""EEEPlus"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""EnableAspm"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""DynamicLTR"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""EnableGreenEthernet"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""GigaLite"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""PowerDownPll"" /t REG_SZ /d ""0"" /f
reg add ""%adapterKey%"" /v ""PowerSavingMode"" /t REG_SZ /d ""0"" /f

:done
netsh interface set interface ""%ActiveNIC%"" admin=disable

netsh interface set interface ""%ActiveNIC%"" admin=Enable",
            RevertBatch = $@"@echo off
setlocal EnableDelayedExpansion

:: Restore device power management
powershell -NoProfile -Command ^
""Get-WmiObject MSPower_DeviceEnable -Namespace root\wmi | ForEach-Object {{ $_.Enable = $true; $_.psbase.Put() }}"" >nul 2>&1

:: Find active adapter
set ""ActiveNIC=""

for /f ""delims="" %%I in ('powershell -NoProfile -Command ""(Get-NetAdapter | Where-Object Status -eq ''Up'' | Select-Object -First 1).Name""') do (
    set ""ActiveNIC=%%I""
)

if ""!ActiveNIC!""=="" (
    echo No active adapter found.
    pause
    exit /b
)

echo Restoring network bindings...

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
""Enable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_lldp -ErrorAction SilentlyContinue; ^
Enable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_rspndr -ErrorAction SilentlyContinue; ^
Enable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_lltdio -ErrorAction SilentlyContinue; ^
Enable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_hyperv -ErrorAction SilentlyContinue; ^
Enable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID vmware_bridge -ErrorAction SilentlyContinue; ^
Enable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_msclient -ErrorAction SilentlyContinue; ^
Enable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_server -ErrorAction SilentlyContinue; ^
Enable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_tcpip6 -ErrorAction SilentlyContinue; ^
Enable-NetAdapterBinding -Name '!ActiveNIC!' -ComponentID ms_implat -ErrorAction SilentlyContinue""

echo Restoring TCP defaults...

netsh int tcp set global autotuninglevel=normal
netsh int tcp set global rss=enabled
netsh int tcp set global rsc=enabled
netsh int tcp set global prr=enabled
netsh int tcp set global hystart=enabled
netsh int tcp set global fastopen=enabled
netsh int tcp set global timestamps=disabled
netsh int tcp set global nonsackrttresiliency=disabled
netsh int tcp set global pacingprofile=off
netsh int tcp set global ecncapability=disabled

:: Locate Intel/Realtek adapter key
for /f ""delims="" %%K in ('
reg query ""HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{{4d36e972-e325-11ce-bfc1-08002be10318}}"" /s ^| findstr /r ""\\[0-9][0-9][0-9][0-9]$""
') do (
    reg query ""%%K"" /v DriverDesc 2>nul | find /i ""Intel"" >nul && (
        set ""adapterKey=%%K""
        goto Found
    )
    reg query ""%%K"" /v DriverDesc 2>nul | find /i ""Realtek"" >nul && (
        set ""adapterKey=%%K""
        goto Found
    )
)

goto Restart

:Found
echo Removing custom NIC properties...

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
    reg delete ""%adapterKey%"" /v %%~V /f >nul 2>&1
)

:Restart

netsh interface set interface ""!ActiveNIC!"" admin=disable >nul
timeout /t 2 >nul
netsh interface set interface ""!ActiveNIC!"" admin=enable >nul"
           }
        };
    }
}
