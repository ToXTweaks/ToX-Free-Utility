using System;
using System.Text.RegularExpressions;

namespace ToX_Free_Utility_NEW_ERA.Tweaks
{
    internal static class BatchScriptNormalizer
    {
        public static string Prepare(string scriptContent)
        {
            if (string.IsNullOrWhiteSpace(scriptContent))
                return scriptContent;

            string script = scriptContent.Replace("\r\n", "\n").Replace('\r', '\n');

            script = Regex.Replace(
                script,
                @"(?<![%])%(~?)([A-Za-z])\b",
                "%%$1$2");

            while (script.Contains("%%%"))
                script = script.Replace("%%%", "%%");

            script = script.Replace("-Confirm:\\$False", "-Confirm:$false");
            script = script.Replace("-Confirm:$False", "-Confirm:$false");
            script = script.Replace("-Confirm:`$false", "-Confirm:$false");

            script = script.TrimEnd();
            script = ReplaceStandaloneExit(script);
            script = ReplaceTerminalExit(script);

            if (!EndsWithSuccessfulExit(script))
            {
                script += "\ncd . >nul 2>&1\nexit /b 0";
            }

            return script.Replace("\n", Environment.NewLine);
        }

        private static string ReplaceStandaloneExit(string script)
        {
            string[] lines = script.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();

                if (trimmed.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    // Plain "exit" terminates cmd.exe with the last command's error code.
                    // Use goto :eof so subroutines below remain callable via "call :label".
                    lines[i] = "goto :eof";
                }
            }

            return string.Join("\n", lines);
        }

        private static string ReplaceTerminalExit(string script)
        {
            string[] lines = script.Split('\n');
            int lastMeaningful = -1;

            for (int i = lines.Length - 1; i >= 0; i--)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith("::", StringComparison.Ordinal))
                    continue;

                lastMeaningful = i;
                break;
            }

            if (lastMeaningful < 0)
                return script;

            string terminal = lines[lastMeaningful].Trim();
            if (terminal.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                terminal.StartsWith("exit /b", StringComparison.OrdinalIgnoreCase))
            {
                lines[lastMeaningful] = "cd . >nul 2>&1";
                return string.Join("\n", lines) + "\nexit /b 0";
            }

            return script;
        }

        private static bool EndsWithSuccessfulExit(string script)
        {
            string[] lines = script.Split('\n');
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith("::", StringComparison.Ordinal))
                    continue;

                return line.Equals("exit /b 0", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
