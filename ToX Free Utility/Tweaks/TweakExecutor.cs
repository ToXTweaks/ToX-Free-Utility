using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility_NEW_ERA.Controls;
using ToX_Free_Utility_NEW_ERA.Settings;
using Vortex.Ui;

namespace ToX_Free_Utility_NEW_ERA.Tweaks
{

    public static class TweakExecutor
    {
        private const string ApplySuccessMessage = "Tweak applied successfully.";
        private const string RevertSuccessMessage = "Tweak reverted successfully.";
        private const string ApplyFailureTitle = "Failed to apply tweak.";
        private const string RevertFailureTitle = "Failed to revert tweak.";
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

            if (isEnabled && IsFeatureBreaking(tweak))
            {
                await OfferRestorePointOnceAsync();

                if (!ConfirmFeatureBreaking(tweak))
                {
                    SetToggleState(toggle, false);
                    return;
                }
            }

            _runningTweaks.Add(runningKey);
            toggle.Enabled = false;

            try
            {
                string title = string.IsNullOrWhiteSpace(tweak.Name) ? tweak.Key : tweak.Name;
                TweakResult result = null;

                bool completed = await TweakProgressOverlay.ExecuteWithOverlayAsync(
                    toggle.FindForm(),
                    title,
                    isEnabled,
                    async () =>
                    {
                        result = await ExecuteBatchScriptAsync(
                            tweakKey,
                            configType.ToString(),
                            isEnabled ? "apply" : "revert",
                            scriptContent,
                            acceptCommandErrors: true);
                    });

                if (!completed || result == null || !result.Success)
                {
                    SetToggleState(toggle, !isEnabled);
                    return;
                }

                TweakStateStore.SetState(configType, tweakKey, isEnabled);
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

        public const string RestorePointScript = @"@echo off
reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore"" /v SystemRestorePointCreationFrequency /t REG_DWORD /d 0 /f >nul 2>&1
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Checkpoint-Computer -Description 'ToX Free Backup' -RestorePointType 'MODIFY_SETTINGS'""
if %errorlevel% equ 0 exit /b 0
wmic.exe /Namespace:\\root\default Path SystemRestore Call CreateRestorePoint ""ToX Free Backup"", 100, 7
exit /b %errorlevel%";

        private static bool _restorePointOfferedThisSession;

        public static Task<TweakResult> CreateRestorePointAsync()
        {
            return ExecuteBatchScriptAsync(
                "CreateBackup",
                "Dashboard",
                "apply",
                RestorePointScript,
                acceptCommandErrors: false);
        }

        public static async Task<TweakResult> ExecuteBatchScriptAsync(
            string tweakKey,
            string configType,
            string mode,
            string scriptContent,
            bool acceptCommandErrors = false)
        {
            try
            {
                Directory.CreateDirectory(TempBatchFolder);

                string safeKey = MakeSafeFileName(tweakKey);
                string safeType = MakeSafeFileName(configType);

                string batchPath = Path.Combine(
                    TempBatchFolder,
                    $"{safeType}_{safeKey}_{mode}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bat");

                string preparedScript = BatchScriptNormalizer.Prepare(scriptContent);
                File.WriteAllText(batchPath, preparedScript, Encoding.ASCII);

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
                        RedirectStandardInput = true,
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
                        process.StandardInput.Close();

                        await WaitForExitAsync(process);

                        string output = outputBuilder.ToString().Trim();
                        string error = errorBuilder.ToString().Trim();
                        bool commandError = process.ExitCode != 0;

                        if (commandError && !acceptCommandErrors)
                        {
                            ActionLog.Write(tweakKey, configType, mode, process.ExitCode, "failed", output, error);
                            return TweakResult.Fail(
                                BuildScriptFailureDetails(process.ExitCode, output, error));
                        }

                        ActionLog.Write(
                            tweakKey,
                            configType,
                            mode,
                            process.ExitCode,
                            commandError ? "applied-with-command-errors" : "applied",
                            output,
                            error);

                        return TweakResult.Ok(string.Empty);
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
                ActionLog.Write(tweakKey, configType, mode, null, "failed", null, ex.ToString());
                return TweakResult.Fail(ex.ToString());
            }
        }

        private static async Task OfferRestorePointOnceAsync()
        {
            if (_restorePointOfferedThisSession)
                return;

            _restorePointOfferedThisSession = true;

            DialogResult answer = VortexMessageBox.Show(
                "This tweak can change or remove Windows features. Create a system restore point before continuing?",
                "Create Restore Point",
                VortexMessageBox.MessageBoxButtons.YesNo,
                true);

            if (answer != DialogResult.Yes)
                return;

            if (!IsRunningAsAdministrator())
            {
                ShowMessage(
                    "A restore point needs administrator privileges. The tweak will continue without one.",
                    "Administrator Required");
                return;
            }

            TweakResult result = await CreateRestorePointAsync();
            if (!result.Success)
            {
                ShowMessage(
                    "Could not create a restore point. The tweak will still continue.\n\n" + result.Message,
                    "Restore Point");
                return;
            }

            TweakStateStore.UpdateDashboardState(state => state.BackupCompleted = true);
            ShowMessage("System restore point created.", "Restore Point");
        }

        private static bool ConfirmFeatureBreaking(EmbeddedTweak tweak)
        {
            string name = string.IsNullOrWhiteSpace(tweak.Name) ? tweak.Key : tweak.Name;
            string warning = ExtractWarning(tweak.Description);
            var message = new StringBuilder();
            message.Append('\'').Append(name).Append("' can remove or break Windows features.");

            if (!string.IsNullOrWhiteSpace(warning))
            {
                message.AppendLine();
                message.AppendLine();
                message.Append(warning);
            }

            message.AppendLine();
            message.AppendLine();
            message.Append("Apply this tweak?");

            DialogResult answer = VortexMessageBox.Show(
                message.ToString().TrimEnd(),
                "Feature Breaking Tweak",
                VortexMessageBox.MessageBoxButtons.YesNo,
                true);

            return answer == DialogResult.Yes;
        }

        private static bool IsFeatureBreaking(EmbeddedTweak tweak)
        {
            if (tweak == null || string.IsNullOrWhiteSpace(tweak.Tags))
                return false;

            string[] parts = tweak.Tags.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string compact = parts[i].Trim().Replace(" ", string.Empty).Replace("_", string.Empty);
                if (compact.Equals("FeatureBreaking", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string ExtractWarning(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return string.Empty;

            const string marker = "Warning:";
            int index = description.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            string text = index >= 0 ? description.Substring(index).Trim() : description.Trim();

            if (text.Length > 700)
                text = text.Substring(0, 700).TrimEnd() + "...";

            return text;
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

        private static string BuildExecutionFailureMessage(string title, string details)
        {
            var message = new StringBuilder();
            message.AppendLine(title);

            if (!string.IsNullOrWhiteSpace(details))
            {
                message.AppendLine();
                message.AppendLine(details.Trim());
            }

            message.AppendLine();

            if (!IsRunningAsAdministrator())
            {
                message.AppendLine("Administrator privileges are required for this tweak.");
                message.AppendLine("Close the app, right-click it, and choose \"Run as administrator\".");
            }
            else
            {
                message.AppendLine("The tweak script reported an error on your system.");
                message.AppendLine("This is not always caused by missing admin rights.");
                message.AppendLine("Some commands may be unsupported on your Windows version, blocked by security software, or require a reboot.");
            }

            return message.ToString().TrimEnd();
        }

        private static string BuildScriptFailureDetails(int exitCode, string output, string error)
        {
            var details = new StringBuilder();
            details.AppendLine($"Script exit code: {exitCode}");

            if (!string.IsNullOrWhiteSpace(error))
            {
                details.AppendLine();
                details.AppendLine("Error output:");
                details.AppendLine(TrimForDialog(error));
            }

            if (!string.IsNullOrWhiteSpace(output))
            {
                details.AppendLine();
                details.AppendLine("Script output:");
                details.AppendLine(TrimForDialog(output));
            }

            return details.ToString().TrimEnd();
        }

        private static string TrimForDialog(string value, int maxLength = 1200)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string trimmed = value.Trim();
            if (trimmed.Length <= maxLength)
                return trimmed;

            return trimmed.Substring(0, maxLength) + Environment.NewLine + "...";
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
