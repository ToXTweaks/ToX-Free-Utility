using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Vortex.Ui
{
    internal static class Gfx
    {
        public static GraphicsPath RoundedPath(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0) return path;
            if (radius <= 0.5f) { path.AddRectangle(rect); return path; }
            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);
            float d = radius * 2f;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // ============================================================
    //  VortexColorPicker : كومبوبوكس (swatch + سهم) ينزّل قائمة منتقي ألوان ناعمة
    // ============================================================
    [ToolboxItem(true)]
    [DefaultEvent("ColorChanged")]
    [DefaultProperty("SelectedColor")]
    public class VortexColorPicker : Control
    {
        private Color _selectedColor = Color.FromArgb(255, 124, 77, 255);
        private Color _boxColor = Color.FromArgb(16, 16, 20);
        private Color _borderColor = Color.FromArgb(60, 60, 70);
        private Color _hoverBorderColor = Color.FromArgb(166, 92, 92);
        private int _cornerRadius = 10;
        private bool _showText = true;

        private bool _isHovered = false;
        private bool _isOpen = false;
        private int _lastCloseTime = -10000;

        private ColorPickerPanel _panel;
        private LayeredColorPopup _popup;

        private Color _panelBackgroundColor = Color.FromArgb(28, 28, 34);
        private Color _panelTextColor = Color.White;
        private Color _accentColor = Color.FromArgb(166, 92, 92);
        private Color _panelValueBoxColor = Color.FromArgb(18, 18, 22);

        public event EventHandler<ColorChangedEventArgs> ColorChanged;

        [Category("Appearance")]
        public Color SelectedColor
        {
            get => _selectedColor;
            set { if (_selectedColor == value) return; _selectedColor = value; _panel?.SetColor(value); Invalidate(); }
        }

        [Category("Appearance")] public Color BoxColor { get => _boxColor; set { _boxColor = value; Invalidate(); } }
        [Category("Appearance")] public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        [Category("Appearance")] public Color HoverBorderColor { get => _hoverBorderColor; set { _hoverBorderColor = value; Invalidate(); } }
        [Category("Appearance"), DefaultValue(10)] public int CornerRadius { get => _cornerRadius; set { _cornerRadius = Math.Max(0, value); Invalidate(); } }
        [Category("Appearance"), DefaultValue(true)] public bool ShowText { get => _showText; set { _showText = value; Invalidate(); } }

        [Category("Picker Theme")]
        public Color BackgroundColor { get => _panelBackgroundColor; set { _panelBackgroundColor = value; if (_panel != null) _panel.PanelBackColor = value; } }
        [Category("Picker Theme")]
        public Color TextColor { get => _panelTextColor; set { _panelTextColor = value; if (_panel != null) _panel.PanelTextColor = value; } }
        [Category("Picker Theme")]
        public Color ValueBoxColor { get => _panelValueBoxColor; set { _panelValueBoxColor = value; if (_panel != null) _panel.ValueBoxColor = value; } }
        [Category("Picker Theme")]
        public Color ActiveTabColor { get => _accentColor; set { _accentColor = value; if (_panel != null) _panel.AccentColor = value; } }
        [Browsable(false)] public Color InactiveTabColor { get; set; } = Color.FromArgb(16, 16, 20);
        [Browsable(false)] public Color TabHoverColor { get => _accentColor; set => _accentColor = value; }

        public VortexColorPicker()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(230, 40);
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 9f, FontStyle.Regular);
        }

        private void EnsureDropDown()
        {
            if (_popup != null) return;

            _panel = new ColorPickerPanel();
            _panel.PanelBackColor = _panelBackgroundColor;
            _panel.PanelTextColor = _panelTextColor;
            _panel.ValueBoxColor = _panelValueBoxColor;
            _panel.AccentColor = _accentColor;
            _panel.SetColor(_selectedColor);
            _panel.ColorChanged += OnPanelColorChanged;

            _popup = new LayeredColorPopup(_panel);
            _popup.Closed += (s, e) => { _isOpen = false; _lastCloseTime = Environment.TickCount; Invalidate(); };
        }

        private void OnPanelColorChanged(object sender, ColorChangedEventArgs e)
        {
            _selectedColor = e.Color;
            Invalidate();
            ColorChanged?.Invoke(this, e);
        }

        private void ToggleDropDown()
        {
            if (_isOpen) { _popup?.ClosePopup(); return; }
            EnsureDropDown();
            _panel.SetColor(_selectedColor);
            _isOpen = true;
            Invalidate();
            _popup.ShowAt(PointToScreen(new Point(0, Height + 4)));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            if (Environment.TickCount - _lastCloseTime < 250) return;
            ToggleDropDown();
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _isHovered = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _isHovered = false; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 2 || Height < 2) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            float radius = Math.Min(_cornerRadius, Math.Min(Width, Height) / 2f);

            using (var path = Gfx.RoundedPath(rect, radius))
            {
                using (var b = new SolidBrush(_boxColor)) g.FillPath(b, path);
                using (var pen = new Pen(_isHovered ? _hoverBorderColor : _borderColor, 1f)) g.DrawPath(pen, path);
            }

            // swatch
            int pad = 6;
            int sw = Height - pad * 2;
            if (sw > 0)
            {
                var srect = new RectangleF(pad + 0.5f, pad + 0.5f, sw - 1f, sw - 1f);
                float sradius = Math.Max(0, radius - 3f);
                using (var sPath = Gfx.RoundedPath(srect, sradius))
                {
                    if (_selectedColor.A < 255)
                    {
                        var clip = g.Clip;
                        g.SetClip(sPath);
                        DrawCheckerboard(g, new Rectangle(pad, pad, sw, sw), 4);
                        g.Clip = clip;
                    }
                    using (var b = new SolidBrush(_selectedColor)) g.FillPath(b, sPath);
                    using (var pen = new Pen(Color.FromArgb(70, 255, 255, 255), 1f)) g.DrawPath(pen, sPath);
                }
            }

            // chevron
            int chevW = 10, chevH = 6;
            int cx = Width - 14 - chevW / 2;
            int cy = Height / 2 - chevH / 2;
            using (var pen = new Pen(Color.FromArgb(180, 200, 200, 205), 1.6f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                g.DrawLines(pen, new[]
                {
                    new PointF(cx, cy),
                    new PointF(cx + chevW / 2f, cy + chevH),
                    new PointF(cx + chevW, cy)
                });
            }

            // hex text
            if (_showText && sw > 0)
            {
                string hex = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}";
                int textX = pad + sw + 10;
                var textRect = new Rectangle(textX, 0, cx - textX - 6, Height);
                if (textRect.Width > 0)
                    TextRenderer.DrawText(g, hex, Font, textRect, Color.FromArgb(210, 210, 215),
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            }
        }

        internal static void DrawCheckerboard(Graphics g, Rectangle rect, int checkSize)
        {
            using (var b1 = new SolidBrush(Color.FromArgb(200, 200, 200)))
            using (var b2 = new SolidBrush(Color.FromArgb(160, 160, 160)))
            {
                for (int x = rect.X; x < rect.Right; x += checkSize)
                    for (int y = rect.Y; y < rect.Bottom; y += checkSize)
                    {
                        var cr = new Rectangle(x, y, Math.Min(checkSize, rect.Right - x), Math.Min(checkSize, rect.Bottom - y));
                        bool first = ((x - rect.X) / checkSize + (y - rect.Y) / checkSize) % 2 == 0;
                        g.FillRectangle(first ? b1 : b2, cr);
                    }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_panel != null) _panel.ColorChanged -= OnPanelColorChanged;
                _popup?.Dispose();
                _panel?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ============================================================
    //  LayeredColorPopup : نافذة منبثقة بـ per-pixel alpha (أركان دائرية ناعمة)
    // ============================================================
    internal sealed class LayeredColorPopup : Form, IMessageFilter
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize,
            IntPtr hdcSrc, ref Point pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BLENDFUNCTION { public byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat; }

        private const int ULW_ALPHA = 2;
        private const byte AC_SRC_OVER = 0, AC_SRC_ALPHA = 1;
        private const int WS_EX_LAYERED = 0x80000, WS_EX_TOOLWINDOW = 0x80, WS_EX_NOACTIVATE = 0x08000000;
        private const int WM_LBUTTONDOWN = 0x0201, WM_RBUTTONDOWN = 0x0204, WM_MBUTTONDOWN = 0x0207, WM_NCLBUTTONDOWN = 0x00A1;

        private readonly ColorPickerPanel _panel;
        private bool _filterAdded;

        public event EventHandler Closed;

        public LayeredColorPopup(ColorPickerPanel panel)
        {
            _panel = panel;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Size = panel.Size;
            _panel.ColorChanged += (s, e) => RenderContent();
            _panel.NeedsRedraw += () => RenderContent();
        }

        // النافذة ماتاخدش activation عشان ماتسرقش الفوكس من الفورم وماتتقفلش غلط
        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get { var cp = base.CreateParams; cp.ExStyle |= WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE; return cp; }
        }

        public void ShowAt(Point screenPos)
        {
            Location = screenPos;
            if (!Visible) Show();
            else BringToFront();
            if (!_filterAdded) { Application.AddMessageFilter(this); _filterAdded = true; }
            RenderContent();
        }

        public void ClosePopup()
        {
            if (_filterAdded) { Application.RemoveMessageFilter(this); _filterAdded = false; }
            if (Visible)
            {
                Hide();
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        // تتقفل فقط عند الضغط خارج القائمة (مش عند تحريك الماوس)
        public bool PreFilterMessage(ref Message m)
        {
            if (!Visible) return false;
            if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_RBUTTONDOWN || m.Msg == WM_MBUTTONDOWN || m.Msg == WM_NCLBUTTONDOWN)
            {
                if (!Bounds.Contains(Cursor.Position))
                    ClosePopup();
            }
            return false;
        }

        protected override void OnMouseDown(MouseEventArgs e) { _panel.HandleMouseDown(e.Location); base.OnMouseDown(e); }
        protected override void OnMouseMove(MouseEventArgs e) { _panel.HandleMouseMove(e.Location); base.OnMouseMove(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _panel.HandleMouseUp(); base.OnMouseUp(e); }

        private void RenderContent()
        {
            if (!IsHandleCreated || !Visible) return;
            int w = Math.Max(1, Width), h = Math.Max(1, Height);

            using (var bmp = new Bitmap(w, h, PixelFormat.Format32bppPArgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    _panel.RenderTo(g);
                }
                SetBitmap(bmp);
            }
        }

        private void SetBitmap(Bitmap bmp)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBmp = bmp.GetHbitmap(Color.FromArgb(0));
            IntPtr oldBmp = SelectObject(memDc, hBmp);
            try
            {
                var size = new Size(bmp.Width, bmp.Height);
                var src = new Point(0, 0);
                var dst = new Point(Left, Top);
                var blend = new BLENDFUNCTION
                {
                    BlendOp = AC_SRC_OVER,
                    BlendFlags = 0,
                    SourceConstantAlpha = 255,
                    AlphaFormat = AC_SRC_ALPHA
                };
                UpdateLayeredWindow(Handle, screenDc, ref dst, ref size, memDc, ref src, 0, ref blend, ULW_ALPHA);
            }
            finally
            {
                SelectObject(memDc, oldBmp);
                DeleteObject(hBmp);
                DeleteDC(memDc);
                ReleaseDC(IntPtr.Zero, screenDc);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_filterAdded) { Application.RemoveMessageFilter(this); _filterAdded = false; }
                _panel?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ============================================================
    //  ColorPickerPanel : حالة + رسم منتقي الألوان (كله AA دائري ناعم)
    // ============================================================
    internal sealed class ColorPickerPanel : Control
    {
        private Color _selectedColor = Color.Red;
        private float _hue = 0f, _saturation = 1f, _brightness = 1f, _alpha = 1f;

        private Color _backgroundColor = Color.FromArgb(28, 28, 34);
        private Color _textColor = Color.White;
        private Color _valueBoxColor = Color.FromArgb(18, 18, 22);
        private Color _accentColor = Color.FromArgb(166, 92, 92);
        private Font _font = new Font("Segoe UI", 9f, FontStyle.Regular);
        private bool _isHexMode = true;

        private Rectangle _svBoxRect, _hueRect, _alphaRect, _previewRect, _valueRect;
        private Point _svSel;
        private int _huePos, _alphaPos;
        private bool _dragSV, _dragHue, _dragAlpha;

        private Bitmap _svCache;
        private float _cachedHue = -1f;
        private Size _cachedSvSize = Size.Empty;

        public event EventHandler<ColorChangedEventArgs> ColorChanged;
        public event Action NeedsRedraw;   // عشان الـ popup يعيد الرسم

        public Color PanelBackColor { get => _backgroundColor; set { _backgroundColor = value; NeedsRedraw?.Invoke(); } }
        public Color PanelTextColor { get => _textColor; set { _textColor = value; NeedsRedraw?.Invoke(); } }
        public Color ValueBoxColor { get => _valueBoxColor; set { _valueBoxColor = value; NeedsRedraw?.Invoke(); } }
        public Color AccentColor { get => _accentColor; set { _accentColor = value; NeedsRedraw?.Invoke(); } }
        public Color SelectedColor => _selectedColor;

        public ColorPickerPanel()
        {
            Size = new Size(250, 292);
            RecalcLayout();
        }

        public void SetColor(Color color)
        {
            _selectedColor = color;
            var hsb = ColorToHSB(color);
            _hue = hsb.H; _saturation = hsb.S; _brightness = hsb.B;
            _alpha = color.A / 255f;
            SyncPositions();
            NeedsRedraw?.Invoke();
        }

        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); RecalcLayout(); SyncPositions(); }

        private void RecalcLayout()
        {
            int pad = 16;
            int w = Math.Max(1, Width - pad * 2);
            int sliderH = 14;

            _svBoxRect = new Rectangle(pad, pad, w, 150);
            int hueY = _svBoxRect.Bottom + 18;
            _hueRect = new Rectangle(pad, hueY, w, sliderH);
            int alphaY = _hueRect.Bottom + 14;
            _alphaRect = new Rectangle(pad, alphaY, w, sliderH);

            int bottomY = _alphaRect.Bottom + 18;
            int previewSize = 34;
            _previewRect = new Rectangle(pad, bottomY, previewSize, previewSize);
            _valueRect = new Rectangle(pad + previewSize + 10, bottomY, Math.Max(1, w - previewSize - 10), previewSize);
        }

        // ====== الرسم ======
        public void RenderTo(Graphics g)
        {
            if (Width < 2 || Height < 2) return;

            // كارت الخلفية (أركان دائرية ناعمة - الكورنرز شفافة)
            var cardRect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            using (var path = Gfx.RoundedPath(cardRect, 14f))
            {
                using (var b = new SolidBrush(_backgroundColor)) g.FillPath(b, path);
                using (var pen = new Pen(Color.FromArgb(70, 70, 80), 1f)) g.DrawPath(pen, path);
            }

            DrawSVBox(g);
            DrawHueSlider(g);
            DrawAlphaSlider(g);
            DrawPreviewAndValue(g);
        }

        protected override void OnPaint(PaintEventArgs e) { RenderTo(e.Graphics); }

        private void DrawSVBox(Graphics g)
        {
            if (_svBoxRect.Width <= 0 || _svBoxRect.Height <= 0) return;

            if (_svCache == null || _cachedHue != _hue || _cachedSvSize != _svBoxRect.Size)
            {
                _svCache?.Dispose();
                _svCache = BuildSVBitmap(_svBoxRect.Width, _svBoxRect.Height, _hue);
                _cachedHue = _hue; _cachedSvSize = _svBoxRect.Size;
            }

            var rf = new RectangleF(_svBoxRect.X + 0.5f, _svBoxRect.Y + 0.5f, _svBoxRect.Width - 1f, _svBoxRect.Height - 1f);
            using (var path = Gfx.RoundedPath(rf, 9f))
            using (var tb = new TextureBrush(_svCache))
            {
                tb.TranslateTransform(_svBoxRect.X, _svBoxRect.Y);
                g.FillPath(tb, path);          // FillPath بالـ AA = أركان ناعمة
                using (var pen = new Pen(Color.FromArgb(70, 0, 0, 0), 1f)) g.DrawPath(pen, path);
            }

            // مؤشر الاختيار
            var c = new PointF(_svBoxRect.X + _svSel.X, _svBoxRect.Y + _svSel.Y);
            using (var pen = new Pen(Color.White, 2f)) g.DrawEllipse(pen, c.X - 6.5f, c.Y - 6.5f, 13f, 13f);
            using (var pen = new Pen(Color.FromArgb(130, 0, 0, 0), 1f)) g.DrawEllipse(pen, c.X - 7.5f, c.Y - 7.5f, 15f, 15f);
        }

        private Bitmap BuildSVBitmap(int w, int h, float hue)
        {
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            int[] pixels = new int[w * h];
            for (int y = 0; y < h; y++)
            {
                float brightness = 1f - (float)y / (h - 1);
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    float saturation = (float)x / (w - 1);
                    pixels[row + x] = ColorFromHSB(hue, saturation, brightness).ToArgb();
                }
            }
            var data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            bmp.UnlockBits(data);
            return bmp;
        }

        private void DrawHueSlider(Graphics g)
        {
            if (_hueRect.Width <= 0 || _hueRect.Height <= 0) return;
            var rf = new RectangleF(_hueRect.X + 0.5f, _hueRect.Y + 0.5f, _hueRect.Width - 1f, _hueRect.Height - 1f);
            using (var path = Gfx.RoundedPath(rf, rf.Height / 2f))
            using (var brush = new LinearGradientBrush(new RectangleF(_hueRect.X, _hueRect.Y, _hueRect.Width, _hueRect.Height), Color.Red, Color.Red, LinearGradientMode.Horizontal))
            {
                var colors = new[] { Color.Red, Color.Yellow, Color.Lime, Color.Cyan, Color.Blue, Color.Magenta, Color.Red };
                var positions = new[] { 0f, 0.16f, 0.33f, 0.5f, 0.66f, 0.83f, 1f };
                brush.InterpolationColors = new ColorBlend { Colors = colors, Positions = positions };
                g.FillPath(brush, path);
            }
            DrawHandle(g, _hueRect.X + _huePos, _hueRect.Y + _hueRect.Height / 2f, ColorFromHSB(_hue, 1f, 1f));
        }

        private void DrawAlphaSlider(Graphics g)
        {
            if (_alphaRect.Width <= 0 || _alphaRect.Height <= 0) return;
            var rf = new RectangleF(_alphaRect.X + 0.5f, _alphaRect.Y + 0.5f, _alphaRect.Width - 1f, _alphaRect.Height - 1f);
            using (var path = Gfx.RoundedPath(rf, rf.Height / 2f))
            {
                var clip = g.Clip;
                g.SetClip(path);
                VortexColorPicker.DrawCheckerboard(g, _alphaRect, 5);
                g.Clip = clip;

                var left = Color.FromArgb(0, _selectedColor.R, _selectedColor.G, _selectedColor.B);
                var right = Color.FromArgb(255, _selectedColor.R, _selectedColor.G, _selectedColor.B);
                using (var brush = new LinearGradientBrush(new RectangleF(_alphaRect.X, _alphaRect.Y, _alphaRect.Width, _alphaRect.Height), left, right, LinearGradientMode.Horizontal))
                    g.FillPath(brush, path);
            }
            var solid = Color.FromArgb(255, _selectedColor.R, _selectedColor.G, _selectedColor.B);
            DrawHandle(g, _alphaRect.X + _alphaPos, _alphaRect.Y + _alphaRect.Height / 2f, solid);
        }

        private void DrawHandle(Graphics g, float cx, float cy, Color innerColor)
        {
            float r = 9f;
            using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, cx - r, cy - r, r * 2f, r * 2f);
            using (var pen = new Pen(Color.FromArgb(60, 0, 0, 0), 1f)) g.DrawEllipse(pen, cx - r, cy - r, r * 2f, r * 2f);
            using (var b = new SolidBrush(innerColor)) g.FillEllipse(b, cx - r + 3f, cy - r + 3f, (r - 3f) * 2f, (r - 3f) * 2f);
        }

        private void DrawPreviewAndValue(Graphics g)
        {
            if (_previewRect.Width > 0)
            {
                var rf = new RectangleF(_previewRect.X + 0.5f, _previewRect.Y + 0.5f, _previewRect.Width - 1f, _previewRect.Height - 1f);
                using (var path = Gfx.RoundedPath(rf, 8f))
                {
                    if (_selectedColor.A < 255)
                    {
                        var clip = g.Clip; g.SetClip(path);
                        VortexColorPicker.DrawCheckerboard(g, _previewRect, 5);
                        g.Clip = clip;
                    }
                    using (var b = new SolidBrush(_selectedColor)) g.FillPath(b, path);
                    using (var p = new Pen(Color.FromArgb(80, 255, 255, 255), 1f)) g.DrawPath(p, path);
                }
            }

            if (_valueRect.Width > 0)
            {
                var rf = new RectangleF(_valueRect.X + 0.5f, _valueRect.Y + 0.5f, _valueRect.Width - 1f, _valueRect.Height - 1f);
                using (var path = Gfx.RoundedPath(rf, 8f))
                {
                    using (var b = new SolidBrush(_valueBoxColor)) g.FillPath(b, path);
                    using (var p = new Pen(Color.FromArgb(70, 70, 80), 1f)) g.DrawPath(p, path);
                }

                string text = _isHexMode
                    ? $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}"
                    : $"{_selectedColor.R}, {_selectedColor.G}, {_selectedColor.B}, {_selectedColor.A}";

                var textRect = Rectangle.Inflate(_valueRect, -10, 0);
                TextRenderer.DrawText(g, text, _font, textRect, _textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
            }
        }

        // ====== التفاعل (يُستدعى من الـ popup) ======
        public void HandleMouseDown(Point p)
        {
            if (_svBoxRect.Contains(p)) { _dragSV = true; UpdateSV(p); }
            else if (HitSlider(_hueRect, p)) { _dragHue = true; UpdateHue(p.X); }
            else if (HitSlider(_alphaRect, p)) { _dragAlpha = true; UpdateAlpha(p.X); }
            else if (_valueRect.Contains(p)) { _isHexMode = !_isHexMode; NeedsRedraw?.Invoke(); }
        }

        public void HandleMouseMove(Point p)
        {
            if (_dragSV) UpdateSV(p);
            else if (_dragHue) UpdateHue(p.X);
            else if (_dragAlpha) UpdateAlpha(p.X);
        }

        public void HandleMouseUp() { _dragSV = _dragHue = _dragAlpha = false; }

        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); HandleMouseDown(e.Location); }
        protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); HandleMouseMove(e.Location); }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); HandleMouseUp(); }

        private static bool HitSlider(Rectangle r, Point p) => Rectangle.Inflate(r, 4, 8).Contains(p);

        private void UpdateSV(Point loc)
        {
            int x = Math.Max(0, Math.Min(_svBoxRect.Width - 1, loc.X - _svBoxRect.X));
            int y = Math.Max(0, Math.Min(_svBoxRect.Height - 1, loc.Y - _svBoxRect.Y));
            _saturation = (float)x / (_svBoxRect.Width - 1);
            _brightness = 1f - (float)y / (_svBoxRect.Height - 1);
            RaiseColor();
        }

        private void UpdateHue(int mouseX)
        {
            int rel = Math.Max(0, Math.Min(_hueRect.Width - 1, mouseX - _hueRect.X));
            _hue = (float)rel / (_hueRect.Width - 1) * 360f;
            RaiseColor();
        }

        private void UpdateAlpha(int mouseX)
        {
            int rel = Math.Max(0, Math.Min(_alphaRect.Width - 1, mouseX - _alphaRect.X));
            _alpha = (float)rel / (_alphaRect.Width - 1);
            RaiseColor();
        }

        private void RaiseColor()
        {
            var c = ColorFromHSB(_hue, _saturation, _brightness);
            _selectedColor = Color.FromArgb((int)Math.Round(_alpha * 255), c.R, c.G, c.B);
            SyncPositions();
            ColorChanged?.Invoke(this, new ColorChangedEventArgs(_selectedColor));
            NeedsRedraw?.Invoke();
        }

        private void SyncPositions()
        {
            if (_svBoxRect.Width > 1 && _svBoxRect.Height > 1)
            {
                _svSel.X = (int)(_saturation * (_svBoxRect.Width - 1));
                _svSel.Y = (int)((1f - _brightness) * (_svBoxRect.Height - 1));
            }
            if (_hueRect.Width > 1) _huePos = (int)((_hue / 360f) * (_hueRect.Width - 1));
            if (_alphaRect.Width > 1) _alphaPos = (int)(_alpha * (_alphaRect.Width - 1));
        }

        private Color ColorFromHSB(float hue, float saturation, float brightness)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);
            brightness = brightness * 255;
            int v = Convert.ToInt32(brightness);
            int p = Convert.ToInt32(brightness * (1 - saturation));
            int q = Convert.ToInt32(brightness * (1 - f * saturation));
            int t = Convert.ToInt32(brightness * (1 - (1 - f) * saturation));
            if (hi == 0) return Color.FromArgb(255, v, t, p);
            if (hi == 1) return Color.FromArgb(255, q, v, p);
            if (hi == 2) return Color.FromArgb(255, p, v, t);
            if (hi == 3) return Color.FromArgb(255, p, q, v);
            if (hi == 4) return Color.FromArgb(255, t, p, v);
            return Color.FromArgb(255, v, p, q);
        }

        private (float H, float S, float B) ColorToHSB(Color color)
        {
            float r = color.R / 255f, g = color.G / 255f, b = color.B / 255f;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;
            float hue = 0;
            if (delta != 0)
            {
                if (max == r) hue = ((g - b) / delta) % 6;
                else if (max == g) hue = (b - r) / delta + 2;
                else hue = (r - g) / delta + 4;
            }
            hue *= 60;
            if (hue < 0) hue += 360;
            float saturation = max == 0 ? 0 : delta / max;
            return (hue, saturation, max);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _font?.Dispose(); _svCache?.Dispose(); }
            base.Dispose(disposing);
        }
    }

    public class ColorChangedEventArgs : EventArgs
    {
        public Color Color { get; }
        public ColorChangedEventArgs(Color color) { Color = color; }
    }
}
