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
                Description = "Uninstalls Microsoft Edge, a bloated browser that can consume significant system\r\nresources. By removing it, you'll decrease the number of processes running on your \r\nmachine, which can lead to improved performance and reduced resource usage.",
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
                Description = "Disables Windows Defender's real-time protection and anti-spyware features\r\nto reduce system resource usage, often used in environments where third-party \r\nantivirus or protection is installed.",
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
                Description = "Disables various unnecessary Windows services to improve system performance \r\nand reduce resource usage. This tweak includes services like Delivery Optimization, \r\nDiagnostics, and Windows Search, which are often not essential for everyday tasks.",
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
                Description = "Disables unnecessary features to enhance system performance. This includes \r\nturning off web search, transparency effects, and background apps, reducing \r\nresource usage and streamlining the interface for improved speed and efficiency.",
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
                Description = "Disabling error reporting, sync, and location tracking boosts privacy and improves \r\nperformance by reducing background activity and data collection. Cutting \r\ndiagnostics and app permissions frees up system resources.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
reg add ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search"" /v AllowCortana /t REG_DWORD /d 0 /f",
                RevertBatch = $@"@echo off
reg delete ""HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search"" /v AllowCortana /f"
            }
        };
    }
}
