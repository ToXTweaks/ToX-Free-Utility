using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ToX_Free_Utility_NEW_ERA.Settings
{
    internal sealed class GameDefaultProfile
    {
        public string[] CandidateExePaths { get; set; } = Array.Empty<string>();
        public string[] SearchFolders { get; set; } = Array.Empty<string>();
        public string[] PreferredExeNames { get; set; } = Array.Empty<string>();
    }

    internal static class GameBoosterDefaults
    {
        private static readonly Dictionary<string, GameDefaultProfile> Profiles =
            new Dictionary<string, GameDefaultProfile>(StringComparer.OrdinalIgnoreCase)
            {
                ["Fortnite"] = new GameDefaultProfile
                {
                    CandidateExePaths = new[]
                    {
                        @"%ProgramFiles%\Epic Games\Fortnite\FortniteGame\Binaries\Win64\FortniteClient-Win64-Shipping.exe",
                        @"%ProgramFiles(x86)%\Epic Games\Fortnite\FortniteGame\Binaries\Win64\FortniteClient-Win64-Shipping.exe"
                    },
                    PreferredExeNames = new[] { "FortniteClient-Win64-Shipping.exe", "FortniteLauncher.exe" }
                },
                ["Valorant"] = new GameDefaultProfile
                {
                    CandidateExePaths = new[]
                    {
                        @"C:\Riot Games\VALORANT\live\ShooterGame\Binaries\Win64\VALORANT-Win64-Shipping.exe",
                        @"%LocalAppData%\Riot Games\Riot Client\RiotClientServices.exe"
                    },
                    SearchFolders = new[] { @"C:\Riot Games\VALORANT\live" },
                    PreferredExeNames = new[] { "VALORANT-Win64-Shipping.exe", "VALORANT.exe" }
                },
                ["Minecraft"] = new GameDefaultProfile
                {
                    CandidateExePaths = new[]
                    {
                        @"%LocalAppData%\Packages\Microsoft.MinecraftUWP_8wekyb3d8bbwe\Microsoft.MinecraftUWP.exe",
                        @"%ProgramFiles(x86)%\Minecraft Launcher\MinecraftLauncher.exe",
                        @"%ProgramFiles%\Minecraft Launcher\MinecraftLauncher.exe"
                    },
                    SearchFolders = new[]
                    {
                        @"%AppData%\.minecraft",
                        @"%LocalAppData%\Packages"
                    },
                    PreferredExeNames = new[] { "MinecraftLauncher.exe", "minecraft.exe" }
                },
                ["Apex"] = new GameDefaultProfile
                {
                    SearchFolders = BuildSteamCommonFolders("Apex Legends"),
                    PreferredExeNames = new[] { "r5apex.exe", "EasyAntiCheat_launcher.exe" }
                },
                ["GTA"] = new GameDefaultProfile
                {
                    SearchFolders = BuildSteamCommonFolders("Grand Theft Auto V"),
                    PreferredExeNames = new[] { "GTA5.exe", "PlayGTAV.exe" }
                },
                ["Roblox"] = new GameDefaultProfile
                {
                    SearchFolders = new[] { @"%LocalAppData%\Roblox\Versions" },
                    PreferredExeNames = new[] { "RobloxPlayerBeta.exe", "RobloxPlayer.exe" }
                },
                ["CallOfDuty"] = new GameDefaultProfile
                {
                    SearchFolders = new[]
                    {
                        @"%ProgramFiles%\Call of Duty",
                        @"%ProgramFiles(x86)%\Call of Duty",
                        @"%ProgramFiles%\Activision",
                        @"%ProgramFiles(x86)%\Activision"
                    },
                    PreferredExeNames = new[] { "cod.exe", "ModernWarfare.exe", "BlackOpsColdWar.exe" }
                },
                ["Rivals"] = new GameDefaultProfile
                {
                    SearchFolders = BuildSteamCommonFolders("Marvel Rivals"),
                    PreferredExeNames = new[] { "Marvel-Win64-Shipping.exe", "MarvelRivals.exe" }
                },
                ["FIFA"] = new GameDefaultProfile
                {
                    SearchFolders = new[]
                    {
                        @"%ProgramFiles%\EA Games",
                        @"%ProgramFiles(x86)%\EA Games"
                    },
                    PreferredExeNames = new[] { "FC25.exe", "FC24.exe", "FIFA22.exe", "FIFA23.exe" }
                },
                ["Warzone"] = new GameDefaultProfile
                {
                    SearchFolders = new[]
                    {
                        @"%ProgramFiles%\Call of Duty",
                        @"%ProgramFiles(x86)%\Call of Duty",
                        @"%ProgramFiles%\Activision\Call of Duty HQ",
                        @"%ProgramFiles(x86)%\Activision\Call of Duty HQ"
                    },
                    PreferredExeNames = new[] { "cod.exe", "ModernWarfare.exe", "Warzone.exe" }
                },
                ["Rust"] = new GameDefaultProfile
                {
                    SearchFolders = BuildSteamCommonFolders("Rust"),
                    PreferredExeNames = new[] { "RustClient.exe", "Rust.exe" }
                },
                ["CS2"] = new GameDefaultProfile
                {
                    SearchFolders = BuildSteamCommonFolders("Counter-Strike Global Offensive"),
                    PreferredExeNames = new[] { "cs2.exe", "csgo.exe" }
                }
            };

        public static bool TryDetectExecutable(string gameKey, out string executablePath)
        {
            executablePath = null;

            if (string.IsNullOrWhiteSpace(gameKey) || !Profiles.TryGetValue(gameKey, out GameDefaultProfile profile))
                return false;

            foreach (string candidate in profile.CandidateExePaths ?? Array.Empty<string>())
            {
                string expanded = ExpandPath(candidate);
                if (File.Exists(expanded))
                {
                    executablePath = expanded;
                    return true;
                }
            }

            foreach (string folder in profile.SearchFolders ?? Array.Empty<string>())
            {
                string expandedFolder = ExpandPath(folder);
                if (!Directory.Exists(expandedFolder))
                    continue;

                string preferred = FindPreferredExe(expandedFolder, profile.PreferredExeNames, maxDepth: 4);
                if (!string.IsNullOrWhiteSpace(preferred))
                {
                    executablePath = preferred;
                    return true;
                }
            }

            return false;
        }

        public static string GetBrowseFolder(string gameKey, string currentPath)
        {
            if (!string.IsNullOrWhiteSpace(currentPath))
            {
                if (File.Exists(currentPath))
                    return Path.GetDirectoryName(currentPath);

                if (Directory.Exists(currentPath))
                    return currentPath;
            }

            if (Profiles.TryGetValue(gameKey, out GameDefaultProfile profile))
            {
                foreach (string candidate in profile.CandidateExePaths ?? Array.Empty<string>())
                {
                    string expanded = ExpandPath(candidate);
                    string folder = Path.GetDirectoryName(expanded);
                    if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
                        return folder;
                }

                foreach (string folder in profile.SearchFolders ?? Array.Empty<string>())
                {
                    string expanded = ExpandPath(folder);
                    if (Directory.Exists(expanded))
                        return expanded;
                }
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }

        public static string GetSuggestedExeFileName(string gameKey)
        {
            if (!Profiles.TryGetValue(gameKey, out GameDefaultProfile profile))
                return string.Empty;

            return profile.PreferredExeNames?.FirstOrDefault() ?? string.Empty;
        }

        private static string[] BuildSteamCommonFolders(string gameFolderName)
        {
            var folders = new List<string>
            {
                $@"%ProgramFiles(x86)%\Steam\steamapps\common\{gameFolderName}",
                $@"%ProgramFiles%\Steam\steamapps\common\{gameFolderName}",
                $@"C:\Steam\steamapps\common\{gameFolderName}",
                $@"D:\SteamLibrary\steamapps\common\{gameFolderName}",
                $@"E:\SteamLibrary\steamapps\common\{gameFolderName}"
            };

            string steamPath = ReadSteamInstallPath();
            if (!string.IsNullOrWhiteSpace(steamPath))
                folders.Add(Path.Combine(steamPath, "steamapps", "common", gameFolderName));

            return folders.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private static string ReadSteamInstallPath()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    return key?.GetValue("SteamPath") as string;
                }
            }
            catch
            {
                return null;
            }
        }

        private static string FindPreferredExe(string rootFolder, string[] preferredNames, int maxDepth)
        {
            if (preferredNames != null && preferredNames.Length > 0)
            {
                foreach (string preferred in preferredNames)
                {
                    string match = FindFileByName(rootFolder, preferred, maxDepth);
                    if (!string.IsNullOrWhiteSpace(match))
                        return match;
                }
            }

            try
            {
                return Directory
                    .EnumerateFiles(rootFolder, "*.exe", SearchOption.AllDirectories)
                    .Where(path => path.IndexOf("\\EasyAntiCheat\\", StringComparison.OrdinalIgnoreCase) < 0)
                    .Where(path => path.IndexOf("\\BattlEye\\", StringComparison.OrdinalIgnoreCase) < 0)
                    .OrderByDescending(File.GetLastWriteTimeUtc)
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static string FindFileByName(string rootFolder, string fileName, int maxDepth)
        {
            try
            {
                foreach (string path in Directory.EnumerateFiles(rootFolder, fileName, SearchOption.AllDirectories))
                {
                    if (GetDepth(rootFolder, path) <= maxDepth)
                        return path;
                }
            }
            catch
            {
            }

            return null;
        }

        private static int GetDepth(string root, string filePath)
        {
            string relative = filePath.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.IsNullOrEmpty(relative))
                return 0;

            return relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length;
        }

        private static string ExpandPath(string path)
        {
            return Environment.ExpandEnvironmentVariables(path ?? string.Empty);
        }
    }
}
