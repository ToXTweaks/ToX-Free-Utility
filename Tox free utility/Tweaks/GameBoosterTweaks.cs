using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ToX_Free_Utility_NEW_ERA.Tweaks
{
    public static class GameBoosterTweaks
    {
        public static List<EmbeddedTweak> All => new List<EmbeddedTweak>
        {
            new EmbeddedTweak
            {
                Key = "Tweak_MMCSS_Service",
                Name = "Tweak MMCSS Service",
                Tags = "InProgress",
                Description = "Optimizes multimedia system settings for low-latency audio, display post-processing\r\nand gaming. It enhances system responsiveness, removes network throttling, and \r\nensures tasks are prioritized based on usage to improve performance and reduce lag.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v SystemResponsiveness /t REG_DWORD /d 0 /f
reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v NetworkThrottlingIndex /t REG_DWORD /d 4294967295 /f",
                RevertBatch = $@"@echo off
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v SystemResponsiveness /f
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"" /v NetworkThrottlingIndex /f"
            },

            new EmbeddedTweak
            {
                Key = "Import_ToX_Power_Plan",
                Name = "Import ToX Free Power Plan V2.2",
                Tags = "PowerHungry,InProgress",
                Description = "Imports and Applies custom Free ToX Power Plan that disables power-saving features, \r\nenhances performance, reduces latency and boosts overall PC's Responsivness.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61
powercfg -setactive e9a42b02-d5df-448d-aa00-03f14749eb61",
                RevertBatch = $@"@echo off
powercfg -setactive SCHEME_BALANCED"
            },

            new EmbeddedTweak
            {
                Key = "Disable_GameBar_DVR",
                Name = "Disable Game Bar and DVR",
                Tags = "Finished",
                Description = "Disabling Game Bar and Game DVR, reduces background resource usage,\r\nhelping to improve system performance and reduce input lag during gaming.",
                RequiresAdmin = false,
                ApplyBatch = $@"@echo off
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR"" /v AppCaptureEnabled /t REG_DWORD /d 0 /f
reg add ""HKCU\System\GameConfigStore"" /v GameDVR_Enabled /t REG_DWORD /d 0 /f",
                RevertBatch = $@"@echo off
reg add ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR"" /v AppCaptureEnabled /t REG_DWORD /d 1 /f
reg add ""HKCU\System\GameConfigStore"" /v GameDVR_Enabled /t REG_DWORD /d 1 /f"
            },

            new EmbeddedTweak
            {
                Key = "Basic_Performance_Tweaks",
                Name = "Basic Performance Tweaks",
                Tags = "InProgress",
                Description = "Enhances performance by enabling Game Mode and hardware GPU scheduling,\r\nwhile disabling power throttling and variable refresh rate to optimize system \r\nresponsiveness and resource allocation.",
                RequiresAdmin = true,
                ApplyBatch = $@"@echo off
reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""GPU Priority"" /t REG_DWORD /d 8 /f
reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v Priority /t REG_DWORD /d 6 /f
reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Scheduling Category"" /t REG_SZ /d High /f",
                RevertBatch = $@"@echo off
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""GPU Priority"" /f
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v Priority /f
reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"" /v ""Scheduling Category"" /f"
            }
        };

        public static bool RequiresAdminForGameOptimize => true;

        public static List<string> GetGameApplyBatch(string gameExecutablePath, string gameName)
        {
            string gamePath = gameExecutablePath ?? string.Empty;
            string gameExeName = Path.GetFileName(gamePath);
            string qosPolicyName = SanitizeQosPolicyName(gameName);

            return new List<string>
            {
                "@echo off",
                $"reg add \"HKCU\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers\" /v \"{EscapeBatchValue(gamePath)}\" /t REG_SZ /d \"~ RUNASADMIN HIGHDPIAWARE\" /f",
                $"reg add \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\{EscapeBatchValue(gameExeName)}\\PerfOptions\" /v \"CpuPriorityClass\" /t REG_DWORD /d \"3\" /f",
                $"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{EscapeBatchValue(qosPolicyName)}\" /v \"Application Name\" /t REG_SZ /d \"{EscapeBatchValue(gameExeName)}\" /f",
                $"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{EscapeBatchValue(qosPolicyName)}\" /v \"DSCP value\" /t REG_SZ /d \"46\" /f",
                $"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{EscapeBatchValue(qosPolicyName)}\" /v \"Local IP\" /t REG_SZ /d \"*\" /f",
                $"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{EscapeBatchValue(qosPolicyName)}\" /v \"Local IP Prefix Length\" /t REG_SZ /d \"*\" /f",
                $"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{EscapeBatchValue(qosPolicyName)}\" /v \"Local Port\" /t REG_SZ /d \"*\" /f",
                $"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{EscapeBatchValue(qosPolicyName)}\" /v \"Protocol\" /t REG_SZ /d \"UDP\" /f",
                $"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{EscapeBatchValue(qosPolicyName)}\" /v \"Remote IP\" /t REG_SZ /d \"*\" /f",
                $"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{EscapeBatchValue(qosPolicyName)}\" /v \"Remote IP Prefix Length\" /t REG_SZ /d \"*\" /f",
                $"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{EscapeBatchValue(qosPolicyName)}\" /v \"Remote Port\" /t REG_SZ /d \"*\" /f",
                $"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{EscapeBatchValue(qosPolicyName)}\" /v \"throttle Rate\" /t REG_SZ /d \"-1\" /f",
                $"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{EscapeBatchValue(qosPolicyName)}\" /v \"version\" /t REG_SZ /d \"1.0\" /f"
            };
        }

        public static List<string> GetGameRevertBatch(string gameExecutablePath, string gameName)
        {
            string gamePath = gameExecutablePath ?? string.Empty;
            string gameExeName = Path.GetFileName(gamePath);
            string qosPolicyName = SanitizeQosPolicyName(gameName);

            return new List<string>
            {
                "@echo off",
                $"reg delete \"HKCU\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers\" /v \"{EscapeBatchValue(gamePath)}\" /f",
                $"reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\{EscapeBatchValue(gameExeName)}\" /f",
                $"reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{EscapeBatchValue(qosPolicyName)}\" /f"
            };
        }

        public static string[] StockGameKeys => new[]
        {
            "Fortnite", "Valorant", "Minecraft", "Apex", "GTA", "Roblox",
            "CallOfDuty", "Rivals", "FIFA", "Warzone", "Rust", "CS2"
        };

        private static string SanitizeQosPolicyName(string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
                return "ToXGame";

            var builder = new StringBuilder(gameName.Length);

            foreach (char c in gameName.Trim())
            {
                if (char.IsLetterOrDigit(c))
                    builder.Append(c);
                else if (c == ' ' || c == '-' || c == '_')
                    builder.Append('_');
            }

            string sanitized = builder.ToString().Trim('_');
            return string.IsNullOrWhiteSpace(sanitized) ? "ToXGame" : sanitized;
        }

        private static string EscapeBatchValue(string value)
        {
            return (value ?? string.Empty).Replace("\"", "\"\"");
        }
    }
}
