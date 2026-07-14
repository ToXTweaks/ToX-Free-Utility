using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vortex.Ui;
using ToX_Free_Utility_NEW_ERA.Tweaks;
using Res = ToX_Free_Utility_NEW_ERA.Properties.Resources;

namespace ToX_Free_Utility_NEW_ERA.Controls
{

    public class TweakCard : UserControl
    {
        private readonly VortexPanel card = new VortexPanel();
        private readonly VortexLabel titleLabel = new VortexLabel();
        private readonly Label descLabel = new Label();
        private readonly FlowLayoutPanel tagsFlow = new FlowLayoutPanel();
        private readonly VortexRoundedSwitch toggle = new VortexRoundedSwitch();

        private bool suppressToggleEvent = false;

        public string TweakKey { get; private set; }
        public TweakConfigType ConfigType { get; private set; }

        public string Title => titleLabel.Text;
        public string Description => descLabel.Text;
        public string TagsRaw { get; private set; }

        public bool IsSelected
        {
            get { return toggle.Checked; }
            set
            {
                suppressToggleEvent = true;
                toggle.Checked = value;
                suppressToggleEvent = false;
            }
        }

        public event Func<TweakCard, bool, Task> TweakToggled;

        public TweakCard()
        {
            BuildUi();
            toggle.Click += Toggle_Click;
        }

        public void SetTweakData(string title, string description, string tags, string tweakKey, TweakConfigType configType)
        {
            titleLabel.Text = title ?? string.Empty;
            descLabel.Text = description ?? string.Empty;
            TweakKey = tweakKey;
            ConfigType = configType;
            TagsRaw = tags ?? string.Empty;
            toggle.Name = tweakKey; 
            BuildTags(tags);
        }

        public void SetBusy(bool busy)
        {
            toggle.Enabled = !busy;
            Cursor = busy ? Cursors.AppStarting : Cursors.Default;
        }

        private async void Toggle_Click(object sender, EventArgs e)
        {
            if (suppressToggleEvent)
                return;

            if (TweakToggled != null)
                await TweakToggled.Invoke(this, toggle.Checked);
            else
                await TweakExecutor.RunTweakToggleAsync(toggle, TweakKey, ConfigType);
        }

        private void BuildUi()
        {
            Size = new Size(580, 120);
            Margin = new Padding(12, 10, 12, 6);

            BackColor = Color.FromArgb(34, 34, 34);

            card.Dock = DockStyle.Fill;
            card.BackColor = Color.Transparent;
            card.BackgroundColor = Color.FromArgb(28, 28, 28);
            card.CornerRadius = 14;
            card.RoundCorners = true;
            Controls.Add(card);

            titleLabel.BackColor = Color.Transparent;
            titleLabel.ForeColor = Color.White;
            titleLabel.Font = new Font("Bahnschrift", 10F);
            titleLabel.Location = new Point(16, 14);
            titleLabel.Size = new Size(450, 18);
            titleLabel.UseAntiAlias = false;
            card.Controls.Add(titleLabel);

            descLabel.AutoSize = false;
            descLabel.BackColor = Color.Transparent;
            descLabel.ForeColor = Color.FromArgb(180, 180, 180);
            descLabel.Font = new Font("Bahnschrift", 9F);
            descLabel.Location = new Point(16, 38);
            descLabel.Size = new Size(470, 42);
            card.Controls.Add(descLabel);

            tagsFlow.AutoSize = false;
            tagsFlow.BackColor = Color.Transparent;
            tagsFlow.FlowDirection = FlowDirection.LeftToRight;
            tagsFlow.WrapContents = false;
            tagsFlow.Location = new Point(14, 84);
            tagsFlow.Size = new Size(450, 32);
            card.Controls.Add(tagsFlow);

            toggle.AccessibleName = "Toggle Switch";
            toggle.AnimationDuration = 200;
            toggle.BackColor = Color.Transparent;
            toggle.Checked = false;
            toggle.Cursor = Cursors.Hand;
            toggle.KnobColorOff = Color.White;
            toggle.KnobColorOn = Color.White;
            toggle.OffBackColor = Color.FromArgb(80, 80, 80);
            toggle.OnBackColor = Color.FromArgb(4, 120, 251);
            toggle.Size = new Size(40, 22);
            toggle.Location = new Point(520, 49);
            toggle.TrackPadding = 5;
            card.Controls.Add(toggle);
        }


        private struct TagStyle
        {
            public string Text;
            public Color Color;
            public Image Icon;
        }

        private static TagStyle MakeTag(string text, Color color, Image icon)
        {
            return new TagStyle { Text = text, Color = color, Icon = icon };
        }

        private static readonly Dictionary<string, TagStyle> TagMap =
            new Dictionary<string, TagStyle>(StringComparer.OrdinalIgnoreCase)
        {
            { "FeatureBreaking", MakeTag("FEATURE BREAKING", Color.FromArgb(255, 222, 33), Res.feature_break) },
            { "PowerHungry",     MakeTag("POWER HUNGRY",     Color.FromArgb(255, 94, 88),  Res.battery_full) },
            { "InProgress",      MakeTag("IN PROGRESS",      Color.FromArgb(7, 181, 96),   Res.inprogress) },
            { "Finished",        MakeTag("FINISHED",         Color.FromArgb(7, 181, 96),   Res.finished) },
            { "Beta",            MakeTag("BETA",             Color.FromArgb(124, 135, 238), Res.beta) },
            { "SafeToUse",       MakeTag("SAFE TO USE",      Color.FromArgb(7, 181, 96),   Res.finished) },
            { "Performance",     MakeTag("PERFORMANCE",      Color.FromArgb(80, 165, 245), null) },
        };

        private static string NormalizeTagKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            string compact = raw.Trim().Replace(" ", string.Empty).Replace("_", string.Empty);

            switch (compact.ToUpperInvariant())
            {
                case "SAFE":
                case "SAFETOUSE":
                    return "SafeToUse";
                case "FEATUREBREAKING":
                    return "FeatureBreaking";
                case "POWERHUNGRY":
                    return "PowerHungry";
                case "INPROGRESS":
                    return "InProgress";
                case "FINISHED":
                    return "Finished";
                case "BETA":
                    return "Beta";
                case "PERFORMANCE":
                    return "Performance";
                default:
                    return raw.Trim();
            }
        }

        private void BuildTags(string tags)
        {
            tagsFlow.Controls.Clear();

            if (string.IsNullOrWhiteSpace(tags))
                return;

            foreach (var raw in tags.Split(','))
            {
                string key = NormalizeTagKey(raw);
                if (key.Length == 0)
                    continue;

                TagStyle style;
                if (!TagMap.TryGetValue(key, out style))
                    style = MakeTag(key.ToUpperInvariant(), Color.FromArgb(150, 150, 150), null);

                var tag = new VortexTag
                {
                    Text = style.Text,
                    AutoSize = true,
                    BackgroundColor = Color.FromArgb(28, 28, 28),
                    TextColor = style.Color,
                    Font = new Font("Bahnschrift", 8.25F),
                    Margin = new Padding(0, 0, 6, 0),
                    BorderRadius = 6,
                    ContentPadding = new Padding(8, 4, 8, 4)
                };

                if (style.Icon != null)
                {
                    tag.Icon = style.Icon;
                    tag.IconColor = style.Color;
                    tag.TintIcon = true;
                    tag.IconSize = new Size(14, 14);
                    tag.ShowIcon = true;
                }
                else
                {
                    tag.ShowIcon = false;
                }

                tag.FitToContent();
                tagsFlow.Controls.Add(tag);
            }
        }
    }
}
