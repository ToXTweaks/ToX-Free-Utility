using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    [Description("A compact breadcrumb control for displaying a section path like Home • Dashboard.")]
    public class VortexBreadcrumb : Control
    {
        private string _rootText = "Home";
        private string _currentText = "Dashboard";
        private Color _rootColor = Color.FromArgb(105, 105, 105);
        private Color _currentColor = Color.White;
        private Color _separatorColor = Color.FromArgb(95, 95, 95);
        private int _itemSpacing = 9;
        private const int SeparatorDiameter = 5;

        public VortexBreadcrumb()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            AutoSize = true;

            try { Font = new Font("Segoe UI", 9f, FontStyle.Regular); }
            catch { Font = SystemFonts.DefaultFont; }
        }

        [Category("Breadcrumb")]
        [DefaultValue("Home")]
        public string RootText
        {
            get => _rootText;
            set { _rootText = value ?? string.Empty; UpdateLayout(); }
        }

        [Category("Breadcrumb")]
        [DefaultValue("Dashboard")]
        public string CurrentText
        {
            get => _currentText;
            set { _currentText = value ?? string.Empty; UpdateLayout(); }
        }

        [Category("Appearance")]
        public Color RootColor
        {
            get => _rootColor;
            set { _rootColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color LeftTextColor
        {
            get => _rootColor;
            set { _rootColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color CurrentColor
        {
            get => _currentColor;
            set { _currentColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color RightTextColor
        {
            get => _currentColor;
            set { _currentColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color SeparatorColor
        {
            get => _separatorColor;
            set { _separatorColor = value; Invalidate(); }
        }

        [Category("Layout")]
        [DefaultValue(9)]
        public int ItemSpacing
        {
            get => _itemSpacing;
            set { _itemSpacing = Math.Max(0, value); UpdateLayout(); }
        }

        [DefaultValue(true)]
        public override bool AutoSize
        {
            get => base.AutoSize;
            set { base.AutoSize = value; UpdateLayout(); }
        }

        protected override Size DefaultSize => new Size(140, 20);

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size rootSize = MeasurePart(_rootText);
            Size separatorSize = HasSeparator ? new Size(SeparatorDiameter, SeparatorDiameter) : Size.Empty;
            Size currentSize = MeasurePart(_currentText);

            int visibleParts = 0;
            if (!string.IsNullOrEmpty(_rootText)) visibleParts++;
            if (HasSeparator) visibleParts++;
            if (!string.IsNullOrEmpty(_currentText)) visibleParts++;

            int spacingCount = Math.Max(0, visibleParts - 1);
            int width = rootSize.Width + separatorSize.Width + currentSize.Width + (_itemSpacing * spacingCount) + Padding.Horizontal;
            int height = Math.Max(rootSize.Height, Math.Max(separatorSize.Height, currentSize.Height)) + Padding.Vertical;

            return new Size(Math.Max(1, width), Math.Max(1, height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int x = Padding.Left;
            DrawPart(e.Graphics, _rootText, _rootColor, ref x, false);
            DrawSeparator(e.Graphics, ref x);
            DrawPart(e.Graphics, _currentText, _currentColor, ref x, false);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            UpdateLayout();
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            UpdateLayout();
        }

        private Size MeasurePart(string text, bool bold = false)
        {
            if (string.IsNullOrEmpty(text))
                return Size.Empty;

            Font measureFont = Font;
            using (Font? boldFont = bold ? new Font(Font, FontStyle.Bold) : null)
            {
                if (boldFont != null)
                    measureFont = boldFont;

                return TextRenderer.MeasureText(text, measureFont, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            }
        }

        private void DrawPart(Graphics graphics, string text, Color color, ref int x, bool bold)
        {
            if (string.IsNullOrEmpty(text))
                return;

            Size size = MeasurePart(text, bold);
            Rectangle rect = new Rectangle(x, Padding.Top, size.Width, Height - Padding.Vertical);

            Font drawFont = Font;
            using (Font? boldFont = bold ? new Font(Font, FontStyle.Bold) : null)
            {
                if (boldFont != null)
                    drawFont = boldFont;

                TextRenderer.DrawText(graphics, text, drawFont, rect, color,
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            x += size.Width + _itemSpacing;
        }

        private bool HasSeparator => !string.IsNullOrEmpty(_rootText) && !string.IsNullOrEmpty(_currentText);

        private void DrawSeparator(Graphics graphics, ref int x)
        {
            if (!HasSeparator)
                return;

            int contentHeight = Height - Padding.Vertical;
            int y = Padding.Top + ((contentHeight - SeparatorDiameter) / 2);
            Rectangle rect = new Rectangle(x, y, SeparatorDiameter, SeparatorDiameter);

            SmoothingMode previousMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush brush = new SolidBrush(_separatorColor))
                graphics.FillEllipse(brush, rect);
            graphics.SmoothingMode = previousMode;

            x += SeparatorDiameter + _itemSpacing;
        }

        private void UpdateLayout()
        {
            if (AutoSize)
                Size = GetPreferredSize(Size.Empty);

            Invalidate();
        }
    }
}
