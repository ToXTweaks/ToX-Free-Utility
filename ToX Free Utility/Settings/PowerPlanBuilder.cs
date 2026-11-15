using System;
using System.Text;

namespace ToX_Free_Utility_NEW_ERA.Settings
{
    public static class PowerPlanBuilder
    {
        public const string HighPerformanceScheme = "e9a42b02-d5df-448d-aa00-03f14749eb61";
        public const string BalancedScheme = "381b4222-f694-41f0-9685-ff5bb260df2e";

        private const string SubProcessor = "SUB_PROCESSOR";
        private const string SubSleep = "SUB_SLEEP";
        private const string SubDisk = "SUB_DISK";
        private const string SubVideo = "SUB_VIDEO";
        private const string SubDisplay = "7516b95f-f776-4464-8c53-06167f40ccf0";
        private const string DisplayBrightness = "aded5e82-b909-4613-9949-f5d71dac0bcb";

        public static (int MinPercent, int MaxPercent) GetCpuBounds(PowerPlanConfig config)
        {
            if (config == null)
                return (5, 100);

            int min = config.EnableHighPerformance ? 5 : 5;
            int max = config.EnableHighPerformance ? 100 : 99;
            return (min, max);
        }

        public static string EnsurePlanGuid(PowerPlanConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(config.PlanGuid) ||
                !Guid.TryParse(config.PlanGuid, out _))
            {
                config.PlanGuid = Guid.NewGuid().ToString();
            }

            return config.PlanGuid;
        }

        public static string BuildBatchScript(PowerPlanConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            string planGuid = EnsurePlanGuid(config);
            string sourceScheme = config.EnableHighPerformance
                ? HighPerformanceScheme
                : BalancedScheme;

            string planName = EscapeBatchValue(
                string.IsNullOrWhiteSpace(config.Name) ? "ToX Custom Plan" : config.Name.Trim());
            string planDescription = EscapeBatchValue(config.Description ?? string.Empty);

            (int cpuMin, int cpuMax) = GetCpuBounds(config);
            int diskIdleSeconds = config.TurnOffHardDisk ? 600 : 0;
            int displayIdleSeconds = config.TurnOffDisplay ? 900 : 0;
            int displayBrightness = config.LowerScreenBrightness ? 40 : 100;
            int hiberboot = config.FastStartup ? 1 : 0;

            var script = new StringBuilder();
            script.AppendLine("@echo off");
            script.AppendLine($"powercfg -duplicatescheme {sourceScheme} {planGuid} >nul 2>nul || ver >nul");
            script.AppendLine($"powercfg -changename {planGuid} \"{planName}\" \"{planDescription}\"");

            AppendProcessorSettings(script, planGuid, cpuMin, cpuMax);

            if (config.DisableSleepMode)
                AppendSleepDisabled(script, planGuid);
            else
                AppendSleepDefaults(script, planGuid);

            AppendSetting(script, planGuid, SubDisk, "DISKIDLE", diskIdleSeconds);
            AppendSetting(script, planGuid, SubVideo, "VIDEOIDLE", displayIdleSeconds);
            AppendSetting(script, planGuid, SubDisplay, DisplayBrightness, displayBrightness);

            script.AppendLine(
                "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Power\" " +
                $"/v HiberbootEnabled /t REG_DWORD /d {hiberboot} /f >nul 2>nul");

            if (config.CustomTweaks != null)
            {
                foreach (PowerPlanCustomTweak tweak in config.CustomTweaks)
                {
                    if (tweak == null || string.IsNullOrWhiteSpace(tweak.Command))
                        continue;

                    string command = tweak.Command.Trim();
                    script.AppendLine(command);
                }
            }

            script.AppendLine($"powercfg -setactive {planGuid} >nul 2>nul");
            script.AppendLine($"echo PLAN_GUID={planGuid}");
            return script.ToString();
        }

        private static void AppendProcessorSettings(StringBuilder script, string planGuid, int min, int max)
        {
            AppendSetting(script, planGuid, SubProcessor, "PROCTHROTTLEMIN", min);
            AppendSetting(script, planGuid, SubProcessor, "PROCTHROTTLEMAX", max);
            AppendSetting(script, planGuid, SubProcessor, "PERFINCPOL", 2);
            AppendSetting(script, planGuid, SubProcessor, "PERFDECPOL", 1);
        }

        private static void AppendSleepDisabled(StringBuilder script, string planGuid)
        {
            AppendSetting(script, planGuid, SubSleep, "STANDBYIDLE", 0);
            AppendSetting(script, planGuid, SubSleep, "HYBRIDSLEEP", 0);
            AppendSetting(script, planGuid, SubSleep, "HIBERNATEIDLE", 0);
            AppendSetting(script, planGuid, SubSleep, "AWAYMODE", 0);
            AppendSetting(script, planGuid, SubSleep, "ALLOWSTANDBY", 0);
        }

        private static void AppendSleepDefaults(StringBuilder script, string planGuid)
        {
            AppendSetting(script, planGuid, SubSleep, "STANDBYIDLE", 1800);
            AppendSetting(script, planGuid, SubSleep, "HIBERNATEIDLE", 3600);
        }

        private static void AppendSetting(
            StringBuilder script,
            string planGuid,
            string subgroup,
            string setting,
            int value)
        {
            script.AppendLine($"powercfg -setacvalueindex {planGuid} {subgroup} {setting} {value} >nul 2>nul");
            script.AppendLine($"powercfg -setdcvalueindex {planGuid} {subgroup} {setting} {value} >nul 2>nul");
        }

        private static string EscapeBatchValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace("\"", "\"\"")
                .Replace("%", "%%");
        }
    }
}
