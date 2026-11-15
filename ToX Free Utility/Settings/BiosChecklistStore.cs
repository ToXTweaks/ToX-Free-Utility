using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToX_Free_Utility_NEW_ERA.Settings
{
    public sealed class BiosChecklistState
    {
        public Dictionary<string, bool> Items { get; set; } =
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public int GearIndex { get; set; }
    }

    public static class BiosChecklistStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static string FilePath =>
            Path.Combine(TweakStateStore.AppDataFolder, "bios-checklist.json");

        public static BiosChecklistState Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new BiosChecklistState();

                BiosChecklistState state = JsonSerializer.Deserialize<BiosChecklistState>(
                    File.ReadAllText(FilePath),
                    JsonOptions);

                if (state == null)
                    return new BiosChecklistState();

                if (state.Items == null)
                    state.Items = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

                return state;
            }
            catch
            {
                return new BiosChecklistState();
            }
        }

        public static void Save(BiosChecklistState state)
        {
            if (state == null)
                return;

            try
            {
                Directory.CreateDirectory(TweakStateStore.AppDataFolder);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(state, JsonOptions));
            }
            catch
            {
            }
        }
    }
}
