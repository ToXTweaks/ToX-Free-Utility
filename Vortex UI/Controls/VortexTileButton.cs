using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    [Description("A compact tile button with a centered icon above centered text.")]
    public class VortexTileButton : Control
    {
        private Image? _icon = null;
        private Size _iconSize = new Size(24, 24);
        private Color _iconColor = Color.White;
        private bool _tintIcon = true;
        private bool _showIcon = true;

        private Color _backgroundColor = Color.Transparent;
        private Color _hoverBackColor = Color.FromArgb(35, 255, 255, 255);
        private Color _pressedBackColor = Color.FromArgb(55, 255, 255, 255);
        private Color _borderColor = Color.FromArgb(45, 255, 255, 255);
        private Color _textColor = Color.FromArgb(170, 170, 170);
        private Color _hoverTextColor = Color.White;

        private Padding _contentPadding = new Padding(6, 6, 6, 6);
        private int _borderRadius = 8;
        private int _borderWidth = 0;
        private int _iconTextSpacing = 4;
        private int _iconOffsetY = 0;
        private int _textOffsetY = 0;
        private bool _enableHoverEffect = true;
        private bool _isHovered = false;
        private bool _isPressed = false;

        public VortexTileButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.Selectable, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            TabStop = true;
            Size = new Size(58, 56);
            MinimumSize = new Size(34, 34);
            Text = "Home";

            try
            {
                Font = new Font("Segoe UI", 8f, FontStyle.Regular);
            }
            catch
            {
                Font = SystemFonts.DefaultFont;
            }
        }

        [Category("Appearance")]
        [Description("The text displayed under the icon.")]
        [DefaultValue("Home")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        [Category("Icon")]
        [DefaultValue(null)]
        [Description("The icon displayed above the text.")]
        public Image? Icon
        {
            get => _icon;
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    Invalidate();
                }
            }
        }

        public bool ShouldSerializeIcon() => _icon != null;
        public void ResetIcon() => Icon = null;

        [Category("Icon")]
        [DefaultValue(typeof(Size), "24, 24")]
        [Description("The rendered icon size.")]
        public Size IconSize
        {
            get => _iconSize;
            set
            {
                Size newValue = new Size(Math.Max(1, value.Width), Math.Max(1, value.Height));
                if (_iconSize != newValue)
                {
                    _iconSize = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Icon")]
        [Description("The icon tint color.")]
        public Color IconColor
        {
            get => _iconColor;
            set
            {
                if (_iconColor != value)
                {
                    _iconColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Icon")]
        [DefaultValue(true)]
        [Description("Tint the icon using IconColor.")]
        public bool TintIcon
        {
            get => _tintIcon;
            set
            {
                if (_tintIcon != value)
                {
                    _tintIcon = value;
                    Invalidate();
                }
            }
        }

        [Category("Icon")]
        [DefaultValue(true)]
        [Description("Shows or hides the icon.")]
        public bool ShowIcon
        {
            get => _showIcon;
            set
            {
                if (_showIcon != value)
                {
                    _showIcon = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
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
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor != value)
                {
                    _borderColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        public Color TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        public Color HoverTextColor
        {
            get => _hoverTextColor;
            set
            {
                if (_hoverTextColor != value)
                {
                    _hoverTextColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
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

        [Category("Appearance")]
        [DefaultValue(0)]
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                int newValue = Math.Max(0, value);
                if (_borderWidth != newValue)
                {
                    _borderWidth = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Layout")]
        public Padding ContentPadding
        {
            get => _contentPadding;
            set
            {
                if (_contentPadding != value)
                {
                    _contentPadding = value;
                    Invalidate();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(4)]
        [Description("Spacing between the centered icon and centered text.")]
        public int IconTextSpacing
        {
            get => _iconTextSpacing;
            set
            {
                int newValue = Math.Max(0, value);
                if (_iconTextSpacing != newValue)
                {
                    _iconTextSpacing = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(0)]
        public int IconOffsetY
        {
            get => _iconOffsetY;
            set
            {
                if (_iconOffsetY != value)
                {
                    _iconOffsetY = value;
                    Invalidate();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(0)]
        public int TextOffsetY
        {
            get => _textOffsetY;
            set
            {
                if (_textOffsetY != value)
                {
                    _textOffsetY = value;
                    Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        public bool EnableHoverEffect
        {
            get => _enableHoverEffect;
            set
            {
                if (_enableHoverEffect != value)
                {
                    _enableHoverEffect = value;
                    Invalidate();
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            if (_enableHoverEffect)
                Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            if (_enableHoverEffect)
                Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                Focus();
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && _isPressed)
            {
                _isPressed = false;
                Invalidate();
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Color backColor = _backgroundColor;
            Color textColor = _textColor;

            if (_enableHoverEffect && _isHovered)
            {
                backColor = _hoverBackColor;
                textColor = _hoverTextColor;
            }

            if (_isPressed)
                backColor = _pressedBackColor;

            RectangleF bounds = new RectangleF(0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1));
            if (backColor.A > 0)
            {
                using (GraphicsPath path = CreateRoundedPath(bounds, _borderRadius))
                using (SolidBrush brush = new SolidBrush(backColor))
                    g.FillPath(brush, path);
            }

            DrawContent(g, textColor);

            if (_borderWidth > 0 && _borderColor.A > 0)
            {
                float inset = _borderWidth / 2f;
                RectangleF borderRect = new RectangleF(inset, inset, Width - _borderWidth, Height - _borderWidth);
                using (GraphicsPath path = CreateRoundedPath(borderRect, _borderRadius))
                using (Pen pen = new Pen(_borderColor, _borderWidth) { LineJoin = LineJoin.Round })
                    g.DrawPath(pen, path);
            }
        }

        private void DrawContent(Graphics g, Color textColor)
        {
            Rectangle contentRect = new Rectangle(
                _contentPadding.Left,
                _contentPadding.Top,
                Math.Max(0, Width - _contentPadding.Horizontal),
                Math.Max(0, Height - _contentPadding.Vertical));

            if (contentRect.Width <= 0 || contentRect.Height <= 0)
                return;

            bool hasText = !string.IsNullOrEmpty(Text);
            bool hasIcon = _showIcon && _icon != null && IsValidImage(_icon);
            Size textSize = hasText
                ? TextRenderer.MeasureText(g, Text, Font, new Size(contentRect.Width, int.MaxValue), TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding)
                : Size.Empty;

            int iconHeight = hasIcon ? _iconSize.Height : 0;
            int textHeight = hasText ? Math.Max(Font.Height, textSize.Height) : 0;
            int spacing = hasIcon && hasText ? _iconTextSpacing : 0;
            int totalHeight = iconHeight + spacing + textHeight;
            int startY = contentRect.Y + (contentRect.Height - totalHeight) / 2;

            if (hasIcon && _icon != null)
            {
                Rectangle iconRect = new Rectangle(
                    contentRect.X + (contentRect.Width - _iconSize.Width) / 2,
                    startY + _iconOffsetY,
                    _iconSize.Width,
                    _iconSize.Height);

                if (_tintIcon)
                    DrawIconWithColor(g, _icon, iconRect, _iconColor);
                else
                    g.DrawImage(_icon, iconRect);
            }

            if (hasText)
            {
                Rectangle textRect = new Rectangle(
                    contentRect.X,
                    startY + iconHeight + spacing + _textOffsetY,
                    contentRect.Width,
                    Math.Max(textHeight + 2, Font.Height + 2));

                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    textRect,
                    textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }

        private static bool IsValidImage(Image image)
        {
            try
            {
                return image.Width > 0 && image.Height > 0;
            }
            catch
            {
                return false;
            }
        }

        private static void DrawIconWithColor(Graphics g, Image icon, Rectangle rect, Color color)
        {
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
            {
                new float[] { color.R / 255f, 0, 0, 0, 0 },
                new float[] { 0, color.G / 255f, 0, 0, 0 },
                new float[] { 0, 0, color.B / 255f, 0, 0 },
                new float[] { 0, 0, 0, color.A / 255f, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            });

            using (ImageAttributes attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(colorMatrix);
                g.DrawImage(icon, rect, 0, 0, icon.Width, icon.Height, GraphicsUnit.Pixel, attributes);
            }
        }

        private static GraphicsPath CreateRoundedPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0)
                return path;

            radius = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f));
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            float diameter = radius * 2f;
            RectangleF arc = new RectangleF(rect.Location, new SizeF(diameter, diameter));
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
