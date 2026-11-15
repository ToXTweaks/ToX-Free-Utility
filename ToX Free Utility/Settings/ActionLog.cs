using System;
using System.IO;
using System.Text;

namespace ToX_Free_Utility_NEW_ERA.Settings
{
    public static class ActionLog
    {
        private static readonly object Gate = new object();
        private const int MaxBytes = 1024 * 1024;

        public static string LogFolder => Path.Combine(TweakStateStore.AppDataFolder, "logs");

        public static string LogFilePath => Path.Combine(LogFolder, "actions.log");

        public static void Write(
            string tweakName,
            string category,
            string mode,
            int? exitCode,
            string status,
            string output,
            string error)
        {
            try
            {
                var entry = new StringBuilder();
                entry.AppendLine("===== " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " =====");
                entry.AppendLine("Tweak: " + (string.IsNullOrWhiteSpace(tweakName) ? "unknown" : tweakName));
                entry.AppendLine("Category: " + (string.IsNullOrWhiteSpace(category) ? "unknown" : category));
                entry.AppendLine("Mode: " + (string.IsNullOrWhiteSpace(mode) ? "unknown" : mode));
                entry.AppendLine("Exit code: " + (exitCode.HasValue ? exitCode.Value.ToString() : "n/a"));
                entry.AppendLine("Status: " + (string.IsNullOrWhiteSpace(status) ? "unknown" : status));

                if (!string.IsNullOrWhiteSpace(output))
                {
                    entry.AppendLine("--- output ---");
                    entry.AppendLine(output.Trim());
                }

                if (!string.IsNullOrWhiteSpace(error))
                {
                    entry.AppendLine("--- error ---");
                    entry.AppendLine(error.Trim());
                }

                entry.AppendLine();

                lock (Gate)
                {
                    Directory.CreateDirectory(LogFolder);
                    File.AppendAllText(LogFilePath, entry.ToString(), Encoding.UTF8);
                    TrimIfNeeded();
                }
            }
            catch
            {
            }
        }

        public static string ReadAll()
        {
            try
            {
                if (!File.Exists(LogFilePath))
                    return "No actions have been logged yet.";

                string text = File.ReadAllText(LogFilePath, Encoding.UTF8);
                return string.IsNullOrWhiteSpace(text)
                    ? "No actions have been logged yet."
                    : text;
            }
            catch (Exception ex)
            {
                return "Could not read the action log." + Environment.NewLine + Environment.NewLine + ex.Message;
            }
        }

        private static void TrimIfNeeded()
        {
            var info = new FileInfo(LogFilePath);
            if (!info.Exists || info.Length <= MaxBytes)
                return;

            string text = File.ReadAllText(LogFilePath, Encoding.UTF8);
            int keep = Math.Min(text.Length, MaxBytes / 2);
            string trimmed = text.Substring(text.Length - keep);
            int lineBreak = trimmed.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (lineBreak >= 0 && lineBreak < 400)
                trimmed = trimmed.Substring(lineBreak + Environment.NewLine.Length);

            File.WriteAllText(LogFilePath, trimmed, Encoding.UTF8);
        }
    }
}
