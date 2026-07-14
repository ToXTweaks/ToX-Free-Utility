using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Vortex.Ui
{
    public enum VortexUsageBarOrientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    public enum VortexUsageBarTextPlacement
    {
        Inside = 0,
        Above = 1,
        Below = 2
    }

    public enum VortexUsageIconKind
    {
        Processor = 0,
        Memory = 1,
        Graphics = 2,
        Disk = 3,
        Custom = 4
    }

    [ToolboxItem(true)]
    [DefaultEvent(nameof(ValueChanged))]
    [Description("A lightweight monitoring card with title, percentage, and a live usage graph.")]
    public class VortexUsageBar : Control
    {
        private const int MinimumHistoryCapacity = 8;
        private const int MaximumHistoryCapacity = 240;
        private const float ChartTension = 0.35f;

        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 50;
        private float _displayValue = 50f;

        private string _title = "Processor";
        private string _subtitle = "3.0 GHz";
        private Image? _icon;
        private VortexUsageIconKind _iconKind = VortexUsageIconKind.Processor;

        private Color _backgroundColor = Color.FromArgb(18, 20, 26);
        private Color _borderColor = Color.FromArgb(42, 46, 56);
        private Color _fillStartColor = Color.FromArgb(255, 118, 145);
        private Color _fillEndColor = Color.FromArgb(92, 18, 38);
        private Color _gridColor = Color.FromArgb(34, 255, 255, 255);
        private Color _titleColor = Color.White;
        private Color _subtitleColor = Color.FromArgb(150, 154, 166);
        private Color _valueColor = Color.White;
        private Color _textColor = Color.White;
        private Color _iconColor = Color.FromArgb(178, 188, 255);
        private Color _iconBackgroundColor = Color.FromArgb(32, 178, 188, 255);
        private Color _stripeColor = Color.FromArgb(80, 22, 0, 0);

        private Padding _contentPadding = new Padding(14, 12, 14, 12);
        private int _headerHeight = 46;
        private int _chartSpacing = 8;
        private int _iconSize = 30;
        private int _borderRadius = 18;
        private int _borderWidth = 1;
        private int _chartLineWidth = 2;
        private int _gridRows = 3;
        private int _gridColumns = 8;
        private int _stripeSpacing = 14;
        private int _stripeWidth = 5;

        private bool _roundTopLeft = true;
        private bool _roundTopRight = true;
        private bool _roundBottomRight = true;
        private bool _roundBottomLeft = true;
        private bool _showValueText = true;
        private bool _showIcon = true;
        private bool _showGrid = true;
        private bool _showChartFill = true;
        private bool _showCurrentDot = true;
        private bool _animateValue = true;
        private int _animationSpeed = 6;
        private string _valueTextFormat = "{0}%";
        private bool _valueTextFormatValid = true;

        private VortexUsageBarOrientation _orientation = VortexUsageBarOrientation.Vertical;
        private VortexUsageBarTextPlacement _textPlacement = VortexUsageBarTextPlacement.Above;
        private int _textAreaHeight = 18;
        private int _textSpacing = 4;
        private bool _showStripes = true;

        private int _historyCapacity = 64;
        private float[] _history = Array.Empty<float>();
        private int _historyStart;
        private int _historyCount;
        private PointF[] _chartPoints = Array.Empty<PointF>();

        private Font? _titleFont;
        private Font? _subtitleFont;
        private Font? _valueFont;

        private readonly Timer _animationTimer;

        public VortexUsageBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            ForeColor = _titleColor;
            Size = new Size(320, 112);
            MinimumSize = new Size(120, 70);

            try
            {
                Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            }
            catch
            {
                Font = SystemFonts.DefaultFont;
            }

            _history = new float[_historyCapacity];
            ResetHistory(_value);

            _animationTimer = new Timer
            {
                Interval = 30
            };
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        public event EventHandler? ValueChanged;

        [Category("Content")]
        [DefaultValue("Processor")]
        [Description("The main title displayed in the top-left header.")]
        public string Title
        {
            get => _title;
            set
            {
                string newValue = value ?? string.Empty;
                if (_title != newValue)
                {
                    _title = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Content")]
        [DefaultValue("3.0 GHz")]
        [Description("The secondary title displayed below the main title.")]
        public string Subtitle
        {
            get => _subtitle;
            set
            {
                string newValue = value ?? string.Empty;
                if (_subtitle != newValue)
                {
                    _subtitle = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Content")]
        [DefaultValue(VortexUsageIconKind.Processor)]
        [Description("The built-in icon drawn when Icon is not set.")]
        public VortexUsageIconKind IconKind
        {
            get => _iconKind;
            set
            {
                if (_iconKind != value)
                {
                    _iconKind = value;
                    Invalidate();
                }
            }
        }

        [Category("Content")]
        [DefaultValue(null)]
        [Description("Optional custom image used instead of the built-in icon.")]
        public Image? Icon
        {
            get => _icon;
            set
            {
                if (!ReferenceEquals(_icon, value))
                {
                    _icon = value;
                    Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(0)]
        [Description("The minimum usage value.")]
        public int Minimum
        {
            get => _minimum;
            set
            {
                int newValue = Math.Min(value, _maximum - 1);
                if (_minimum != newValue)
                {
                    _minimum = newValue;
                    if (_value < _minimum)
                        Value = _minimum;
                    else
                        Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(100)]
        [Description("The maximum usage value.")]
        public int Maximum
        {
            get => _maximum;
            set
            {
                int newValue = Math.Max(value, _minimum + 1);
                if (_maximum != newValue)
                {
                    _maximum = newValue;
                    if (_value > _maximum)
                        Value = _maximum;
                    else
                        Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(50)]
        [Description("The current usage value.")]
        public int Value
        {
            get => _value;
            set
            {
                int newValue = Math.Max(_minimum, Math.Min(_maximum, value));
                if (_value != newValue)
                {
                    _value = newValue;
                    AppendHistoryValue(newValue);
                    OnValueChanged(EventArgs.Empty);

                    if (_animateValue && IsHandleCreated && Visible)
                    {
                        if (!_animationTimer.Enabled)
                            _animationTimer.Start();
                    }
                    else
                    {
                        _displayValue = _value;
                        Invalidate();
                    }
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Percentage => GetPercentage(_value);

        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Animates the percentage and latest graph point when Value changes.")]
        public bool AnimateValue
        {
            get => _animateValue;
            set
            {
                if (_animateValue != value)
                {
                    _animateValue = value;
                    if (!_animateValue)
                    {
                        _animationTimer.Stop();
                        _displayValue = _value;
                    }
                    Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(6)]
        [Description("Animation speed from 1 to 10.")]
        public int AnimationSpeed
        {
            get => _animationSpeed;
            set => _animationSpeed = Math.Max(1, Math.Min(10, value));
        }

        [Category("Behavior")]
        [DefaultValue(64)]
        [Description("The amount of recent samples retained for the chart.")]
        public int HistoryCapacity
        {
            get => _historyCapacity;
            set
            {
                int newValue = Math.Max(MinimumHistoryCapacity, Math.Min(MaximumHistoryCapacity, value));
                if (_historyCapacity != newValue)
                {
                    ResizeHistory(newValue);
                    Invalidate();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(typeof(Padding), "14, 12, 14, 12")]
        [Description("The inner spacing around the header and chart.")]
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
        [DefaultValue(46)]
        [Description("The reserved height for the title, subtitle, icon, and percentage.")]
        public int HeaderHeight
        {
            get => _headerHeight;
            set
            {
                int newValue = Math.Max(28, value);
                if (_headerHeight != newValue)
                {
                    _headerHeight = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Layout")]
        [DefaultValue(8)]
        [Description("Spacing between the header and the graph.")]
        public int ChartSpacing
        {
            get => _chartSpacing;
            set
            {
                int newValue = Math.Max(0, value);
                if (_chartSpacing != newValue)
                {
                    _chartSpacing = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The card background color.")]
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
        [Description("The card border color.")]
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
        [DefaultValue(1)]
        [Description("The card border width.")]
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
        [DefaultValue(18)]
        [Description("The card corner radius.")]
        public int CornerRadius
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

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderRadius
        {
            get => CornerRadius;
            set => CornerRadius = value;
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("Rounds the top-left corner.")]
        public bool RoundTopLeft
        {
            get => _roundTopLeft;
            set
            {
                if (_roundTopLeft != value)
                {
                    _roundTopLeft = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("Rounds the top-right corner.")]
        public bool RoundTopRight
        {
            get => _roundTopRight;
            set
            {
                if (_roundTopRight != value)
                {
                    _roundTopRight = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("Rounds the bottom-right corner.")]
        public bool RoundBottomRight
        {
            get => _roundBottomRight;
            set
            {
                if (_roundBottomRight != value)
                {
                    _roundBottomRight = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("Rounds the bottom-left corner.")]
        public bool RoundBottomLeft
        {
            get => _roundBottomLeft;
            set
            {
                if (_roundBottomLeft != value)
                {
                    _roundBottomLeft = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The graph line color.")]
        public Color FillStartColor
        {
            get => _fillStartColor;
            set
            {
                if (_fillStartColor != value)
                {
                    _fillStartColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The graph area gradient end color.")]
        public Color FillEndColor
        {
            get => _fillEndColor;
            set
            {
                if (_fillEndColor != value)
                {
                    _fillEndColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The main title color.")]
        public Color TitleColor
        {
            get => _titleColor;
            set
            {
                if (_titleColor != value)
                {
                    _titleColor = value;
                    ForeColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The secondary title color.")]
        public Color SubtitleColor
        {
            get => _subtitleColor;
            set
            {
                if (_subtitleColor != value)
                {
                    _subtitleColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The percentage text color.")]
        public Color ValueColor
        {
            get => _valueColor;
            set
            {
                if (_valueColor != value)
                {
                    _valueColor = value;
                    _textColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("Legacy alias for ValueColor.")]
        public Color TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    _valueColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The built-in icon stroke color.")]
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

        [Category("Appearance")]
        [Description("The background color behind the icon.")]
        public Color IconBackgroundColor
        {
            get => _iconBackgroundColor;
            set
            {
                if (_iconBackgroundColor != value)
                {
                    _iconBackgroundColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The grid line color.")]
        public Color GridColor
        {
            get => _gridColor;
            set
            {
                if (_gridColor != value)
                {
                    _gridColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Text")]
        [DefaultValue(true)]
        [Description("Shows or hides the usage percentage text.")]
        public bool ShowValueText
        {
            get => _showValueText;
            set
            {
                if (_showValueText != value)
                {
                    _showValueText = value;
                    Invalidate();
                }
            }
        }

        [Category("Text")]
        [DefaultValue("{0}%")]
        [Description("Text format. {0}=percentage, {1}=value, {2}=maximum.")]
        public string ValueTextFormat
        {
            get => _valueTextFormat;
            set
            {
                string newValue = string.IsNullOrWhiteSpace(value) ? "{0}%" : value;
                if (_valueTextFormat != newValue)
                {
                    _valueTextFormat = newValue;
                    _valueTextFormatValid = true;
                    Invalidate();
                }
            }
        }

        [Category("Chart")]
        [DefaultValue(true)]
        [Description("Shows or hides the chart grid.")]
        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                if (_showGrid != value)
                {
                    _showGrid = value;
                    Invalidate();
                }
            }
        }

        [Category("Chart")]
        [DefaultValue(true)]
        [Description("Shows or hides the soft fill below the chart line.")]
        public bool ShowChartFill
        {
            get => _showChartFill;
            set
            {
                if (_showChartFill != value)
                {
                    _showChartFill = value;
                    Invalidate();
                }
            }
        }

        [Category("Chart")]
        [DefaultValue(true)]
        [Description("Shows or hides the latest-sample dot.")]
        public bool ShowCurrentDot
        {
            get => _showCurrentDot;
            set
            {
                if (_showCurrentDot != value)
                {
                    _showCurrentDot = value;
                    Invalidate();
                }
            }
        }

        [Category("Chart")]
        [DefaultValue(2)]
        [Description("The chart line width.")]
        public int ChartLineWidth
        {
            get => _chartLineWidth;
            set
            {
                int newValue = Math.Max(1, value);
                if (_chartLineWidth != newValue)
                {
                    _chartLineWidth = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Chart")]
        [DefaultValue(3)]
        [Description("The number of horizontal grid divisions.")]
        public int GridRows
        {
            get => _gridRows;
            set
            {
                int newValue = Math.Max(0, Math.Min(12, value));
                if (_gridRows != newValue)
                {
                    _gridRows = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Chart")]
        [DefaultValue(8)]
        [Description("The number of vertical grid divisions.")]
        public int GridColumns
        {
            get => _gridColumns;
            set
            {
                int newValue = Math.Max(0, Math.Min(24, value));
                if (_gridColumns != newValue)
                {
                    _gridColumns = newValue;
                    Invalidate();
                }
            }
        }

        [Category("Icon")]
        [DefaultValue(true)]
        [Description("Shows or hides the header icon.")]
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
        [DefaultValue(30)]
        [Description("The header icon size.")]
        public int IconSize
        {
            get => _iconSize;
            set
            {
                int newValue = Math.Max(12, value);
                if (_iconSize != newValue)
                {
                    _iconSize = newValue;
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public VortexUsageBarOrientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public VortexUsageBarTextPlacement TextPlacement
        {
            get => _textPlacement;
            set
            {
                if (_textPlacement != value)
                {
                    _textPlacement = value;
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int TextAreaHeight
        {
            get => _textAreaHeight;
            set => _textAreaHeight = Math.Max(0, value);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int TextSpacing
        {
            get => _textSpacing;
            set => _textSpacing = Math.Max(0, value);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShowStripes
        {
            get => _showStripes;
            set => _showStripes = value;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color StripeColor
        {
            get => _stripeColor;
            set => _stripeColor = value;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int StripeSpacing
        {
            get => _stripeSpacing;
            set => _stripeSpacing = Math.Max(4, value);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int StripeWidth
        {
            get => _stripeWidth;
            set => _stripeWidth = Math.Max(1, value);
        }

        public void ClearHistory()
        {
            ResetHistory(_value);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer.Stop();
                _animationTimer.Tick -= AnimationTimer_Tick;
                _animationTimer.Dispose();
                DisposeCachedFonts();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            Rectangle clientRect = new Rectangle(0, 0, Width, Height);
            if (clientRect.Width <= 1 || clientRect.Height <= 1)
                return;

            DrawCard(g, clientRect);

            Rectangle contentRect = GetContentRectangle(clientRect);
            if (contentRect.Width <= 0 || contentRect.Height <= 0)
                return;

            Rectangle chartRect = DrawHeader(g, contentRect);
            DrawChart(g, chartRect);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            DisposeCachedFonts();
            Invalidate();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _animationTimer.Stop();
            _displayValue = _value;
            base.OnHandleDestroyed(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (!Visible && _animationTimer.Enabled)
            {
                _animationTimer.Stop();
                _displayValue = _value;
            }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (!IsHandleCreated || !Visible)
            {
                _displayValue = _value;
                _animationTimer.Stop();
                return;
            }

            float difference = _value - _displayValue;
            if (Math.Abs(difference) <= 0.35f)
            {
                _displayValue = _value;
                _animationTimer.Stop();
                Invalidate();
                return;
            }

            float divisor = 12 - _animationSpeed;
            _displayValue += difference / Math.Max(2f, divisor);
            Invalidate();
        }

        private void DrawCard(Graphics g, Rectangle clientRect)
        {
            float inset = _borderWidth > 0 ? _borderWidth / 2f : 0f;
            RectangleF bounds = new RectangleF(
                clientRect.X + inset,
                clientRect.Y + inset,
                Math.Max(0, clientRect.Width - _borderWidth),
                Math.Max(0, clientRect.Height - _borderWidth));

            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            using (GraphicsPath path = CreateRoundedPath(
                bounds,
                _borderRadius,
                _roundTopLeft,
                _roundTopRight,
                _roundBottomRight,
                _roundBottomLeft))
            {
                if (_backgroundColor.A > 0)
                {
                    Color topColor = Lighten(_backgroundColor, 8);
                    using (LinearGradientBrush backgroundBrush = new LinearGradientBrush(bounds, topColor, _backgroundColor, LinearGradientMode.Vertical))
                        g.FillPath(backgroundBrush, path);
                }

                if (_borderWidth > 0 && _borderColor.A > 0)
                {
                    using (Pen borderPen = new Pen(_borderColor, _borderWidth) { LineJoin = LineJoin.Round })
                        g.DrawPath(borderPen, path);
                }
            }
        }

        private Rectangle GetContentRectangle(Rectangle clientRect)
        {
            int left = clientRect.Left + Padding.Left + _contentPadding.Left + _borderWidth;
            int top = clientRect.Top + Padding.Top + _contentPadding.Top + _borderWidth;
            int right = clientRect.Right - Padding.Right - _contentPadding.Right - _borderWidth;
            int bottom = clientRect.Bottom - Padding.Bottom - _contentPadding.Bottom - _borderWidth;

            if (right < left)
                right = left;
            if (bottom < top)
                bottom = top;

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private Rectangle DrawHeader(Graphics g, Rectangle contentRect)
        {
            int headerHeight = Math.Min(_headerHeight, contentRect.Height);
            if (contentRect.Height < 90)
                headerHeight = Math.Min(contentRect.Height, Math.Max(30, contentRect.Height / 2));

            Rectangle headerRect = new Rectangle(contentRect.X, contentRect.Y, contentRect.Width, headerHeight);
            Rectangle chartRect = new Rectangle(
                contentRect.X,
                headerRect.Bottom + Math.Min(_chartSpacing, Math.Max(0, contentRect.Height - headerHeight)),
                contentRect.Width,
                Math.Max(0, contentRect.Bottom - headerRect.Bottom - _chartSpacing));

            if (headerRect.Width <= 0 || headerRect.Height <= 0)
                return chartRect;

            EnsureFonts();

            int iconAdvance = 0;
            if (_showIcon)
            {
                int iconSize = Math.Min(_iconSize, Math.Max(12, headerRect.Height - 4));
                Rectangle iconRect = new Rectangle(
                    headerRect.Left,
                    headerRect.Top + Math.Max(0, (headerRect.Height - iconSize) / 2),
                    iconSize,
                    iconSize);

                DrawIcon(g, iconRect);
                iconAdvance = iconSize + 10;
            }

            int valueWidth = _showValueText ? Math.Min(76, Math.Max(44, headerRect.Width / 3)) : 0;
            Rectangle valueRect = new Rectangle(headerRect.Right - valueWidth, headerRect.Top, valueWidth, headerRect.Height);
            int titleRight = _showValueText ? valueRect.Left - 8 : headerRect.Right;
            Rectangle titleArea = Rectangle.FromLTRB(headerRect.Left + iconAdvance, headerRect.Top, Math.Max(headerRect.Left + iconAdvance, titleRight), headerRect.Bottom);

            if (titleArea.Width > 0)
            {
                bool hasSubtitle = !string.IsNullOrEmpty(_subtitle);
                Rectangle titleRect = hasSubtitle
                    ? new Rectangle(titleArea.Left, titleArea.Top + 2, titleArea.Width, Math.Max(14, titleArea.Height / 2))
                    : titleArea;
                Rectangle subtitleRect = new Rectangle(titleArea.Left, titleRect.Bottom - 1, titleArea.Width, Math.Max(12, titleArea.Bottom - titleRect.Bottom));

                if (!string.IsNullOrEmpty(_title))
                {
                    TextRenderer.DrawText(
                        g,
                        _title,
                        _titleFont ?? Font,
                        titleRect,
                        _titleColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                }

                if (hasSubtitle)
                {
                    TextRenderer.DrawText(
                        g,
                        _subtitle,
                        _subtitleFont ?? Font,
                        subtitleRect,
                        _subtitleColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                }
            }

            if (_showValueText && valueRect.Width > 0)
            {
                TextRenderer.DrawText(
                    g,
                    GetValueText(),
                    _valueFont ?? Font,
                    valueRect,
                    _valueColor,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }

            return chartRect;
        }

        private void DrawChart(Graphics g, Rectangle chartRect)
        {
            if (chartRect.Width <= 2 || chartRect.Height <= 2 || _historyCount <= 0)
                return;

            Rectangle innerRect = Rectangle.FromLTRB(
                chartRect.Left,
                chartRect.Top + 2,
                chartRect.Right,
                chartRect.Bottom - 1);

            if (innerRect.Width <= 1 || innerRect.Height <= 1)
                return;

            if (_showGrid)
                DrawGrid(g, innerRect);

            EnsurePointBuffer(_historyCount);
            int count = BuildChartPoints(innerRect);
            if (count <= 0)
                return;

            GraphicsState state = g.Save();
            try
            {
                g.SetClip(innerRect);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (_showChartFill && count > 1)
                    DrawChartFill(g, innerRect, count);

                using (Pen linePen = new Pen(_fillStartColor, _chartLineWidth)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round,
                    LineJoin = LineJoin.Round
                })
                {
                    DrawSmoothLine(g, linePen, count);
                }

                if (_showCurrentDot && count > 0)
                    DrawCurrentDot(g, _chartPoints[count - 1]);
            }
            finally
            {
                g.Restore(state);
            }
        }

        private void DrawGrid(Graphics g, Rectangle rect)
        {
            if (_gridColor.A <= 0)
                return;

            SmoothingMode previousMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.None;

            using (Pen gridPen = new Pen(_gridColor, 1f))
            {
                if (_gridRows > 0)
                {
                    for (int i = 0; i <= _gridRows; i++)
                    {
                        int y = rect.Top + (int)Math.Round(rect.Height * (i / (float)_gridRows));
                        g.DrawLine(gridPen, rect.Left, y, rect.Right, y);
                    }
                }

                if (_gridColumns > 0)
                {
                    for (int i = 0; i <= _gridColumns; i++)
                    {
                        int x = rect.Left + (int)Math.Round(rect.Width * (i / (float)_gridColumns));
                        g.DrawLine(gridPen, x, rect.Top, x, rect.Bottom);
                    }
                }
            }

            g.SmoothingMode = previousMode;
        }

        private int BuildChartPoints(Rectangle rect)
        {
            int count = _historyCount;
            if (count <= 0)
                return 0;

            float usableHeight = Math.Max(1f, rect.Height - 1f);
            float step = count > 1 ? rect.Width / (float)(count - 1) : rect.Width;

            for (int i = 0; i < count; i++)
            {
                float sample = i == count - 1 ? _displayValue : GetHistoryValue(i);
                float percentage = GetProgressPercentage(sample);
                float x = count > 1 ? rect.Left + (step * i) : rect.Left;
                float y = rect.Bottom - (percentage * usableHeight);
                _chartPoints[i] = new PointF(x, y);
            }

            return count;
        }

        private void DrawChartFill(Graphics g, Rectangle chartRect, int count)
        {
            using (GraphicsPath areaPath = new GraphicsPath())
            {
                AddSmoothLine(areaPath, count);
                areaPath.AddLine(_chartPoints[count - 1].X, _chartPoints[count - 1].Y, _chartPoints[count - 1].X, chartRect.Bottom);
                areaPath.AddLine(_chartPoints[count - 1].X, chartRect.Bottom, _chartPoints[0].X, chartRect.Bottom);
                areaPath.CloseFigure();

                using (LinearGradientBrush fillBrush = new LinearGradientBrush(
                    chartRect,
                    WithAlpha(_fillStartColor, 86),
                    WithAlpha(_fillEndColor, 8),
                    LinearGradientMode.Vertical))
                {
                    g.FillPath(fillBrush, areaPath);
                }
            }
        }

        private void DrawSmoothLine(Graphics g, Pen pen, int count)
        {
            if (count == 1)
            {
                g.DrawLine(pen, _chartPoints[0].X, _chartPoints[0].Y, _chartPoints[0].X + 1f, _chartPoints[0].Y);
                return;
            }

            if (count == 2)
            {
                g.DrawLine(pen, _chartPoints[0], _chartPoints[1]);
                return;
            }

            g.DrawCurve(pen, _chartPoints, 0, count - 1, ChartTension);
        }

        private void AddSmoothLine(GraphicsPath path, int count)
        {
            if (count == 1)
            {
                path.AddLine(_chartPoints[0].X, _chartPoints[0].Y, _chartPoints[0].X + 1f, _chartPoints[0].Y);
                return;
            }

            if (count == 2)
            {
                path.AddLine(_chartPoints[0], _chartPoints[1]);
                return;
            }

            path.AddCurve(_chartPoints, 0, count - 1, ChartTension);
        }

        private void DrawCurrentDot(Graphics g, PointF point)
        {
            float dotSize = Math.Max(4f, _chartLineWidth + 3f);
            RectangleF dotRect = new RectangleF(point.X - (dotSize / 2f), point.Y - (dotSize / 2f), dotSize, dotSize);

            using (SolidBrush dotBrush = new SolidBrush(_fillStartColor))
                g.FillEllipse(dotBrush, dotRect);

            using (Pen dotBorder = new Pen(WithAlpha(Color.White, 150), 1f))
                g.DrawEllipse(dotBorder, dotRect);
        }

        private void DrawIcon(Graphics g, Rectangle iconRect)
        {
            if (iconRect.Width <= 0 || iconRect.Height <= 0)
                return;

            using (GraphicsPath backgroundPath = CreateRoundedPath(iconRect, Math.Max(5, iconRect.Width / 4), true, true, true, true))
            using (SolidBrush backgroundBrush = new SolidBrush(_iconBackgroundColor))
            {
                g.FillPath(backgroundBrush, backgroundPath);
            }

            Rectangle innerRect = Rectangle.Inflate(iconRect, -Math.Max(3, iconRect.Width / 7), -Math.Max(3, iconRect.Height / 7));
            if (innerRect.Width <= 0 || innerRect.Height <= 0)
                return;

            if (_icon != null)
            {
                g.DrawImage(_icon, innerRect);
                return;
            }

            switch (_iconKind)
            {
                case VortexUsageIconKind.Memory:
                    DrawMemoryIcon(g, innerRect);
                    break;
                case VortexUsageIconKind.Graphics:
                    DrawGraphicsIcon(g, innerRect);
                    break;
                case VortexUsageIconKind.Disk:
                    DrawDiskIcon(g, innerRect);
                    break;
                case VortexUsageIconKind.Custom:
                    DrawCustomIcon(g, innerRect);
                    break;
                default:
                    DrawProcessorIcon(g, innerRect);
                    break;
            }
        }

        private void DrawProcessorIcon(Graphics g, Rectangle rect)
        {
            using (Pen pen = new Pen(_iconColor, 1.4f) { LineJoin = LineJoin.Round })
            using (SolidBrush fillBrush = new SolidBrush(WithAlpha(_iconColor, 40)))
            {
                RectangleF body = new RectangleF(
                    rect.Left + rect.Width * 0.18f,
                    rect.Top + rect.Height * 0.18f,
                    rect.Width * 0.64f,
                    rect.Height * 0.64f);

                using (GraphicsPath bodyPath = CreateRoundedPath(body, 3f, true, true, true, true))
                {
                    g.FillPath(fillBrush, bodyPath);
                    g.DrawPath(pen, bodyPath);
                }

                for (int i = 0; i < 4; i++)
                {
                    float offset = body.Top + 3f + (i * (body.Height - 6f) / 3f);
                    g.DrawLine(pen, rect.Left + 1f, offset, body.Left, offset);
                    g.DrawLine(pen, body.Right, offset, rect.Right - 1f, offset);
                }

                for (int i = 0; i < 4; i++)
                {
                    float offset = body.Left + 3f + (i * (body.Width - 6f) / 3f);
                    g.DrawLine(pen, offset, rect.Top + 1f, offset, body.Top);
                    g.DrawLine(pen, offset, body.Bottom, offset, rect.Bottom - 1f);
                }

                float cellWidth = body.Width / 3f;
                float cellHeight = body.Height / 3f;
                for (int row = 1; row < 3; row++)
                    g.DrawLine(pen, body.Left + 3f, body.Top + (row * cellHeight), body.Right - 3f, body.Top + (row * cellHeight));
                for (int col = 1; col < 3; col++)
                    g.DrawLine(pen, body.Left + (col * cellWidth), body.Top + 3f, body.Left + (col * cellWidth), body.Bottom - 3f);
            }
        }

        private void DrawMemoryIcon(Graphics g, Rectangle rect)
        {
            using (Pen pen = new Pen(_iconColor, 1.5f) { LineJoin = LineJoin.Round })
            using (SolidBrush fillBrush = new SolidBrush(WithAlpha(_iconColor, 38)))
            {
                RectangleF module = new RectangleF(rect.Left + 1f, rect.Top + rect.Height * 0.25f, rect.Width - 2f, rect.Height * 0.48f);
                using (GraphicsPath modulePath = CreateRoundedPath(module, 3f, true, true, true, true))
                {
                    g.FillPath(fillBrush, modulePath);
                    g.DrawPath(pen, modulePath);
                }

                int chipCount = 3;
                float chipGap = 2f;
                float chipWidth = (module.Width - 8f - ((chipCount - 1) * chipGap)) / chipCount;
                for (int i = 0; i < chipCount; i++)
                {
                    RectangleF chip = new RectangleF(module.Left + 4f + (i * (chipWidth + chipGap)), module.Top + 4f, chipWidth, module.Height - 8f);
                    g.DrawRectangle(pen, chip.X, chip.Y, chip.Width, chip.Height);
                }

                for (int i = 0; i < 6; i++)
                {
                    float x = module.Left + 4f + (i * (module.Width - 8f) / 5f);
                    g.DrawLine(pen, x, module.Bottom, x, rect.Bottom - 1f);
                }
            }
        }

        private void DrawGraphicsIcon(Graphics g, Rectangle rect)
        {
            using (Pen pen = new Pen(_iconColor, 1.5f) { LineJoin = LineJoin.Round })
            using (SolidBrush fillBrush = new SolidBrush(WithAlpha(_iconColor, 36)))
            {
                RectangleF card = new RectangleF(rect.Left + 1f, rect.Top + rect.Height * 0.18f, rect.Width - 4f, rect.Height * 0.58f);
                using (GraphicsPath cardPath = CreateRoundedPath(card, 3f, true, true, true, true))
                {
                    g.FillPath(fillBrush, cardPath);
                    g.DrawPath(pen, cardPath);
                }

                float fanSize = Math.Min(card.Width, card.Height) * 0.42f;
                RectangleF fan = new RectangleF(card.Left + 4f, card.Top + ((card.Height - fanSize) / 2f), fanSize, fanSize);
                g.DrawEllipse(pen, fan);
                g.DrawLine(pen, fan.Left + fan.Width / 2f, fan.Top + 2f, fan.Left + fan.Width / 2f, fan.Bottom - 2f);
                g.DrawLine(pen, fan.Left + 2f, fan.Top + fan.Height / 2f, fan.Right - 2f, fan.Top + fan.Height / 2f);

                float portX = card.Right - 5f;
                g.DrawLine(pen, portX, card.Top + 4f, portX, card.Bottom - 4f);
                g.DrawLine(pen, rect.Left + rect.Width * 0.18f, card.Bottom, rect.Left + rect.Width * 0.18f, rect.Bottom - 1f);
                g.DrawLine(pen, rect.Left + rect.Width * 0.18f, rect.Bottom - 1f, rect.Right - 2f, rect.Bottom - 1f);
            }
        }

        private void DrawDiskIcon(Graphics g, Rectangle rect)
        {
            using (Pen pen = new Pen(_iconColor, 1.5f) { LineJoin = LineJoin.Round })
            using (SolidBrush fillBrush = new SolidBrush(WithAlpha(_iconColor, 38)))
            using (SolidBrush ledBrush = new SolidBrush(_iconColor))
            {
                RectangleF drive = new RectangleF(rect.Left + 1f, rect.Top + rect.Height * 0.16f, rect.Width - 2f, rect.Height * 0.68f);
                using (GraphicsPath drivePath = CreateRoundedPath(drive, 4f, true, true, true, true))
                {
                    g.FillPath(fillBrush, drivePath);
                    g.DrawPath(pen, drivePath);
                }

                float platterSize = Math.Min(drive.Width, drive.Height) * 0.44f;
                RectangleF platter = new RectangleF(drive.Left + 5f, drive.Top + 5f, platterSize, platterSize);
                g.DrawEllipse(pen, platter);
                g.FillEllipse(ledBrush, drive.Right - 7f, drive.Bottom - 7f, 3f, 3f);
                g.DrawLine(pen, drive.Left + 4f, drive.Bottom - 10f, drive.Right - 4f, drive.Bottom - 10f);
            }
        }

        private void DrawCustomIcon(Graphics g, Rectangle rect)
        {
            using (Pen pen = new Pen(_iconColor, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                PointF p1 = new PointF(rect.Left + 1f, rect.Bottom - rect.Height * 0.25f);
                PointF p2 = new PointF(rect.Left + rect.Width * 0.35f, rect.Top + rect.Height * 0.55f);
                PointF p3 = new PointF(rect.Left + rect.Width * 0.62f, rect.Top + rect.Height * 0.62f);
                PointF p4 = new PointF(rect.Right - 1f, rect.Top + rect.Height * 0.28f);
                g.DrawCurve(pen, new[] { p1, p2, p3, p4 }, ChartTension);
            }
        }

        private void EnsureFonts()
        {
            if (_titleFont != null && _subtitleFont != null && _valueFont != null)
                return;

            _titleFont = CreateFont(Math.Max(8f, Font.Size), FontStyle.Bold);
            _subtitleFont = CreateFont(Math.Max(7f, Font.Size - 1.25f), FontStyle.Regular);
            _valueFont = CreateFont(Math.Max(8f, Font.Size), FontStyle.Bold);
        }

        private Font CreateFont(float size, FontStyle style)
        {
            try
            {
                return new Font(Font.FontFamily, size, style, Font.Unit);
            }
            catch
            {
                return new Font(Font, style);
            }
        }

        private void DisposeCachedFonts()
        {
            _titleFont?.Dispose();
            _subtitleFont?.Dispose();
            _valueFont?.Dispose();
            _titleFont = null;
            _subtitleFont = null;
            _valueFont = null;
        }

        private string GetValueText()
        {
            int percentage = GetPercentage(_displayValue);

            if (!_valueTextFormatValid)
                return percentage.ToString(CultureInfo.InvariantCulture) + "%";

            try
            {
                return string.Format(CultureInfo.InvariantCulture, _valueTextFormat, percentage, (int)Math.Round(_displayValue), _maximum);
            }
            catch (FormatException)
            {
                _valueTextFormatValid = false;
                return percentage.ToString(CultureInfo.InvariantCulture) + "%";
            }
        }

        private int GetPercentage(float value)
        {
            return (int)Math.Round(GetProgressPercentage(value) * 100f);
        }

        private float GetProgressPercentage(float value)
        {
            int range = _maximum - _minimum;
            if (range <= 0)
                return 0f;

            return Math.Max(0f, Math.Min(1f, (value - _minimum) / range));
        }

        private void AppendHistoryValue(float value)
        {
            if (_history.Length == 0)
                return;

            if (_historyCount < _history.Length)
            {
                _history[(_historyStart + _historyCount) % _history.Length] = value;
                _historyCount++;
                return;
            }

            _history[_historyStart] = value;
            _historyStart = (_historyStart + 1) % _history.Length;
        }

        private float GetHistoryValue(int offset)
        {
            if (_historyCount == 0 || _history.Length == 0)
                return _value;

            offset = Math.Max(0, Math.Min(_historyCount - 1, offset));
            return _history[(_historyStart + offset) % _history.Length];
        }

        private void ResetHistory(float value)
        {
            if (_history.Length == 0)
                return;

            for (int i = 0; i < _history.Length; i++)
                _history[i] = value;

            _historyStart = 0;
            _historyCount = _history.Length;
        }

        private void ResizeHistory(int newCapacity)
        {
            int retainedCount = Math.Min(_historyCount, newCapacity);
            float[] retainedValues = new float[retainedCount];
            int firstOffset = Math.Max(0, _historyCount - retainedCount);

            for (int i = 0; i < retainedCount; i++)
                retainedValues[i] = GetHistoryValue(firstOffset + i);

            _historyCapacity = newCapacity;
            _history = new float[newCapacity];
            _historyStart = 0;
            _historyCount = 0;

            if (retainedValues.Length == 0)
            {
                ResetHistory(_value);
                return;
            }

            foreach (float retainedValue in retainedValues)
                AppendHistoryValue(retainedValue);
        }

        private void EnsurePointBuffer(int count)
        {
            if (_chartPoints.Length < count)
                _chartPoints = new PointF[count];
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        private static Color WithAlpha(Color color, int alpha)
        {
            return Color.FromArgb(Math.Max(0, Math.Min(255, alpha)), color.R, color.G, color.B);
        }

        private static Color Lighten(Color color, int amount)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, color.R + amount),
                Math.Min(255, color.G + amount),
                Math.Min(255, color.B + amount));
        }

        private static GraphicsPath CreateRoundedPath(
            RectangleF rect,
            float radius,
            bool topLeft,
            bool topRight,
            bool bottomRight,
            bool bottomLeft)
        {
            GraphicsPath path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0)
                return path;

            radius = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f));
            if (radius <= 0 || (!topLeft && !topRight && !bottomRight && !bottomLeft))
            {
                path.AddRectangle(rect);
                return path;
            }

            float diameter = radius * 2f;

            path.StartFigure();

            if (topLeft)
                path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            else
                path.AddLine(rect.Left, rect.Top, rect.Left, rect.Top);

            path.AddLine(topLeft ? rect.Left + radius : rect.Left, rect.Top, topRight ? rect.Right - radius : rect.Right, rect.Top);

            if (topRight)
                path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);

            path.AddLine(rect.Right, topRight ? rect.Top + radius : rect.Top, rect.Right, bottomRight ? rect.Bottom - radius : rect.Bottom);

            if (bottomRight)
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);

            path.AddLine(bottomRight ? rect.Right - radius : rect.Right, rect.Bottom, bottomLeft ? rect.Left + radius : rect.Left, rect.Bottom);

            if (bottomLeft)
                path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.AddLine(rect.Left, bottomLeft ? rect.Bottom - radius : rect.Bottom, rect.Left, topLeft ? rect.Top + radius : rect.Top);
            path.CloseFigure();

            return path;
        }
    }
}
