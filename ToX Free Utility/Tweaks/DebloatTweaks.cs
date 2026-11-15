using System.Collections.Generic;

namespace ToX_Free_Utility_NEW_ERA.Tweaks
{
    public static class DebloatTweaks
    {
        public static List<EmbeddedTweak> All => new List<EmbeddedTweak>
        {
            new EmbeddedTweak
            {
                Key = "Uninstall_Microsoft_Edge",
                Name = "Uninstall Microsoft Edge",
                Tags = "FeatureBreaking",
                Description = "Uninstalls Microsoft Edge to remove its background processes, auto-update services, and startup entries. Removes the Edge AppX package and clears related registry entries. Windows may reinstall Edge after major updates. Warning: Breaks Edge as default browser, PDF handler, and any apps that depend on WebView2 until you reinstall Edge manually.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
taskkill /f /im msedge.exe >nul 2>&1
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-AppxPackage -AllUsers *Microsoft.MicrosoftEdge.Stable* -ErrorAction SilentlyContinue | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue""",
                RevertBatch = $@"@echo off
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Start-Process 'https://www.microsoft.com/edge/download'"""
            },

            new EmbeddedTweak
            {
                Key = "Disable_Windows_Defender",
                Name = "Disable Windows Defender",
                Tags = "FeatureBreaking",
                Description = "Disables Windows Defender real-time protection and anti-spyware via group policy. Stops continuous file scanning, cloud sample submission, and background threat monitoring to reduce CPU and disk usage. Warning: Your PC will have no built-in antivirus protection. Only use if you have a third-party antivirus installed.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender"" /v DisableAntiSpyware /t REG_DWORD /d 1 /f",
                RevertBatch = $@"@echo off
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows Defender"" /v DisableAntiSpyware /f"
            },

            new EmbeddedTweak
            {
                Key = "Disable_Basic_Services",
                Name = "Disable Basic Services",
                Tags = "InProgress",
                Description = "Disables the DiagTrack (Connected User Experiences and Telemetry) service. Stops Windows from collecting and sending diagnostic and usage data to Microsoft in the background. Warning: Reduces telemetry only. Windows may re-enable this service after major updates.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
sc config DiagTrack start= disabled
sc stop DiagTrack",
                RevertBatch = $@"@echo off
sc config DiagTrack start= auto
sc start DiagTrack"
            },

            new EmbeddedTweak
            {
                Key = "Disable_Useless_Features",
                Name = "Disable Useless Features",
                Tags = "FeatureBreaking",
                Description = "Disables Windows Spotlight and suggested content in the Start menu and lock screen. Turns off automatic installation of suggested apps, OEM pre-installed app promotions, and lock screen tips/suggestions. Warning: You will no longer see Windows tips, featured content, or app suggestions on the lock screen and Start menu.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
reg add ""HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v SubscribedContent-338389Enabled /t REG_DWORD /d 0 /f",
                RevertBatch = $@"@echo off
reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager"" /v SubscribedContent-338389Enabled /f"
            },

            new EmbeddedTweak
            {
                Key = "Disable_Cortana",
                Name = "Disable Cortana",
                Tags = "FeatureBreaking",
                Description = "Disables Cortana voice assistant and search integration via group policy. Removes Cortana from the taskbar search box — search will only find local files and apps, not web results or voice commands. Warning: Breaks Cortana voice commands, reminders, and web search from the Start menu.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search"" /v AllowCortana /t REG_DWORD /d 0 /f",
                RevertBatch = $@"@echo off
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search"" /v AllowCortana /f"
            }
        };
    }
}
