using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToX_Free_Utility_NEW_ERA.Tweaks;

namespace ToX_Free_Utility_NEW_ERA.Settings
{
    public static class TweakStateStore
    {
        public static string AppDataFolder =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ToXFree");

        private static string StateFilePath => Path.Combine(AppDataFolder, "tweak-states.json");

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static TweakStateFile _cache;
        private static bool _loaded;

        public static void EnsureLoaded()
        {
            if (_loaded)
                return;

            _loaded = true;

            try
            {
                Directory.CreateDirectory(AppDataFolder);

                if (!File.Exists(StateFilePath))
                {
                    _cache = new TweakStateFile();
                    return;
                }

                string json = File.ReadAllText(StateFilePath);
                _cache = JsonSerializer.Deserialize<TweakStateFile>(json, JsonOptions) ?? new TweakStateFile();
            }
            catch
            {
                _cache = new TweakStateFile();
            }

            if (_cache.Tweaks == null)
                _cache.Tweaks = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            if (_cache.Dashboard == null)
                _cache.Dashboard = new DashboardTaskState();
        }

        public static bool TryGetState(TweakConfigType configType, string tweakKey, out bool enabled)
        {
            EnsureLoaded();

            string id = MakeId(configType, tweakKey);
            return _cache.Tweaks.TryGetValue(id, out enabled);
        }

        public static void SetState(TweakConfigType configType, string tweakKey, bool enabled)
        {
            EnsureLoaded();

            string id = MakeId(configType, tweakKey);
            _cache.Tweaks[id] = enabled;
            Save();
        }

        public static int GetEnabledTweakCount()
        {
            EnsureLoaded();

            int count = 0;
            foreach (bool enabled in _cache.Tweaks.Values)
            {
                if (enabled)
                    count++;
            }

            return count;
        }

        public static DashboardTaskState GetDashboardState()
        {
            EnsureLoaded();
            return _cache.Dashboard;
        }

        public static void UpdateDashboardState(Action<DashboardTaskState> update)
        {
            EnsureLoaded();
            update?.Invoke(_cache.Dashboard);
            Save();
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(AppDataFolder);
                string json = JsonSerializer.Serialize(_cache, JsonOptions);
                File.WriteAllText(StateFilePath, json);
            }
            catch
            {
            }
        }

        private static string MakeId(TweakConfigType configType, string tweakKey) =>
            $"{configType}|{tweakKey}";
    }

    public sealed class TweakStateFile
    {
        public Dictionary<string, bool> Tweaks { get; set; } =
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public DashboardTaskState Dashboard { get; set; } = new DashboardTaskState();
    }

    public sealed class DashboardTaskState
    {
        public bool BackupCompleted { get; set; }
        public bool CleanupCompleted { get; set; }
        public bool GamesOptimized { get; set; }
    }
}
