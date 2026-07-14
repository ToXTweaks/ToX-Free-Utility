using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexNumericUpDown : NumericUpDown
    {
        // Basic properties
        private int _cornerCutSize = 6;
        private bool _showBorder = true;
        private Color _borderColor = Color.FromArgb(64, 158, 255);
        private float _borderWidth = 2f;
        private Color _buttonColor = Color.FromArgb(64, 158, 255);
        private Color _backgroundColor = Color.FromArgb(11, 14, 19);
        private Color _textColor = Color.White;
        private Color _arrowColor = Color.White;
        private Color _buttonHoverColor = Color.FromArgb(84, 178, 255);
        private bool _isUpButtonHovered = false;
        private bool _isDownButtonHovered = false;

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
        [Description("Determines whether to show border around the numeric updown")]
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
        [Description("Color of the numeric updown border")]
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
        [Description("Width of the numeric updown border")]
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
        [Description("Color of the up/down buttons")]
        public Color ButtonColor
        {
            get { return _buttonColor; }
            set
            {
                _buttonColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Background color of the numeric updown")]
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Text color of the numeric updown")]
        public Color TextColor
        {
            get { return _textColor; }
            set
            {
                _textColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color of the up/down arrows")]
        public Color ArrowColor
        {
            get { return _arrowColor; }
            set
            {
                _arrowColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Hover color for buttons")]
        public Color ButtonHoverColor
        {
            get { return _buttonHoverColor; }
            set
            {
                _buttonHoverColor = value;
                Invalidate();
            }
        }

        #endregion

        // Override Size property to ensure proper resizing
        public new Size Size
        {
            get { return base.Size; }
            set
            {
                base.Size = value;
                Invalidate();
            }
        }

        // Override SetBoundsCore to allow free resizing
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return Size; // Return current size instead of default
        }

        public override Size MinimumSize
        {
            get { return new Size(50, 20); } // Allow smaller sizes
            set { base.MinimumSize = value; }
        }

        public override Size MaximumSize
        {
            get { return new Size(1000, 100); } // Allow larger sizes
            set { base.MaximumSize = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AutoSize
        {
            get { return false; }
            set { base.AutoSize = false; }
        }

        public VortexNumericUpDown()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, true);

            BorderStyle = BorderStyle.None;
            BackColor = _backgroundColor;
            ForeColor = _textColor;
            
            // Disable AutoSize and set custom size
            AutoSize = false;
            base.MinimumSize = new Size(50, 20);
            base.MaximumSize = new Size(1000, 100);
            Size = new Size(120, 30); // Default size

            try
            {
                Font = new Font("Bahnschrift", 9);
            }
            catch
            {
                Font = new Font("Segoe UI", 9);
            }

            // Hide the default up/down buttons
            if (Controls.Count > 0)
            {
                Controls[0].Visible = false; // UpDownButtons
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            // Create numeric updown path with the special shape
            GraphicsPath path = CreateNumericUpDownPath();

            // Draw background
            using (SolidBrush backgroundBrush = new SolidBrush(_backgroundColor))
            {
                e.Graphics.FillPath(backgroundBrush, path);
            }

            // Calculate button areas
            int buttonWidth = 20;
            Rectangle upButtonRect = new Rectangle(Width - buttonWidth, 0, buttonWidth, Height / 2);
            Rectangle downButtonRect = new Rectangle(Width - buttonWidth, Height / 2, buttonWidth, Height / 2);

            // Draw up button
            DrawButton(e.Graphics, upButtonRect, true, _isUpButtonHovered);

            // Draw down button
            DrawButton(e.Graphics, downButtonRect, false, _isDownButtonHovered);

            // Draw text value
            string valueText = Value.ToString();
            Rectangle textRect = new Rectangle(5, 0, Width - buttonWidth - 10, Height);

            TextRenderer.DrawText(
                e.Graphics,
                valueText,
                Font,
                textRect,
                _textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left
            );

            // Draw border if enabled
            if (_showBorder)
            {
                using (Pen borderPen = new Pen(_borderColor, _borderWidth))
                {
                    borderPen.LineJoin = LineJoin.Round;
                    e.Graphics.DrawPath(borderPen, path);
                }
            }
        }

        private void DrawButton(Graphics g, Rectangle rect, bool isUpButton, bool isHovered)
        {
            Color buttonColor = isHovered ? _buttonHoverColor : _buttonColor;

            // Create button path
            GraphicsPath buttonPath;
            if (isUpButton)
            {
                buttonPath = CreateUpButtonPath(rect);
            }
            else
            {
                buttonPath = CreateDownButtonPath(rect);
            }

            // Fill button
            using (SolidBrush buttonBrush = new SolidBrush(buttonColor))
            {
                g.FillPath(buttonBrush, buttonPath);
            }

            // Draw arrow
            DrawArrow(g, rect, isUpButton);
        }

        private void DrawArrow(Graphics g, Rectangle rect, bool isUpButton)
        {
            int arrowSize = 6;
            int x = rect.Left + (rect.Width - arrowSize) / 2;
            int y = rect.Top + (rect.Height - arrowSize) / 2;

            Point[] arrowPoints;
            if (isUpButton)
            {
                arrowPoints = new Point[]
                {
                    new Point(x, y + arrowSize / 2),
                    new Point(x + arrowSize, y + arrowSize / 2),
                    new Point(x + arrowSize / 2, y)
                };
            }
            else
            {
                arrowPoints = new Point[]
                {
                    new Point(x, y),
                    new Point(x + arrowSize, y),
                    new Point(x + arrowSize / 2, y + arrowSize / 2)
                };
            }

            using (SolidBrush arrowBrush = new SolidBrush(_arrowColor))
            {
                g.FillPolygon(arrowBrush, arrowPoints);
            }
        }

        private GraphicsPath CreateNumericUpDownPath()
        {
            GraphicsPath path = new GraphicsPath();

            // Determine points based on properties (same as ComboBox)
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

        private GraphicsPath CreateUpButtonPath(Rectangle rect)
        {
            GraphicsPath path = new GraphicsPath();

            // Up button doesn't have corner cut
            PointF[] points = new PointF[]
            {
                new PointF(rect.Left, rect.Top),                    // Top left
                new PointF(rect.Right, rect.Top),                   // Top right
                new PointF(rect.Right, rect.Bottom),                // Bottom right
                new PointF(rect.Left, rect.Bottom)                  // Bottom left
            };

            path.AddPolygon(points);
            return path;
        }

        private GraphicsPath CreateDownButtonPath(Rectangle rect)
        {
            GraphicsPath path = new GraphicsPath();

            // Down button has corner cut (same as main control)
            PointF[] points = new PointF[]
            {
                new PointF(rect.Left, rect.Top),                                // Top left
                new PointF(rect.Right, rect.Top),                               // Top right
                new PointF(rect.Right, rect.Bottom - _cornerCutSize),           // Right before cut
                new PointF(rect.Right - _cornerCutSize, rect.Bottom),           // Bottom after cut
                new PointF(rect.Left, rect.Bottom)                              // Bottom left
            };

            path.AddPolygon(points);
            return path;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Check if mouse is over buttons
            int buttonWidth = 20;
            Rectangle upButtonRect = new Rectangle(Width - buttonWidth, 0, buttonWidth, Height / 2);
            Rectangle downButtonRect = new Rectangle(Width - buttonWidth, Height / 2, buttonWidth, Height / 2);

            bool wasUpHovered = _isUpButtonHovered;
            bool wasDownHovered = _isDownButtonHovered;

            _isUpButtonHovered = upButtonRect.Contains(e.Location);
            _isDownButtonHovered = downButtonRect.Contains(e.Location);

            if (wasUpHovered != _isUpButtonHovered || wasDownHovered != _isDownButtonHovered)
            {
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_isUpButtonHovered || _isDownButtonHovered)
            {
                _isUpButtonHovered = false;
                _isDownButtonHovered = false;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Handle button clicks
            int buttonWidth = 20;
            Rectangle upButtonRect = new Rectangle(Width - buttonWidth, 0, buttonWidth, Height / 2);
            Rectangle downButtonRect = new Rectangle(Width - buttonWidth, Height / 2, buttonWidth, Height / 2);

            if (upButtonRect.Contains(e.Location))
            {
                if (Value < Maximum)
                {
                    Value += Increment;
                }
            }
            else if (downButtonRect.Contains(e.Location))
            {
                if (Value > Minimum)
                {
                    Value -= Increment;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Handle keyboard input
            if (e.KeyCode == Keys.Up)
            {
                if (Value < Maximum)
                {
                    Value += Increment;
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (Value > Minimum)
                {
                    Value -= Increment;
                }
                e.Handled = true;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }

        protected override void OnValueChanged(EventArgs e)
        {
            base.OnValueChanged(e);
            Invalidate(); // Redraw to update displayed value
        }
    }
}