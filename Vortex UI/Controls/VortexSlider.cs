using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexSlider : Control
    {
        // core
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;

        // animation state
        private float _animatedValue = 0f;

        // appearance
        private int _trackHeight = 8;
        private float _trackRadius = 4f; // corner radius for track background
        private Color _trackColor = Color.FromArgb(60, 60, 60);
        private Color _trackFillColor = Color.FromArgb(0, 188, 212);

        // thumb
        private bool _showThumb = true;
        private int _thumbWidth = 22;   // default reduced
        private int _thumbHeight = 12;  // default reduced
        private float _thumbRadius = 2f;
        private Color _thumbColor = Color.White;
        // NOTE: Thumb border properties removed as requested

        // timing + animation
        private Timer _animationTimer;
        private const int TimerIntervalMs = 16; // ~60 FPS

        // hover / ripple state
        private float _hoverAlpha = 0f; // 0..1 - affects only fill
        private bool _isHovering = false;

        // dragging state
        private bool _isDragging = false;

        // ripple data (single ripple at a time)
        private bool _rippleActive = false;
        private int _rippleStartTick = 0;
        private int _rippleDurationMs = 200;
        private Point _rippleCenter;
        private float _rippleSizeFactor = 0.1f;
        private float _rippleOpacity = 0.4f; // 0..1

        [Category("Animation"), Description("Speed of smooth ease-out (higher = faster, typical 0.08..0.3)")]
        [DefaultValue(0.18)]
        public double SmoothSpeed { get; set; } = 0.30;

        [Category("Hover"), Description("Enable hover fade effect on the filled portion.")]
        [DefaultValue(true)]
        public bool HoverFadeEnabled { get; set; } = true;

        [Category("Hover"), Description("Hover fade duration in milliseconds.")]
        [DefaultValue(100)]
        public int HoverFadeDuration { get; set; } = 100;

        [Category("Ripple"), Description("Enable ripple on click.")]
        [DefaultValue(true)]
        public bool RippleEnabled { get; set; } = true;

        [Category("Ripple"), Description("Ripple duration in milliseconds.")]
        [DefaultValue(300)]
        public int RippleDuration
        {
            get => _rippleDurationMs;
            set => _rippleDurationMs = Math.Max(1, value);
        }

        [Category("Ripple"), Description("Ripple maximum opacity (0..1).")]
        [DefaultValue(0.4f)]
        public float RippleOpacity
        {
            get => _rippleOpacity;
            set => _rippleOpacity = Math.Max(0f, Math.Min(1f, value));
        }

        [Category("Ripple"), Description("Ripple size factor (1.0 = width, >1 expands).")]
        [DefaultValue(1.2f)]
        public float RippleSizeFactor
        {
            get => _rippleSizeFactor;
            set => _rippleSizeFactor = Math.Max(0.1f, value);
        }

        // Public events
        public event EventHandler ValueChanged;

        #region Behavior/Appearance properties
        [Category("Behavior"), DefaultValue(0)]
        public int Minimum
        {
            get => _minimum;
            set
            {
                if (value < _maximum)
                {
                    _minimum = value;
                    if (_value < _minimum) Value = _minimum;
                    Invalidate();
                }
            }
        }

        [Category("Behavior"), DefaultValue(100)]
        public int Maximum
        {
            get => _maximum;
            set
            {
                if (value > _minimum)
                {
                    _maximum = value;
                    if (_value > _maximum) Value = _maximum;
                    Invalidate();
                }
            }
        }

        [Category("Behavior"), DefaultValue(0)]
        public int Value
        {
            get => _value;
            set
            {
                int newValue = (int)Math.Round(MathHelper.Clamp(value, _minimum, _maximum));
                if (_value != newValue)
                {
                    _value = newValue;
                    ValueChanged?.Invoke(this, EventArgs.Empty);

                    // Design-time should reflect immediately
                    if (DesignMode)
                    {
                        _animatedValue = _value;
                        Invalidate();
                    }
                    else
                    {
                        // runtime: animate toward new Value
                    }
                }
            }
        }
        #endregion

        [Category("Appearance"), DefaultValue(6)]
        public int TrackHeight
        {
            get => _trackHeight;
            set
            {
                _trackHeight = Math.Max(2, value);
                // ensure control height can fit track and thumb
                Height = Math.Max(_trackHeight, _thumbHeight);
                Invalidate();
            }
        }

        [Category("Appearance"), DefaultValue(4f), Description("Corner radius for the track background.")]
        public float TrackRadius
        {
            get => _trackRadius;
            set
            {
                _trackRadius = Math.Max(0f, value);
                Invalidate();
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // allow height to be at least max(track, thumb)
            int enforcedHeight = Math.Max(_trackHeight, _thumbHeight);
            base.SetBoundsCore(x, y, width, enforcedHeight, specified);
        }

        [Category("Appearance")]
        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color TrackFillColor
        {
            get => _trackFillColor;
            set { _trackFillColor = value; Invalidate(); }
        }

        #region Thumb properties
        [Category("Appearance"), DefaultValue(true), Description("Show or hide the draggable thumb.")]
        public bool ShowThumb
        {
            get => _showThumb;
            set
            {
                _showThumb = value;
                // adjust height if necessary
                Height = Math.Max(_trackHeight, _thumbHeight);
                Invalidate();
            }
        }

        [Category("Appearance"), DefaultValue(12), Description("Thumb width in pixels.")]
        public int ThumbWidth
        {
            get => _thumbWidth;
            set
            {
                _thumbWidth = Math.Max(2, value);
                Height = Math.Max(_trackHeight, _thumbHeight);
                Invalidate();
            }
        }

        [Category("Appearance"), DefaultValue(12), Description("Thumb height in pixels.")]
        public int ThumbHeight
        {
            get => _thumbHeight;
            set
            {
                _thumbHeight = Math.Max(2, value);
                Height = Math.Max(_trackHeight, _thumbHeight);
                Invalidate();
            }
        }

        [Category("Appearance"), DefaultValue(6f), Description("Thumb corner radius. Set 0 for square thumb.")]
        public float ThumbRadius
        {
            get => _thumbRadius;
            set
            {
                _thumbRadius = Math.Max(0f, value);
                Invalidate();
            }
        }

        [Category("Appearance"), DefaultValue(typeof(Color), "White"), Description("Thumb fill color.")]
        public Color ThumbColor
        {
            get => _thumbColor;
            set { _thumbColor = value; Invalidate(); }
        }
        #endregion

        public VortexSlider()
        {
            // painting styles
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Size = new Size(200, Math.Max(10, _thumbHeight));

            // timer
            _animationTimer = new Timer { Interval = TimerIntervalMs };
            _animationTimer.Tick += AnimationTimer_Tick;
            // start in OnHandleCreated to avoid design-time issues
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _animatedValue = _value; // start synced
            if (!DesignMode)
                _animationTimer.Start(); // runtime only
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            try
            {
                if (_animationTimer != null)
                {
                    _animationTimer.Stop();
                }
            }
            finally
            {
                base.OnHandleDestroyed(e);
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (DesignMode) return;

            float dt = TimerIntervalMs / 1000f;
            float target = _value;
            float diff = target - _animatedValue;

            // smoothing factor (frame-rate-aware approximation)
            float factor = 1f - (float)Math.Exp(-SmoothSpeed * dt * 60f);
            _animatedValue += diff * factor;

            // small snap
            if (Math.Abs(diff) < 0.01f)
                _animatedValue = target;

            // hover smoothing
            if (HoverFadeEnabled)
            {
                float hoverTarget = _isHovering ? 1f : 0f;
                _hoverAlpha += (hoverTarget - _hoverAlpha) * Math.Min(1f, (float)TimerIntervalMs / Math.Max(1, HoverFadeDuration));
            }

            // ripple expiry
            if (_rippleActive)
            {
                int now = Environment.TickCount;
                if (now - _rippleStartTick > _rippleDurationMs)
                    _rippleActive = false;
            }

            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovering = true;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovering = false;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                // start dragging mode
                _isDragging = true;

                // set Value (but do NOT force animatedValue here so click animates smoothly)
                UpdateValueFromMouse(e.X, immediate: false);

                // ripple
                if (RippleEnabled)
                {
                    _rippleActive = true;
                    _rippleStartTick = Environment.TickCount;
                    _rippleCenter = new Point(e.X, e.Y);
                }

                Capture = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging && (e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // while dragging, update immediately so pointer feels snappy
                UpdateValueFromMouse(e.X, immediate: true);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;
                Capture = false;
            }
        }

        /// <summary>
        /// Update value from mouse position. If immediate==true then animatedValue is set immediately (drag).
        /// If immediate==false (click), we only set Value so animation interpolates towards it.
        /// Maps mouseX to thumb center range if thumb shown.
        /// </summary>
        private void UpdateValueFromMouse(int mouseX, bool immediate)
        {
            int trackWidth = Math.Max(1, Width);

            float effectiveMinX = (_showThumb ? _thumbWidth / 2f : 0f);
            float effectiveMaxX = (_showThumb ? trackWidth - _thumbWidth / 2f : trackWidth);

            float percent;
            if (effectiveMaxX - effectiveMinX <= 0.0001f)
                percent = 0f;
            else
                percent = (mouseX - effectiveMinX) / (effectiveMaxX - effectiveMinX);

            percent = MathHelper.Clamp(percent, 0f, 1f);
            int newValue = _minimum + (int)Math.Round((_maximum - _minimum) * percent);

            Value = newValue;

            if (immediate)
            {
                _animatedValue = newValue;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (BackColor == Color.Transparent && Parent != null && VortexDesign.IsDesignerProcess)
            {
                using (var brush = new SolidBrush(Parent.BackColor.A > 0 ? Parent.BackColor : System.Drawing.SystemColors.Control))
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
            }
            else if (BackColor == Color.Transparent && Parent != null)
            {
                var g = e.Graphics;
                var state = g.Save();
                try
                {
                    g.TranslateTransform(-Left, -Top);
                    var rect = new Rectangle(Left, Top, Width, Height);
                    var pe = new PaintEventArgs(g, rect);
                    InvokePaintBackground(Parent, pe);
                    InvokePaint(Parent, pe);
                }
                finally { g.Restore(state); }
            }
            else base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // safety
            if (Width <= 0 || Height <= 0) return;
            if (_maximum == _minimum) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighSpeed;

            int trackY = (Height - _trackHeight) / 2;
            int trackWidth = Width;

            // compute fill based on animated value
            float percent = (_animatedValue - _minimum) / (float)Math.Max(1, (_maximum - _minimum));
            percent = Math.Max(0f, Math.Min(1f, percent));
            int fillWidth = (int)(trackWidth * percent);

            // draw background track (uses track radius)
            using (var trackBrush = new SolidBrush(_trackColor))
            {
                // if track radius <= 0 use rectangle
                if (_trackRadius <= 0f)
                    g.FillRectangle(trackBrush, 0, trackY, trackWidth, _trackHeight);
                else
                    g.FillRoundedRectangle(trackBrush, 0, trackY, trackWidth, _trackHeight, _trackRadius);
            }

            // --- DRAW FILL with rounded ends that depend on fillWidth ---
            if (fillWidth > 0)
            {
                // choose radius for the filled portion:
                // - cannot exceed track radius
                // - cannot exceed half of track height
                // - cannot exceed half of fill width (so small fills become circular)
                float fillRadius = Math.Min(_trackRadius, _trackHeight / 2f);
                fillRadius = Math.Min(fillRadius, fillWidth / 2f);

                Color hoverBlend = InterpolateColor(_trackFillColor, Color.White, 0.28f * _hoverAlpha);
                using (var fillBrush = new SolidBrush(hoverBlend))
                {
                    if (fillRadius <= 0f)
                        g.FillRectangle(fillBrush, 0, trackY, fillWidth, _trackHeight);
                    else
                        g.FillRoundedRectangle(fillBrush, 0, trackY, fillWidth, _trackHeight, fillRadius);
                }
            }

            // draw ripple on top if active (subtle)
            if (_rippleActive)
            {
                int now = Environment.TickCount;
                int elapsed = now - _rippleStartTick;
                float progress = MathHelper.Clamp(elapsed / (float)_rippleDurationMs, 0f, 1f);

                // ease-out for ripple opacity and size
                float sizeProgress = 1f - (float)Math.Pow(1f - progress, 2f); // ease out
                float alphaProgress = 1f - progress;

                // ripple color derived from trackFillColor, lighter, with opacity
                Color baseRipple = InterpolateColor(_trackFillColor, Color.White, 0.2f);
                int a = (int)(255f * (_rippleOpacity * alphaProgress));
                a = Math.Max(0, Math.Min(255, a));
                Color rippleColor = Color.FromArgb(a, baseRipple.R, baseRipple.G, baseRipple.B);

                using (var rb = new SolidBrush(rippleColor))
                {
                    // radius: make relative to control width and the configured factor
                    float maxRadius = Math.Max(Width, Height) * RippleSizeFactor;
                    float radius = maxRadius * sizeProgress;

                    // center is ripple center from mouse; draw ellipse centered at point
                    RectangleF r = new RectangleF(
                        _rippleCenter.X - radius / 2f,
                        _rippleCenter.Y - radius / 2f,
                        radius,
                        radius);

                    var oldMode = g.CompositingMode;
                    g.CompositingMode = CompositingMode.SourceOver;
                    g.FillEllipse(rb, r);
                    g.CompositingMode = oldMode;
                }

                if (progress >= 1f)
                    _rippleActive = false;
            }

            // draw thumb if enabled (no border)
            if (_showThumb)
            {
                // thumb center: respect thumb size so it does not go outside track
                float centerX = (_thumbWidth / 2f) + percent * (trackWidth - _thumbWidth);
                float centerY = Height / 2f;

                float tx = centerX - _thumbWidth / 2f;
                float ty = centerY - _thumbHeight / 2f;

                // Draw thumb: if ThumbRadius == 0 => rectangle, else rounded rect
                using (var tb = new SolidBrush(_thumbColor))
                {
                    if (_thumbRadius <= 0f)
                        g.FillRectangle(tb, tx, ty, _thumbWidth, _thumbHeight);
                    else
                        g.FillRoundedRectangle(tb, tx, ty, _thumbWidth, _thumbHeight, Math.Min(_thumbRadius, Math.Min(_thumbWidth, _thumbHeight) / 2f));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_animationTimer != null)
                {
                    _animationTimer.Stop();
                    _animationTimer.Tick -= AnimationTimer_Tick;
                    _animationTimer.Dispose();
                    _animationTimer = null;
                }
            }
            base.Dispose(disposing);
        }

        // helper color lerp
        private static Color InterpolateColor(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t)
            );
        }

        // Rounded rectangle helper that returns GraphicsPath (used when needed)
        private static GraphicsPath CreateRoundedRectPath(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0f)
            {
                path.AddRectangle(bounds);
                return path;
            }

            float d = radius * 2f;
            d = Math.Min(d, Math.Min(bounds.Width, bounds.Height));
            var arc = new RectangleF(bounds.X, bounds.Y, d, d);

            // top-left
            path.AddArc(arc, 180, 90);

            // top edge
            path.AddLine(bounds.X + radius, bounds.Y, bounds.Right - radius, bounds.Y);

            // top-right
            arc.X = bounds.Right - d;
            path.AddArc(arc, 270, 90);

            // right edge
            path.AddLine(bounds.Right, bounds.Y + radius, bounds.Right, bounds.Bottom - radius);

            // bottom-right
            arc.Y = bounds.Bottom - d;
            path.AddArc(arc, 0, 90);

            // bottom edge
            path.AddLine(bounds.Right - radius, bounds.Bottom, bounds.X + radius, bounds.Bottom);

            // bottom-left
            arc.X = bounds.X;
            path.AddArc(arc, 90, 90);

            // left edge
            path.AddLine(bounds.X, bounds.Bottom - radius, bounds.X, bounds.Y + radius);

            path.CloseFigure();
            return path;
        }
    }

    internal static class MathHelper
    {
        public static float Clamp(float v, float min, float max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }
    }

    internal static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, float x, float y, float w, float h, float r)
        {
            // If radius is not positive, draw plain rectangle to avoid arc with zero size (which can throw)
            if (r <= 0f)
            {
                g.FillRectangle(brush, x, y, w, h);
                return;
            }

            if (w <= 0 || h <= 0) return;
            using (var path = new GraphicsPath())
            {
                float d = r * 2f;
                d = Math.Min(d, Math.Min(w, h));
                float rr = d / 2f;
                path.AddArc(x, y, d, d, 180, 90);
                path.AddArc(x + w - d, y, d, d, 270, 90);
                path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
                path.AddArc(x, y + h - d, d, d, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
        }
    }
}
