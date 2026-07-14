using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.IO;

namespace Vortex.Ui
{
    public enum ImageAlign
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    public enum TextAlign
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    [ToolboxItem(true)]
    public class VortexButton : Button
    {
        // Basic properties
        private bool _topLeftCut = false;
        private bool _topRightCut = false;
        private bool _bottomLeftCut = false;
        private bool _bottomRightCut = true;
        private int _cornerCutSize = 10;
        private Color _buttonColor = Color.FromArgb(70, 130, 180);
        private Color _hoverColor = Color.FromArgb(100, 149, 237);
        private Color _pressedColor = Color.FromArgb(65, 105, 225);

        // Gradient properties
        private bool _useGradient = false;
        private Color _gradientColor1 = Color.FromArgb(70, 130, 180);
        private Color _gradientColor2 = Color.FromArgb(100, 149, 237);
        private LinearGradientMode _gradientDirection = LinearGradientMode.Vertical;

        // Image properties
        private Image _buttonImage = null;
        private Size _imageSize = new Size(16, 16);
        private float _imageOpacity = 1.0f;

        // Simple alignment properties
        private ImageAlign _imageAlign = ImageAlign.Left;
        private TextAlign _textAlign = TextAlign.Center;

        // Offset properties
        private int _imageOffsetX = 0;
        private int _imageOffsetY = 0;
        private int _textOffsetX = 0;
        private int _textOffsetY = 0;

        // Animation properties
        private bool _isHovering = false;
        private bool _isPressed = false;
        private float _hoverAnimationProgress = 0f;
        private float _pressAnimationProgress = 0f;

        // Loading properties
        private bool _isLoading = false;
        private bool _enableLoadingAnimation = false;
        private int _loadingDuration = 3000;
        private float _loadingProgress = 0f;
        private CancellationTokenSource _loadingCts;
        private Color _loadingProgressColor = Color.FromArgb(86, 156, 255);
        private Color _loadingBackgroundColor = Color.FromArgb(60, 86, 156, 255);
        private int _loadingThickness = 3;
        private int _loadingSize = 20;

        // Animation cancellation tokens
        private CancellationTokenSource _hoverAnimationCts;
        private CancellationTokenSource _pressAnimationCts;
        private CancellationTokenSource _loadingAnimationCts;

        #region Properties

        // أضف override للـ BackColor عشان تقدر تتحكم فيه من Properties
        [Category("Appearance")]
        [Description("Background color of the button")]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                base.BackColor = value;
                Invalidate();
            }
        }


        [Category("Corner Cuts")]
        [Description("Enable cut for top left corner")]
        public bool TopLeftCut
        {
            get { return _topLeftCut; }
            set { _topLeftCut = value; Invalidate(); }
        }

        [Category("Corner Cuts")]
        [Description("Enable cut for top right corner")]
        public bool TopRightCut
        {
            get { return _topRightCut; }
            set { _topRightCut = value; Invalidate(); }
        }

        [Category("Corner Cuts")]
        [Description("Enable cut for bottom left corner")]
        public bool BottomLeftCut
        {
            get { return _bottomLeftCut; }
            set { _bottomLeftCut = value; Invalidate(); }
        }

        [Category("Corner Cuts")]
        [Description("Enable cut for bottom right corner")]
        public bool BottomRightCut
        {
            get { return _bottomRightCut; }
            set { _bottomRightCut = value; Invalidate(); }
        }

        [Category("Corner Cuts")]
        [Description("Size of the cut corners in pixels")]
        public int CornerCutSize
        {
            get { return _cornerCutSize; }
            set { _cornerCutSize = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Button background color")]
        public Color ButtonColor
        {
            get { return _buttonColor; }
            set { _buttonColor = value; Invalidate(); }
        }

        [Category("Gradient")]
        [Description("Enable gradient background")]
        public bool UseGradient
        {
            get { return _useGradient; }
            set { _useGradient = value; Invalidate(); }
        }

        [Category("Gradient")]
        [Description("First gradient color")]
        public Color GradientColor1
        {
            get { return _gradientColor1; }
            set { _gradientColor1 = value; Invalidate(); }
        }

        [Category("Gradient")]
        [Description("Second gradient color")]
        public Color GradientColor2
        {
            get { return _gradientColor2; }
            set { _gradientColor2 = value; Invalidate(); }
        }

        [Category("Gradient")]
        [Description("Direction of the gradient")]
        public LinearGradientMode GradientDirection
        {
            get { return _gradientDirection; }
            set { _gradientDirection = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Color when mouse hovers over the button")]
        public Color HoverColor
        {
            get { return _hoverColor; }
            set { _hoverColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Color when button is pressed")]
        public Color PressedColor
        {
            get { return _pressedColor; }
            set { _pressedColor = value; Invalidate(); }
        }

        [Category("Image")]
        [Description("Image to display on the button")]
        public Image ButtonImage
        {
            get { return _buttonImage; }
            set { _buttonImage = value; Invalidate(); }
        }

        // إخفاء خصائص الصورة المكررة من Button الأساسي
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Image Image
        {
            get { return base.Image; }
            set { base.Image = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new ImageList ImageList
        {
            get { return base.ImageList; }
            set { base.ImageList = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new int ImageIndex
        {
            get { return base.ImageIndex; }
            set { base.ImageIndex = value; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string ImageKey
        {
            get { return base.ImageKey; }
            set { base.ImageKey = value; }
        }

        [Category("Image")]
        [Description("Size of the image on the button")]
        public Size ImageSize
        {
            get { return _imageSize; }
            set { _imageSize = value; Invalidate(); }
        }

        [Category("Layout")]
        [Description("Alignment of the image within the button")]
        public ImageAlign ImageAlign
        {
            get { return _imageAlign; }
            set { _imageAlign = value; Invalidate(); }
        }

        [Category("Layout")]
        [Description("Alignment of the text within the button")]
        public TextAlign TextAlign
        {
            get { return _textAlign; }
            set { _textAlign = value; Invalidate(); }
        }

        [Category("Layout")]
        [Description("Horizontal offset for image position")]
        public int ImageOffsetX
        {
            get { return _imageOffsetX; }
            set { _imageOffsetX = value; Invalidate(); }
        }

        [Category("Layout")]
        [Description("Vertical offset for image position")]
        public int ImageOffsetY
        {
            get { return _imageOffsetY; }
            set { _imageOffsetY = value; Invalidate(); }
        }

        [Category("Layout")]
        [Description("Horizontal offset for text position")]
        public int TextOffsetX
        {
            get { return _textOffsetX; }
            set { _textOffsetX = value; Invalidate(); }
        }

        [Category("Layout")]
        [Description("Vertical offset for text position")]
        public int TextOffsetY
        {
            get { return _textOffsetY; }
            set { _textOffsetY = value; Invalidate(); }
        }

        [Category("Image")]
        [Description("Opacity of the image (0.0 = fully transparent, 1.0 = fully opaque)")]
        public float ImageOpacity
        {
            get { return _imageOpacity; }
            set
            {
                _imageOpacity = Math.Max(0.0f, Math.Min(1.0f, value));
                Invalidate();
            }
        }



        [Category("Loading")]
        [Description("Enable loading animation when button is clicked")]
        public bool EnableLoadingAnimation
        {
            get { return _enableLoadingAnimation; }
            set { _enableLoadingAnimation = value; }
        }

        [Category("Loading")]
        [Description("Duration of loading animation in milliseconds")]
        public int LoadingDuration
        {
            get { return _loadingDuration; }
            set { _loadingDuration = Math.Max(50, value); }
        }

        [Browsable(false)]
        public bool IsLoading
        {
            get { return _isLoading; }
        }

        [Category("Loading")]
        [Description("Color of the loading progress arc")]
        public Color LoadingProgressColor
        {
            get { return _loadingProgressColor; }
            set { _loadingProgressColor = value; Invalidate(); }
        }

        [Category("Loading")]
        [Description("Color of the loading progress background circle")]
        public Color LoadingBackgroundColor
        {
            get { return _loadingBackgroundColor; }
            set { _loadingBackgroundColor = value; Invalidate(); }
        }

        [Category("Loading")]
        [Description("Thickness of the loading progress ring in pixels")]
        [DefaultValue(3)]
        public int LoadingThickness
        {
            get { return _loadingThickness; }
            set { _loadingThickness = Math.Max(1, Math.Min(10, value)); Invalidate(); }
        }

        [Category("Loading")]
        [Description("Size (diameter) of the loading progress circle in pixels")]
        [DefaultValue(20)]
        public int LoadingSize
        {
            get { return _loadingSize; }
            set { _loadingSize = Math.Max(12, Math.Min(60, value)); Invalidate(); }
        }

        public event EventHandler LoadingComplete;
        #endregion

        public VortexButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            ForeColor = Color.Black;

            try
            {
                Font = new Font("Bahnschrift", 10, FontStyle.Regular);
            }
            catch
            {
                Font = new Font("Segoe UI", 10, FontStyle.Regular);
            }

            Size = new Size(250, 40);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelAnimations();
            }
            base.Dispose(disposing);
        }

        private void CancelAnimations()
        {
            _hoverAnimationCts?.Cancel();
            _hoverAnimationCts?.Dispose();
            _hoverAnimationCts = null;

            _pressAnimationCts?.Cancel();
            _pressAnimationCts?.Dispose();
            _pressAnimationCts = null;

            _loadingCts?.Cancel();
            _loadingCts?.Dispose();
            _loadingCts = null;

            _loadingAnimationCts?.Cancel();
            _loadingAnimationCts?.Dispose();
            _loadingAnimationCts = null;
        }

        #region Animation Methods

        /// <summary>
        /// Easing function for smoother animations
        /// </summary>
        private float EaseOutQuart(float t)
        {
            return 1 - (float)Math.Pow(1 - t, 4);
        }

        /// <summary>
        /// Easing function for smooth entry animations
        /// </summary>
        private float EaseInOutCubic(float t)
        {
            return t < 0.5f ? 4 * t * t * t : 1 - (float)Math.Pow(-2 * t + 2, 3) / 2;
        }
        private async Task AnimateHoverAsync(bool hovering, CancellationToken cancelToken)
        {
            try
            {
                float startValue = _hoverAnimationProgress;
                float targetValue = hovering ? 1.0f : 0.0f;
                
                // Instant response - no animation delay
                _hoverAnimationProgress = targetValue;
                
                if (IsHandleCreated && !IsDisposed)
                {
                    try
                    {
                        Invalidate();
                    }
                    catch { }
                }
                
                await Task.CompletedTask;
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hover animation error: {ex.Message}");
            }
        }

        private async Task AnimatePressAsync(bool pressed, CancellationToken cancelToken)
        {
            try
            {
                float startValue = _pressAnimationProgress;
                float targetValue = pressed ? 1.0f : 0.0f;
                
                // Instant response - no animation delay
                _pressAnimationProgress = targetValue;
                
                if (IsHandleCreated && !IsDisposed)
                {
                    try
                    {
                        Invalidate();
                    }
                    catch { }
                }
                
                await Task.CompletedTask;
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Press animation error: {ex.Message}");
            }
        }

        private async Task AnimateLoadingAsync(CancellationToken cancelToken)
        {
            try
            {
                _loadingProgress = 0f;
                
                // Use time-based animation instead of fixed steps for accuracy
                const int frameTime = 16; // ~60 FPS
                float duration = _loadingDuration;
                float elapsed = 0f;
                
                var startTime = DateTime.Now;

                while (!cancelToken.IsCancellationRequested && elapsed < duration)
                {
                    // Calculate actual elapsed time for accuracy
                    elapsed = (float)(DateTime.Now - startTime).TotalMilliseconds;
                    
                    // Calculate progress (0.0 to 1.0)
                    _loadingProgress = Math.Min(elapsed / duration, 1.0f);

                    if (IsHandleCreated && !IsDisposed)
                    {
                        try
                        {
                            BeginInvoke(new Action(() => Invalidate()));
                        }
                        catch
                        {
                            break;
                        }
                    }
                    else
                        break;

                    // Wait for next frame
                    await Task.Delay(frameTime, cancelToken);
                }

                // Ensure we reach 100%
                if (!cancelToken.IsCancellationRequested)
                {
                    _loadingProgress = 1.0f;
                    if (IsHandleCreated && !IsDisposed)
                        Invalidate();
                    
                    await Task.Delay(100, cancelToken);

                    _isLoading = false;
                    if (IsHandleCreated && !IsDisposed)
                        Invalidate();
                    
                    OnLoadingComplete(EventArgs.Empty);
                }
            }
            catch (TaskCanceledException)
            {
                _isLoading = false;
                if (IsHandleCreated && !IsDisposed)
                    Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Loading animation error: {ex.Message}");
                _isLoading = false;
                if (IsHandleCreated && !IsDisposed)
                    Invalidate();
            }
        }

        public void CancelLoading()
        {
            _loadingCts?.Cancel();
            _isLoading = false;
            Invalidate();
        }

        protected virtual void OnLoadingComplete(EventArgs e)
        {
            LoadingComplete?.Invoke(this, e);
        }

        private Color InterpolateColor(Color start, Color end, float ratio)
        {
            int r = (int)(start.R + (end.R - start.R) * ratio);
            int g = (int)(start.G + (end.G - start.G) * ratio);
            int b = (int)(start.B + (end.B - start.B) * ratio);
            return Color.FromArgb(r, g, b);
        }
        #endregion

        #region Mouse Event Handlers
        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovering = true;
            _hoverAnimationCts?.Cancel();
            _hoverAnimationCts?.Dispose();
            _hoverAnimationCts = new CancellationTokenSource();
            _ = AnimateHoverAsync(true, _hoverAnimationCts.Token);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovering = false;
            _isPressed = false;
            _hoverAnimationCts?.Cancel();
            _hoverAnimationCts?.Dispose();
            _hoverAnimationCts = new CancellationTokenSource();
            _pressAnimationCts?.Cancel();
            _pressAnimationCts?.Dispose();
            _pressAnimationCts = new CancellationTokenSource();
            _ = AnimateHoverAsync(false, _hoverAnimationCts.Token);
            _ = AnimatePressAsync(false, _pressAnimationCts.Token);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                _pressAnimationCts?.Cancel();
                _pressAnimationCts?.Dispose();
                _pressAnimationCts = new CancellationTokenSource();
                _ = AnimatePressAsync(true, _pressAnimationCts.Token);
                if (_enableLoadingAnimation && !_isLoading)
                    _ = StartLoadingAsync();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = false;
                _pressAnimationCts?.Cancel();
                _pressAnimationCts?.Dispose();
                _pressAnimationCts = new CancellationTokenSource();
                _ = AnimatePressAsync(false, _pressAnimationCts.Token);
            }
            base.OnMouseUp(e);
        }
        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            GraphicsPath path = CreateButtonPath();
            
            // Set region first to define the button's shape
            Region = new Region(path);
            
            // Calculate current color first
            Color currentColor = _buttonColor;
            if (_hoverAnimationProgress > 0)
                currentColor = InterpolateColor(currentColor, _hoverColor, _hoverAnimationProgress);
            if (_pressAnimationProgress > 0)
                currentColor = InterpolateColor(currentColor, _pressedColor, _pressAnimationProgress);

            // Offscreen rendering to fully control edge compositing
            using (var bmp = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
            using (var g2 = Graphics.FromImage(bmp))
            {
                g2.SmoothingMode = SmoothingMode.HighQuality;
                g2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g2.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g2.CompositingQuality = CompositingQuality.HighQuality;

                // Pre-clear with the target color so AA samples blend with same color
                g2.Clear(currentColor);

                if (_useGradient)
                {
                    Color gradColor1 = _gradientColor1;
                    Color gradColor2 = _gradientColor2;

                    // Apply hover/press effects to gradient colors
                    if (_isPressed && _pressAnimationProgress > 0)
                    {
                        gradColor1 = InterpolateColor(gradColor1, _pressedColor, _pressAnimationProgress);
                        gradColor2 = InterpolateColor(gradColor2, _pressedColor, _pressAnimationProgress);
                    }
                    else if (_isHovering && _hoverAnimationProgress > 0)
                    {
                        gradColor1 = InterpolateColor(gradColor1, _hoverColor, _hoverAnimationProgress);
                        gradColor2 = InterpolateColor(gradColor2, _hoverColor, _hoverAnimationProgress);
                    }

                    using (LinearGradientBrush gradientBrush = new LinearGradientBrush(ClientRectangle, gradColor1, gradColor2, _gradientDirection))
                    {
                        g2.FillPath(gradientBrush, path);
                    }
                }
                else
                {
                    using (SolidBrush brush = new SolidBrush(currentColor))
                    {
                        g2.FillPath(brush, path);
                    }
                }

                // Intentionally skip drawing text/images offscreen to keep TextRenderer crisp
                
                // Blit the composed image
                e.Graphics.DrawImageUnscaled(bmp, 0, 0);
            }

            // Draw content on the real Graphics for crisp text rendering
            if (!_isLoading)
            {
                DrawContent(e.Graphics, 0);
            }
            else
            {
                DrawLoadingProgress(e.Graphics);
            }

            // Reset clipping for safety
            e.Graphics.ResetClip();

            path.Dispose();
        }

        private void DrawContent(Graphics g, int pressOffset)
        {
            bool hasImage = _buttonImage != null;
            bool hasText = !string.IsNullOrEmpty(Text);

            if (!hasImage && !hasText) return;

            Rectangle buttonRect = new Rectangle(pressOffset, pressOffset, Width - pressOffset, Height - pressOffset);

            // Calculate positions
            Point imagePos = Point.Empty;
            Rectangle textRect = Rectangle.Empty;

            if (hasImage && hasText)
            {
                // Both image and text
                CalculateImageTextPositions(buttonRect, out imagePos, out textRect);
            }
            else if (hasImage)
            {
                // Only image
                imagePos = CalculateImagePosition(buttonRect);
            }
            else if (hasText)
            {
                // Only text
                textRect = CalculateTextRectangle(buttonRect);
            }

            // Draw image
            if (hasImage && imagePos != Point.Empty)
            {
                DrawImage(g, imagePos);
            }

            // Draw text
            if (hasText && !textRect.IsEmpty)
            {
                DrawText(g, textRect);
            }
        }

        private void CalculateImageTextPositions(Rectangle buttonRect, out Point imagePos, out Rectangle textRect)
        {
            int padding = 10;
            int spacing = 8; // spacing between icon and text
            int iconX, iconY;
            
            iconY = buttonRect.Y + (buttonRect.Height - _imageSize.Height) / 2 + _imageOffsetY;

            if (_imageAlign == ImageAlign.Right)
            {
                iconX = buttonRect.Right - _imageSize.Width - padding + _imageOffsetX;

                // Text occupies left area
                textRect = new Rectangle(
                    buttonRect.X + padding,
                    buttonRect.Y,
                    buttonRect.Width - (_imageSize.Width + spacing + padding * 2),
                    buttonRect.Height);
            }
            else if (_imageAlign == ImageAlign.Center)
            {
                // Calculate total width needed for icon + spacing + text
                Size textSize = GetTextSize();
                int totalWidth = _imageSize.Width + spacing + textSize.Width;
                
                // Center the combined icon+text as a unit
                int startX = buttonRect.X + (buttonRect.Width - totalWidth) / 2;
                
                iconX = startX + _imageOffsetX;
                
                // Text starts after icon + spacing
                textRect = new Rectangle(
                    startX + _imageSize.Width + spacing,
                    buttonRect.Y,
                    textSize.Width,
                    buttonRect.Height);
            }
            else // Left position
            {
                iconX = buttonRect.X + padding + _imageOffsetX;

                // Text starts after the icon
                textRect = new Rectangle(
                    buttonRect.X + padding + _imageSize.Width + spacing,
                    buttonRect.Y,
                    buttonRect.Width - (_imageSize.Width + spacing + padding * 2),
                    buttonRect.Height);
            }

            imagePos = new Point(iconX, iconY);
            
            // Apply text offsets
            textRect = new Rectangle(
                textRect.X + _textOffsetX,
                textRect.Y + _textOffsetY,
                textRect.Width - _textOffsetX,
                textRect.Height - _textOffsetY);
        }

        private Point CalculateImagePosition(Rectangle buttonRect)
        {
            int x, y;
            int padding = 10;

            switch (_imageAlign)
            {
                case ImageAlign.Left:
                    x = buttonRect.X + padding;
                    break;
                case ImageAlign.Right:
                    x = buttonRect.Right - _imageSize.Width - padding;
                    break;
                case ImageAlign.Center:
                default:
                    x = buttonRect.X + (buttonRect.Width - _imageSize.Width) / 2;
                    break;
            }

            y = buttonRect.Y + (buttonRect.Height - _imageSize.Height) / 2;
            return new Point(x + _imageOffsetX, y + _imageOffsetY);
        }

        private Rectangle CalculateTextRectangle(Rectangle buttonRect)
        {
            int padding = 10;
            int x, width;

            switch (_textAlign)
            {
                case TextAlign.Left:
                    x = buttonRect.X + padding;
                    width = buttonRect.Width - (padding * 2);
                    break;
                case TextAlign.Right:
                    x = buttonRect.X + padding;
                    width = buttonRect.Width - (padding * 2);
                    break;
                case TextAlign.Center:
                default:
                    x = buttonRect.X + padding;
                    width = buttonRect.Width - (padding * 2);
                    break;
            }

            return new Rectangle(x + _textOffsetX, buttonRect.Y + _textOffsetY, width, buttonRect.Height);
        }

        private Size GetTextSize()
        {
            if (string.IsNullOrEmpty(Text)) return Size.Empty;
            return TextRenderer.MeasureText(Text, Font);
        }

        private void DrawText(Graphics g, Rectangle textRect)
        {
            if (string.IsNullOrEmpty(Text)) return;

            TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;

            // If there's an image, text alignment is determined by the combined layout
            // If no image, use the text alignment setting
            if (_buttonImage == null)
            {
                switch (_textAlign)
                {
                    case TextAlign.Left:
                        flags |= TextFormatFlags.Left;
                        break;
                    case TextAlign.Right:
                        flags |= TextFormatFlags.Right;
                        break;
                    case TextAlign.Center:
                    default:
                        flags |= TextFormatFlags.HorizontalCenter;
                        break;
                }
            }
            else
            {
                // With image, always use left alignment for the text area
                flags |= TextFormatFlags.Left;
            }

            TextRenderer.DrawText(g, Text, Font, textRect, ForeColor, flags);
        }

        private void DrawImage(Graphics g, Point location)
        {
            if (_buttonImage == null) return;

            try
            {
                Rectangle imageRect = new Rectangle(location, _imageSize);

                DrawImageWithOpacity(g, _buttonImage, imageRect, _imageOpacity);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error drawing button image: {ex.Message}");
            }
        }

        private void DrawImageWithOpacity(Graphics g, Image image, Rectangle destRect, float opacity)
        {
            try
            {
                if (opacity >= 1.0f)
                {
                    g.DrawImage(image, destRect);
                }
                else
                {
                    using (ImageAttributes imageAttr = new ImageAttributes())
                    {
                        ColorMatrix colorMatrix = new ColorMatrix();
                        colorMatrix.Matrix33 = opacity;
                        imageAttr.SetColorMatrix(colorMatrix);
                        g.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttr);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error drawing image with opacity: {ex.Message}");
                g.DrawImage(image, destRect);
            }
        }



        private void DrawLoadingProgress(Graphics g)
        {
            if (_loadingProgress <= 0) return;

            try
            {
                // Save original graphics state
                var originalSmoothingMode = g.SmoothingMode;
                var originalInterpolationMode = g.InterpolationMode;
                var originalCompositingQuality = g.CompositingQuality;
                var originalPixelOffsetMode = g.PixelOffsetMode;

                // Set high-quality rendering for smooth progress ring
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                int centerX = Width / 2;
                int centerY = Height / 2;
                int diameter = _loadingSize;
                int radius = diameter / 2;

                // Just the progress circle centered
                RectangleF circleRect = new RectangleF(
                    centerX - radius,
                    centerY - radius,
                    diameter,
                    diameter);

                // Inset the circle rect by half thickness for proper centering
                float inset = _loadingThickness / 2f;
                circleRect.Inflate(-inset, -inset);

                // Background circle (full circle)
                using (Pen backgroundPen = new Pen(_loadingBackgroundColor, _loadingThickness))
                {
                    backgroundPen.StartCap = LineCap.Round;
                    backgroundPen.EndCap = LineCap.Round;
                    g.DrawEllipse(backgroundPen, circleRect);
                }

                // Progress arc (only draw if progress > 0)
                if (_loadingProgress > 0.01f)
                {
                    float sweepAngle = 360f * _loadingProgress;
                    
                    // Clamp sweep angle to prevent overdraw
                    sweepAngle = Math.Min(sweepAngle, 359.99f);
                    
                    using (Pen progressPen = new Pen(_loadingProgressColor, _loadingThickness))
                    {
                        progressPen.StartCap = LineCap.Round;
                        progressPen.EndCap = LineCap.Round;
                        
                        // Draw arc starting from top (-90 degrees)
                        g.DrawArc(progressPen, circleRect, -90, sweepAngle);
                    }
                }

                // Restore original graphics state
                g.SmoothingMode = originalSmoothingMode;
                g.InterpolationMode = originalInterpolationMode;
                g.CompositingQuality = originalCompositingQuality;
                g.PixelOffsetMode = originalPixelOffsetMode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error drawing loading progress: {ex.Message}");
            }
        }

        private GraphicsPath CreateButtonPath()
        {
            GraphicsPath path = new GraphicsPath();
            
            // Remove scaling - it causes lag and glitches
            RectangleF rect = new RectangleF(0, 0, Width, Height);
            float scaledCutSize = _cornerCutSize;

            // Pixel-align edges to avoid AA gaps
            float l = (float)Math.Floor(rect.Left) + 0.5f;
            float t = (float)Math.Floor(rect.Top) + 0.5f;
            float r = (float)Math.Ceiling(rect.Right) - 0.5f;
            float b = (float)Math.Ceiling(rect.Bottom) - 0.5f;

            // Create the path with proper corner cuts
            if (_topLeftCut || _topRightCut || _bottomLeftCut || _bottomRightCut)
            {
                // Start from top-left corner
                if (_topLeftCut)
                {
                    path.AddLine(l + scaledCutSize, t, r - (_topRightCut ? scaledCutSize : 0), t);
                }
                else
                {
                    path.AddLine(l, t, r - (_topRightCut ? scaledCutSize : 0), t);
                }

                // Top-right corner
                if (_topRightCut)
                {
                    path.AddLine(r - (_topRightCut ? scaledCutSize : 0), t, r, t + scaledCutSize);
                }
                else
                {
                    path.AddLine(r - (_topRightCut ? scaledCutSize : 0), t, r, t);
                }

                // Right edge to bottom-right (slightly extend to cover AA fringe)
                if (_bottomRightCut)
                {
                    path.AddLine(r + 0.5f, t + (_topRightCut ? scaledCutSize : 0), r + 0.5f, b - scaledCutSize);
                    path.AddLine(r + 0.5f, b - scaledCutSize, r - scaledCutSize, b + 0.5f);
                }
                else
                {
                    path.AddLine(r + 0.5f, t + (_topRightCut ? scaledCutSize : 0), r + 0.5f, b);
                }

                // Bottom edge (slightly extend to cover AA fringe)
                if (_bottomLeftCut)
                {
                    path.AddLine(r - (_bottomRightCut ? scaledCutSize : 0), b + 0.5f, l + scaledCutSize, b + 0.5f);
                    path.AddLine(l + scaledCutSize, b + 0.5f, l, b - scaledCutSize);
                }
                else
                {
                    path.AddLine(r - (_bottomRightCut ? scaledCutSize : 0), b + 0.5f, l, b + 0.5f);
                }

                // Left edge back to start
                if (_topLeftCut)
                {
                    path.AddLine(l, b - (_bottomLeftCut ? scaledCutSize : 0), l, t + scaledCutSize);
                }
                else
                {
                    path.AddLine(l, b - (_bottomLeftCut ? scaledCutSize : 0), l, t);
                }

                path.CloseFigure();
            }
            else
            {
                // No cuts, create regular rectangle
                path.AddRectangle(rect);
            }

            return path;
        }

        #region Public Methods
        /// <summary>
        /// Starts the loading animation
        /// </summary>
        public async Task StartLoadingAsync()
        {
            if (_isLoading) return;

            _isLoading = true;
            _loadingAnimationCts?.Cancel();
            _loadingAnimationCts?.Dispose();
            _loadingAnimationCts = new CancellationTokenSource();

            try
            {
                await AnimateLoadingAsync(_loadingAnimationCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Animation was cancelled
            }
        }

        /// <summary>
        /// Stops the loading animation
        /// </summary>
        public void StopLoading()
        {
            _isLoading = false;
            _loadingAnimationCts?.Cancel();
            _loadingProgress = 0;
            Invalidate();
        }

        /// <summary>
        /// Sets the button image from file path
        /// </summary>
        /// <param name="imagePath">Path to the image file</param>
        public void SetImageFromFile(string imagePath)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    _buttonImage?.Dispose();
                    _buttonImage = Image.FromFile(imagePath);
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image from file: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the button image from resources
        /// </summary>
        /// <param name="resourceImage">Image from resources</param>
        public void SetImageFromResource(Image resourceImage)
        {
            try
            {
                _buttonImage?.Dispose();
                _buttonImage = resourceImage?.Clone() as Image;
                Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting image from resource: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears the button image
        /// </summary>
        public void ClearImage()
        {
            _buttonImage?.Dispose();
            _buttonImage = null;
            Invalidate();
        }

        /// <summary>
        /// Resets all animations to default state
        /// </summary>
        public void ResetAnimations()
        {
            _hoverAnimationCts?.Cancel();
            _pressAnimationCts?.Cancel();
            _loadingAnimationCts?.Cancel();

            _hoverAnimationProgress = 0;
            _pressAnimationProgress = 0;
            _loadingProgress = 0;
            _isHovering = false;
            _isPressed = false;
            _isLoading = false;

            Invalidate();
        }

        /// <summary>
        /// Sets corner cuts for specific corners
        /// </summary>
        /// <param name="topLeft">Cut top-left corner</param>
        /// <param name="topRight">Cut top-right corner</param>
        /// <param name="bottomLeft">Cut bottom-left corner</param>
        /// <param name="bottomRight">Cut bottom-right corner</param>
        public void SetCornerCuts(bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
        {
            _topLeftCut = topLeft;
            _topRightCut = topRight;
            _bottomLeftCut = bottomLeft;
            _bottomRightCut = bottomRight;
            Invalidate();
        }

        /// <summary>
        /// Sets all corner cuts at once
        /// </summary>
        /// <param name="enableCuts">Enable cuts for all corners</param>
        public void SetAllCornerCuts(bool enableCuts)
        {
            SetCornerCuts(enableCuts, enableCuts, enableCuts, enableCuts);
        }
        #endregion
    }
}
