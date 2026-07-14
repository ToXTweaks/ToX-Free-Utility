using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexScrollBar : Control
    {
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;
        private int _smallChange = 1;
        private int _largeChange = 10;
        private int _thumbSize = 40;
        
        private Color _trackColor = Color.FromArgb(30, 30, 30);
        private Color _thumbColor = Color.FromArgb(80, 80, 84);
        private Color _thumbHoverColor = Color.FromArgb(100, 100, 104);
        private Color _thumbActiveColor = Color.FromArgb(120, 120, 124);
        private int _thumbRadius = 4;
        private int _thumbWidth = 8;
        
        private bool _isThumbHovered = false;
        private bool _isThumbDragging = false;
        private int _dragStartY = 0;
        private int _dragStartValue = 0;

        public event ScrollEventHandler Scroll;

        [Category("Behavior")]
        public int Minimum
        {
            get => _minimum;
            set { _minimum = value; Invalidate(); }
        }

        [Category("Behavior")]
        public int Maximum
        {
            get => _maximum;
            set { _maximum = Math.Max(value, _minimum); Invalidate(); }
        }

        [Category("Behavior")]
        public int Value
        {
            get => _value;
            set
            {
                int newValue = Math.Max(_minimum, Math.Min(_maximum, value));
                if (_value != newValue)
                {
                    _value = newValue;
                    Invalidate();
                    OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, _value));
                }
            }
        }

        [Category("Behavior")]
        public int SmallChange
        {
            get => _smallChange;
            set => _smallChange = Math.Max(1, value);
        }

        [Category("Behavior")]
        public int LargeChange
        {
            get => _largeChange;
            set { _largeChange = Math.Max(1, value); Invalidate(); }
        }

        [Category("Appearance")]
        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color ThumbColor
        {
            get => _thumbColor;
            set { _thumbColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color ThumbHoverColor
        {
            get => _thumbHoverColor;
            set { _thumbHoverColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color ThumbActiveColor
        {
            get => _thumbActiveColor;
            set { _thumbActiveColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public int ThumbRadius
        {
            get => _thumbRadius;
            set { _thumbRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        public int ThumbWidth
        {
            get => _thumbWidth;
            set { _thumbWidth = Math.Max(4, Math.Min(Width, value)); Invalidate(); }
        }

        public VortexScrollBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);

            Width = 16;
            Height = 200;
        }

        protected virtual void OnScroll(ScrollEventArgs e)
        {
            Scroll?.Invoke(this, e);
        }

        private Rectangle GetThumbRectangle()
        {
            if (_maximum <= _minimum) return Rectangle.Empty;

            int range = _maximum - _minimum;
            int trackHeight = Height - _thumbSize;
            
            if (trackHeight <= 0) return Rectangle.Empty;

            float ratio = (float)(_value - _minimum) / range;
            int thumbY = (int)(ratio * trackHeight);

            int thumbX = (Width - _thumbWidth) / 2;
            return new Rectangle(thumbX, thumbY, _thumbWidth, _thumbSize);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Draw track
            using (SolidBrush trackBrush = new SolidBrush(_trackColor))
            {
                g.FillRectangle(trackBrush, ClientRectangle);
            }

            // Draw thumb
            Rectangle thumbRect = GetThumbRectangle();
            if (!thumbRect.IsEmpty)
            {
                Color thumbColor = _isThumbDragging ? _thumbActiveColor :
                                   _isThumbHovered ? _thumbHoverColor : _thumbColor;

                using (GraphicsPath thumbPath = CreateRoundedRectangle(thumbRect, _thumbRadius))
                using (SolidBrush thumbBrush = new SolidBrush(thumbColor))
                {
                    g.FillPath(thumbBrush, thumbPath);
                }
            }
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isThumbDragging)
            {
                int deltaY = e.Y - _dragStartY;
                int trackHeight = Height - _thumbSize;
                
                if (trackHeight > 0)
                {
                    int range = _maximum - _minimum;
                    int deltaValue = (int)((float)deltaY / trackHeight * range);
                    Value = _dragStartValue + deltaValue;
                }
            }
            else
            {
                Rectangle thumbRect = GetThumbRectangle();
                bool wasHovered = _isThumbHovered;
                _isThumbHovered = thumbRect.Contains(e.Location);
                
                if (wasHovered != _isThumbHovered)
                    Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                Rectangle thumbRect = GetThumbRectangle();
                
                if (thumbRect.Contains(e.Location))
                {
                    _isThumbDragging = true;
                    _dragStartY = e.Y;
                    _dragStartValue = _value;
                    Invalidate();
                }
                else
                {
                    // Click on track - jump to position
                    int trackHeight = Height - _thumbSize;
                    if (trackHeight > 0)
                    {
                        float ratio = (float)e.Y / Height;
                        int range = _maximum - _minimum;
                        Value = _minimum + (int)(ratio * range);
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            if (_isThumbDragging)
            {
                _isThumbDragging = false;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isThumbHovered = false;
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            
            int delta = e.Delta > 0 ? -_smallChange : _smallChange;
            Value = _value + delta;
        }
    }
}
