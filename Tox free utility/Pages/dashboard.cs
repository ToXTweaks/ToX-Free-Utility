using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility_NEW_ERA.Settings;
using ToX_Free_Utility_NEW_ERA.Tweaks;
using Vortex.Ui;
using Res = ToX_Free_Utility_NEW_ERA.Properties.Resources;

namespace ToX_Free_Utility_NEW_ERA.Pages
{
    public partial class dashboard : UserControl
    {
        private const int RequiredTweakCount = 10;
        private const string ReviewUrl = "https://toxtweaks.com";
        private const string PremiumUrl = "https://toxtweaks.com";

        private readonly Image _pendingTaskIcon;
        private readonly Image _completedTaskIcon;
        private readonly DashboardMetrics _metrics = new DashboardMetrics();

        public dashboard()
        {
            InitializeComponent();

            _pendingTaskIcon = vortexIconButton21.Icon;
            _completedTaskIcon = Res.done;

            RefreshDashboard();
        }

        private void dashboard_HandleDestroyed(object sender, EventArgs e)
        {
            _metrics.Dispose();
        }

        private void dashboard_Load(object sender, EventArgs e)
        {
            TweakStateStore.EnsureLoaded();
            _metrics.EnsureInitialized();
            RefreshDashboard();
            metricsTimer.Start();
        }

        private void dashboard_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible)
                return;

            RefreshDashboard();
        }

        private void metricsTimer_Tick(object sender, EventArgs e)
        {
            RefreshLiveMetrics();
        }

        private void RefreshDashboard()
        {
            RefreshOptimizationStats();
            RefreshTaskIndicators();
            RefreshLiveMetrics();
        }

        private void RefreshOptimizationStats()
        {
            int enabled = TweakStateStore.GetEnabledTweakCount();
            int total = TweakCatalog.GetTotalTweakCount();
            label2.Text = $"{enabled}/{total}";
        }

        private void RefreshLiveMetrics()
        {
            _metrics.EnsureInitialized();

            float cpuUsage = _metrics.GetCpuUsagePercent();
            vortexCircularProgressbar2.Value = cpuUsage;
            label20.Text = TruncateText(_metrics.CpuName, 34);

            DashboardMetrics.MemorySnapshot memory = _metrics.GetMemorySnapshot();
            vortexCircularProgressbar1.Value = memory.LoadPercent;
            label21.Text = memory.GetSummaryText();

            label3.Text = $"Process count: {_metrics.GetProcessCount()}";
            label4.Text = $"Thread count: {_metrics.GetThreadCount()}";
        }

        private async void Create_Backup_Click(object sender, EventArgs e)
        {
            if (!TweakExecutor.IsRunningAsAdministrator())
            {
                TweakExecutor.ShowMessage(
                    "Creating a system restore point requires administrator privileges.",
                    "Administrator Required");
                return;
            }

            SetButtonLoading(Create_Backup, true);

            try
            {
                const string script = @"@echo off
wmic.exe /Namespace:\\root\default Path SystemRestore Call CreateRestorePoint ""ToX Free Backup"", 100, 7 >nul 2>&1
if %errorlevel% neq 0 (
  powershell -NoProfile -ExecutionPolicy Bypass -Command ""Checkpoint-Computer -Description 'ToX Free Backup' -RestorePointType MODIFY_SETTINGS""
  exit /b %errorlevel%
)
exit /b 0";

                TweakResult result = await TweakExecutor.ExecuteBatchScriptAsync(
                    "CreateBackup",
                    "Dashboard",
                    "apply",
                    script);

                if (!result.Success)
                {
                    TweakExecutor.ShowMessage(
                        "Could not create a restore point.\n\n" + result.Message,
                        "Backup Failed");
                    return;
                }

                TweakStateStore.UpdateDashboardState(state => state.BackupCompleted = true);
                RefreshDashboard();
                TweakExecutor.ShowMessage(
                    "System restore point created successfully.",
                    "Backup Created");
            }
            finally
            {
                SetButtonLoading(Create_Backup, false);
            }
        }

        private async void Quick_Cleanup_Click(object sender, EventArgs e)
        {
            if (!TweakExecutor.IsRunningAsAdministrator())
            {
                TweakExecutor.ShowMessage(
                    "Quick cleanup requires administrator privileges.",
                    "Administrator Required");
                return;
            }

            SetButtonLoading(Quick_Cleanup, true);

            try
            {
                string script = GetCleanupScript();
                TweakResult result = await TweakExecutor.ExecuteBatchScriptAsync(
                    "QuickCleanup",
                    "Dashboard",
                    "apply",
                    script);

                if (!result.Success)
                {
                    TweakExecutor.ShowMessage(
                        "Cleanup failed.\n\n" + result.Message,
                        "Cleanup Failed");
                    return;
                }

                TweakStateStore.UpdateDashboardState(state => state.CleanupCompleted = true);
                RefreshDashboard();
                TweakExecutor.ShowMessage(
                    "Temporary files and caches were cleaned.",
                    "Cleanup Complete");
            }
            finally
            {
                SetButtonLoading(Quick_Cleanup, false);
            }
        }

        private void Make_Review_Click(object sender, EventArgs e)
        {
            OpenExternalUrl(ReviewUrl, "review page");
        }

        private void vortexSidebarButton10_Click(object sender, EventArgs e)
        {
            OpenExternalUrl(PremiumUrl, "premium page");
        }

        private void Restart_Your_PC_Click(object sender, EventArgs e)
        {
            DialogResult confirm = VortexMessageBox.Show(
                "Your PC will restart in 10 seconds. Save your work before continuing.",
                "Restart PC",
                VortexMessageBox.MessageBoxButtons.YesNo);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown.exe",
                    Arguments = "/r /t 10 /c \"ToX Free Utility scheduled restart\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                TweakExecutor.ShowMessage(
                    "Restart scheduled in 10 seconds.",
                    "Restart Scheduled");
            }
            catch (Exception ex)
            {
                TweakExecutor.ShowMessage(
                    "Could not schedule restart.\n\n" + ex.Message,
                    "Restart Failed");
            }
        }

        private void RefreshTaskIndicators()
        {
            TweakStateStore.EnsureLoaded();
            DashboardTaskState state = TweakStateStore.GetDashboardState();

            bool backupDone = state.BackupCompleted;
            bool tweaksDone = TweakStateStore.GetEnabledTweakCount() >= RequiredTweakCount;
            bool cleanupDone = state.CleanupCompleted;
            bool gamesDone = state.GamesOptimized || HasSavedGamePaths();

            SetTaskIcon(vortexIconButton24, backupDone);
            SetTaskIcon(vortexIconButton21, tweaksDone);
            SetTaskIcon(vortexIconButton22, cleanupDone);
            SetTaskIcon(vortexIconButton23, gamesDone);

            if (gamesDone && !state.GamesOptimized)
            {
                TweakStateStore.UpdateDashboardState(s => s.GamesOptimized = true);
            }
        }

        private void SetTaskIcon(VortexIconButton button, bool completed)
        {
            if (button == null)
                return;

            button.Icon = completed ? _completedTaskIcon : _pendingTaskIcon;
        }

        private static string GetCleanupScript()
        {
            var catalog = TweakCatalog.Load();

            if (catalog.TryGetValue(TweakConfigType.WindowsTweaks, out var windowsTweaks) &&
                windowsTweaks != null &&
                windowsTweaks.TryGetValue("Cleaner System and Application Caches", out EmbeddedTweak tweak) &&
                !string.IsNullOrWhiteSpace(tweak.ApplyBatch))
            {
                return tweak.ApplyBatch;
            }

            return @"@echo off
del /f /s /q ""%TEMP%\*"" >nul 2>&1
for /d %%D in (""%TEMP%\*"") do rd /s /q ""%%D"" >nul 2>&1
cleanmgr /sagerun:1 >nul 2>&1";
        }

        private static bool HasSavedGamePaths()
        {
            try
            {
                string path = Path.Combine(TweakStateStore.AppDataFolder, "SavedGamePaths.json");

                if (!File.Exists(path))
                    return false;

                using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path)))
                {
                    if (!doc.RootElement.TryGetProperty("paths", out JsonElement paths) ||
                        paths.ValueKind != JsonValueKind.Object)
                    {
                        return false;
                    }

                    foreach (JsonProperty entry in paths.EnumerateObject())
                    {
                        if (!string.IsNullOrWhiteSpace(entry.Value.GetString()))
                            return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        private static void OpenExternalUrl(string url, string pageName)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                TweakExecutor.ShowMessage(
                    $"Could not open the {pageName}.\n\n{ex.Message}",
                    "Browser Error");
            }
        }

        private static string TruncateText(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            value = value.Trim();
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
        }

        private static void SetButtonLoading(VortexRoundButton button, bool loading)
        {
            if (button == null)
                return;

            button.IsLoading = loading;
            button.Enabled = !loading;
        }
    }
}
