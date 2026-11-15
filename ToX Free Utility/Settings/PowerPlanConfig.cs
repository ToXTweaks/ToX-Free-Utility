using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToX_Free_Utility_NEW_ERA.Settings
{
    public sealed class PowerPlanConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "ToX Custom Plan";

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("planGuid")]
        public string PlanGuid { get; set; } = string.Empty;

        [JsonPropertyName("fastStartup")]
        public bool FastStartup { get; set; } = true;

        [JsonPropertyName("disableSleepMode")]
        public bool DisableSleepMode { get; set; } = true;

        [JsonPropertyName("enableHighPerformance")]
        public bool EnableHighPerformance { get; set; } = true;

        [JsonPropertyName("lowerScreenBrightness")]
        public bool LowerScreenBrightness { get; set; } = true;

        [JsonPropertyName("turnOffHardDisk")]
        public bool TurnOffHardDisk { get; set; } = true;

        [JsonPropertyName("turnOffDisplay")]
        public bool TurnOffDisplay { get; set; } = false;

        [JsonPropertyName("customTweaks")]
        public List<PowerPlanCustomTweak> CustomTweaks { get; set; } =
            new List<PowerPlanCustomTweak>();
    }

    public sealed class PowerPlanCustomTweak
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("command")]
        public string Command { get; set; }
    }

    public static class PowerPlanStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string ConfigFilePath =>
            Path.Combine(TweakStateStore.AppDataFolder, "custom-power-plan.json");

        public static PowerPlanConfig Load()
        {
            try
            {
                Directory.CreateDirectory(TweakStateStore.AppDataFolder);

                if (!File.Exists(ConfigFilePath))
                    return CreateDefault();

                string json = File.ReadAllText(ConfigFilePath);
                PowerPlanConfig config = JsonSerializer.Deserialize<PowerPlanConfig>(json, JsonOptions);
                return config ?? CreateDefault();
            }
            catch
            {
                return CreateDefault();
            }
        }

        public static void Save(PowerPlanConfig config)
        {
            if (config == null)
                return;

            try
            {
                Directory.CreateDirectory(TweakStateStore.AppDataFolder);
                string json = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch
            {
            }
        }

        public static PowerPlanConfig CreateDefault()
        {
            return new PowerPlanConfig();
        }
    }
}
