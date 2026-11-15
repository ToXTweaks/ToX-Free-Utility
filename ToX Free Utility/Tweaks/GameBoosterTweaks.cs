using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

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
                Description = "Optimizes the Windows MMCSS (Multimedia Class Scheduler) service for gaming and low-latency workloads. Sets SystemResponsiveness to 0 (no CPU reserved for background tasks) and removes network throttling that can limit bandwidth during multimedia activity. Note: May cause background downloads and system tasks to run slower while gaming.",
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
                Description = "Imports and activates the ToX Free Power Plan V2.2 based on Windows Ultimate Performance. Disables CPU idle power saving, USB selective suspend, disk parking, and sets minimum processor state to 100% for consistent clock speeds. Warning: Significantly increases power draw and heat. Not recommended for laptops on battery.",
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
                Description = "Disables Xbox Game Bar background recording and Game DVR capture. Turns off AppCaptureEnabled and GameDVR_Enabled so Windows stops recording gameplay in the background. Warning: You lose instant replay and background clip recording. Use a dedicated recorder like OBS instead.",
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
                Description = "Sets high GPU and CPU priority for the Windows \"Games\" multimedia task profile. Assigns GPU Priority 8, CPU Priority 6, and \"High\" scheduling category so games get preferential access to GPU and CPU resources. Note: Works alongside MMCSS. May cause background apps to stutter while gaming.",
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

        private const string BoosterCompatibilityFlag = "~ RUNASADMIN HIGHDPIAWARE";
        private const string LayersSubKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
        private const string IfeoSubKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";

        private static readonly string[] FortniteExecutables =
        {
            "FortniteClient-Win64-Shipping.exe",
            "FortniteClient-Win64-Shipping_EAC.exe",
            "FortniteClient-Win64-Shipping_BE.exe",
            "FortniteLauncher.exe"
        };

        public static void RepairAntiCheatLaunchBlocks()
        {
            try
            {
                RemoveBoosterCompatibilityLayers(Registry.CurrentUser);
                RemoveBoosterCompatibilityLayers(Registry.LocalMachine);
                RemoveFortniteImageOptions(RegistryView.Registry64);
                RemoveFortniteImageOptions(RegistryView.Registry32);
            }
            catch
            {
            }
        }

        public static List<string> GetGameApplyBatch(string gameExecutablePath, string gameName)
        {
            string gamePath = gameExecutablePath ?? string.Empty;
            string gameExeName = Path.GetFileName(gamePath);
            string qosPolicyName = SanitizeQosPolicyName(gameName);
            string safeExe = EscapeBatchValue(gameExeName);

            var lines = new List<string>
            {
                "@echo off"
            };

            if (!string.IsNullOrWhiteSpace(gamePath))
            {
                lines.Add(
                    "reg delete \"HKCU\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers\" " +
                    $"/v \"{EscapeBatchValue(gamePath)}\" /f >nul 2>&1");
            }

            if (!string.IsNullOrWhiteSpace(gameExeName))
            {
                lines.Add(
                    "reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\" +
                    safeExe + "\" /f >nul 2>&1");
                lines.Add(
                    "reg delete \"HKLM\\SOFTWARE\\WOW6432Node\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\" +
                    safeExe + "\" /f >nul 2>&1");
            }

            string qosKey = EscapeBatchValue(qosPolicyName);
            string qosExe = EscapeBatchValue(gameExeName);
            lines.Add($"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{qosKey}\" /v \"Application Name\" /t REG_SZ /d \"{qosExe}\" /f");
            lines.Add($"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{qosKey}\" /v \"DSCP value\" /t REG_SZ /d \"46\" /f");
            lines.Add($"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{qosKey}\" /v \"Local IP\" /t REG_SZ /d \"*\" /f");
            lines.Add($"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{qosKey}\" /v \"Local IP Prefix Length\" /t REG_SZ /d \"*\" /f");
            lines.Add($"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{qosKey}\" /v \"Local Port\" /t REG_SZ /d \"*\" /f");
            lines.Add($"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{qosKey}\" /v \"Protocol\" /t REG_SZ /d \"UDP\" /f");
            lines.Add($"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{qosKey}\" /v \"Remote IP\" /t REG_SZ /d \"*\" /f");
            lines.Add($"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{qosKey}\" /v \"Remote IP Prefix Length\" /t REG_SZ /d \"*\" /f");
            lines.Add($"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{qosKey}\" /v \"Remote Port\" /t REG_SZ /d \"*\" /f");
            lines.Add($"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{qosKey}\" /v \"throttle Rate\" /t REG_SZ /d \"-1\" /f");
            lines.Add($"reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\QoS\\{qosKey}\" /v \"version\" /t REG_SZ /d \"1.0\" /f");

            return lines;
        }

        private static void RemoveBoosterCompatibilityLayers(RegistryKey hive)
        {
            if (hive == null)
                return;

            using (RegistryKey layers = hive.OpenSubKey(LayersSubKey, writable: true))
            {
                if (layers == null)
                    return;

                foreach (string valueName in layers.GetValueNames())
                {
                    string data = layers.GetValue(valueName) as string;
                    if (!ShouldClearCompatibilityFlag(valueName, data))
                        continue;

                    try
                    {
                        layers.DeleteValue(valueName, throwOnMissingValue: false);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static bool ShouldClearCompatibilityFlag(string executablePath, string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return false;

            string flags = data.Trim();
            if (flags.Equals(BoosterCompatibilityFlag, StringComparison.OrdinalIgnoreCase))
                return true;

            if (flags.IndexOf("RUNASADMIN", StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return IsFortniteExecutable(executablePath);
        }

        private static void RemoveFortniteImageOptions(RegistryView view)
        {
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
            using (RegistryKey ifeo = baseKey.OpenSubKey(IfeoSubKey, writable: true))
            {
                if (ifeo == null)
                    return;

                foreach (string exeName in FortniteExecutables)
                {
                    try
                    {
                        ifeo.DeleteSubKeyTree(exeName, throwOnMissingSubKey: false);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static bool IsFortniteExecutable(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (path.IndexOf("Fortnite", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            string fileName = Path.GetFileName(path);
            for (int i = 0; i < FortniteExecutables.Length; i++)
            {
                if (fileName.Equals(FortniteExecutables[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
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
