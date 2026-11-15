using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using Timer = System.Windows.Forms.Timer;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexProgressBar : Control
    {
        // Basic properties
        private int _cornerCutSize = 10;
        private bool _showBorder = true;
        private Color _borderColor = Color.FromArgb(64, 158, 255);
        private float _borderWidth = 2f;
        private Color _progressColor = Color.FromArgb(44, 115, 200); // Orange color
        private Color _backgroundColor = Color.FromArgb(11, 14, 19);
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;
        private int _step = 10;
        private bool _showPercentage = true;
        private bool _animateProgress = true;
        private int _animationSpeed = 5;
        private int _displayValue = 0;
        private Timer _animationTimer;

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
        [Description("Determines whether to show border around the progress bar")]
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
        [Description("Color of the progress bar border")]
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
        [Description("Width of the progress bar border")]
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
        [Description("Color of the progress indicator")]
        public Color ProgressColor
        {
            get { return _progressColor; }
            set
            {
                _progressColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Background color of the progress bar")]
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [Description("Minimum value of the progress bar")]
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                if (value >= _maximum)
                    throw new ArgumentException("Minimum value cannot be greater than or equal to Maximum value");

                _minimum = value;
                if (_value < _minimum)
                    Value = _minimum;

                Invalidate();
            }
        }

        [Category("Behavior")]
        [Description("Maximum value of the progress bar")]
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (value <= _minimum)
                    throw new ArgumentException("Maximum value cannot be less than or equal to Minimum value");

                _maximum = value;
                if (_value > _maximum)
                    Value = _maximum;

                Invalidate();
            }
        }

        [Category("Behavior")]
        [Description("Current value of the progress bar")]
        public int Value
        {
            get { return _value; }
            set
            {
                if (value < _minimum)
                    value = _minimum;
                if (value > _maximum)
                    value = _maximum;

                if (_value != value)
                {
                    _value = value;
                    OnValueChanged(EventArgs.Empty);

                    if (_animateProgress)
                    {
                        if (!_animationTimer.Enabled)
                            _animationTimer.Start();
                    }
                    else
                    {
                        _displayValue = _value;
                        Invalidate();
                    }
                }
            }
        }

        [Category("Behavior")]
        [Description("Step value for incrementing the progress bar")]
        public int Step
        {
            get { return _step; }
            set
            {
                _step = Math.Max(1, value);
            }
        }

        [Category("Appearance")]
        [Description("Show percentage text on the progress bar")]
        public bool ShowPercentage
        {
            get { return _showPercentage; }
            set
            {
                _showPercentage = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [Description("Animate progress changes")]
        public bool AnimateProgress
        {
            get { return _animateProgress; }
            set
            {
                _animateProgress = value;
                if (!_animateProgress)
                {
                    _displayValue = _value;
                    _animationTimer.Stop();
                }
                Invalidate();
            }
        }

        [Category("Behavior")]
        [Description("Speed of the progress animation (1-10)")]
        public int AnimationSpeed
        {
            get { return _animationSpeed; }
            set
            {
                _animationSpeed = Math.Max(1, Math.Min(10, value));
            }
        }

        // Event that fires when value changes
        public event EventHandler ValueChanged;

        // Event that fires when progress reaches maximum
        public event EventHandler Maximum_Reached;

        #endregion

        public VortexProgressBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            Size = new Size(250, 30);
            BackColor = Color.Transparent;
            ForeColor = Color.White;

            try
            {
                Font = new Font("Bahnschrift", 9, FontStyle.Bold);
            }
            catch
            {
                Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }

            _animationTimer = new Timer();
            _animationTimer.Interval = 10;
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer.Stop();
                _animationTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (_displayValue < _value)
            {
                _displayValue += Math.Max(1, (_value - _displayValue) / (11 - _animationSpeed));
                if (_displayValue > _value)
                    _displayValue = _value;
                Invalidate();
            }
            else if (_displayValue > _value)
            {
                _displayValue -= Math.Max(1, (_displayValue - _value) / (11 - _animationSpeed));
                if (_displayValue < _value)
                    _displayValue = _value;
                Invalidate();
            }
            else
            {
                _animationTimer.Stop();
            }
        }

        /// <summary>
        /// Advances the current position of the progress bar by the amount of the Step property.
        /// </summary>
        public void PerformStep()
        {
            int newValue = _value + _step;

            // Check if this step will reach or exceed the maximum
            bool willReachMax = newValue >= _maximum;

            // Set the new value (will be capped at maximum)
            Value = newValue;

            // If we reached the maximum, raise the event
            if (willReachMax)
            {
                OnMaximumReached(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Advances the current position of the progress bar by the specified amount.
        /// </summary>
        /// <param name="value">The amount to increment the progress bar.</param>
        public void Increment(int value)
        {
            int newValue = _value + value;

            // Check if this increment will reach or exceed the maximum
            bool willReachMax = newValue >= _maximum;

            // Set the new value (will be capped at maximum)
            Value = newValue;

            // If we reached the maximum, raise the event
            if (willReachMax)
            {
                OnMaximumReached(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Resets the progress bar to the minimum value.
        /// </summary>
        public void Reset()
        {
            Value = _minimum;
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        protected virtual void OnMaximumReached(EventArgs e)
        {
            Maximum_Reached?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            // Create progress bar path with the special shape
            GraphicsPath path = CreateProgressBarPath();

            // Draw background
            using (SolidBrush backgroundBrush = new SolidBrush(_backgroundColor))
            {
                e.Graphics.FillPath(backgroundBrush, path);
            }

            // Calculate progress width
            float progressPercentage = (_maximum - _minimum) == 0 ? 0 :
                (float)(_displayValue - _minimum) / (_maximum - _minimum);
            int progressWidth = (int)(Width * progressPercentage);

            // Create progress path
            GraphicsPath progressPath = CreateProgressPath(progressWidth);

            // Draw progress
            using (SolidBrush progressBrush = new SolidBrush(_progressColor))
            {
                e.Graphics.FillPath(progressBrush, progressPath);
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

            // Draw percentage text if enabled
            if (_showPercentage)
            {
                string percentText = $"{(int)(progressPercentage * 100)}%";
                TextRenderer.DrawText(
                    e.Graphics,
                    percentText,
                    Font,
                    ClientRectangle,
                    ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );
            }

            base.OnPaint(e);
        }

        private GraphicsPath CreateProgressBarPath()
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

        private GraphicsPath CreateProgressPath(int progressWidth)
        {
            if (progressWidth <= 0)
                return new GraphicsPath();

            GraphicsPath path = new GraphicsPath();

            // Calculate cut size for progress
            int cutSize = 0;
            if (progressWidth >= Width - _cornerCutSize)
            {
                // Progress reaches the cut corner
                cutSize = Width - progressWidth;
            }

            // Determine points based on progress width
            PointF[] points;

            if (progressWidth >= Width)
            {
                // Full progress - same as the background shape
                points = new PointF[]
                {
                    new PointF(0, 0),                                    // Top left
                    new PointF(Width, 0),                                // Top right
                    new PointF(Width, Height - _cornerCutSize),          // Right before cut
                    new PointF(Width - _cornerCutSize, Height),          // Bottom after cut
                    new PointF(0, Height)                                // Bottom left
                };
            }
            else if (progressWidth > Width - _cornerCutSize)
            {
                // Progress reaches into the cut corner
                points = new PointF[]
                {
                    new PointF(0, 0),                                    // Top left
                    new PointF(progressWidth, 0),                        // Top right at progress
                    new PointF(progressWidth, Height - cutSize),         // Right side at progress
                    new PointF(progressWidth - cutSize, Height),         // Bottom at progress with cut
                    new PointF(0, Height)                                // Bottom left
                };
            }
            else
            {
                // Progress doesn't reach the cut corner
                points = new PointF[]
                {
                    new PointF(0, 0),                                    // Top left
                    new PointF(progressWidth, 0),                        // Top right at progress
                    new PointF(progressWidth, Height),                   // Bottom right at progress
                    new PointF(0, Height)                                // Bottom left
                };
            }

            // Create path
            path.AddPolygon(points);
            return path;
        }
    }
}