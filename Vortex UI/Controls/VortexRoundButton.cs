using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ContentAlignment = System.Drawing.ContentAlignment;
using Timer = System.Windows.Forms.Timer;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexRoundButton : Control
    {
        #region Private Fields
        private int _cornerRadius = 15;
        private Color _backgroundColor = Color.FromArgb(70, 130, 180);
        private Color _hoverColor = Color.FromArgb(100, 149, 237);
        private Color _pressedColor = Color.FromArgb(65, 105, 225);
        private string _text = "Button";
        private Font _font;
        private Color _textColor = Color.White;
        private Image _image;
        private Size _imageSize = new Size(16, 16);
        private ContentAlignment _imageAlign = ContentAlignment.MiddleLeft;
        private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;
        private int _imagePadding = 8;
        private int _textPadding = 8;

        // Animation
        private float _animationProgress = 0f;
        private Timer _animationTimer;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private bool _animatingIn = false;

        // Loading
        private bool _isLoading = false;
        private float _loadingAngle = 0f;
        private Timer _loadingTimer;
        #endregion

        #region Properties
        [Category("Appearance")]
        [Description("Corner radius for rounded edges")]
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
        [Description("Background color of the button")]
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color when mouse hovers over button")]
        public Color HoverColor
        {
            get => _hoverColor;
            set
            {
                _hoverColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color when button is pressed")]
        public Color PressedColor
        {
            get => _pressedColor;
            set
            {
                _pressedColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Button text")]
        public override string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Text color")]
        public Color TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Button font")]
        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                Invalidate();
            }
        }

        [Category("Image")]
        [Description("Button image")]
        public Image Image
        {
            get => _image;
            set
            {
                _image = value;
                Invalidate();
            }
        }

        [Category("Image")]
        [Description("Size of the image")]
        public Size ImageSize
        {
            get => _imageSize;
            set
            {
                _imageSize = value;
                Invalidate();
            }
        }

        [Category("Layout")]
        [Description("Image alignment")]
        public ContentAlignment ImageAlign
        {
            get => _imageAlign;
            set
            {
                _imageAlign = value;
                Invalidate();
            }
        }

        [Category("Layout")]
        [Description("Text alignment")]
        public ContentAlignment TextAlign
        {
            get => _textAlign;
            set
            {
                _textAlign = value;
                Invalidate();
            }
        }

        [Category("Layout")]
        [Description("Image padding")]
        public int ImagePadding
        {
            get => _imagePadding;
            set
            {
                _imagePadding = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("Layout")]
        [Description("Text padding")]
        public int TextPadding
        {
            get => _textPadding;
            set
            {
                _textPadding = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("Behavior")]
        [Description("Show loading animation")]
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    if (_isLoading)
                        StartLoadingAnimation();
                    else
                        StopLoadingAnimation();
                    Invalidate();
                }
            }
        }
        #endregion

        #region Constructor
        public VortexRoundButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            Size = new Size(120, 40);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            // Initialize animation timer
            _animationTimer = new Timer();
            _animationTimer.Interval = 16; // ~60 FPS
            _animationTimer.Tick += AnimationTimer_Tick;

            // Initialize loading timer
            _loadingTimer = new Timer();
            _loadingTimer.Interval = 16;
            _loadingTimer.Tick += LoadingTimer_Tick;
        }
        #endregion

        #region Animation Methods
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (_animatingIn)
            {
                _animationProgress += 0.1f;
                if (_animationProgress >= 1f)
                {
                    _animationProgress = 1f;
                    _animationTimer.Stop();
                }
            }
            else
            {
                _animationProgress -= 0.15f;
                if (_animationProgress <= 0f)
                {
                    _animationProgress = 0f;
                    _animationTimer.Stop();
                }
            }
            Invalidate();
        }

        private void LoadingTimer_Tick(object sender, EventArgs e)
        {
            _loadingAngle += 10f;
            if (_loadingAngle >= 360f)
                _loadingAngle = 0f;
            Invalidate();
        }

        private void StartLoadingAnimation()
        {
            _loadingTimer.Start();
        }

        private void StopLoadingAnimation()
        {
            _loadingTimer.Stop();
            _loadingAngle = 0f;
        }
        #endregion

        #region Mouse Events
        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            _animatingIn = true;
            _animationTimer.Start();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            _animatingIn = false;
            _animationTimer.Start();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = false;
                Invalidate();
            }
            base.OnMouseUp(e);
        }
        #endregion

        #region Paint Method
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Ultra-high quality rendering settings for smoothest corners
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Create rounded rectangle path with improved precision
            using (GraphicsPath path = CreateSmoothRoundedRectangle())
            {
                // Calculate current color based on state and animation
                Color currentColor = GetCurrentBackgroundColor();

                // Fill background with smooth brush
                using (SolidBrush brush = new SolidBrush(currentColor))
                {
                    g.FillPath(brush, path);
                }

                // Set clipping region for content
                g.SetClip(path);
            }

            // Draw content
            if (_isLoading)
            {
                DrawLoadingSpinner(g);
            }
            else
            {
                DrawContent(g);
            }

            // Reset clipping
            g.ResetClip();
        }

        private GraphicsPath CreateSmoothRoundedRectangle()
        {
            GraphicsPath path = new GraphicsPath();

            // Use floating point for better precision
            RectangleF rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            float radius = Math.Min(_cornerRadius, Math.Min(rect.Width, rect.Height) / 2f);

            if (radius <= 0)
            {
                path.AddRectangle(Rectangle.Round(rect));
                return path;
            }

            float diameter = radius * 2f;

            // Create arcs with floating point precision for ultra-smooth corners
            // Top-left corner
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180f, 90f);

            // Top edge
            path.AddLine(rect.X + radius, rect.Y, rect.X + rect.Width - radius, rect.Y);

            // Top-right corner
            path.AddArc(rect.X + rect.Width - diameter, rect.Y, diameter, diameter, 270f, 90f);

            // Right edge
            path.AddLine(rect.X + rect.Width, rect.Y + radius, rect.X + rect.Width, rect.Y + rect.Height - radius);

            // Bottom-right corner
            path.AddArc(rect.X + rect.Width - diameter, rect.Y + rect.Height - diameter, diameter, diameter, 0f, 90f);

            // Bottom edge
            path.AddLine(rect.X + rect.Width - radius, rect.Y + rect.Height, rect.X + radius, rect.Y + rect.Height);

            // Bottom-left corner
            path.AddArc(rect.X, rect.Y + rect.Height - diameter, diameter, diameter, 90f, 90f);

            // Left edge back to start
            path.AddLine(rect.X, rect.Y + rect.Height - radius, rect.X, rect.Y + radius);

            path.CloseFigure();
            return path;
        }

        private Color GetCurrentBackgroundColor()
        {
            if (_isPressed)
                return BlendColors(_backgroundColor, _pressedColor, _animationProgress);
            else if (_isHovered)
                return BlendColors(_backgroundColor, _hoverColor, _animationProgress);
            else
                return BlendColors(_hoverColor, _backgroundColor, 1f - _animationProgress);
        }

        private Color BlendColors(Color color1, Color color2, float ratio)
        {
            ratio = Math.Max(0f, Math.Min(1f, ratio));

            int r = (int)(color1.R + (color2.R - color1.R) * ratio);
            int g = (int)(color1.G + (color2.G - color1.G) * ratio);
            int b = (int)(color1.B + (color2.B - color1.B) * ratio);
            int a = (int)(color1.A + (color2.A - color1.A) * ratio);

            return Color.FromArgb(a, r, g, b);
        }

        private void DrawContent(Graphics g)
        {
            Rectangle contentRect = ClientRectangle;

            bool hasImage = _image != null;
            bool hasText = !string.IsNullOrEmpty(_text);

            if (hasImage && hasText)
            {
                DrawImageAndText(g, contentRect);
            }
            else if (hasImage)
            {
                DrawImage(g, contentRect);
            }
            else if (hasText)
            {
                DrawBetterText(g, contentRect);
            }
        }

        private void DrawImageAndText(Graphics g, Rectangle contentRect)
        {
            Rectangle imageRect = GetImageRectangle(contentRect);
            Rectangle textRect = GetTextRectangle(contentRect, imageRect);

            // Draw image
            if (_image != null)
            {
                g.DrawImage(_image, imageRect);
            }

            // Draw text
            if (!string.IsNullOrEmpty(_text))
            {
                using (SolidBrush textBrush = new SolidBrush(_textColor))
                using (StringFormat sf = GetStringFormat(_textAlign))
                {
                    DrawBetterText(g, textRect);
                }
            }
        }

        private void DrawBetterText(Graphics g, Rectangle rect)
        {
            if (string.IsNullOrEmpty(_text)) return;

            TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                                    TextFormatFlags.VerticalCenter |
                                    TextFormatFlags.EndEllipsis;

            TextRenderer.DrawText(g, _text, Font, rect, _textColor, flags);
        }


        private void DrawImage(Graphics g, Rectangle contentRect)
        {
            if (_image == null) return;

            Rectangle imageRect = GetImageRectangle(contentRect);
            g.DrawImage(_image, imageRect);
        }

        private void DrawSystemLikeText(Graphics g, Rectangle rect)
        {
            if (string.IsNullOrEmpty(_text)) return;

            VisualStyleRenderer renderer =
                new VisualStyleRenderer(VisualStyleElement.Button.PushButton.Normal);

            renderer.DrawText(g, rect, _text, false, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private Rectangle GetImageRectangle(Rectangle contentRect)
        {
            Size imageSize = _imageSize;
            Point imageLocation = GetAlignedPosition(contentRect, imageSize, _imageAlign);

            return new Rectangle(
                imageLocation.X + _imagePadding,
                imageLocation.Y + _imagePadding,
                                imageSize.Width,
                imageSize.Height
            );
        }

        private Rectangle GetTextRectangle(Rectangle contentRect, Rectangle imageRect)
        {
            Rectangle textRect = contentRect;

            // Adjust text rectangle based on image position
            switch (_imageAlign)
            {
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.TopLeft:
                case ContentAlignment.BottomLeft:
                    textRect.X = imageRect.Right + _imagePadding;
                    textRect.Width = contentRect.Right - textRect.X - _textPadding;
                    break;

                case ContentAlignment.MiddleRight:
                case ContentAlignment.TopRight:
                case ContentAlignment.BottomRight:
                    textRect.Width = imageRect.X - contentRect.X - _imagePadding - _textPadding;
                    break;

                case ContentAlignment.TopCenter:
                    textRect.Y = imageRect.Bottom + _imagePadding;
                    textRect.Height = contentRect.Bottom - textRect.Y - _textPadding;
                    break;

                case ContentAlignment.BottomCenter:
                    textRect.Height = imageRect.Y - contentRect.Y - _imagePadding - _textPadding;
                    break;
            }

            // Apply text padding
            textRect.X += _textPadding;
            textRect.Y += _textPadding;
            textRect.Width -= _textPadding * 2;
            textRect.Height -= _textPadding * 2;

            return textRect;
        }

        private Point GetAlignedPosition(Rectangle container, Size itemSize, ContentAlignment alignment)
        {
            Point position = container.Location;

            switch (alignment)
            {
                case ContentAlignment.TopLeft:
                    // Already set
                    break;
                case ContentAlignment.TopCenter:
                    position.X += (container.Width - itemSize.Width) / 2;
                    break;
                case ContentAlignment.TopRight:
                    position.X += container.Width - itemSize.Width;
                    break;
                case ContentAlignment.MiddleLeft:
                    position.Y += (container.Height - itemSize.Height) / 2;
                    break;
                case ContentAlignment.MiddleCenter:
                    position.X += (container.Width - itemSize.Width) / 2;
                    position.Y += (container.Height - itemSize.Height) / 2;
                    break;
                case ContentAlignment.MiddleRight:
                    position.X += container.Width - itemSize.Width;
                    position.Y += (container.Height - itemSize.Height) / 2;
                    break;
                case ContentAlignment.BottomLeft:
                    position.Y += container.Height - itemSize.Height;
                    break;
                case ContentAlignment.BottomCenter:
                    position.X += (container.Width - itemSize.Width) / 2;
                    position.Y += container.Height - itemSize.Height;
                    break;
                case ContentAlignment.BottomRight:
                    position.X += container.Width - itemSize.Width;
                    position.Y += container.Height - itemSize.Height;
                    break;
            }

            return position;
        }

        private StringFormat GetStringFormat(ContentAlignment alignment)
        {
            StringFormat sf = new StringFormat(StringFormatFlags.NoWrap)
            {
                Trimming = StringTrimming.EllipsisCharacter,
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            switch (alignment)
            {
                case ContentAlignment.TopLeft:
                    sf.LineAlignment = StringAlignment.Near;
                    sf.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopCenter:
                    sf.LineAlignment = StringAlignment.Near;
                    sf.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.TopRight:
                    sf.LineAlignment = StringAlignment.Near;
                    sf.Alignment = StringAlignment.Far;
                    break;
                case ContentAlignment.MiddleLeft:
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.MiddleCenter:
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.MiddleRight:
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Far;
                    break;
                case ContentAlignment.BottomLeft:
                    sf.LineAlignment = StringAlignment.Far;
                    sf.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.BottomCenter:
                    sf.LineAlignment = StringAlignment.Far;
                    sf.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.BottomRight:
                    sf.LineAlignment = StringAlignment.Far;
                    sf.Alignment = StringAlignment.Far;
                    break;
            }

            return sf;
        }

        private void DrawLoadingSpinner(Graphics g)
        {
            Rectangle contentRect = ClientRectangle;
            int size = Math.Min(contentRect.Width, contentRect.Height) / 3;
            int centerX = contentRect.X + contentRect.Width / 2;
            int centerY = contentRect.Y + contentRect.Height / 2;

            Rectangle spinnerRect = new Rectangle(
                centerX - size / 2,
                centerY - size / 2,
                size,
                size
            );

            using (Pen pen = new Pen(_textColor, 3))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                // Draw background arc
                using (Pen backgroundPen = new Pen(Color.FromArgb(50, _textColor), 3))
                {
                    backgroundPen.StartCap = LineCap.Round;
                    backgroundPen.EndCap = LineCap.Round;
                    g.DrawEllipse(backgroundPen, spinnerRect);
                }

                // Draw animated arc
                g.DrawArc(pen, spinnerRect, _loadingAngle, 90);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Makes the button perfectly circular
        /// </summary>
        public void MakeCircular()
        {
            int minDimension = Math.Min(Width, Height);
            CornerRadius = minDimension / 2;
        }

        /// <summary>
        /// Sets the button to rectangular (no rounded corners)
        /// </summary>
        public void MakeRectangular()
        {
            CornerRadius = 0;
        }

        /// <summary>
        /// Animates the corner radius change
        /// </summary>
        public async Task AnimateCornerRadiusAsync(int targetRadius, int durationMs = 300)
        {
            int startRadius = _cornerRadius;
            int steps = 30;
            int stepDelay = durationMs / steps;

            for (int i = 0; i <= steps; i++)
            {
                float progress = (float)i / steps;
                _cornerRadius = (int)(startRadius + (targetRadius - startRadius) * progress);

                Invalidate();
                await Task.Delay(stepDelay);
            }

            _cornerRadius = targetRadius;
            Invalidate();
        }

        /// <summary>
        /// Sets button colors for different states
        /// </summary>
        public void SetColors(Color background, Color hover, Color pressed)
        {
            BackgroundColor = background;
            HoverColor = hover;
            PressedColor = pressed;
        }

        /// <summary>
        /// Sets button colors with text color
        /// </summary>
        public void SetColors(Color background, Color hover, Color pressed, Color text)
        {
            SetColors(background, hover, pressed);
            TextColor = text;
        }

        /// <summary>
        /// Applies a modern flat design style
        /// </summary>

        public void ResetState()
        {
            _animationTimer.Stop();
            _loadingTimer.Stop();
            _animationProgress = 0f;
            _loadingAngle = 0f;
            _isHovered = false;
            _isPressed = false;
            _isLoading = false;
            Invalidate();
        }
        #endregion

        #region Cleanup
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();
                _loadingTimer?.Stop();
                _loadingTimer?.Dispose();
                _font?.Dispose();
                _image?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
