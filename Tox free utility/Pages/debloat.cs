using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility_NEW_ERA.Tweaks;
using Vortex.Ui;

namespace ToX_Free_Utility_NEW_ERA.Pages
{
    public partial class debloat : UserControl
    {
        private readonly Dictionary<Control, string> _appCheckboxes = new Dictionary<Control, string>();
        private readonly Dictionary<Control, string> _junkCheckboxes = new Dictionary<Control, string>();
        private bool _syncingCheckboxes;

        private const string EmptyDirSubroutine = @"
:EmptyDir
if exist ""%~1"" (
    del /q /f /s ""%~1\*.*"" >nul 2>&1
    for /d %%x in (""%~1\*"") do rd /s /q ""%%x"" >nul 2>&1
)
exit /b 0";

        public debloat()
        {
            InitializeComponent();

            _appCheckboxes[BingBloatware_CheckBox] = "BingBloatware";
            _appCheckboxes[XboxApp_CheckBox] = "XboxApp";
            _appCheckboxes[WindowsPhone_CheckBox] = "WindowsPhone";
            _appCheckboxes[WindowsAlarms_CheckBox] = "WindowsAlarms";
            _appCheckboxes[Microsoft3DViewer_CheckBox] = "Microsoft3DViewer";
            _appCheckboxes[WindowsMaps_CheckBox] = "WindowsMaps";
            _appCheckboxes[M3D_Builder_CheckBox] = "M3D_Builder";
            _appCheckboxes[WindowsSoundRecorder_CheckBox] = "WindowsSoundRecorder";
            _appCheckboxes[MicrosoftEdge_CheckBox] = "MicrosoftEdge";
            _appCheckboxes[WindowsCamera_CheckBox] = "WindowsCamera";

            _junkCheckboxes[vortexCheckBox22] = "WindowsTempFiles";
            _junkCheckboxes[vortexCheckBox21] = "WindowsCacheFiles";
            _junkCheckboxes[vortexCheckBox20] = "DnsCache";
            _junkCheckboxes[vortexCheckBox19] = "GamesCache";
            _junkCheckboxes[vortexCheckBox18] = "BrowsersCache";
            _junkCheckboxes[vortexCheckBox17] = "WindowsCookies";
            _junkCheckboxes[vortexCheckBox16] = "WindowsPrefetch";
            _junkCheckboxes[vortexCheckBox15] = "WindowsHistory";
            _junkCheckboxes[vortexCheckBox14] = "DiscordCache";
            _junkCheckboxes[vortexCheckBox13] = "RecycleBin";
        }

        private void debloat_Load(object sender, EventArgs e)
        {
            ControlDebloatPages_PanelShown(ControlDebloatPages, appdebloatPage);
        }

        private void ControlDebloatPages_PanelShown(object sender, Panel panel)
        {
            bool onAppsTab = panel.Name == "appdebloatPage";

            installApps.Visible = onAppsTab;
            uninstallApps.Visible = onAppsTab;
            cleanSelectedJunk.Visible = !onAppsTab;
        }

        private static string NormalizeBatchFragment(string batch)
        {
            if (string.IsNullOrWhiteSpace(batch))
                return string.Empty;

            var lines = batch.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var normalized = new StringBuilder();

            foreach (string line in lines)
            {
                if (line.Trim().Equals("@echo off", StringComparison.OrdinalIgnoreCase))
                    continue;

                normalized.AppendLine(line);
            }

            return normalized.ToString().TrimEnd();
        }

        private static string BuildCombinedBatch(IEnumerable<string> fragments, bool includeEmptyDir)
        {
            var script = new StringBuilder();
            script.AppendLine("@echo off");
            script.AppendLine("setlocal EnableDelayedExpansion");

            foreach (string fragment in fragments)
            {
                string normalized = NormalizeBatchFragment(fragment);

                if (!string.IsNullOrWhiteSpace(normalized))
                    script.AppendLine(normalized);
            }

            script.AppendLine("endlocal");
            script.AppendLine("exit /b 0");

            if (includeEmptyDir)
                script.Append(EmptyDirSubroutine);

            return script.ToString();
        }

        private List<AppDebloatTweak> GetSelectedApps()
        {
            var selected = new List<AppDebloatTweak>();

            foreach (KeyValuePair<Control, string> entry in _appCheckboxes)
            {
                if (!TweakExecutor.GetToggleState(entry.Key))
                    continue;

                AppDebloatTweak tweak = AppDebloatTweaks.GetByKey(entry.Value);

                if (tweak != null)
                    selected.Add(tweak);
            }

            return selected;
        }

        private List<JunkCleanTweak> GetSelectedJunkItems()
        {
            var selected = new List<JunkCleanTweak>();

            foreach (KeyValuePair<Control, string> entry in _junkCheckboxes)
            {
                if (!TweakExecutor.GetToggleState(entry.Key))
                    continue;

                JunkCleanTweak tweak = JunkCleanTweaks.All
                    .FirstOrDefault(x => x.Key.Equals(entry.Value, StringComparison.OrdinalIgnoreCase));

                if (tweak != null)
                    selected.Add(tweak);
            }

            return selected;
        }

        private void SyncSelectAllState(Control selectAllCheckbox, IEnumerable<Control> children)
        {
            bool allChecked = children.All(TweakExecutor.GetToggleState);

            _syncingCheckboxes = true;
            try
            {
                TweakExecutor.SetToggleState(selectAllCheckbox, allChecked);
            }
            finally
            {
                _syncingCheckboxes = false;
            }
        }

        private void ApplySelectAllState(Control selectAllCheckbox, IEnumerable<Control> children)
        {
            bool targetState = TweakExecutor.GetToggleState(selectAllCheckbox);

            _syncingCheckboxes = true;
            try
            {
                foreach (Control checkbox in children)
                    TweakExecutor.SetToggleState(checkbox, targetState);
            }
            finally
            {
                _syncingCheckboxes = false;
            }
        }

        private void SelectAllApps_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_syncingCheckboxes)
                return;

            ApplySelectAllState(SelectAllApps_CheckBox, _appCheckboxes.Keys);
        }

        private void SelectAllJunk_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_syncingCheckboxes)
                return;

            ApplySelectAllState(vortexCheckBox12, _junkCheckboxes.Keys);
        }

        private void AppCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (_syncingCheckboxes)
                return;

            SyncSelectAllState(SelectAllApps_CheckBox, _appCheckboxes.Keys);
        }

        private void JunkCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (_syncingCheckboxes)
                return;

            SyncSelectAllState(vortexCheckBox12, _junkCheckboxes.Keys);
        }

        private async void installApps_Click(object sender, EventArgs e)
        {
            List<AppDebloatTweak> selected = GetSelectedApps();

            if (selected.Count == 0)
            {
                TweakExecutor.ShowMessage("Select at least one app.", "No Selection");
                return;
            }

            installApps.Enabled = false;
            uninstallApps.Enabled = false;

            try
            {
                IEnumerable<string> fragments = selected.Select(t =>
                {
                    return $"echo Installing {t.Name}...{Environment.NewLine}{t.InstallBatch}";
                });

                string script = BuildCombinedBatch(fragments, includeEmptyDir: false);

                TweakResult result = await TweakExecutor.ExecuteBatchScriptAsync(
                    "InstallApps",
                    "Debloat",
                    "install",
                    script);

                if (!result.Success)
                {
                    TweakExecutor.ShowMessage(result.Message, "Install Failed");
                    return;
                }

                TweakExecutor.ShowMessage("Selected apps install triggered.", "Success");
            }
            finally
            {
                installApps.Enabled = true;
                uninstallApps.Enabled = true;
            }
        }

        private async void uninstallApps_Click(object sender, EventArgs e)
        {
            List<AppDebloatTweak> selected = GetSelectedApps();

            if (selected.Count == 0)
            {
                TweakExecutor.ShowMessage("Select at least one app.", "No Selection");
                return;
            }

            DialogResult confirm = VortexMessageBox.Show(
                $"Remove {selected.Count} selected app(s)? This cannot be undone easily.",
                "Confirm Uninstall",
                VortexMessageBox.MessageBoxButtons.YesNo);

            if (confirm != DialogResult.Yes)
                return;

            installApps.Enabled = false;
            uninstallApps.Enabled = false;

            try
            {
                IEnumerable<string> fragments = selected.Select(t =>
                {
                    return $"echo Removing {t.Name}...{Environment.NewLine}{t.UninstallBatch}";
                });

                string script = BuildCombinedBatch(fragments, includeEmptyDir: false);

                TweakResult result = await TweakExecutor.ExecuteBatchScriptAsync(
                    "UninstallApps",
                    "Debloat",
                    "remove",
                    script);

                if (!result.Success)
                {
                    TweakExecutor.ShowMessage(result.Message, "Uninstall Failed");
                    return;
                }

                TweakExecutor.ShowMessage("Selected apps removed successfully.", "Success");
            }
            finally
            {
                installApps.Enabled = true;
                uninstallApps.Enabled = true;
            }
        }

        private async void cleanSelectedJunk_Click(object sender, EventArgs e)
        {
            List<JunkCleanTweak> selected = GetSelectedJunkItems();

            if (selected.Count == 0)
            {
                TweakExecutor.ShowMessage("Select at least one cleanup item.", "No Selection");
                return;
            }

            DialogResult confirm = VortexMessageBox.Show(
                $"Run {selected.Count} selected cleanup task(s)? Some files may be permanently deleted.",
                "Confirm Cleanup",
                VortexMessageBox.MessageBoxButtons.YesNo);

            if (confirm != DialogResult.Yes)
                return;

            cleanSelectedJunk.Enabled = false;

            try
            {
                IEnumerable<string> fragments = selected.Select(t =>
                {
                    return $"echo {t.Name}...{Environment.NewLine}{t.CleanBatch}";
                });

                bool includeEmptyDir = selected.Any(t => t.UsesEmptyDir);
                string script = BuildCombinedBatch(fragments, includeEmptyDir);

                TweakResult result = await TweakExecutor.ExecuteBatchScriptAsync(
                    "CleanJunk",
                    "Debloat",
                    "clean",
                    script);

                if (!result.Success)
                {
                    TweakExecutor.ShowMessage(result.Message, "Cleanup Failed");
                    return;
                }

                TweakExecutor.ShowMessage("Selected cleanup tasks completed.", "Success");
            }
            finally
            {
                cleanSelectedJunk.Enabled = true;
            }
        }
    }
}
