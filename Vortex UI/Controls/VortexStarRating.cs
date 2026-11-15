using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    [DefaultEvent(nameof(RatingChanged))]
    [Description("A lightweight star rating control with hover preview and optional half-star selection.")]
    public class VortexStarRating : Control
    {
        private int _maximumStars = 5;
        private float _rating = 3f;
        private int _starSize = 28;
        private int _starSpacing = 8;
        private Padding _contentPadding = new Padding(0);
        private Color _filledColor = Color.FromArgb(255, 221, 0);
        private Color _emptyColor = Color.FromArgb(218, 221, 225);
        private Color _hoverColor = Color.FromArgb(255, 230, 40);
        private Color _outlineColor = Color.Transparent;
        private bool _readOnly = false;
        private bool _allowHalfStars = false;
        private bool _showHoverPreview = true;
        private float _hoverRating = -1f;

        public VortexStarRating()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            AutoSize = true;
            Size = GetPreferredSize(Size.Empty);
            MinimumSize = new Size(24, 24);
            Cursor = Cursors.Hand;
        }

        public event EventHandler? RatingChanged;

        [Category("Behavior")]
        [DefaultValue(3f)]
        [Description("The selected rating value.")]
        public float Rating
        {
            get => _rating;
            set
            {
                float newValue = NormalizeRating(value);
                if (Math.Abs(_rating - newValue) > 0.001f)
                {
                    _rating = newValue;
                    RatingChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(5)]
        [Description("The number of stars displayed.")]
        public int MaximumStars
        {
            get => _maximumStars;
            set
            {
                int newValue = Math.Max(1, value);
                if (_maximumStars != newValue)
                {
                    _maximumStars = newValue;
                    Rating = _rating;
                    UpdateLayout();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Allows selecting half-star values.")]
        public bool AllowHalfStars
        {
            get => _allowHalfStars;
            set
            {
                if (_allowHalfStars != value)
                {
                    _allowHalfStars = value;
                    Rating = _rating;
                    Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Disables user interaction when true.")]
        public bool ReadOnly
        {
            get => _readOnly;
            set
            {
                if (_readOnly != value)
                {
                    _readOnly = value;
                    Cursor = _readOnly ? Cursors.Default : Cursors.Hand;
                    _hoverRating = -1f;
                    Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Shows a temporary rating preview while hovering.")]
        public bool ShowHoverPreview
        {
            get => _showHoverPreview;
            set
            {
                if (_showHoverPreview != value)
                {
                    _showHoverPreview = value;
                    _hoverRating = -1f;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(28)]
        [Description("The size of each star in pixels.")]
        public int StarSize
        {
            get => _starSize;
            set
            {
                int newValue = Math.Max(8, value);
                if (_starSize != newValue)
                {
                    _starSize = newValue;
                    UpdateLayout();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(8)]
        [Description("The spacing between stars.")]
        public int StarSpacing
        {
            get => _starSpacing;
            set
            {
                int newValue = Math.Max(0, value);
                if (_starSpacing != newValue)
                {
                    _starSpacing = newValue;
                    UpdateLayout();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Padding), "0, 0, 0, 0")]
        [Description("Inner spacing around the stars.")]
        public Padding ContentPadding
        {
            get => _contentPadding;
            set
            {
                if (_contentPadding != value)
                {
                    _contentPadding = value;
                    UpdateLayout();
                }
            }
        }

        [Category("Appearance")]
        [Description("The color of selected stars.")]
        public Color FilledColor
        {
            get => _filledColor;
            set
            {
                if (_filledColor != value)
                {
                    _filledColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The color of unselected stars.")]
        public Color EmptyColor
        {
            get => _emptyColor;
            set
            {
                if (_emptyColor != value)
                {
                    _emptyColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The color used while hovering over stars.")]
        public Color HoverColor
        {
            get => _hoverColor;
            set
            {
                if (_hoverColor != value)
                {
                    _hoverColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("Optional outline color for each star.")]
        public Color OutlineColor
        {
            get => _outlineColor;
            set
            {
                if (_outlineColor != value)
                {
                    _outlineColor = value;
                    Invalidate();
                }
            }
        }

        [DefaultValue(true)]
        public override bool AutoSize
        {
            get => base.AutoSize;
            set
            {
                base.AutoSize = value;
                UpdateLayout();
            }
        }

        protected override Size DefaultSize => new Size(172, 32);

        public override Size GetPreferredSize(Size proposedSize)
        {
            int width = (_maximumStars * _starSize) + ((_maximumStars - 1) * _starSpacing) + _contentPadding.Horizontal + Padding.Horizontal;
            int height = _starSize + _contentPadding.Vertical + Padding.Vertical;
            return new Size(Math.Max(1, width), Math.Max(1, height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            float visibleRating = _hoverRating >= 0f && _showHoverPreview && !_readOnly ? _hoverRating : _rating;
            Color activeColor = _hoverRating >= 0f && _showHoverPreview && !_readOnly ? _hoverColor : _filledColor;

            for (int i = 0; i < _maximumStars; i++)
            {
                RectangleF starRect = GetStarRectangle(i);
                if (starRect.Width <= 0f || starRect.Height <= 0f)
                    continue;

                float fillAmount = Math.Max(0f, Math.Min(1f, visibleRating - i));
                DrawStar(g, starRect, fillAmount, activeColor);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_readOnly || !_showHoverPreview)
                return;

            float newHoverRating = GetRatingFromPoint(e.Location);
            if (Math.Abs(_hoverRating - newHoverRating) > 0.001f)
            {
                _hoverRating = newHoverRating;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_hoverRating >= 0f)
            {
                _hoverRating = -1f;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (_readOnly || e.Button != MouseButtons.Left)
                return;

            Rating = GetRatingFromPoint(e.Location);
        }

        private void DrawStar(Graphics g, RectangleF rect, float fillAmount, Color activeColor)
        {
            using (GraphicsPath starPath = CreateStarPath(rect))
            {
                using (SolidBrush emptyBrush = new SolidBrush(_emptyColor))
                    g.FillPath(emptyBrush, starPath);

                if (fillAmount > 0f)
                {
                    GraphicsState state = g.Save();
                    try
                    {
                        RectangleF clipRect = new RectangleF(rect.Left, rect.Top, rect.Width * fillAmount, rect.Height);
                        g.SetClip(clipRect);
                        using (SolidBrush filledBrush = new SolidBrush(activeColor))
                            g.FillPath(filledBrush, starPath);
                    }
                    finally
                    {
                        g.Restore(state);
                    }
                }

                if (_outlineColor.A > 0)
                {
                    using (Pen outlinePen = new Pen(_outlineColor, 1f) { LineJoin = LineJoin.Round })
                        g.DrawPath(outlinePen, starPath);
                }
            }
        }

        private RectangleF GetStarRectangle(int index)
        {
            int availableHeight = Height - Padding.Vertical - _contentPadding.Vertical;
            int size = Math.Min(_starSize, Math.Max(0, availableHeight));
            float x = Padding.Left + _contentPadding.Left + (index * (_starSize + _starSpacing));
            float y = Padding.Top + _contentPadding.Top + Math.Max(0, (availableHeight - size) / 2f);
            return new RectangleF(x, y, size, size);
        }

        private float GetRatingFromPoint(Point point)
        {
            int left = Padding.Left + _contentPadding.Left;
            int totalWidth = (_maximumStars * _starSize) + ((_maximumStars - 1) * _starSpacing);
            int relativeX = Math.Max(0, Math.Min(totalWidth - 1, point.X - left));
            int slotWidth = _starSize + _starSpacing;
            int starIndex = Math.Min(_maximumStars - 1, relativeX / Math.Max(1, slotWidth));
            int xInSlot = relativeX - (starIndex * slotWidth);

            if (xInSlot >= _starSize && starIndex < _maximumStars - 1)
                return starIndex + 1f;

            if (_allowHalfStars)
                return starIndex + (xInSlot <= _starSize / 2f ? 0.5f : 1f);

            return starIndex + 1f;
        }

        private float NormalizeRating(float value)
        {
            float clamped = Math.Max(0f, Math.Min(_maximumStars, value));
            if (_allowHalfStars)
                return (float)(Math.Round(clamped * 2f, MidpointRounding.AwayFromZero) / 2d);

            return (float)Math.Round(clamped, MidpointRounding.AwayFromZero);
        }

        private void UpdateLayout()
        {
            if (AutoSize)
                Size = GetPreferredSize(Size.Empty);

            Invalidate();
        }

        private static GraphicsPath CreateStarPath(RectangleF rect)
        {
            GraphicsPath path = new GraphicsPath();

            float centerX = rect.Left + rect.Width / 2f;
            float centerY = rect.Top + rect.Height / 2f;
            float outerRadius = Math.Min(rect.Width, rect.Height) / 2f;
            float innerRadius = outerRadius * 0.48f;
            PointF[] points = new PointF[10];

            for (int i = 0; i < points.Length; i++)
            {
                double angle = (-90 + (i * 36)) * Math.PI / 180d;
                float radius = i % 2 == 0 ? outerRadius : innerRadius;
                points[i] = new PointF(
                    centerX + (float)Math.Cos(angle) * radius,
                    centerY + (float)Math.Sin(angle) * radius);
            }

            path.AddPolygon(points);
            path.CloseFigure();
            return path;
        }
    }
}
