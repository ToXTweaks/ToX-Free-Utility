using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Vortex.Ui
{
    public enum VortexTagIconPosition
    {
        Left = 0,
        Right = 1
    }

    [ToolboxItem(true)]
    [Description("A compact rounded tag control with optional tintable icon.")]
    public class VortexTag : Control
    {
        private Color _backgroundColor = Color.FromArgb(34, 34, 36);
        private Color _hoverBackColor = Color.FromArgb(44, 44, 48);
        private Color _borderColor = Color.FromArgb(48, 48, 52);
        private Color _textColor = Color.Gainsboro;
        private Color _hoverTextColor = Color.White;
        private int _borderRadius = 6;
        private int _borderWidth = 0;
        private bool _enableHoverEffect = false;
        private bool _isHovered = false;

        private Image? _icon = null;
        private Size _iconSize = new Size(16, 16);
        private Color _iconColor = Color.White;
        private bool _tintIcon = true;
        private bool _showIcon = true;
        private int _iconSpacing = 7;
        private int _iconOffsetX = 0;
        private int _iconOffsetY = 0;
        private VortexTagIconPosition _iconPosition = VortexTagIconPosition.Left;

        private Padding _contentPadding = new Padding(12, 7, 12, 7);
        private int _textOffsetX = 0;
        private int _textOffsetY = 0;

        public VortexTag()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            ForeColor = _textColor;
            Size = new Size(80, 28);
            MinimumSize = new Size(18, 18);

            try
            {
                Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            }
            catch
            {
                Font = SystemFonts.DefaultFont;
            }

            Text = "Tag";
            AutoSize = true;
        }

        [Category("Appearance")]
        [Description("The text displayed inside the tag.")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        [Category("Appearance")]
        [Description("The tag background color.")]
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
        [Description("The tag background color when hovered.")]
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
        [Description("The tag border color.")]
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
        [DefaultValue(6)]
        [Description("The radius of the tag corners.")]
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
        [Description("The width of the tag border.")]
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

        [Category("Appearance")]
        [Description("The tag text color.")]
        public Color TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    ForeColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The tag text color when hovered.")]
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

        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Enables hover colors for the tag.")]
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

        [Category("Icon")]
        [DefaultValue(null)]
        [Description("The icon displayed inside the tag.")]
        public Image? Icon
        {
            get => _icon;
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    LayoutChanged();
                }
            }
        }

        public bool ShouldSerializeIcon() => _icon != null;
        public void ResetIcon() => Icon = null;

        [Category("Icon")]
        [DefaultValue(typeof(Size), "16, 16")]
        [Description("The icon size.")]
        public Size IconSize
        {
            get => _iconSize;
            set
            {
                Size newValue = new Size(Math.Max(1, value.Width), Math.Max(1, value.Height));
                if (_iconSize != newValue)
                {
                    _iconSize = newValue;
                    LayoutChanged();
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
                    LayoutChanged();
                }
            }
        }

        [Category("Icon")]
        [DefaultValue(7)]
        [Description("The spacing between the icon and text.")]
        public int IconSpacing
        {
            get => _iconSpacing;
            set
            {
                int newValue = Math.Max(0, value);
                if (_iconSpacing != newValue)
                {
                    _iconSpacing = newValue;
                    LayoutChanged();
                }
            }
        }

        [Category("Icon")]
        [DefaultValue(VortexTagIconPosition.Left)]
        [Description("The icon position inside the tag.")]
        public VortexTagIconPosition IconPosition
        {
            get => _iconPosition;
            set
            {
                if (_iconPosition != value)
                {
                    _iconPosition = value;
                    Invalidate();
                }
            }
        }

        [Category("Icon")]
        [DefaultValue(0)]
        [Description("Horizontal icon offset.")]
        public int IconOffsetX
        {
            get => _iconOffsetX;
            set
            {
                if (_iconOffsetX != value)
                {
                    _iconOffsetX = value;
                    Invalidate();
                }
            }
        }

        [Category("Icon")]
        [DefaultValue(0)]
        [Description("Vertical icon offset.")]
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
        [DefaultValue(typeof(Padding), "12, 7, 12, 7")]
        [Description("The padding around the tag content.")]
        public Padding ContentPadding
        {
            get => _contentPadding;
            set
            {
                if (_contentPadding != value)
                {
                    _contentPadding = value;
                    LayoutChanged();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(0)]
        [Description("Horizontal text offset.")]
        public int TextOffsetX
        {
            get => _textOffsetX;
            set
            {
                if (_textOffsetX != value)
                {
                    _textOffsetX = value;
                    Invalidate();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(0)]
        [Description("Vertical text offset.")]
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

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Category("Layout")]
        [DefaultValue(true)]
        [Description("Automatically resizes the tag to fit its text and icon.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override bool AutoSize
        {
            get => base.AutoSize;
            set
            {
                if (base.AutoSize != value)
                {
                    base.AutoSize = value;
                    if (value)
                        AdjustSize();
                    Invalidate();
                }
            }
        }

        public void FitToContent()
        {
            AdjustSize();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size textSize = Size.Empty;
            string text = Text ?? string.Empty;

            if (!string.IsNullOrEmpty(text))
            {
                textSize = TextRenderer.MeasureText(
                    text,
                    Font,
                    new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            }

            bool hasIcon = _icon != null && _showIcon && IsValidImage(_icon);
            bool hasText = !string.IsNullOrEmpty(text);
            int contentWidth = textSize.Width;
            int contentHeight = hasText ? textSize.Height : Font.Height;

            if (hasIcon)
            {
                contentWidth += _iconSize.Width;
                contentHeight = Math.Max(contentHeight, _iconSize.Height);

                if (hasText)
                    contentWidth += _iconSpacing;
            }

            int width = contentWidth + _contentPadding.Horizontal;
            int height = contentHeight + _contentPadding.Vertical;

            return new Size(
                Math.Max(MinimumSize.Width, width),
                Math.Max(MinimumSize.Height, height));
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            LayoutChanged();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            LayoutChanged();
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (AutoSize)
            {
                Size preferredSize = GetPreferredSize(Size.Empty);
                width = preferredSize.Width;
                height = preferredSize.Height;
            }

            base.SetBoundsCore(x, y, width, height, specified);
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
            if (_enableHoverEffect)
                Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle clientRect = new Rectangle(0, 0, Width, Height);
            if (clientRect.Width <= 0 || clientRect.Height <= 0)
                return;

            Color currentBackColor = _enableHoverEffect && _isHovered ? _hoverBackColor : _backgroundColor;
            Color currentTextColor = _enableHoverEffect && _isHovered ? _hoverTextColor : _textColor;

            RectangleF backgroundRect = new RectangleF(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = CreateRoundedPath(backgroundRect, _borderRadius))
            {
                if (currentBackColor.A > 0)
                {
                    using (SolidBrush brush = new SolidBrush(currentBackColor))
                        g.FillPath(brush, path);
                }
            }

            DrawContent(g, currentTextColor);

            if (_borderWidth > 0 && _borderColor.A > 0)
            {
                float inset = _borderWidth / 2f;
                RectangleF borderRect = new RectangleF(inset, inset, Width - _borderWidth, Height - _borderWidth);
                using (GraphicsPath borderPath = CreateRoundedPath(borderRect, _borderRadius))
                using (Pen pen = new Pen(_borderColor, _borderWidth) { LineJoin = LineJoin.Round })
                    g.DrawPath(pen, borderPath);
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

            bool hasIcon = _icon != null && _showIcon && IsValidImage(_icon);
            bool hasText = !string.IsNullOrEmpty(Text);
            if (!hasIcon && !hasText)
                return;

            Size textSize = Size.Empty;
            if (hasText)
            {
                textSize = TextRenderer.MeasureText(
                    g,
                    Text,
                    Font,
                    new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            }

            int desiredWidth = (hasIcon ? _iconSize.Width : 0) + (hasText ? textSize.Width : 0);
            if (hasIcon && hasText)
                desiredWidth += _iconSpacing;

            int groupWidth = Math.Min(desiredWidth, contentRect.Width);
            int groupHeight = Math.Max(hasIcon ? _iconSize.Height : 0, hasText ? Math.Max(Font.Height, textSize.Height) : 0);
            int groupX = contentRect.X + (contentRect.Width - groupWidth) / 2;
            int groupY = contentRect.Y + (contentRect.Height - groupHeight) / 2;

            int textWidth = hasText ? textSize.Width : 0;
            if (hasText)
            {
                int reservedIconWidth = hasIcon ? _iconSize.Width + _iconSpacing : 0;
                textWidth = Math.Max(0, Math.Min(textSize.Width, groupWidth - reservedIconWidth));
            }

            if (hasIcon && _icon != null)
            {
                int iconY = groupY + (groupHeight - _iconSize.Height) / 2 + _iconOffsetY;
                int iconX;

                if (_iconPosition == VortexTagIconPosition.Right)
                {
                    iconX = groupX + (hasText ? textWidth + _iconSpacing : 0) + _iconOffsetX;
                }
                else
                {
                    iconX = groupX + _iconOffsetX;
                }

                Rectangle iconRect = new Rectangle(iconX, iconY, _iconSize.Width, _iconSize.Height);
                DrawIcon(g, _icon, iconRect);
            }

            if (hasText && textWidth > 0)
            {
                int textHeight = Math.Max(Font.Height, textSize.Height);
                int textX;

                if (hasIcon && _iconPosition == VortexTagIconPosition.Left)
                    textX = groupX + _iconSize.Width + _iconSpacing;
                else
                    textX = groupX;

                Rectangle textRect = new Rectangle(
                    textX + _textOffsetX,
                    groupY + (groupHeight - textHeight) / 2 + _textOffsetY,
                    Math.Max(0, textWidth - _textOffsetX),
                    Math.Max(0, textHeight - _textOffsetY));

                TextRenderer.DrawText(
                    g,
                    Text,
                    Font,
                    textRect,
                    textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }

        private void DrawIcon(Graphics g, Image icon, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0 || !IsValidImage(icon))
                return;

            if (_tintIcon)
                DrawIconWithColor(g, icon, rect, _iconColor);
            else
                g.DrawImage(icon, rect);
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

        private void LayoutChanged()
        {
            if (AutoSize)
                AdjustSize();
            Invalidate();
        }

        private void AdjustSize()
        {
            Size preferredSize = GetPreferredSize(Size.Empty);
            if (Size != preferredSize)
                Size = preferredSize;
        }
    }
}
