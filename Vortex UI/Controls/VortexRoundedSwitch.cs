using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexRoundedSwitch : Control
    {
        // Appearance
        private Color _onBackColor = Color.FromArgb(255, 99, 132); // Pink/Red like in the new image
        private Color _offBackColor = Color.FromArgb(75, 85, 99); // Dark gray for OFF state
        private Color _knobColorOn = Color.White;
        private Color _knobColorOff = Color.White;
        private int _trackPadding = 4; // More padding for cleaner look
        private bool _fillFollowsKnob = false; // Simple color change like in image

        // New rounding and knob border controls
        private int _trackRound = -1; // -1 => auto (height/2)
        private int _knobRound = -1;  // -1 => auto (knobSize/2)
        private Color _knobBorderColor = Color.FromArgb(30, 0, 0, 0);
        private float _knobBorderWidth = 0.5f;
        // Behavior/Animation
        private bool _checked = false;
        private int _animationDuration = 250; // Smoother, longer animation like in GIF
        private float _transition = 0f; // 0(off) -> 1(on)
        private CancellationTokenSource _cts;

        public VortexRoundedSwitch()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(40, 22); // More elongated like in image
            MinimumSize = new Size(40,22);
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleName = "Toggle Switch";
        }

        #region Properties
        [Category("Appearance")]
        [Description("Track color when ON")] 
        public Color OnBackColor { get => _onBackColor; set { _onBackColor = value; Invalidate(); } }

        [Category("Appearance")]
        [Description("Track color when OFF")] 
        public Color OffBackColor { get => _offBackColor; set { _offBackColor = value; Invalidate(); } }

        [Category("Appearance")]
        [Description("Knob color when switch is ON")] 
        public Color KnobColorOn { get => _knobColorOn; set { _knobColorOn = value; Invalidate(); } }

        [Category("Appearance")]
        [Description("Knob color when switch is OFF")] 
        public Color KnobColorOff { get => _knobColorOff; set { _knobColorOff = value; Invalidate(); } }

        [Category("Layout")]
        [Description("Inner padding between the track and the knob")] 
        public int TrackPadding { get => _trackPadding; set { _trackPadding = Math.Max(0, value); Invalidate(); } }

        [Category("Behavior")]
        [Description("Current state of the switch")]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                OnCheckedChanged(EventArgs.Empty);
                StartAnimation(_checked);
            }
        }

        [Category("Animation")]
        [Description("Duration of the ON/OFF animation in milliseconds")]
        public int AnimationDuration { get => _animationDuration; set => _animationDuration = Math.Max(50, value); }

        public event EventHandler CheckedChanged;
        #endregion

        protected virtual void OnCheckedChanged(EventArgs e) => CheckedChanged?.Invoke(this, e);

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked; // toggles and animates
            base.OnClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                Checked = !Checked;
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        private void CancelAnimation()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void StartAnimation(bool target)
        {
            CancelAnimation();
            _cts = new CancellationTokenSource();
            _ = AnimateTo(target ? 1f : 0f, _cts.Token);
        }

        private async Task AnimateTo(float target, CancellationToken token)
        {
            try
            {
                float from = _transition;
                int duration = _animationDuration;
                var sw = Stopwatch.StartNew();
                const int frameDelay = 8; // ~120 FPS for extra smoothness
                while (!token.IsCancellationRequested)
                {
                    float t = (float)(sw.Elapsed.TotalMilliseconds / duration);
                    if (t >= 1f) break;
                    float eased = EaseInOutCubic(Math.Max(0f, Math.Min(1f, t)));
                    _transition = from + (target - from) * eased;

                    if (IsHandleCreated && !IsDisposed) BeginInvoke(new Action(Invalidate)); else break;
                    await Task.Delay(frameDelay, token);
                }
                _transition = target;
                if (IsHandleCreated && !IsDisposed) BeginInvoke(new Action(Invalidate));
            }
            catch (TaskCanceledException) { }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"RoundedSwitch animation error: {ex.Message}"); }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) CancelAnimation();
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            int pad = _trackPadding;

            // Track rectangle (no border)
            RectangleF trackRect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);

            float autoTrackRadius = trackRect.Height / 2f;
            float radius = _trackRound >= 0 ? Math.Min(_trackRound, (int)autoTrackRadius) : autoTrackRadius;

            using (GraphicsPath trackPath = CreateRoundRect(trackRect, radius))
            {
                Color trackColor = Interpolate(_offBackColor, _onBackColor, _transition);
                if (!Enabled) trackColor = Color.FromArgb(120, trackColor);
                using (SolidBrush trackBrush = new SolidBrush(trackColor))
                    g.FillPath(trackBrush, trackPath);
            }

            // Knob calculations (exact centering within track)
            float knobDiameter = Math.Max(2f, trackRect.Height - 2f * pad);
            float leftX = trackRect.X + pad;
            float rightX = trackRect.Right - pad - knobDiameter;
            float knobX = leftX + (rightX - leftX) * _transition;
            float knobY = trackRect.Y + (trackRect.Height - knobDiameter) / 2f; // perfect vertical centering relative to track
            RectangleF knobRect = new RectangleF(knobX, knobY, knobDiameter, knobDiameter);

            // If a custom knob rounding is set, draw as rounded rectangle else as circle
            float knobRadius = _knobRound >= 0 ? Math.Min(_knobRound, (int)(knobDiameter / 2f)) : knobDiameter / 2f;

            Color currentKnobColor = Interpolate(_knobColorOff, _knobColorOn, _transition);
            DrawSmoothKnob(g, knobRect, knobRadius, Enabled ? currentKnobColor : Color.FromArgb(200, currentKnobColor));
        }

        private void DrawSmoothKnob(Graphics g, RectangleF knobRect, float knobRadius, Color knobColor)
        {
            const int scale = 4;
            float margin = Math.Max(2f, _knobBorderWidth + 2f);
            RectangleF renderBounds = knobRect;
            renderBounds.Inflate(margin, margin);

            int bitmapWidth = Math.Max(1, (int)Math.Ceiling(renderBounds.Width * scale));
            int bitmapHeight = Math.Max(1, (int)Math.Ceiling(renderBounds.Height * scale));

            using (Bitmap buffer = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format32bppPArgb))
            using (Graphics bg = Graphics.FromImage(buffer))
            {
                bg.Clear(Color.Transparent);
                bg.SmoothingMode = SmoothingMode.AntiAlias;
                bg.InterpolationMode = InterpolationMode.HighQualityBicubic;
                bg.PixelOffsetMode = PixelOffsetMode.HighQuality;
                bg.CompositingQuality = CompositingQuality.HighQuality;
                bg.ScaleTransform(scale, scale);
                bg.TranslateTransform(-renderBounds.X, -renderBounds.Y);

                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(28, 0, 0, 0)))
                {
                    RectangleF sh = knobRect;
                    sh.Offset(0.6f, 0.9f);
                    using (GraphicsPath shp = CreateKnobPath(sh, knobRadius))
                        bg.FillPath(shadowBrush, shp);
                }

                using (SolidBrush knobBrush = new SolidBrush(knobColor))
                using (GraphicsPath knobPath = CreateKnobPath(knobRect, knobRadius))
                    bg.FillPath(knobBrush, knobPath);

                if (_knobBorderWidth > 0f)
                {
                    RectangleF borderRect = knobRect;
                    float borderInset = _knobBorderWidth / 2f;
                    borderRect.Inflate(-borderInset, -borderInset);
                    float borderRadius = Math.Max(0f, knobRadius - borderInset);

                    using (Pen kp = new Pen(_knobBorderColor, _knobBorderWidth))
                    using (GraphicsPath borderPath = CreateKnobPath(borderRect, borderRadius))
                    {
                        kp.LineJoin = LineJoin.Round; // smooth border joins
                        bg.DrawPath(kp, borderPath);
                    }
                }

                g.DrawImage(buffer, renderBounds);
            }
        }

        private static GraphicsPath CreateKnobPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float maxRadius = Math.Min(rect.Width, rect.Height) / 2f;
            if (radius >= maxRadius - 0.01f && Math.Abs(rect.Width - rect.Height) < 0.01f)
            {
                path.AddEllipse(rect);
                path.CloseFigure();
                return path;
            }

            path.Dispose();
            return CreateRoundRect(rect, radius);
        }

        private static GraphicsPath CreateRoundRect(RectangleF rect, float radius)
        {
            float d = radius * 2f;
            GraphicsPath path = new GraphicsPath();
            if (d <= 0f)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }
            float r = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);
            d = r * 2f;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Color Interpolate(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            int r = (int)(a.R + (b.R - a.R) * t);
            int g = (int)(a.G + (b.G - a.G) * t);
            int bl = (int)(a.B + (b.B - a.B) * t);
            int al = (int)(a.A + (b.A - a.A) * t);
            return Color.FromArgb(al, r, g, bl);
        }

        private static float EaseInOutCubic(float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return t < 0.5f ? 4f * t * t * t : 1f - (float)Math.Pow(-2f * t + 2f, 3f) / 2f;
        }
    }
}
