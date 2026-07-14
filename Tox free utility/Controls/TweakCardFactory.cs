using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Vortex.Ui;
using ToX_Free_Utility_NEW_ERA.Settings;
using ToX_Free_Utility_NEW_ERA.Tweaks;

namespace ToX_Free_Utility_NEW_ERA.Controls
{
    public static class TweakCardFactory
    {
        public static VortexPanel BuildPage(Control root, params string[] keys)
        {
            int h = Math.Max(160, root.Height - 78 - 24);
            return AddHost(root, new Point(36, 78), new Size(622, h), keys);
        }

        public static VortexPanel BuildCategoryPage(Control root, TweakConfigType configType)
        {
            return BuildPage(root, TweakCatalog.GetCategoryKeys(configType));
        }

        public static VortexPanel FillPanel(Control container, params string[] keys)
        {
            int w = Math.Max(200, container.Width - 16);
            int h = Math.Max(160, container.Height - 12);
            return AddHost(container, new Point(8, 6), new Size(w, h), keys);
        }

        private static VortexPanel AddHost(Control parent, Point location, Size size, string[] keys)
        {
            var host = new VortexPanel
            {
                Location = location,
                Size = size,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent,
                BackgroundColor = Color.FromArgb(34, 34, 34),
                CornerRadius = 14,
                RoundCorners = true,
                AutoScrollMargin = new Size(0, 20),
                EnableCustomScrollbar = true,
                AutoHideScrollbar = false,
                UseBitmapScrollSnapshots = false,
                ScrollbarThickness = 6,
                ScrollbarCornerRadius = 4,
                ScrollbarOffset = 8,
                ScrollbarThumbColor = Color.FromArgb(4, 120, 251),
                ScrollbarThumbHoverColor = Color.FromArgb(4, 120, 251),
                ScrollbarThumbActiveColor = Color.FromArgb(4, 120, 251),
                ScrollbarTrackColor = Color.FromArgb(34, 34, 34),
                ScrollSpeed = 50
            };

            parent.Controls.Add(host);
            host.BringToFront();

            var catalog = TweakCatalog.Load();
            int x = 16;
            int y = 14;
            foreach (var key in keys ?? new string[0])
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;
                if (!TryFindCategory(catalog, key, out TweakConfigType type))
                    continue;

                EmbeddedTweak tweak = catalog[type][key];

                var card = CreateCard(tweak, type);
                card.Location = new Point(x, y);
                host.Controls.Add(card);

                y += card.Height + 12;
            }

            return host;
        }

        private static TweakCard CreateCard(EmbeddedTweak tweak, TweakConfigType type)
        {
            var card = new TweakCard();
            card.SetTweakData(
                string.IsNullOrWhiteSpace(tweak.Name) ? tweak.Key : tweak.Name,
                tweak.Description,
                tweak.Tags,
                tweak.Key,
                type);

            if (TweakStateStore.TryGetState(type, tweak.Key, out bool enabled))
                card.IsSelected = enabled;

            return card;
        }

        private static bool TryFindCategory(
            Dictionary<TweakConfigType, Dictionary<string, EmbeddedTweak>> catalog,
            string key,
            out TweakConfigType type)
        {
            foreach (var pair in catalog)
            {
                if (pair.Value != null && pair.Value.ContainsKey(key))
                {
                    type = pair.Key;
                    return true;
                }
            }

            type = default;
            return false;
        }
    }
}
