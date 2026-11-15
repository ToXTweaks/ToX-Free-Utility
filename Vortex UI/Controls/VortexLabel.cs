using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    [Description("A smooth, anti-aliased label control that behaves exactly like a standard Label with AutoSize enabled by default.")]
    public class VortexLabel : Label
    {
        // True when hosted inside the Visual Studio designer process. Rendering the full
        // parent surface per control to fake transparency is far too expensive for the
        // designer (hundreds of controls), so we fall back to a cheap solid fill there.
        internal static readonly bool IsDesignerProcess = VortexDesign.IsDesignerProcess;

        private TextRenderingHint _textRenderingHint = TextRenderingHint.AntiAlias;
        private bool _useAntiAlias = false;
        private bool _useClearType = false;
        private Bitmap? _transparentBackgroundCache;
        private Size _transparentBackgroundCacheSize = Size.Empty;
        private Point _transparentBackgroundCacheLocation = Point.Empty;
        private Control? _transparentBackgroundCacheParent;
        private int _transparentBackgroundCacheZOrder = -1;
        private bool _buildingTransparentBackground = false;
        private bool _suppressTransparentBackgroundForSnapshot = false;

        public VortexLabel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.Opaque, true);

            BackColor = Color.Transparent;
            ForeColor = Color.White;

            try
            {
                Font = new Font("Bahnschrift", 10, FontStyle.Regular);
            }
            catch
            {
                Font = new Font("Segoe UI", 10, FontStyle.Regular);
            }

            // ✅ AutoSize ON by default (like a normal Label that expands to fit text)
            AutoSize = true;

            UpdateTextRenderingHint();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            InvalidateTransparentBackground();
            Invalidate();
        }

        [Category("Appearance")]
        [Description("Determines whether anti-aliasing is used for text rendering.")]
        [DefaultValue(true)]
        public bool UseAntiAlias
        {
            get => _useAntiAlias;
            set
            {
                if (_useAntiAlias != value)
                {
                    _useAntiAlias = value;
                    UpdateTextRenderingHint();
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("Determines whether ClearType is used for text rendering.")]
        [DefaultValue(false)]
        public bool UseClearType
        {
            get => _useClearType;
            set
            {
                if (_useClearType != value)
                {
                    _useClearType = value;
                    UpdateTextRenderingHint();
                    Invalidate();
                }
            }
        }

        [DefaultValue(true)]
        public override bool AutoSize
        {
            get => base.AutoSize;
            set => base.AutoSize = value;
        }

        private void UpdateTextRenderingHint()
        {
            if (_useClearType)
                _textRenderingHint = TextRenderingHint.ClearTypeGridFit;
            else if (_useAntiAlias)
                _textRenderingHint = TextRenderingHint.AntiAlias;
            else
                _textRenderingHint = TextRenderingHint.SystemDefault;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (string.IsNullOrEmpty(Text))
                return new Size(Padding.Horizontal, Font.Height + Padding.Vertical);

            bool unconstrained = proposedSize.Width <= 0;
            TextFormatFlags flags = GetTextFormatFlagsForMeasure();

            bool hasLineBreaks = Text.Contains("\n") || Text.Contains("\r");

            if (unconstrained)
            {
                if (!hasLineBreaks)
                    flags |= TextFormatFlags.SingleLine;

                flags |= TextFormatFlags.NoPrefix;
                Size measured = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue), flags);
                return new Size(measured.Width + Padding.Horizontal, measured.Height + Padding.Vertical);
            }
            else
            {
                flags |= TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix;
                int maxWidth = proposedSize.Width;
                if (MaximumSize.Width > 0)
                    maxWidth = Math.Min(maxWidth, MaximumSize.Width);

                Size measured = TextRenderer.MeasureText(Text, Font, new Size(maxWidth, int.MaxValue), flags);
                int finalWidth = Math.Min(measured.Width, maxWidth);
                return new Size(finalWidth + Padding.Horizontal, measured.Height + Padding.Vertical);
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (AutoSize)
            {
                Size pref = GetPreferredSize(Size.Empty);
                width = pref.Width;
                height = pref.Height;
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (AutoSize)
                AdjustSize();
            else
                Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (AutoSize)
                AdjustSize();
            else
                Invalidate();
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            if (AutoSize)
                AdjustSize();
            else
                Invalidate();
        }

        protected override void OnAutoSizeChanged(EventArgs e)
        {
            base.OnAutoSizeChanged(e);
            if (AutoSize)
                AdjustSize();
            else
                Invalidate();
        }

        private void AdjustSize()
        {
            Size pref = GetPreferredSize(Size.Empty);
            Size = pref;
        }

        private void InvalidateTransparentBackground()
        {
            if (_transparentBackgroundCache != null)
            {
                _transparentBackgroundCache.Dispose();
                _transparentBackgroundCache = null;
            }

            _transparentBackgroundCacheSize = Size.Empty;
            _transparentBackgroundCacheLocation = Point.Empty;
            _transparentBackgroundCacheParent = null;
            _transparentBackgroundCacheZOrder = -1;
        }

        internal void ResetTransparentBackgroundCache()
        {
            InvalidateTransparentBackground();
            Invalidate();
        }

        internal void DrawTransparentContentToBitmap(Bitmap bitmap)
        {
            _suppressTransparentBackgroundForSnapshot = true;
            try
            {
                DrawToBitmap(bitmap, new Rectangle(Point.Empty, Size));
            }
            finally
            {
                _suppressTransparentBackgroundForSnapshot = false;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (BackColor.A == 0)
            {
                if (_suppressTransparentBackgroundForSnapshot) return;
                PaintTransparentBackground(e.Graphics);
                return;
            }

            base.OnPaintBackground(e);
        }

        private void PaintTransparentBackground(Graphics graphics)
        {
            if (Parent == null || Width <= 0 || Height <= 0)
                return;

            // Design-time: skip the expensive parent snapshot, just fill with the parent color.
            if (IsDesignerProcess)
            {
                using (SolidBrush brush = new SolidBrush(GetFallbackBackColor()))
                    graphics.FillRectangle(brush, ClientRectangle);
                return;
            }

            int zOrder = GetParentZOrder();
            if (_transparentBackgroundCache == null ||
                _transparentBackgroundCacheSize != ClientSize ||
                _transparentBackgroundCacheLocation != Location ||
                _transparentBackgroundCacheParent != Parent ||
                _transparentBackgroundCacheZOrder != zOrder)
            {
                BuildTransparentBackgroundCache(zOrder);
            }

            if (_transparentBackgroundCache != null)
                graphics.DrawImageUnscaled(_transparentBackgroundCache, 0, 0);
        }

        private void BuildTransparentBackgroundCache(int zOrder)
        {
            InvalidateTransparentBackground();

            Control? parent = Parent;
            if (parent == null || Width <= 0 || Height <= 0 || _buildingTransparentBackground)
                return;

            Bitmap bitmap = new Bitmap(Width, Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                using (SolidBrush brush = new SolidBrush(GetFallbackBackColor()))
                    graphics.FillRectangle(brush, 0, 0, Width, Height);

                PaintParentSurface(graphics, parent);
                PaintCoveredSiblingSurfaces(graphics, parent, zOrder);
            }

            _transparentBackgroundCache = bitmap;
            _transparentBackgroundCacheSize = ClientSize;
            _transparentBackgroundCacheLocation = Location;
            _transparentBackgroundCacheParent = parent;
            _transparentBackgroundCacheZOrder = zOrder;
        }

        private void PaintParentSurface(Graphics graphics, Control parent)
        {
            GraphicsState state = graphics.Save();
            try
            {
                _buildingTransparentBackground = true;
                graphics.TranslateTransform(-Left, -Top);
                using (PaintEventArgs pe = new PaintEventArgs(graphics, Bounds))
                {
                    InvokePaintBackground(parent, pe);
                    InvokePaint(parent, pe);
                }
            }
            catch
            {
                // A solid fallback is already drawn; keep the label stable if a parent cannot be rendered off-screen.
            }
            finally
            {
                _buildingTransparentBackground = false;
                graphics.Restore(state);
            }
        }

        private void PaintCoveredSiblingSurfaces(Graphics graphics, Control parent, int zOrder)
        {
            if (zOrder < 0)
                return;

            Rectangle labelBounds = Bounds;
            for (int i = parent.Controls.Count - 1; i > zOrder; i--)
            {
                Control sibling = parent.Controls[i];
                if (sibling == this || !sibling.Visible || sibling.Width <= 0 || sibling.Height <= 0)
                    continue;

                Rectangle intersection = Rectangle.Intersect(labelBounds, sibling.Bounds);
                if (intersection.IsEmpty)
                    continue;

                PaintSiblingSurface(graphics, sibling, intersection);
            }
        }

        private void PaintSiblingSurface(Graphics graphics, Control sibling, Rectangle intersection)
        {
            try
            {
                using (Bitmap siblingBitmap = new Bitmap(sibling.Width, sibling.Height))
                {
                    sibling.DrawToBitmap(siblingBitmap, new Rectangle(Point.Empty, sibling.Size));

                    Rectangle source = new Rectangle(
                        intersection.X - sibling.Left,
                        intersection.Y - sibling.Top,
                        intersection.Width,
                        intersection.Height);

                    Rectangle destination = new Rectangle(
                        intersection.X - Left,
                        intersection.Y - Top,
                        intersection.Width,
                        intersection.Height);

                    graphics.DrawImage(siblingBitmap, destination, source, GraphicsUnit.Pixel);
                }
            }
            catch
            {
                // Some WinForms controls cannot DrawToBitmap reliably; ignore them instead of repainting live.
            }
        }

        private int GetParentZOrder()
        {
            try
            {
                return Parent?.Controls.GetChildIndex(this, false) ?? -1;
            }
            catch
            {
                return -1;
            }
        }

        private Color GetFallbackBackColor()
        {
            Control? parent = Parent;
            while (parent != null)
            {
                if (parent.BackColor.A > 0)
                    return parent.BackColor;

                parent = parent.Parent;
            }

            return SystemColors.Control;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            InvalidateTransparentBackground();
            Invalidate();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            InvalidateTransparentBackground();
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            InvalidateTransparentBackground();
            Invalidate();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            InvalidateTransparentBackground();
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                InvalidateTransparentBackground();

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (BackColor.A == 0)
            {
                if (!_suppressTransparentBackgroundForSnapshot) PaintTransparentBackground(e.Graphics);
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(BackColor))
                    e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            if (string.IsNullOrEmpty(Text))
                return;

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.TextRenderingHint = _textRenderingHint;

            Rectangle rect = ClientRectangle;

            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            TextFormatFlags flags = TextFormatFlags.NoPrefix;

            if (!UseMnemonic)
                flags |= TextFormatFlags.NoPrefix;

            bool hasLineBreaks = Text.Contains("\n") || Text.Contains("\r");

            if (AutoSize && !hasLineBreaks)
                flags |= TextFormatFlags.SingleLine;
            else
                flags |= TextFormatFlags.WordBreak;

            // Horizontal Alignment
            switch (TextAlign)
            {
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    flags |= TextFormatFlags.HorizontalCenter;
                    break;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    flags |= TextFormatFlags.Right;
                    break;
                default: // Left alignment
                    flags |= TextFormatFlags.Left;
                    break;
            }

            // Vertical Alignment
            switch (TextAlign)
            {
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.MiddleRight:
                    flags |= TextFormatFlags.VerticalCenter;
                    break;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomRight:
                    flags |= TextFormatFlags.Bottom;
                    break;
                default: // Top alignment
                    flags |= TextFormatFlags.Top;
                    break;
            }

            TextRenderer.DrawText(e.Graphics, Text, Font, rect, ForeColor, flags);
        }

        private TextFormatFlags GetTextFormatFlagsForMeasure()
        {
            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.Top;
            if (!UseMnemonic)
                flags |= TextFormatFlags.NoPrefix;
            return flags;
        }
    }
}
