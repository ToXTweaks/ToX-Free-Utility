using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Vortex.Ui
{
    public enum VortexFeatureIndicatorStyle
    {
        Dots = 0,
        Lines = 1,
        Pills = 2
    }

    public enum VortexFeatureTransitionStyle
    {
        Fade = 0,
        SlideUp = 1,
        SlideLeft = 2
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class VortexFeatureShowcaseItem
    {
        private string _text = "Feature";
        private string _description = string.Empty;
        private Image? _icon = null;
        private Color _iconColor = Color.Empty;
        private Color _textColor = Color.Empty;
        private Color _descriptionColor = Color.Empty;

        [Category("Appearance")]
        [DefaultValue("Feature")]
        public string Text
        {
            get => _text;
            set => _text = value ?? string.Empty;
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Description("A smaller details line displayed under Text.")]
        public string Description
        {
            get => _description;
            set => _description = value ?? string.Empty;
        }

        [Category("Icon")]
        [DefaultValue(null)]
        public Image? Icon
        {
            get => _icon;
            set => _icon = value;
        }

        [Category("Icon")]
        [Description("Leave empty to use the control IconColor.")]
        public Color IconColor
        {
            get => _iconColor;
            set => _iconColor = value;
        }

        [Category("Appearance")]
        [Description("Leave empty to use the control TextColor.")]
        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }

        [Category("Appearance")]
        [Description("Leave empty to use the control DescriptionColor.")]
        public Color DescriptionColor
        {
            get => _descriptionColor;
            set => _descriptionColor = value;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(_text) ? "Feature" : _text;
        }
    }

    public class VortexFeatureShowcaseItemCollection : Collection<VortexFeatureShowcaseItem>
    {
        private readonly VortexFeatureShowcase _owner;

        public VortexFeatureShowcaseItemCollection(VortexFeatureShowcase owner)
        {
            _owner = owner;
        }

        protected override void InsertItem(int index, VortexFeatureShowcaseItem item)
        {
            base.InsertItem(index, item ?? new VortexFeatureShowcaseItem());
            _owner.ItemsChanged();
        }

        protected override void SetItem(int index, VortexFeatureShowcaseItem item)
        {
            base.SetItem(index, item ?? new VortexFeatureShowcaseItem());
            _owner.ItemsChanged();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            _owner.ItemsChanged();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _owner.ItemsChanged();
        }
    }

    [ToolboxItem(true)]
    [Description("An animated feature showcase with changing text, icon, and indicators.")]
    public class VortexFeatureShowcase : Control
    {
        private readonly VortexFeatureShowcaseItemCollection _items;
        private readonly System.Windows.Forms.Timer _autoPlayTimer;
        private readonly System.Windows.Forms.Timer _animationTimer;

        private Rectangle[] _indicatorSlots = Array.Empty<Rectangle>();
        private int _selectedIndex = 0;
        private int _previousIndex = -1;
        private int _transitionDirection = 1;
        private float _transitionProgress = 1f;
        private DateTime _transitionStartTime = DateTime.MinValue;
        private bool _isHovered = false;

        private bool _autoPlay = true;
        private bool _loop = true;
        private bool _pauseOnHover = true;
        private bool _enableAnimation = true;
        private bool _enableIndicatorClick = true;
        private int _autoPlayInterval = 2500;
        private int _transitionDuration = 320;
        private int _slideDistance = 18;
        private int _placeholderIndicatorCount = 3;
        private VortexFeatureTransitionStyle _transitionStyle = VortexFeatureTransitionStyle.SlideUp;

        private Image? _icon = null;
        private Size _iconSize = new Size(24, 24);
        private Color _iconColor = Color.White;
        private bool _tintIcon = true;
        private bool _showIcon = true;
        private bool _showDefaultGlyph = true;
        private int _iconTextSpacing = 8;
        private int _iconOffsetY = 0;
        private int _textOffsetY = 0;
        private int _descriptionSpacing = 3;
        private int _descriptionMaxLines = 2;
        private float _descriptionFontScale = 0.86f;
        private Font? _textFont = null;
        private Font? _descriptionFont = null;

        private Color _textColor = Color.Gainsboro;
        private Color _descriptionColor = Color.FromArgb(170, 220, 220, 220);
        private Color _backgroundColor = Color.Transparent;
        private string _descriptionText = string.Empty;
        private Padding _contentPadding = new Padding(10, 12, 10, 12);

        private bool _showIndicators = true;
        private VortexFeatureIndicatorStyle _indicatorStyle = VortexFeatureIndicatorStyle.Lines;
        private Color _indicatorColor = Color.FromArgb(120, 170, 170, 170);
        private Color _activeIndicatorColor = Color.White;
        private int _indicatorWidth = 20;
        private int _activeIndicatorWidth = 20;
        private int _indicatorHeight = 3;
        private int _indicatorSpacing = 7;
        private int _indicatorRadius = 3;
        private int _indicatorTopSpacing = 15;
        private int _indicatorBottomMargin = 2;
        private int _indicatorOffsetY = 0;

        public VortexFeatureShowcase()
        {
            _items = new VortexFeatureShowcaseItemCollection(this);

            _autoPlayTimer = new System.Windows.Forms.Timer();
            _autoPlayTimer.Interval = _autoPlayInterval;
            _autoPlayTimer.Tick += AutoPlayTimer_Tick;

            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 16;
            _animationTimer.Tick += AnimationTimer_Tick;

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(190, 120);
            MinimumSize = new Size(70, 70);
            Text = "Really affective";
            Cursor = Cursors.Hand;

            try
            {
                Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            }
            catch
            {
                Font = SystemFonts.DefaultFont;
            }
        }

        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Description("Feature items displayed by the showcase.")]
        public VortexFeatureShowcaseItemCollection Items => _items;

        [Category("Appearance")]
        [Description("Fallback text used when Items is empty.")]
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

        [Category("Behavior")]
        [DefaultValue(0)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SelectIndex(value, _enableAnimation);
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        public bool AutoPlay
        {
            get => _autoPlay;
            set
            {
                if (_autoPlay != value)
                {
                    _autoPlay = value;
                    RestartAutoPlay();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(2500)]
        public int AutoPlayInterval
        {
            get => _autoPlayInterval;
            set
            {
                int newValue = Math.Max(300, value);
                if (_autoPlayInterval != newValue)
                {
                    _autoPlayInterval = newValue;
                    _autoPlayTimer.Interval = newValue;
                    RestartAutoPlay();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        public bool Loop
        {
            get => _loop;
            set => _loop = value;
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        public bool PauseOnHover
        {
            get => _pauseOnHover;
            set => _pauseOnHover = value;
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        public bool EnableIndicatorClick
        {
            get => _enableIndicatorClick;
            set => _enableIndicatorClick = value;
        }

        [Category("Animation")]
        [DefaultValue(true)]
        public bool EnableAnimation
        {
            get => _enableAnimation;
            set => _enableAnimation = value;
        }

        [Category("Animation")]
        [DefaultValue(320)]
        public int TransitionDuration
        {
            get => _transitionDuration;
            set => _transitionDuration = Math.Max(1, value);
        }

        [Category("Animation")]
        [DefaultValue(18)]
        public int SlideDistance
        {
            get => _slideDistance;
            set => _slideDistance = Math.Max(0, value);
        }

        [Category("Animation")]
        [DefaultValue(VortexFeatureTransitionStyle.SlideUp)]
        public VortexFeatureTransitionStyle TransitionStyle
        {
            get => _transitionStyle;
            set
            {
                if (_transitionStyle != value)
                {
                    _transitionStyle = value;
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

        [Category("Text")]
        [Description("Font used for the main title text. Reset it to use the control Font.")]
        public Font TextFont
        {
            get => _textFont ?? Font;
            set
            {
                if (_textFont != value)
                {
                    _textFont = value;
                    Invalidate();
                }
            }
        }

        public bool ShouldSerializeTextFont() => _textFont != null;
        public void ResetTextFont()
        {
            _textFont = null;
            Invalidate();
        }

        [Category("Appearance")]
        [Description("Fallback details text used when Items is empty.")]
        [DefaultValue("")]
        public string DescriptionText
        {
            get => _descriptionText;
            set
            {
                string newValue = value ?? string.Empty;
                if (_descriptionText != newValue)
                {
                    _descriptionText = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("Default color for the details text under the title.")]
        public Color DescriptionColor
        {
            get => _descriptionColor;
            set
            {
                if (_descriptionColor != value)
                {
                    _descriptionColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Description")]
        [Description("Font used for the details text. Reset it to use DescriptionFontScale from the control Font.")]
        public Font DescriptionFont
        {
            get => _descriptionFont ?? Font;
            set
            {
                if (_descriptionFont != value)
                {
                    _descriptionFont = value;
                    Invalidate();
                }
            }
        }

        public bool ShouldSerializeDescriptionFont() => _descriptionFont != null;
        public void ResetDescriptionFont()
        {
            _descriptionFont = null;
            Invalidate();
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

        [Category("Icon")]
        [DefaultValue(null)]
        [Description("Fallback icon used when the selected item does not provide one.")]
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

        [Category("Icon")]
        [DefaultValue(true)]
        [Description("Draws a clean built-in rocket glyph when no icon image is supplied.")]
        public bool ShowDefaultGlyph
        {
            get => _showDefaultGlyph;
            set
            {
                if (_showDefaultGlyph != value)
                {
                    _showDefaultGlyph = value;
                    Invalidate();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(8)]
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

        [Category("Layout")]
        [DefaultValue(3)]
        [Description("Spacing between the title and details text.")]
        public int DescriptionSpacing
        {
            get => _descriptionSpacing;
            set
            {
                int newValue = Math.Max(0, value);
                if (_descriptionSpacing != newValue)
                {
                    _descriptionSpacing = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(2)]
        [Description("Maximum lines for the details text.")]
        public int DescriptionMaxLines
        {
            get => _descriptionMaxLines;
            set
            {
                int newValue = Math.Max(1, value);
                if (_descriptionMaxLines != newValue)
                {
                    _descriptionMaxLines = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(0.86f)]
        [Description("Details text font scale relative to the main Font.")]
        public float DescriptionFontScale
        {
            get => _descriptionFontScale;
            set
            {
                float newValue = Math.Max(0.4f, Math.Min(1.5f, value));
                if (Math.Abs(_descriptionFontScale - newValue) > 0.001f)
                {
                    _descriptionFontScale = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        [DefaultValue(true)]
        public bool ShowIndicators
        {
            get => _showIndicators;
            set
            {
                if (_showIndicators != value)
                {
                    _showIndicators = value;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        [DefaultValue(VortexFeatureIndicatorStyle.Lines)]
        public VortexFeatureIndicatorStyle IndicatorStyle
        {
            get => _indicatorStyle;
            set
            {
                if (_indicatorStyle != value)
                {
                    _indicatorStyle = value;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        public Color IndicatorColor
        {
            get => _indicatorColor;
            set
            {
                if (_indicatorColor != value)
                {
                    _indicatorColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        public Color ActiveIndicatorColor
        {
            get => _activeIndicatorColor;
            set
            {
                if (_activeIndicatorColor != value)
                {
                    _activeIndicatorColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        [DefaultValue(20)]
        public int IndicatorWidth
        {
            get => _indicatorWidth;
            set
            {
                int newValue = Math.Max(1, value);
                if (_indicatorWidth != newValue)
                {
                    _indicatorWidth = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        [DefaultValue(20)]
        public int ActiveIndicatorWidth
        {
            get => _activeIndicatorWidth;
            set
            {
                int newValue = Math.Max(1, value);
                if (_activeIndicatorWidth != newValue)
                {
                    _activeIndicatorWidth = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        [DefaultValue(3)]
        public int IndicatorHeight
        {
            get => _indicatorHeight;
            set
            {
                int newValue = Math.Max(1, value);
                if (_indicatorHeight != newValue)
                {
                    _indicatorHeight = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        [DefaultValue(7)]
        public int IndicatorSpacing
        {
            get => _indicatorSpacing;
            set
            {
                int newValue = Math.Max(0, value);
                if (_indicatorSpacing != newValue)
                {
                    _indicatorSpacing = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        [DefaultValue(3)]
        public int IndicatorRadius
        {
            get => _indicatorRadius;
            set
            {
                int newValue = Math.Max(0, value);
                if (_indicatorRadius != newValue)
                {
                    _indicatorRadius = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        [DefaultValue(15)]
        public int IndicatorTopSpacing
        {
            get => _indicatorTopSpacing;
            set
            {
                int newValue = Math.Max(0, value);
                if (_indicatorTopSpacing != newValue)
                {
                    _indicatorTopSpacing = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        [DefaultValue(2)]
        public int IndicatorBottomMargin
        {
            get => _indicatorBottomMargin;
            set
            {
                int newValue = Math.Max(0, value);
                if (_indicatorBottomMargin != newValue)
                {
                    _indicatorBottomMargin = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        [DefaultValue(0)]
        public int IndicatorOffsetY
        {
            get => _indicatorOffsetY;
            set
            {
                if (_indicatorOffsetY != value)
                {
                    _indicatorOffsetY = value;
                    Invalidate();
                }
            }
        }

        [Category("Indicators")]
        [DefaultValue(3)]
        [Description("Indicator count used when Items is empty.")]
        public int PlaceholderIndicatorCount
        {
            get => _placeholderIndicatorCount;
            set
            {
                int newValue = Math.Max(1, value);
                if (_placeholderIndicatorCount != newValue)
                {
                    _placeholderIndicatorCount = newValue;
                    NormalizeSelectedIndex();
                    RestartAutoPlay();
                    Invalidate();
                }
            }
        }

        public void Next()
        {
            SelectIndex(_selectedIndex + 1, _enableAnimation);
        }

        public void Previous()
        {
            SelectIndex(_selectedIndex - 1, _enableAnimation);
        }

        public void SelectIndex(int index, bool animated)
        {
            int count = GetDisplayCount();
            if (count <= 0)
            {
                _selectedIndex = -1;
                _previousIndex = -1;
                _transitionProgress = 1f;
                Invalidate();
                return;
            }

            int newIndex = index;
            if (_loop)
                newIndex = ((index % count) + count) % count;
            else
                newIndex = Math.Max(0, Math.Min(count - 1, index));

            if (newIndex == _selectedIndex)
                return;

            int oldIndex = _selectedIndex < 0 ? newIndex : _selectedIndex;
            _previousIndex = oldIndex;
            _selectedIndex = newIndex;
            _transitionDirection = GetTransitionDirection(oldIndex, newIndex, count);

            if (animated && CanAnimate())
            {
                _transitionProgress = 0f;
                _transitionStartTime = DateTime.UtcNow;
                _animationTimer.Stop();
                _animationTimer.Start();
            }
            else
            {
                _previousIndex = -1;
                _transitionProgress = 1f;
                _animationTimer.Stop();
            }

            Invalidate();
        }

        internal void ItemsChanged()
        {
            NormalizeSelectedIndex();
            RestartAutoPlay();
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RestartAutoPlay();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _autoPlayTimer.Stop();
            _animationTimer.Stop();
            base.OnHandleDestroyed(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left || !_enableIndicatorClick)
                return;

            int index = HitTestIndicator(e.Location);
            if (index >= 0)
            {
                SelectIndex(index, _enableAnimation);
                RestartAutoPlay();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            if (_backgroundColor.A > 0)
            {
                using (SolidBrush brush = new SolidBrush(_backgroundColor))
                    g.FillRectangle(brush, ClientRectangle);
            }

            int count = GetDisplayCount();
            if (count <= 0)
                return;

            Rectangle featureBounds = GetFeatureBounds();
            float eased = EaseOutCubic(_transitionProgress);

            if (_previousIndex >= 0 && _previousIndex != _selectedIndex && _transitionProgress < 1f)
            {
                Point previousOffset = GetTransitionOffset(-eased);
                Point currentOffset = GetTransitionOffset(1f - eased);

                DrawFeature(g, GetItemOrNull(_previousIndex), featureBounds, 1f - eased, previousOffset.X, previousOffset.Y);
                DrawFeature(g, GetItemOrNull(_selectedIndex), featureBounds, eased, currentOffset.X, currentOffset.Y);
            }
            else
            {
                DrawFeature(g, GetItemOrNull(_selectedIndex), featureBounds, 1f, 0, 0);
            }

            if (_showIndicators)
                DrawIndicators(g, count, eased);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoPlayTimer.Stop();
                _animationTimer.Stop();
                _autoPlayTimer.Tick -= AutoPlayTimer_Tick;
                _animationTimer.Tick -= AnimationTimer_Tick;
                _autoPlayTimer.Dispose();
                _animationTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        private void AutoPlayTimer_Tick(object? sender, EventArgs e)
        {
            if (_pauseOnHover && _isHovered)
                return;

            if (GetDisplayCount() > 1)
                Next();
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            double elapsed = (DateTime.UtcNow - _transitionStartTime).TotalMilliseconds;
            _transitionProgress = Math.Min(1f, (float)(elapsed / _transitionDuration));

            if (_transitionProgress >= 1f)
            {
                _transitionProgress = 1f;
                _previousIndex = -1;
                _animationTimer.Stop();
            }

            Invalidate();
        }

        private void DrawFeature(Graphics g, VortexFeatureShowcaseItem? item, Rectangle bounds, float opacity, int offsetX, int offsetY)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            opacity = Clamp01(opacity);

            string text = GetDisplayText(item);
            string description = GetDisplayDescription(item);
            Image? icon = GetDisplayIcon(item);
            Color iconColor = GetDisplayIconColor(item);
            Color textColor = GetDisplayTextColor(item);
            Color descriptionColor = GetDisplayDescriptionColor(item);

            bool hasText = !string.IsNullOrEmpty(text);
            bool hasDescription = !string.IsNullOrEmpty(description);
            bool hasImageIcon = _showIcon && icon != null && IsValidImage(icon);
            bool hasGlyphIcon = _showIcon && !hasImageIcon && _showDefaultGlyph;
            bool hasAnyIcon = hasImageIcon || hasGlyphIcon;

            int availableWidth = Math.Max(1, bounds.Width);
            Font titleFont = GetEffectiveTextFont();
            Font? disposableDescriptionFont = null;
            Font descriptionFont;
            if (_descriptionFont != null)
            {
                descriptionFont = _descriptionFont;
            }
            else
            {
                disposableDescriptionFont = CreateDescriptionFont();
                descriptionFont = disposableDescriptionFont;
            }
            SizeF textSize = SizeF.Empty;

            try
            {
                if (hasText)
                    textSize = g.MeasureString(text, titleFont, availableWidth);

                SizeF descriptionSize = SizeF.Empty;
                if (hasDescription)
                    descriptionSize = g.MeasureString(description, descriptionFont, availableWidth);

                int iconHeight = hasAnyIcon ? _iconSize.Height : 0;
                int textBlockSpacing = hasText && hasDescription ? _descriptionSpacing : 0;
                int textHeight = hasText ? Math.Max(1, (int)Math.Ceiling(textSize.Height)) : 0;
                int maxDescriptionHeight = descriptionFont.Height * _descriptionMaxLines + 2;
                int descriptionHeight = hasDescription ? Math.Min(maxDescriptionHeight, Math.Max(descriptionFont.Height, (int)Math.Ceiling(descriptionSize.Height))) : 0;
                int textBlockHeight = textHeight + textBlockSpacing + descriptionHeight;
                int iconSpacing = hasAnyIcon && textBlockHeight > 0 ? _iconTextSpacing : 0;
                int totalHeight = iconHeight + iconSpacing + textBlockHeight;
                int startY = bounds.Y + (bounds.Height - totalHeight) / 2 + offsetY;

                if (hasAnyIcon)
                {
                    Rectangle iconRect = new Rectangle(
                        bounds.X + (bounds.Width - _iconSize.Width) / 2 + offsetX,
                        startY + _iconOffsetY,
                        _iconSize.Width,
                        _iconSize.Height);

                    if (hasImageIcon && icon != null)
                        DrawIcon(g, icon, iconRect, iconColor, opacity);
                    else
                        DrawDefaultGlyph(g, iconRect, iconColor, opacity);
                }

                if (hasText)
                {
                    int textY = startY + iconHeight + iconSpacing + _textOffsetY;
                    RectangleF textRect = new RectangleF(bounds.X + offsetX, textY, bounds.Width, Math.Max(textHeight + 4, titleFont.Height + 4));

                    using (StringFormat format = new StringFormat())
                    using (SolidBrush brush = new SolidBrush(WithAlpha(textColor, opacity)))
                    {
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Center;
                        format.Trimming = StringTrimming.EllipsisCharacter;
                        format.FormatFlags = StringFormatFlags.NoWrap;
                        g.DrawString(text, titleFont, brush, textRect, format);
                    }
                }

                if (hasDescription)
                {
                    int descriptionY = startY + iconHeight + iconSpacing + _textOffsetY + textHeight;
                    if (hasText)
                        descriptionY += _descriptionSpacing;

                    RectangleF descriptionRect = new RectangleF(bounds.X + offsetX, descriptionY, bounds.Width, Math.Max(descriptionHeight + 3, descriptionFont.Height + 3));

                    using (StringFormat format = new StringFormat())
                    using (SolidBrush brush = new SolidBrush(WithAlpha(descriptionColor, opacity)))
                    {
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Near;
                        format.Trimming = StringTrimming.EllipsisCharacter;

                        if (_descriptionMaxLines <= 1)
                            format.FormatFlags = StringFormatFlags.NoWrap;
                        else
                            format.FormatFlags = StringFormatFlags.LineLimit;

                        g.DrawString(description, descriptionFont, brush, descriptionRect, format);
                    }
                }
            }
            finally
            {
                if (disposableDescriptionFont != null)
                {
                    disposableDescriptionFont.Dispose();
                }
            }
        }

        private void DrawIndicators(Graphics g, int count, float easedProgress)
        {
            if (count <= 0)
                return;

            IndicatorMetrics metrics = GetIndicatorMetrics();
            Rectangle[] slots = GetIndicatorSlots(count, metrics);
            _indicatorSlots = slots;

            for (int i = 0; i < slots.Length; i++)
            {
                Rectangle slot = slots[i];
                RectangleF indicatorRect = CenterRect(slot, metrics.InactiveWidth, metrics.Height);
                DrawRoundedCapsule(g, indicatorRect, _indicatorRadius, _indicatorColor);
            }

            int activeIndex = Math.Max(0, Math.Min(slots.Length - 1, _selectedIndex));
            int previousIndex = _previousIndex >= 0 ? Math.Max(0, Math.Min(slots.Length - 1, _previousIndex)) : activeIndex;

            float activeCenterX = Lerp(GetCenterX(slots[previousIndex]), GetCenterX(slots[activeIndex]), easedProgress);
            RectangleF activeRect = new RectangleF(
                activeCenterX - metrics.ActiveWidth / 2f,
                slots[activeIndex].Y + (slots[activeIndex].Height - metrics.Height) / 2f,
                metrics.ActiveWidth,
                metrics.Height);

            DrawRoundedCapsule(g, activeRect, _indicatorRadius, _activeIndicatorColor);
        }

        private void DrawIcon(Graphics g, Image icon, Rectangle rect, Color color, float opacity)
        {
            if (!_tintIcon)
            {
                ColorMatrix opacityMatrix = new ColorMatrix();
                opacityMatrix.Matrix33 = Clamp01(opacity);

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(opacityMatrix);
                    g.DrawImage(icon, rect, 0, 0, icon.Width, icon.Height, GraphicsUnit.Pixel, attributes);
                }

                return;
            }

            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
            {
                new float[] { color.R / 255f, 0, 0, 0, 0 },
                new float[] { 0, color.G / 255f, 0, 0, 0 },
                new float[] { 0, 0, color.B / 255f, 0, 0 },
                new float[] { 0, 0, 0, (color.A / 255f) * Clamp01(opacity), 0 },
                new float[] { 0, 0, 0, 0, 1 }
            });

            using (ImageAttributes attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(colorMatrix);
                g.DrawImage(icon, rect, 0, 0, icon.Width, icon.Height, GraphicsUnit.Pixel, attributes);
            }
        }

        private void DrawDefaultGlyph(Graphics g, Rectangle rect, Color color, float opacity)
        {
            Color finalColor = WithAlpha(color, opacity);
            if (finalColor.A <= 0)
                return;

            GraphicsState state = g.Save();
            try
            {
                float scaleX = rect.Width / 24f;
                float scaleY = rect.Height / 24f;
                g.TranslateTransform(rect.X, rect.Y);
                g.ScaleTransform(scaleX, scaleY);

                using (Pen pen = new Pen(finalColor, 1.8f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                })
                using (GraphicsPath body = new GraphicsPath())
                {
                    body.StartFigure();
                    body.AddBezier(12f, 2f, 17f, 5f, 18f, 10f, 14.5f, 15.5f);
                    body.AddBezier(14.5f, 15.5f, 13f, 17f, 11f, 17f, 9.5f, 15.5f);
                    body.AddBezier(9.5f, 15.5f, 6f, 10f, 7f, 5f, 12f, 2f);
                    body.CloseFigure();

                    g.DrawPath(pen, body);
                    g.DrawEllipse(pen, 10.1f, 7f, 3.8f, 3.8f);
                    g.DrawLine(pen, 8.8f, 14.4f, 5.2f, 18.2f);
                    g.DrawLine(pen, 15.2f, 14.4f, 18.8f, 18.2f);
                    g.DrawLine(pen, 10.2f, 18.3f, 8.8f, 21f);
                    g.DrawLine(pen, 12f, 18.6f, 12f, 22f);
                    g.DrawLine(pen, 13.8f, 18.3f, 15.2f, 21f);
                }
            }
            finally
            {
                g.Restore(state);
            }
        }

        private Rectangle GetFeatureBounds()
        {
            int reservedIndicatorHeight = _showIndicators ? _indicatorTopSpacing + _indicatorHeight + _indicatorBottomMargin : 0;
            return new Rectangle(
                _contentPadding.Left,
                _contentPadding.Top,
                Math.Max(0, Width - _contentPadding.Horizontal),
                Math.Max(0, Height - _contentPadding.Vertical - reservedIndicatorHeight));
        }

        private IndicatorMetrics GetIndicatorMetrics()
        {
            int inactiveWidth = _indicatorStyle == VortexFeatureIndicatorStyle.Dots ? _indicatorHeight : _indicatorWidth;
            int activeWidth = _indicatorStyle == VortexFeatureIndicatorStyle.Pills ? _activeIndicatorWidth : inactiveWidth;
            int slotWidth = Math.Max(inactiveWidth, activeWidth);
            return new IndicatorMetrics(inactiveWidth, activeWidth, _indicatorHeight, slotWidth);
        }

        private Rectangle[] GetIndicatorSlots(int count, IndicatorMetrics metrics)
        {
            Rectangle[] slots = new Rectangle[count];
            int totalWidth = count * metrics.SlotWidth + (count - 1) * _indicatorSpacing;
            int startX = (Width - totalWidth) / 2;
            int y = Height - _contentPadding.Bottom - _indicatorBottomMargin - metrics.Height + _indicatorOffsetY;

            for (int i = 0; i < count; i++)
            {
                int x = startX + i * (metrics.SlotWidth + _indicatorSpacing);
                slots[i] = new Rectangle(x, y - 5, metrics.SlotWidth, metrics.Height + 10);
            }

            return slots;
        }

        private int HitTestIndicator(Point point)
        {
            int count = GetDisplayCount();
            if (_indicatorSlots.Length != count)
                _indicatorSlots = GetIndicatorSlots(count, GetIndicatorMetrics());

            for (int i = 0; i < _indicatorSlots.Length; i++)
            {
                Rectangle hitRect = _indicatorSlots[i];
                hitRect.Inflate(4, 6);
                if (hitRect.Contains(point))
                    return i;
            }

            return -1;
        }

        private void RestartAutoPlay()
        {
            _autoPlayTimer.Stop();

            if (_autoPlay && !IsInDesignMode() && IsHandleCreated && GetDisplayCount() > 1)
                _autoPlayTimer.Start();
        }

        private bool CanAnimate()
        {
            return _enableAnimation && !IsInDesignMode() && IsHandleCreated;
        }

        private bool IsInDesignMode()
        {
            return DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private void NormalizeSelectedIndex()
        {
            int count = GetDisplayCount();
            if (count <= 0)
            {
                _selectedIndex = -1;
                _previousIndex = -1;
                _transitionProgress = 1f;
                return;
            }

            if (_selectedIndex < 0 || _selectedIndex >= count)
                _selectedIndex = 0;
        }

        private int GetDisplayCount()
        {
            return _items.Count > 0 ? _items.Count : _placeholderIndicatorCount;
        }

        private VortexFeatureShowcaseItem? GetItemOrNull(int index)
        {
            if (_items.Count == 0 || index < 0 || index >= _items.Count)
                return null;

            return _items[index];
        }

        private string GetDisplayText(VortexFeatureShowcaseItem? item)
        {
            if (item != null)
                return item.Text;

            return Text ?? string.Empty;
        }

        private string GetDisplayDescription(VortexFeatureShowcaseItem? item)
        {
            if (item != null)
                return item.Description;

            return _descriptionText;
        }

        private Image? GetDisplayIcon(VortexFeatureShowcaseItem? item)
        {
            return item?.Icon ?? _icon;
        }

        private Color GetDisplayIconColor(VortexFeatureShowcaseItem? item)
        {
            if (item != null && item.IconColor != Color.Empty)
                return item.IconColor;

            return _iconColor;
        }

        private Color GetDisplayTextColor(VortexFeatureShowcaseItem? item)
        {
            if (item != null && item.TextColor != Color.Empty)
                return item.TextColor;

            return _textColor;
        }

        private Color GetDisplayDescriptionColor(VortexFeatureShowcaseItem? item)
        {
            if (item != null && item.DescriptionColor != Color.Empty)
                return item.DescriptionColor;

            return _descriptionColor;
        }

        private Font GetEffectiveTextFont()
        {
            return _textFont ?? Font;
        }

        private Font CreateDescriptionFont()
        {
            float size = Math.Max(1f, Font.Size * _descriptionFontScale);

            try
            {
                return new Font(Font.FontFamily, size, Font.Style, Font.Unit);
            }
            catch
            {
                return new Font(SystemFonts.DefaultFont.FontFamily, size, FontStyle.Regular);
            }
        }

        private Point GetTransitionOffset(float amount)
        {
            int distance = (int)Math.Round(amount * _slideDistance) * _transitionDirection;

            switch (_transitionStyle)
            {
                case VortexFeatureTransitionStyle.SlideLeft:
                    return new Point(distance, 0);
                case VortexFeatureTransitionStyle.SlideUp:
                    return new Point(0, distance);
                default:
                    return Point.Empty;
            }
        }

        private int GetTransitionDirection(int oldIndex, int newIndex, int count)
        {
            if (count <= 1)
                return 1;

            if (oldIndex == count - 1 && newIndex == 0)
                return 1;

            if (oldIndex == 0 && newIndex == count - 1)
                return -1;

            return newIndex >= oldIndex ? 1 : -1;
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

        private static void DrawRoundedCapsule(Graphics g, RectangleF rect, int radius, Color color)
        {
            if (rect.Width <= 0 || rect.Height <= 0 || color.A <= 0)
                return;

            float finalRadius = radius <= 0 ? 0 : Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);
            using (GraphicsPath path = CreateRoundedPath(rect, finalRadius))
            using (SolidBrush brush = new SolidBrush(color))
                g.FillPath(brush, path);
        }

        private static GraphicsPath CreateRoundedPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0)
                return path;

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

        private static RectangleF CenterRect(Rectangle slot, int width, int height)
        {
            return new RectangleF(
                slot.X + (slot.Width - width) / 2f,
                slot.Y + (slot.Height - height) / 2f,
                width,
                height);
        }

        private static float GetCenterX(Rectangle rect)
        {
            return rect.X + rect.Width / 2f;
        }

        private static float Lerp(float start, float end, float amount)
        {
            return start + (end - start) * Clamp01(amount);
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }

        private static float EaseOutCubic(float value)
        {
            value = Clamp01(value);
            return 1f - (float)Math.Pow(1f - value, 3);
        }

        private static Color WithAlpha(Color color, float opacity)
        {
            int alpha = (int)Math.Round(color.A * Clamp01(opacity));
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        private readonly struct IndicatorMetrics
        {
            public IndicatorMetrics(int inactiveWidth, int activeWidth, int height, int slotWidth)
            {
                InactiveWidth = inactiveWidth;
                ActiveWidth = activeWidth;
                Height = height;
                SlotWidth = slotWidth;
            }

            public int InactiveWidth { get; }
            public int ActiveWidth { get; }
            public int Height { get; }
            public int SlotWidth { get; }
        }
    }
}
