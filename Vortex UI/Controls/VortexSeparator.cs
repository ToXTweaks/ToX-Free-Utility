using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Vortex.Ui
{
    public enum SeparatorOrientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    public enum SeparatorShape
    {
        Solid = 0,
        Dashed = 1,
        Dotted = 2,
        DoubleLine = 3,
        Zigzag = 4,
        Wave = 5,
        Cutting = 6
    }

    [ToolboxItem(true)]
    public class VortexSeparator : Control
    {
        #region Private Fields

        private Color _separatorColor = Color.FromArgb(70, 130, 180);
        private int _thickness = 2;
        private int _cornerRadius = 0;
        private SeparatorOrientation _orientation = SeparatorOrientation.Horizontal;
        private bool _useGradient = false;
        private Color _gradientStartColor = Color.FromArgb(70, 130, 180);
        private Color _gradientEndColor = Color.FromArgb(100, 149, 237);
        private LinearGradientMode _gradientDirection = LinearGradientMode.Horizontal;
        private bool _highQuality = true;
        private float _opacity = 1.0f;
        private SeparatorShape _shape = SeparatorShape.Solid;
        private int _dashLength = 5;
        private int _gapLength = 3;

        #endregion

        #region Public Properties

        [Category("Appearance")]
        [Description("The color of the separator.")]
        public Color SeparatorColor
        {
            get => _separatorColor;
            set
            {
                _separatorColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The thickness of the separator in pixels.")]
        [DefaultValue(2)]
        public int Thickness
        {
            get => _thickness;
            set
            {
                _thickness = Math.Max(1, value);
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The corner radius for rounded separators.")]
        [DefaultValue(0)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The orientation of the separator.")]
        [DefaultValue(SeparatorOrientation.Horizontal)]
        public SeparatorOrientation Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                Invalidate();
            }
        }

        [Category("Gradient")]
        [Description("Whether to use gradient coloring for the separator.")]
        [DefaultValue(false)]
        public bool UseGradient
        {
            get => _useGradient;
            set
            {
                _useGradient = value;
                Invalidate();
            }
        }

        [Category("Gradient")]
        [Description("The starting color of the gradient.")]
        public Color GradientStartColor
        {
            get => _gradientStartColor;
            set
            {
                _gradientStartColor = value;
                Invalidate();
            }
        }

        [Category("Gradient")]
        [Description("The ending color of the gradient.")]
        public Color GradientEndColor
        {
            get => _gradientEndColor;
            set
            {
                _gradientEndColor = value;
                Invalidate();
            }
        }

        [Category("Gradient")]
        [Description("The direction of the gradient.")]
        [DefaultValue(LinearGradientMode.Horizontal)]
        public LinearGradientMode GradientDirection
        {
            get => _gradientDirection;
            set
            {
                _gradientDirection = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Whether to use high quality rendering.")]
        [DefaultValue(true)]
        public bool HighQuality
        {
            get => _highQuality;
            set
            {
                _highQuality = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The opacity of the separator (0.0 - 1.0).")]
        [DefaultValue(1.0f)]
        public float Opacity
        {
            get => _opacity;
            set
            {
                _opacity = Extensions.Clamp(value, 0.0f, 1.0f);
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The shape style of the separator.")]
        [DefaultValue(SeparatorShape.Solid)]
        public SeparatorShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The length of dashes for dashed separators.")]
        [DefaultValue(5)]
        public int DashLength
        {
            get => _dashLength;
            set
            {
                _dashLength = Math.Max(1, value);
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The length of gaps for dashed separators.")]
        [DefaultValue(3)]
        public int GapLength
        {
            get => _gapLength;
            set
            {
                _gapLength = Math.Max(1, value);
                Invalidate();
            }
        }

        #endregion

        #region Constructor

        public VortexSeparator()
        {
            // Set default size based on orientation
            Size = new Size(100, 2);
            
            // Set double buffering to reduce flicker
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.CacheText, true);
            
            // Set transparent background
            BackColor = Color.Transparent;
        }

        #endregion

        #region Overridden Methods

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Configure graphics quality optimized for scrolling
            if (HighQuality)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.Bilinear;
                g.PixelOffsetMode = PixelOffsetMode.None; // Prevents rendering issues during scrolling
                g.CompositingQuality = CompositingQuality.HighSpeed;
            }

            // Calculate separator rectangle based on orientation
            Rectangle separatorRect = GetSeparatorRectangle();

            if (separatorRect.Width <= 0 || separatorRect.Height <= 0)
                return;

            // Draw the separator based on shape
            DrawSeparatorShape(g, separatorRect);

            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        #endregion

        #region Private Methods

        private Rectangle GetSeparatorRectangle()
        {
            Rectangle clientRect = ClientRectangle;
            
            if (Orientation == SeparatorOrientation.Horizontal)
            {
                // Horizontal separator - center vertically
                int y = (clientRect.Height - Thickness) / 2;
                return new Rectangle(0, y, clientRect.Width, Thickness);
            }
            else
            {
                // Vertical separator - center horizontally
                int x = (clientRect.Width - Thickness) / 2;
                return new Rectangle(x, 0, Thickness, clientRect.Height);
            }
        }

        private void DrawSeparatorShape(Graphics g, Rectangle rect)
        {
            switch (Shape)
            {
                case SeparatorShape.Solid:
                    DrawSolidSeparator(g, rect);
                    break;
                case SeparatorShape.Dashed:
                    DrawDashedSeparator(g, rect);
                    break;
                case SeparatorShape.Dotted:
                    DrawDottedSeparator(g, rect);
                    break;
                case SeparatorShape.DoubleLine:
                    DrawDoubleLineSeparator(g, rect);
                    break;
                case SeparatorShape.Zigzag:
                    DrawZigzagSeparator(g, rect);
                    break;
                case SeparatorShape.Wave:
                    DrawWaveSeparator(g, rect);
                    break;
                case SeparatorShape.Cutting:
                    DrawCuttingSeparator(g, rect);
                    break;
            }
        }

        private Brush CreateSeparatorBrush(Rectangle rect)
        {
            Color finalColor = Color.FromArgb(
                (int)(255 * Opacity),
                SeparatorColor.R,
                SeparatorColor.G,
                SeparatorColor.B
            );

            if (UseGradient)
            {
                Color startColor = Color.FromArgb(
                    (int)(255 * Opacity),
                    GradientStartColor.R,
                    GradientStartColor.G,
                    GradientStartColor.B
                );

                Color endColor = Color.FromArgb(
                    (int)(255 * Opacity),
                    GradientEndColor.R,
                    GradientEndColor.G,
                    GradientEndColor.B
                );

                return new LinearGradientBrush(rect, startColor, endColor, GradientDirection);
            }
            else
            {
                return new SolidBrush(finalColor);
            }
        }

        private void DrawSolidSeparator(Graphics g, Rectangle rect)
        {
            Brush brush = CreateSeparatorBrush(rect);
            try
            {
                if (CornerRadius > 0)
                {
                    DrawRoundedSeparator(g, brush, rect);
                }
                else
                {
                    g.FillRectangle(brush, rect);
                }
            }
            finally
            {
                brush?.Dispose();
            }
        }

        private void DrawDashedSeparator(Graphics g, Rectangle rect)
        {
            using (Pen pen = CreateSeparatorPen())
            {
                pen.DashStyle = DashStyle.Custom;
                pen.DashPattern = new float[] { DashLength, GapLength };
                pen.Width = Thickness;
                
                // Apply rounded line caps when CornerRadius > 0
                if (CornerRadius > 0)
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    pen.DashCap = DashCap.Round;
                }
                else
                {
                    pen.StartCap = LineCap.Flat;
                    pen.EndCap = LineCap.Flat;
                    pen.DashCap = DashCap.Flat;
                }
                
                if (Orientation == SeparatorOrientation.Horizontal)
                {
                    int y = rect.Y + rect.Height / 2;
                    g.DrawLine(pen, rect.X, y, rect.Right, y);
                }
                else
                {
                    int x = rect.X + rect.Width / 2;
                    g.DrawLine(pen, x, rect.Y, x, rect.Bottom);
                }
            }
        }

        private void DrawDottedSeparator(Graphics g, Rectangle rect)
        {
            using (Pen pen = CreateSeparatorPen())
            {
                pen.DashStyle = DashStyle.Dot;
                pen.Width = Thickness;
                
                // Apply rounded line caps when CornerRadius > 0
                if (CornerRadius > 0)
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    pen.DashCap = DashCap.Round;
                }
                else
                {
                    pen.StartCap = LineCap.Flat;
                    pen.EndCap = LineCap.Flat;
                    pen.DashCap = DashCap.Flat;
                }
                
                if (Orientation == SeparatorOrientation.Horizontal)
                {
                    int y = rect.Y + rect.Height / 2;
                    g.DrawLine(pen, rect.X, y, rect.Right, y);
                }
                else
                {
                    int x = rect.X + rect.Width / 2;
                    g.DrawLine(pen, x, rect.Y, x, rect.Bottom);
                }
            }
        }

        private void DrawDoubleLineSeparator(Graphics g, Rectangle rect)
        {
            using (Pen pen = CreateSeparatorPen())
            {
                pen.Width = Math.Max(1, Thickness / 3);
                
                if (CornerRadius > 0)
                {
                    // Create two smaller rectangles for double line effect with rounded corners
                    if (Orientation == SeparatorOrientation.Horizontal)
                    {
                        int lineHeight = Math.Max(1, rect.Height / 4);
                        Rectangle rect1 = new Rectangle(rect.X, rect.Y + rect.Height / 4 - lineHeight / 2, rect.Width, lineHeight);
                        Rectangle rect2 = new Rectangle(rect.X, rect.Y + (3 * rect.Height) / 4 - lineHeight / 2, rect.Width, lineHeight);
                        
                        using (GraphicsPath path1 = CreateRoundedRectanglePath(rect1, CornerRadius / 2))
                        using (GraphicsPath path2 = CreateRoundedRectanglePath(rect2, CornerRadius / 2))
                        {
                            g.DrawPath(pen, path1);
                            g.DrawPath(pen, path2);
                        }
                    }
                    else
                    {
                        int lineWidth = Math.Max(1, rect.Width / 4);
                        Rectangle rect1 = new Rectangle(rect.X + rect.Width / 4 - lineWidth / 2, rect.Y, lineWidth, rect.Height);
                        Rectangle rect2 = new Rectangle(rect.X + (3 * rect.Width) / 4 - lineWidth / 2, rect.Y, lineWidth, rect.Height);
                        
                        using (GraphicsPath path1 = CreateRoundedRectanglePath(rect1, CornerRadius / 2))
                        using (GraphicsPath path2 = CreateRoundedRectanglePath(rect2, CornerRadius / 2))
                        {
                            g.DrawPath(pen, path1);
                            g.DrawPath(pen, path2);
                        }
                    }
                }
                else
                {
                    if (Orientation == SeparatorOrientation.Horizontal)
                    {
                        int y1 = rect.Y + rect.Height / 4;
                        int y2 = rect.Y + (3 * rect.Height) / 4;
                        g.DrawLine(pen, rect.X, y1, rect.Right, y1);
                        g.DrawLine(pen, rect.X, y2, rect.Right, y2);
                    }
                    else
                    {
                        int x1 = rect.X + rect.Width / 4;
                        int x2 = rect.X + (3 * rect.Width) / 4;
                        g.DrawLine(pen, x1, rect.Y, x1, rect.Bottom);
                        g.DrawLine(pen, x2, rect.Y, x2, rect.Bottom);
                    }
                }
            }
        }

        private void DrawZigzagSeparator(Graphics g, Rectangle rect)
        {
            if (CornerRadius > 0)
            {
                // Draw rounded background first
                using (Brush brush = CreateSeparatorBrush(rect))
                using (GraphicsPath path = CreateRoundedRectanglePath(rect, CornerRadius))
                {
                    g.FillPath(brush, path);
                }
            }
            else
            {
                using (Pen pen = CreateSeparatorPen())
                {
                    List<Point> points = new List<Point>();
                    int amplitude = Math.Max(2, Thickness);
                    
                    if (Orientation == SeparatorOrientation.Horizontal)
                    {
                        int centerY = rect.Y + rect.Height / 2;
                        int step = DashLength;
                        
                        for (int x = rect.X; x <= rect.Right; x += step)
                        {
                            int y = centerY + (points.Count % 2 == 0 ? -amplitude : amplitude);
                            points.Add(new Point(Math.Min(x, rect.Right), y));
                        }
                    }
                    else
                    {
                        int centerX = rect.X + rect.Width / 2;
                        int step = DashLength;
                        
                        for (int y = rect.Y; y <= rect.Bottom; y += step)
                        {
                            int x = centerX + (points.Count % 2 == 0 ? -amplitude : amplitude);
                            points.Add(new Point(x, Math.Min(y, rect.Bottom)));
                        }
                    }
                    
                    if (points.Count > 1)
                    {
                        g.DrawLines(pen, points.ToArray());
                    }
                }
            }
        }

        private void DrawWaveSeparator(Graphics g, Rectangle rect)
        {
            if (CornerRadius > 0)
            {
                // Draw rounded background first
                using (Brush brush = CreateSeparatorBrush(rect))
                using (GraphicsPath path = CreateRoundedRectanglePath(rect, CornerRadius))
                {
                    g.FillPath(brush, path);
                }
            }
            else
            {
                using (Pen pen = CreateSeparatorPen())
                {
                    List<Point> points = new List<Point>();
                    int amplitude = Math.Max(2, Thickness);
                    
                    if (Orientation == SeparatorOrientation.Horizontal)
                    {
                        int centerY = rect.Y + rect.Height / 2;
                        
                        for (int x = rect.X; x <= rect.Right; x += 2)
                        {
                            double angle = (x - rect.X) * Math.PI / (DashLength * 2);
                            int y = centerY + (int)(Math.Sin(angle) * amplitude);
                            points.Add(new Point(x, y));
                        }
                    }
                    else
                    {
                        int centerX = rect.X + rect.Width / 2;
                        
                        for (int y = rect.Y; y <= rect.Bottom; y += 2)
                        {
                            double angle = (y - rect.Y) * Math.PI / (DashLength * 2);
                            int x = centerX + (int)(Math.Sin(angle) * amplitude);
                            points.Add(new Point(x, y));
                        }
                    }
                    
                    if (points.Count > 1)
                    {
                        g.DrawLines(pen, points.ToArray());
                    }
                }
            }
        }

        private void DrawCuttingSeparator(Graphics g, Rectangle rect)
        {
            if (CornerRadius > 0)
            {
                // Draw rounded background first
                using (Brush brush = CreateSeparatorBrush(rect))
                using (GraphicsPath path = CreateRoundedRectanglePath(rect, CornerRadius))
                {
                    g.FillPath(brush, path);
                }
            }
            else
            {
                using (Pen pen = CreateSeparatorPen())
                {
                    List<Point> points = new List<Point>();
                    int cutSize = Math.Max(3, Thickness * 2);
                    
                    if (Orientation == SeparatorOrientation.Horizontal)
                    {
                        int centerY = rect.Y + rect.Height / 2;
                        int step = DashLength + GapLength;
                        
                        for (int x = rect.X; x < rect.Right; x += step)
                        {
                            // Create cutting pattern (triangular cuts)
                            points.Add(new Point(x, centerY));
                            points.Add(new Point(Math.Min(x + DashLength / 2, rect.Right), centerY - cutSize));
                            points.Add(new Point(Math.Min(x + DashLength, rect.Right), centerY));
                            points.Add(new Point(Math.Min(x + DashLength / 2, rect.Right), centerY + cutSize));
                            points.Add(new Point(x, centerY));
                            
                            if (points.Count > 1)
                            {
                                g.DrawLines(pen, points.ToArray());
                            }
                            points.Clear();
                        }
                    }
                    else
                    {
                        int centerX = rect.X + rect.Width / 2;
                        int step = DashLength + GapLength;
                        
                        for (int y = rect.Y; y < rect.Bottom; y += step)
                        {
                            // Create cutting pattern (triangular cuts)
                            points.Add(new Point(centerX, y));
                            points.Add(new Point(centerX - cutSize, Math.Min(y + DashLength / 2, rect.Bottom)));
                            points.Add(new Point(centerX, Math.Min(y + DashLength, rect.Bottom)));
                            points.Add(new Point(centerX + cutSize, Math.Min(y + DashLength / 2, rect.Bottom)));
                            points.Add(new Point(centerX, y));
                            
                            if (points.Count > 1)
                            {
                                g.DrawLines(pen, points.ToArray());
                            }
                            points.Clear();
                        }
                    }
                }
            }
        }

        private Pen CreateSeparatorPen()
        {
            Color finalColor = Color.FromArgb(
                (int)(255 * Opacity),
                SeparatorColor.R,
                SeparatorColor.G,
                SeparatorColor.B
            );
            
            return new Pen(finalColor, Math.Max(1, Thickness));
        }

        private void DrawRoundedSeparator(Graphics g, Brush brush, Rectangle rect)
        {
            using (GraphicsPath path = CreateRoundedRectanglePath(rect, CornerRadius))
            {
                g.FillPath(brush, path);
            }
        }

        private GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            
            // Ensure radius doesn't exceed half the smallest dimension
            int actualRadius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
            
            if (actualRadius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = actualRadius * 2;
            
            // Top-left corner
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            
            // Top-right corner
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            
            // Bottom-right corner
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            
            // Bottom-left corner
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            
            path.CloseFigure();
            return path;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the separator to horizontal orientation with specified thickness.
        /// </summary>
        /// <param name="thickness">The thickness of the horizontal separator.</param>
        public void SetHorizontal(int thickness = 2)
        {
            Orientation = SeparatorOrientation.Horizontal;
            Thickness = thickness;
            if (Width < 50) Width = 100; // Ensure reasonable width for horizontal separator
            Height = Math.Max(thickness, 10); // Ensure enough height to contain the separator
        }

        /// <summary>
        /// Sets the separator to vertical orientation with specified thickness.
        /// </summary>
        /// <param name="thickness">The thickness of the vertical separator.</param>
        public void SetVertical(int thickness = 2)
        {
            Orientation = SeparatorOrientation.Vertical;
            Thickness = thickness;
            if (Height < 50) Height = 100; // Ensure reasonable height for vertical separator
            Width = Math.Max(thickness, 10); // Ensure enough width to contain the separator
        }

        /// <summary>
        /// Applies a preset style to the separator.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        public void ApplyStyle(SeparatorStyle style)
        {
            switch (style)
            {
                case SeparatorStyle.Simple:
                    Shape = SeparatorShape.Solid;
                    UseGradient = false;
                    CornerRadius = 0;
                    Opacity = 1.0f;
                    break;
                    
                case SeparatorStyle.Rounded:
                    Shape = SeparatorShape.Solid;
                    UseGradient = false;
                    CornerRadius = Thickness;
                    Opacity = 1.0f;
                    break;
                    
                case SeparatorStyle.Gradient:
                    Shape = SeparatorShape.Solid;
                    UseGradient = true;
                    CornerRadius = 0;
                    Opacity = 1.0f;
                    break;
                    
                case SeparatorStyle.RoundedGradient:
                    Shape = SeparatorShape.Solid;
                    UseGradient = true;
                    CornerRadius = Thickness;
                    Opacity = 1.0f;
                    break;
                    
                case SeparatorStyle.Subtle:
                    Shape = SeparatorShape.Solid;
                    UseGradient = false;
                    CornerRadius = 0;
                    Opacity = 0.5f;
                    break;
                    
                case SeparatorStyle.Dashed:
                    Shape = SeparatorShape.Dashed;
                    UseGradient = false;
                    CornerRadius = 0;
                    Opacity = 1.0f;
                    break;
                    
                case SeparatorStyle.Dotted:
                    Shape = SeparatorShape.Dotted;
                    UseGradient = false;
                    CornerRadius = 0;
                    Opacity = 1.0f;
                    break;
                    
                case SeparatorStyle.Cutting:
                    Shape = SeparatorShape.Cutting;
                    UseGradient = false;
                    CornerRadius = 0;
                    Opacity = 1.0f;
                    break;
                    
                case SeparatorStyle.Wave:
                    Shape = SeparatorShape.Wave;
                    UseGradient = false;
                    CornerRadius = 0;
                    Opacity = 1.0f;
                    break;
                    
                case SeparatorStyle.Zigzag:
                    Shape = SeparatorShape.Zigzag;
                    UseGradient = false;
                    CornerRadius = 0;
                    Opacity = 1.0f;
                    break;
            }
        }

        #endregion
    }

    /// <summary>
    /// Predefined styles for the VortexSeparator.
    /// </summary>
    public enum SeparatorStyle
    {
        Simple,
        Rounded,
        Gradient,
        RoundedGradient,
        Subtle,
        Dashed,
        Dotted,
        Cutting,
        Wave,
        Zigzag
    }
}