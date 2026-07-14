using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility_NEW_ERA.Settings;
using ToX_Free_Utility_NEW_ERA.Tweaks;
using Vortex.Ui;

namespace ToX_Free_Utility_NEW_ERA.Pages
{
    public partial class advanced : UserControl
    {
        private PowerPlanConfig _config;
        private bool _syncingUi;

        public advanced()
        {
            InitializeComponent();
        }

        private void advanced_Load(object sender, EventArgs e)
        {
            _config = PowerPlanStore.Load();
            ApplyConfigToUi(_config);
            RefreshPreview();
        }

        private void ApplyConfigToUi(PowerPlanConfig config)
        {
            if (config == null)
                return;

            _syncingUi = true;
            try
            {
                vortexRoundTextBox1.Text = config.Name ?? string.Empty;
                vortexRoundTextBox2.Text = config.Description ?? string.Empty;

                TweakExecutor.SetToggleState(vortexRoundedSwitch56, config.FastStartup);
                TweakExecutor.SetToggleState(vortexRoundedSwitch57, config.DisableSleepMode);
                TweakExecutor.SetToggleState(vortexRoundedSwitch58, config.EnableHighPerformance);
                TweakExecutor.SetToggleState(vortexRoundedSwitch59, config.LowerScreenBrightness);

                vortexCheckBox23.Checked = config.TurnOffHardDisk;
                vortexCheckBox24.Checked = config.TurnOffDisplay;
            }
            finally
            {
                _syncingUi = false;
            }
        }

        private PowerPlanConfig ReadConfigFromUi()
        {
            if (_config == null)
                _config = PowerPlanStore.CreateDefault();

            _config.Name = vortexRoundTextBox1.Text?.Trim();
            _config.Description = vortexRoundTextBox2.Text?.Trim();

            _config.FastStartup = TweakExecutor.GetToggleState(vortexRoundedSwitch56);
            _config.DisableSleepMode = TweakExecutor.GetToggleState(vortexRoundedSwitch57);
            _config.EnableHighPerformance = TweakExecutor.GetToggleState(vortexRoundedSwitch58);
            _config.LowerScreenBrightness = TweakExecutor.GetToggleState(vortexRoundedSwitch59);

            _config.TurnOffHardDisk = vortexCheckBox23.Checked;
            _config.TurnOffDisplay = vortexCheckBox24.Checked;

            return _config;
        }

        private void RefreshPreview()
        {
            PowerPlanConfig config = ReadConfigFromUi();
            (int minPercent, int maxPercent) = PowerPlanBuilder.GetCpuBounds(config);

            vortexLabel101.Text = $"Min {minPercent}%";
            vortexLabel102.Text = $"Max {maxPercent}%";
            vortexLabel100.Text = $"{maxPercent}%";
            vortexProgressBar1.Value = Math.Max(0, Math.Min(100, maxPercent));
        }

        private void TweakSetting_Changed(object sender, EventArgs e)
        {
            if (_syncingUi)
                return;

            RefreshPreview();
        }

        private void vortexSidebarButton1_Click(object sender, EventArgs e)
        {
            using (var dialog = new CustomTweakDialog())
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
                    return;

                PowerPlanConfig config = ReadConfigFromUi();
                config.CustomTweaks.Add(new PowerPlanCustomTweak
                {
                    Name = dialog.TweakName,
                    Command = dialog.TweakCommand
                });

                string existingDescription = vortexRoundTextBox2.Text?.Trim();
                string tweakLine = $"+ {dialog.TweakName}";
                vortexRoundTextBox2.Text = string.IsNullOrWhiteSpace(existingDescription)
                    ? tweakLine
                    : existingDescription + Environment.NewLine + tweakLine;

                _config = config;
                TweakExecutor.ShowMessage(
                    $"Custom tweak '{dialog.TweakName}' added. It will run when you generate the plan.",
                    "Custom Tweak Added");
            }
        }

        private void vortexSidebarButton2_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Import Power Plan";
                dialog.Filter = "Power Plan JSON (*.json)|*.json|All files (*.*)|*.*";
                dialog.InitialDirectory = TweakStateStore.AppDataFolder;

                if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
                    return;

                try
                {
                    string json = File.ReadAllText(dialog.FileName);
                    PowerPlanConfig imported = JsonSerializer.Deserialize<PowerPlanConfig>(json);
                    if (imported == null)
                    {
                        TweakExecutor.ShowMessage("The selected file does not contain a valid power plan.", "Import Failed");
                        return;
                    }

                    _config = imported;
                    ApplyConfigToUi(_config);
                    RefreshPreview();
                    TweakExecutor.ShowMessage("Power plan imported successfully.", "Import Complete");
                }
                catch (Exception ex)
                {
                    TweakExecutor.ShowMessage($"Could not import power plan.\n\n{ex.Message}", "Import Failed");
                }
            }
        }

        private void vortexSidebarButton3_Click(object sender, EventArgs e)
        {
            PowerPlanConfig config = ReadConfigFromUi();

            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "Export Power Plan";
                dialog.Filter = "Power Plan JSON (*.json)|*.json|All files (*.*)|*.*";
                dialog.FileName = MakeSafeFileName(config.Name) + ".json";
                dialog.InitialDirectory = TweakStateStore.AppDataFolder;

                if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
                    return;

                try
                {
                    Directory.CreateDirectory(TweakStateStore.AppDataFolder);
                    string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(dialog.FileName, json);
                    TweakExecutor.ShowMessage("Power plan exported successfully.", "Export Complete");
                }
                catch (Exception ex)
                {
                    TweakExecutor.ShowMessage($"Could not export power plan.\n\n{ex.Message}", "Export Failed");
                }
            }
        }

        private async void vortexSidebarButton4_Click(object sender, EventArgs e)
        {
            PowerPlanConfig config = ReadConfigFromUi();

            if (string.IsNullOrWhiteSpace(config.Name))
            {
                TweakExecutor.ShowMessage("Enter a power plan name before generating.", "Missing Name");
                return;
            }

            if (!TweakExecutor.IsRunningAsAdministrator())
            {
                TweakExecutor.ShowMessage(
                    "Generating a power plan requires administrator privileges. Restart the app as administrator.",
                    "Administrator Required");
                return;
            }

            vortexSidebarButton4.Enabled = false;

            try
            {
                string script = PowerPlanBuilder.BuildBatchScript(config);
                TweakResult result = await TweakExecutor.ExecuteBatchScriptAsync(
                    "custom-power-plan",
                    "Advanced",
                    "generate",
                    script);

                if (!result.Success)
                {
                    TweakExecutor.ShowMessage(
                        $"Failed to generate the power plan.\n\n{result.Message}",
                        "Generation Failed");
                    return;
                }

                PowerPlanStore.Save(config);
                _config = config;

                TweakExecutor.ShowMessage(
                    $"Power plan '{config.Name}' was created and activated.\n\nGUID: {config.PlanGuid}",
                    "Plan Generated");
            }
            catch (Exception ex)
            {
                TweakExecutor.ShowMessage($"Unexpected error while generating plan.\n\n{ex}", "Generation Error");
            }
            finally
            {
                vortexSidebarButton4.Enabled = true;
            }
        }

        private void vortexSidebarButton5_Click(object sender, EventArgs e)
        {
            PowerPlanConfig config = ReadConfigFromUi();
            PowerPlanStore.Save(config);
            _config = config;
            TweakExecutor.ShowMessage("Power plan settings saved.", "Saved");
        }

        private void vortexSidebarButton6_Click(object sender, EventArgs e)
        {
            PowerPlanConfig config = ReadConfigFromUi();
            string guid = config.PlanGuid;

            if (string.IsNullOrWhiteSpace(guid))
            {
                guid = PowerPlanBuilder.EnsurePlanGuid(config);
                _config = config;
            }

            try
            {
                Clipboard.SetText(guid);
                TweakExecutor.ShowMessage($"Power plan GUID copied to clipboard.\n\n{guid}", "GUID Copied");
            }
            catch (Exception ex)
            {
                TweakExecutor.ShowMessage($"Could not copy GUID.\n\n{ex.Message}", "Copy Failed");
            }
        }

        private static string MakeSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "custom-power-plan";

            foreach (char invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '_');

            return value.Trim();
        }

        private sealed class CustomTweakDialog : Form
        {
            private readonly VortexRoundTextBox _nameBox;
            private readonly VortexRoundTextBox _commandBox;

            public string TweakName => _nameBox.Text?.Trim();
            public string TweakCommand => _commandBox.Text?.Trim();

            public CustomTweakDialog()
            {
                Text = "Add Custom Tweak";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MaximizeBox = false;
                MinimizeBox = false;
                ClientSize = new Size(420, 210);
                BackColor = Color.FromArgb(34, 34, 34);

                var nameLabel = new Label
                {
                    Text = "Tweak name",
                    ForeColor = Color.White,
                    Location = new Point(16, 16),
                    AutoSize = true
                };

                _nameBox = new VortexRoundTextBox
                {
                    Location = new Point(16, 38),
                    Size = new Size(388, 34),
                    PlaceholderText = "e.g. Disable USB selective suspend"
                };

                var commandLabel = new Label
                {
                    Text = "Command (powercfg or reg)",
                    ForeColor = Color.White,
                    Location = new Point(16, 82),
                    AutoSize = true
                };

                _commandBox = new VortexRoundTextBox
                {
                    Location = new Point(16, 104),
                    Size = new Size(388, 34),
                    PlaceholderText = "powercfg -setacvalueindex scheme_current SUB_USB ..."
                };

                var okButton = new Button
                {
                    Text = "Add",
                    DialogResult = DialogResult.OK,
                    Location = new Point(248, 156),
                    Size = new Size(75, 30)
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(329, 156),
                    Size = new Size(75, 30)
                };

                Controls.Add(nameLabel);
                Controls.Add(_nameBox);
                Controls.Add(commandLabel);
                Controls.Add(_commandBox);
                Controls.Add(okButton);
                Controls.Add(cancelButton);

                AcceptButton = okButton;
                CancelButton = cancelButton;
            }

            protected override void OnFormClosing(FormClosingEventArgs e)
            {
                if (DialogResult == DialogResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(TweakName) || string.IsNullOrWhiteSpace(TweakCommand))
                    {
                        TweakExecutor.ShowMessage("Enter both a tweak name and command.", "Missing Information");
                        e.Cancel = true;
                        return;
                    }
                }

                base.OnFormClosing(e);
            }
        }
    }
}
