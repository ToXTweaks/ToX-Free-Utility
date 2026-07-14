using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexSwitch : Control
    {
        // Basic properties
        private int _cornerCutSize = 6;
        private bool _showBorder = true;
        private Color _borderColor = Color.FromArgb(64, 158, 255);
        private float _borderWidth = 1f;
        private Color _onColor = Color.FromArgb(64, 158, 255); // Orange color
        private Color _offColor = Color.FromArgb(11, 14, 19); // Gray color
        private Color _onTextColor = Color.White;
        private Color _offTextColor = Color.White;
        private bool _checked = false;
        private string _onText = "ON";
        private string _offText = "OFF";
        private bool _showText = true;

        // Animation properties
        private float _transitionPosition = 0f; // 0 = off, 1 = on
        private int _animationDuration = 150;
        private CancellationTokenSource _animationCts;

        #region Properties

        [Category("Appearance")]
        [Description("Size of the cut corner in pixels")]
        public int CornerCutSize
        {
            get { return _cornerCutSize; }
            set
            {
                _cornerCutSize = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines whether to show border around the switch")]
        public bool ShowBorder
        {
            get { return _showBorder; }
            set
            {
                _showBorder = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color of the switch border")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Width of the switch border")]
        public float BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                _borderWidth = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color when switch is ON")]
        public Color OnColor
        {
            get { return _onColor; }
            set
            {
                _onColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color when switch is OFF")]
        public Color OffColor
        {
            get { return _offColor; }
            set
            {
                _offColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Text color when switch is ON")]
        public Color OnTextColor
        {
            get { return _onTextColor; }
            set
            {
                _onTextColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Text color when switch is OFF")]
        public Color OffTextColor
        {
            get { return _offTextColor; }
            set
            {
                _offTextColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Text displayed when switch is ON")]
        public string OnText
        {
            get { return _onText; }
            set
            {
                _onText = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Text displayed when switch is OFF")]
        public string OffText
        {
            get { return _offText; }
            set
            {
                _offText = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Whether to show ON/OFF text")]
        public bool ShowText
        {
            get { return _showText; }
            set
            {
                _showText = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [Description("Current state of the switch")]
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    OnCheckedChanged(EventArgs.Empty);
                    AnimateTransition(_checked);
                }
            }
        }

        [Category("Animation")]
        [Description("Duration of animation in milliseconds")]
        public int AnimationDuration
        {
            get { return _animationDuration; }
            set
            {
                _animationDuration = Math.Max(50, value);
            }
        }

        // Event that fires when the switch state changes
        public event EventHandler CheckedChanged;

        #endregion

        public VortexSwitch()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            Size = new Size(52, 26);
            BackColor = Color.Transparent;
            ForeColor = Color.FromArgb(120, 120, 120);
            Cursor = Cursors.Hand;

            try
            {
                Font = new Font("Bahnschrift", 8, FontStyle.Bold);
            }
            catch
            {
                Font = new Font("Segoe UI", 8, FontStyle.Bold);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelAnimation();
            }
            base.Dispose(disposing);
        }

        private void CancelAnimation()
        {
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = null;
        }

        private async Task AnimateTransition(bool targetState, CancellationToken cancelToken = default)
        {
            try
            {
                float targetPosition = targetState ? 1.0f : 0.0f;
                float stepSize = 0.1f;
                int stepDuration = Math.Max(10, _animationDuration / 10);

                while (!cancelToken.IsCancellationRequested)
                {
                    if ((targetState && _transitionPosition >= targetPosition) ||
                        (!targetState && _transitionPosition <= targetPosition))
                    {
                        break;
                    }

                    _transitionPosition = targetState
                        ? Math.Min(_transitionPosition + stepSize, targetPosition)
                        : Math.Max(_transitionPosition - stepSize, targetPosition);

                    if (IsHandleCreated && !IsDisposed)
                    {
                        BeginInvoke(new Action(() => Invalidate()));
                    }
                    else
                    {
                        break;
                    }

                    await Task.Delay(stepDuration, cancelToken);
                }

                // Ensure we end at the exact target position
                _transitionPosition = targetPosition;
                if (IsHandleCreated && !IsDisposed)
                {
                    BeginInvoke(new Action(() => Invalidate()));
                }
            }
            catch (TaskCanceledException)
            {
                // Animation was canceled, this is expected behavior
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Switch animation error: {ex.Message}");
            }
        }

        private void AnimateTransition(bool targetState)
        {
            // Cancel any ongoing animation
            CancelAnimation();

            // Start new animation
            _animationCts = new CancellationTokenSource();
            _ = AnimateTransition(targetState, _animationCts.Token);
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            CheckedChanged?.Invoke(this, e);
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            // Create switch path with the special shape
            GraphicsPath path = CreateSwitchPath();

            // Calculate current color based on transition position
            Color currentColor = InterpolateColor(_offColor, _onColor, _transitionPosition);

            // Draw switch background
            using (SolidBrush brush = new SolidBrush(currentColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Draw border if enabled
            if (_showBorder)
            {
                using (Pen borderPen = new Pen(_borderColor, _borderWidth))
                {
                    borderPen.LineJoin = LineJoin.Round;
                    e.Graphics.DrawPath(borderPen, path);
                }
            }

            // Draw text if enabled
            if (_showText)
            {
                // Determine which text to show based on transition position
                string displayText;
                Color textColor;

                // During transition, crossfade between texts
                if (_transitionPosition < 0.5f)
                {
                    // Show OFF text
                    displayText = _offText;
                    textColor = _offTextColor;
                    Rectangle textRect = new Rectangle(10, 0, Width - 15, Height);
                    TextRenderer.DrawText(
                        e.Graphics,
                        displayText,
                        Font,
                        textRect,
                        textColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                    );
                }
                else
                {
                    // Show ON text
                    displayText = _onText;
                    textColor = _onTextColor;
                    Rectangle textRect = new Rectangle(10, 0, Width - 15, Height);
                    TextRenderer.DrawText(
                        e.Graphics,
                        displayText,
                        Font,
                        textRect,
                        textColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                    );
                }
            }

            base.OnPaint(e);
        }

        private GraphicsPath CreateSwitchPath()
        {
            GraphicsPath path = new GraphicsPath();

            // Determine points based on properties
            PointF[] points = new PointF[]
            {
                new PointF(0, 0),                                    // Top left
                new PointF(Width, 0),                                // Top right
                new PointF(Width, Height - _cornerCutSize),          // Right before cut
                new PointF(Width - _cornerCutSize, Height),          // Bottom after cut
                new PointF(0, Height)                                // Bottom left
            };

            // Create path
            path.AddPolygon(points);
            return path;
        }

        // Helper method to interpolate between colors
        private Color InterpolateColor(Color start, Color end, float ratio)
        {
            int r = (int)(start.R + (end.R - start.R) * ratio);
            int g = (int)(start.G + (end.G - start.G) * ratio);
            int b = (int)(start.B + (end.B - start.B) * ratio);
            int a = (int)(start.A + (end.A - start.A) * ratio);
            return Color.FromArgb(a, r, g, b);
        }
    }
}