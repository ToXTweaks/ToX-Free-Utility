using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexIconButton : Control
    {
        private Image _icon;
        private Color _normalBackColor = Color.Transparent;
        private Color _hoverBackColor = Color.FromArgb(50, 150, 200, 255);
        private Color _pressedBackColor = Color.FromArgb(100, 100, 150, 255);
        private int _borderRadius = 8;
        private Size _iconSize = new Size(24, 24);
        private bool _isHovered = false;
        private bool _isPressed = false;

        public VortexIconButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.CacheText, true);

            BackColor = Color.Transparent;
            Size = new Size(40, 40);
            Cursor = Cursors.Hand;
        }

        [Category("Appearance")]
        [Description("The icon image displayed on the button")]
        [DefaultValue(null)]
        [Localizable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        public Image Icon
        {
            get => _icon;
            set
            {
                if (_icon != value)
                {
                    // Don't dispose the old image here - let the designer handle it
                    // or the caller handle disposal
                    _icon = value;
                    
                    // Only invalidate if not in design mode or if the control is created
                    if (!DesignMode || IsHandleCreated)
                    {
                        Invalidate();
                    }
                }
            }
        }

        // Designer support helpers
        public bool ShouldSerializeIcon() => _icon != null;
        public void ResetIcon() => Icon = null;

        [Category("Appearance")]
        [Description("Background color when button is in normal state")]
        [DefaultValue(typeof(Color), "Transparent")]
        public Color NormalBackColor
        {
            get => _normalBackColor;
            set
            {
                if (_normalBackColor != value)
                {
                    _normalBackColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("Background color when button is hovered")]
        [DefaultValue(typeof(Color), "50, 150, 200, 255")]
        public Color HoverBackColor
        {
            get => _hoverBackColor;
            set
            {
                if (_hoverBackColor != value)
                {
                    _hoverBackColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("Background color when button is pressed")]
        [DefaultValue(typeof(Color), "100, 100, 150, 255")]
        public Color PressedBackColor
        {
            get => _pressedBackColor;
            set
            {
                if (_pressedBackColor != value)
                {
                    _pressedBackColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("Size of the icon")]
        [DefaultValue(typeof(Size), "24, 24")]
        public Size IconSize
        {
            get => _iconSize;
            set
            {
                if (_iconSize != value)
                {
                    _iconSize = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("Border radius for rounded corners")]
        [DefaultValue(8)]
        public int BorderRadius
        {
            get => _borderRadius;
            set
            {
                int newValue = Math.Max(0, value);
                if (_borderRadius != newValue)
                {
                    _borderRadius = newValue;
                    Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Safety check to prevent designer exceptions
            if (e?.Graphics == null)
                return;

            Graphics g = e.Graphics;

            try
            {
                // Set rendering modes optimized for scrolling performance
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.Bilinear;
                g.PixelOffsetMode = PixelOffsetMode.None; // Prevents sub-pixel rendering issues during scrolling
                g.CompositingQuality = CompositingQuality.HighSpeed;

                Rectangle bounds = new Rectangle(0, 0, Width, Height);

                // Determine background color
                Color backColor = _normalBackColor;
                if (_isPressed)
                    backColor = _pressedBackColor;
                else if (_isHovered)
                    backColor = _hoverBackColor;

                // Draw background
                if (backColor != Color.Transparent)
                {
                    using (GraphicsPath path = CreateRoundedRectangle(bounds, _borderRadius))
                    using (SolidBrush brush = new SolidBrush(backColor))
                    {
                        g.FillPath(brush, path);
                    }
                }

                // Draw icon - mathematically perfect centering
                if (_icon != null && IsValidImage(_icon) && _iconSize.Width > 0 && _iconSize.Height > 0)
                {
                    // Use precise floating point for perfect centering, then round to nearest pixel
                    float exactCenterX = Width / 2f;
                    float exactCenterY = Height / 2f;

                    float exactIconX = exactCenterX - (_iconSize.Width / 2f);
                    float exactIconY = exactCenterY - (_iconSize.Height / 2f);

                    // Round to nearest integers for pixel-perfect placement
                    int iconX = (int)Math.Round(exactIconX);
                    int iconY = (int)Math.Round(exactIconY);

                    Rectangle iconRect = new Rectangle(iconX, iconY, _iconSize.Width, _iconSize.Height);

                    if (_isPressed)
                    {
                        try
                        {
                            ColorMatrix colorMatrix = new ColorMatrix();
                            colorMatrix.Matrix33 = 0.7f;

                            using (ImageAttributes attributes = new ImageAttributes())
                            {
                                attributes.SetColorMatrix(colorMatrix);
                                g.DrawImage(_icon, iconRect, 0, 0, _icon.Width, _icon.Height, GraphicsUnit.Pixel, attributes);
                            }
                        }
                        catch
                        {
                            // Fallback to normal drawing if color matrix fails
                            g.DrawImage(_icon, iconRect);
                        }
                    }
                    else
                    {
                        g.DrawImage(_icon, iconRect);
                    }
                }
            }
            catch (Exception ex)
            {
                // In design mode, show a placeholder
                if (DesignMode)
                {
                    using (SolidBrush brush = new SolidBrush(Color.LightGray))
                    using (Pen pen = new Pen(Color.DarkGray))
                    {
                        Rectangle iconPlaceholder = new Rectangle(
                            (Width - 16) / 2,
                            (Height - 16) / 2,
                            16, 16);
                        g.FillRectangle(brush, iconPlaceholder);
                        g.DrawRectangle(pen, iconPlaceholder);
                        g.DrawLine(pen, iconPlaceholder.Left, iconPlaceholder.Top, iconPlaceholder.Right, iconPlaceholder.Bottom);
                        g.DrawLine(pen, iconPlaceholder.Right, iconPlaceholder.Top, iconPlaceholder.Left, iconPlaceholder.Bottom);
                    }
                }
                // In runtime, we can optionally log or handle the error differently
                System.Diagnostics.Debug.WriteLine($"VortexIconButton OnPaint error: {ex.Message}");
            }
        }

        private bool IsValidImage(Image image)
        {
            if (image == null)
                return false;

            try
            {
                // Try to access image properties to validate it's not disposed
                var width = image.Width;
                var height = image.Height;
                return width > 0 && height > 0;
            }
            catch
            {
                return false;
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

            radius = Math.Min(radius, Math.Min(rect.Width / 2, rect.Height / 2));
            int diameter = radius * 2;

            // Create the rounded rectangle using the exact bounds
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            Invalidate();
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Do NOT dispose _icon here because it is typically provided by the caller or Resources
                // and may be shared/managed elsewhere (e.g., designer resources). Disposing it here can
                // cause "Parameter is not valid" exceptions during painting and designer reloads.
            }
            base.Dispose(disposing);
        }
    }
}