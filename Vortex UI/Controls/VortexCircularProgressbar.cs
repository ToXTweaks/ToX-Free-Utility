using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    [Description("Circular progress bar control with glow effect.")]
    public class VortexCircularProgressbar : Control
    {
        // Appearance
        private Color _backgroundColor = Color.FromArgb(11, 14, 19);
        private Color _ringActiveColor = Color.FromArgb(0, 255, 120);
        private Color _ringInactiveColor = Color.FromArgb(60, 80, 95, 110);
        private Color _textColor = Color.White;
        private string _title = "";
        private bool _showPercentageText = true;

        // Layout
        private int _ringThickness = 12;
        private float _ringGapAngle = 38f;
        private int _padding = 20;

        // Data
        private float _value = 0f;
        private float _minimum = 0f;
        private float _maximum = 100f;

        public VortexCircularProgressbar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(150, 150);

            try { Font = new Font("Bahnschrift", 12, FontStyle.Bold); }
            catch { Font = new Font("Segoe UI", 12, FontStyle.Bold); }
        }

        #region Properties
        [Category("Appearance")] 
        public Color BackgroundColor 
        { 
            get => _backgroundColor; 
            set { _backgroundColor = value; Invalidate(); } 
        }

        [Category("Appearance")] 
        public Color RingActiveColor 
        { 
            get => _ringActiveColor; 
            set { _ringActiveColor = value; Invalidate(); } 
        }

        [Category("Appearance")] 
        public Color RingInactiveColor 
        { 
            get => _ringInactiveColor; 
            set { _ringInactiveColor = value; Invalidate(); } 
        }

        [Category("Appearance")] 
        public Color TextColor 
        { 
            get => _textColor; 
            set { _textColor = value; Invalidate(); } 
        }

        [Category("Appearance")] 
        public string Title 
        { 
            get => _title; 
            set { _title = value; Invalidate(); } 
        }

        [Category("Appearance")] 
        public bool ShowPercentageText 
        { 
            get => _showPercentageText; 
            set { _showPercentageText = value; Invalidate(); } 
        }

        [Category("Layout")] 
        public int RingThickness 
        { 
            get => _ringThickness; 
            set { _ringThickness = Math.Max(6, value); Invalidate(); } 
        }

        [Category("Layout")] 
        public float RingGapAngle 
        { 
            get => _ringGapAngle; 
            set { _ringGapAngle = Math.Max(0, Math.Min(120, value)); Invalidate(); } 
        }

        [Category("Layout")] 
        public int Padding 
        { 
            get => _padding; 
            set { _padding = Math.Max(10, value); Invalidate(); } 
        }

        [Category("Data")] 
        public float Value 
        { 
            get => _value; 
            set 
            { 
                _value = Math.Max(_minimum, Math.Min(_maximum, value)); 
                Invalidate();
            } 
        }

        [Category("Data")] 
        public float Minimum 
        { 
            get => _minimum; 
            set 
            { 
                _minimum = value; 
                if (_value < _minimum) _value = _minimum;
                Invalidate();
            } 
        }

        [Category("Data")] 
        public float Maximum 
        { 
            get => _maximum; 
            set 
            { 
                _maximum = value; 
                if (_value > _maximum) _value = _maximum;
                Invalidate();
            } 
        }
        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Fill background
            using (var bg = new SolidBrush(_backgroundColor))
            {
                g.FillRectangle(bg, ClientRectangle);
            }

            // Calculate ring rectangle
            var ringRect = GetRingRect();
            
            // Draw the ring
            DrawRing(g, ringRect);

            // Draw title and percentage
            DrawText(g, ringRect);
        }

        private Rectangle GetRingRect()
        {
            int size = Math.Min(Width - _padding * 2, Height - _padding * 2);
            int x = (Width - size) / 2;
            int y = (Height - size) / 2;
            return new Rectangle(x, y, size, size);
        }

        private void DrawRing(Graphics g, Rectangle r)
        {
            if (r.Width < 20 || r.Height < 20) return;

            // Glow behind ring
            var glowRect = r; 
            glowRect.Inflate(8, 8);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(glowRect);
                using (var br = new PathGradientBrush(path))
                {
                    br.CenterColor = Color.FromArgb(20, _ringActiveColor);
                    br.SurroundColors = new[] { Color.FromArgb(0, _ringActiveColor) };
                    g.FillPath(br, path);
                }
            }

            float startAngle = -90f - _ringGapAngle / 2f;
            float sweepFull = 360f - _ringGapAngle;

            // Align to sub-pixel for crisp edges when thickness is odd
            g.PixelOffsetMode = PixelOffsetMode.Half;
            RectangleF rf = new RectangleF(r.X, r.Y, r.Width, r.Height);
            float subPixel = (_ringThickness % 2 == 1) ? 0.5f : 0f;
            if (subPixel > 0f)
                rf = new RectangleF(r.X + subPixel, r.Y + subPixel, r.Width - subPixel * 2f, r.Height - subPixel * 2f);

            // Background arc (inactive) with smoother edges
            using (var pen = new Pen(_ringInactiveColor, _ringThickness) 
            { 
                StartCap = LineCap.Round, 
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round 
            })
            {
                g.DrawArc(pen, rf, startAngle, sweepFull);
            }

            // Active arc (progress)
            float progress = (_value - _minimum) / (_maximum - _minimum);
            float sweepActive = sweepFull * progress;
            
            if (sweepActive > 0)
            {
                // Solid active arc (no glow), same look as inactive
                using (var pen = new Pen(_ringActiveColor, _ringThickness)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                })
                {
                    g.DrawArc(pen, rf, startAngle, sweepActive);
                }
            }
        }

        private void DrawText(Graphics g, Rectangle ringRect)
        {
            // Draw percentage in center if enabled
            if (_showPercentageText)
            {
                float percentage = ((_value - _minimum) / (_maximum - _minimum)) * 100f;
                string percentText = $"{percentage:0}%";

                var centerRect = new Rectangle(ringRect.X + _ringThickness, ringRect.Y + _ringThickness, 
                                             ringRect.Width - _ringThickness * 2, ringRect.Height - _ringThickness * 2);
                
                using (var brush = new SolidBrush(_textColor))
                using (var percentFont = new Font(Font.FontFamily, Font.Size + 2, FontStyle.Bold))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(percentText, percentFont, brush, centerRect, sf);
                }
            }

            // Draw title below the ring only if not empty
            if (!string.IsNullOrEmpty(_title))
            {
                var titleRect = new Rectangle(0, ringRect.Bottom + 10, Width, 30);
                using (var brush = new SolidBrush(Color.FromArgb(180, _textColor)))
                using (var titleFont = new Font(Font.FontFamily, Font.Size - 1, FontStyle.Regular))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(_title, titleFont, brush, titleRect, sf);
                }
            }
        }

        #region Public Methods
        /// <summary>
        /// Sets the progress value with validation
        /// </summary>
        /// <param name="value">The progress value</param>
        public void SetProgress(float value)
        {
            Value = value;
        }

        /// <summary>
        /// Increments the progress by the specified amount
        /// </summary>
        /// <param name="increment">Amount to increment</param>
        public void Increment(float increment)
        {
            Value += increment;
        }

        /// <summary>
        /// Resets the progress to minimum value
        /// </summary>
        public void Reset()
        {
            Value = _minimum;
        }
        #endregion
    }
}