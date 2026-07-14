using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility_NEW_ERA.Settings;
using Vortex.Ui;

namespace ToX_Free_Utility_NEW_ERA.Tweaks
{

    public static class TweakExecutor
    {
        private static readonly Dictionary<TweakConfigType, Dictionary<string, EmbeddedTweak>> _tweakConfigs =
            TweakCatalog.Load();

        private static readonly HashSet<string> _runningTweaks =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static string TempBatchFolder =>
            Path.Combine(Path.GetTempPath(), "ToXTweaks", "BatchTweaks");

        public static DialogResult ShowMessage(string message, string title = "Message")
        {
            return VortexMessageBox.Show(
                message,
                title,
                VortexMessageBox.MessageBoxButtons.OK,
                true);
        }

        private static Task WaitForExitAsync(Process process)
        {
            if (process.HasExited)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>();

            void ProcessExited(object sender, EventArgs e)
            {
                tcs.TrySetResult(true);
                process.Exited -= ProcessExited;
            }

            process.EnableRaisingEvents = true;
            process.Exited += ProcessExited;

            if (process.HasExited)
            {
                process.Exited -= ProcessExited;
                return Task.CompletedTask;
            }

            return tcs.Task;
        }

        public static async Task RunTweakToggleAsync(VortexRoundedSwitch toggle, string tweakKey, TweakConfigType configType)
        {
            if (toggle == null)
            {
                ShowMessage("Invalid toggle sender.", "Error");
                return;
            }

            if (string.IsNullOrWhiteSpace(tweakKey))
            {
                ShowMessage("Invalid tweak key.", "Error");
                return;
            }

            if (!_tweakConfigs.TryGetValue(configType, out Dictionary<string, EmbeddedTweak> tweaksForType) ||
                tweaksForType == null ||
                !tweaksForType.TryGetValue(tweakKey, out EmbeddedTweak tweak))
            {
                ShowMessage(
                    $"Tweak '{tweakKey}' was not found in embedded category '{configType}'.",
                    "Missing Tweak Definition");
                return;
            }

            string runningKey = $"{configType}:{tweakKey}";
            if (_runningTweaks.Contains(runningKey))
                return;

            bool isEnabled;

            try
            {
                isEnabled = GetToggleState(toggle);
            }
            catch (Exception ex)
            {
                ShowMessage(
                    $"Could not read toggle state for '{toggle.Name}'.\n\n{ex.Message}",
                    "Toggle Error");
                return;
            }

            if (tweak.RequiresAdmin && !IsRunningAsAdministrator())
            {
                ShowMessage(
                    $"'{tweak.Name ?? tweak.Key}' requires administrator privileges.",
                    "Administrator Required");

                SetToggleState(toggle, !isEnabled);
                return;
            }

            string scriptContent = isEnabled ? tweak.ApplyBatch : tweak.RevertBatch;

            if (string.IsNullOrWhiteSpace(scriptContent))
            {
                SetToggleState(toggle, !isEnabled);

                ShowMessage(
                    $"No {(isEnabled ? "apply" : "revert")} batch script found for '{tweak.Name ?? tweak.Key}'.",
                    "Missing Batch Script");
                return;
            }

            _runningTweaks.Add(runningKey);
            toggle.Enabled = false;

            try
            {
                TweakResult result = await ExecuteBatchScriptAsync(
                    tweakKey,
                    configType.ToString(),
                    isEnabled ? "apply" : "revert",
                    scriptContent);

                if (!result.Success)
                {
                    SetToggleState(toggle, !isEnabled);

                    ShowMessage(
                        $"Failed to {(isEnabled ? "apply" : "revert")} '{tweak.Name ?? tweak.Key}'.\n\n{result.Message}",
                        "Execution Failed");
                }
                else
                {
                    TweakStateStore.SetState(configType, tweakKey, isEnabled);

                    ShowMessage(
                        $"'{tweak.Name ?? tweak.Key}' was {(isEnabled ? "applied (enabled)" : "reverted (disabled)")} successfully.",
                        isEnabled ? "Tweak Applied" : "Tweak Reverted");
                }
            }
            catch (Exception ex)
            {
                SetToggleState(toggle, !isEnabled);

                ShowMessage(
                    $"Unexpected error while executing '{tweak.Name ?? tweak.Key}'.\n\n{ex}",
                    "Execution Error");
            }
            finally
            {
                toggle.Enabled = true;
                _runningTweaks.Remove(runningKey);
            }
        }

        public static async Task<TweakResult> ExecuteBatchScriptAsync(
            string tweakKey,
            string configType,
            string mode,
            string scriptContent)
        {
            try
            {
                Directory.CreateDirectory(TempBatchFolder);

                string safeKey = MakeSafeFileName(tweakKey);
                string safeType = MakeSafeFileName(configType);

                string batchPath = Path.Combine(
                    TempBatchFolder,
                    $"{safeType}_{safeKey}_{mode}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bat");

                File.WriteAllText(batchPath, scriptContent, Encoding.ASCII);

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"\"{batchPath}\"\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    using (Process process = new Process())
                    {
                        process.StartInfo = psi;

                        StringBuilder outputBuilder = new StringBuilder();
                        StringBuilder errorBuilder = new StringBuilder();

                        process.OutputDataReceived += (_, e) =>
                        {
                            if (e.Data != null)
                                outputBuilder.AppendLine(e.Data);
                        };

                        process.ErrorDataReceived += (_, e) =>
                        {
                            if (e.Data != null)
                                errorBuilder.AppendLine(e.Data);
                        };

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        await WaitForExitAsync(process);

                        string output = outputBuilder.ToString().Trim();
                        string error = errorBuilder.ToString().Trim();

                        if (process.ExitCode != 0)
                        {
                            return TweakResult.Fail(
                                $"Batch exited with code {process.ExitCode}.\n\nOutput:\n{output}\n\nError:\n{error}");
                        }

                        return TweakResult.Ok(output);
                    }
                }
                finally
                {
                    try
                    {
                        if (File.Exists(batchPath))
                            File.Delete(batchPath);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                return TweakResult.Fail(ex.ToString());
            }
        }

        public static bool GetToggleState(Control control)
        {
            Type type = control.GetType();

            var checkedProp = type.GetProperty("Checked");
            if (checkedProp != null && checkedProp.PropertyType == typeof(bool))
                return (bool)checkedProp.GetValue(control);

            var isOnProp = type.GetProperty("IsOn");
            if (isOnProp != null && isOnProp.PropertyType == typeof(bool))
                return (bool)isOnProp.GetValue(control);

            throw new InvalidOperationException(
                $"Control '{control.Name}' does not expose Checked or IsOn.");
        }

        public static void SetToggleState(Control control, bool state)
        {
            Type type = control.GetType();

            var checkedProp = type.GetProperty("Checked");
            if (checkedProp != null && checkedProp.PropertyType == typeof(bool) && checkedProp.CanWrite)
            {
                checkedProp.SetValue(control, state);
                return;
            }

            var isOnProp = type.GetProperty("IsOn");
            if (isOnProp != null && isOnProp.PropertyType == typeof(bool) && isOnProp.CanWrite)
            {
                isOnProp.SetValue(control, state);
                return;
            }
        }

        public static bool IsRunningAsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private static string MakeSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";

            foreach (char c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');

            return value;
        }
    }
}
