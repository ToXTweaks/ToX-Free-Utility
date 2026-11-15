using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace Vortex.Ui
{
    public enum SidebarIconPosition { Left = 0, Right = 1, Center = 2, Top = 3 }
    public enum SidebarSeparatorAlignment { Left = 0, Right = 1, Up = 2, Down = 3 }
    public enum SeparatorVisibility { Never = 0, Always = 1, OnHover = 2 }
    public enum TextAlignment { Left = 0, Center = 1, Right = 2 }

    [ToolboxItem(true)]
    public class VortexSidebarButton : Control
    {
        #region Private Fields

        // Basic appearance
        private Color _backgroundColor = Color.Transparent;
        private Color _hoverBackColor = Color.FromArgb(40, 120, 120, 120);
        private Color _borderColor = Color.FromArgb(50, 150, 150, 150);
        private int _borderRadius = 8;
        private int _borderWidth = 0;

        // Icon
        private Image _icon = null;
        private Size _iconSize = new Size(18, 18);
        private SidebarIconPosition _iconPosition = SidebarIconPosition.Left;
        private int _iconSpacing = 8;
        private Color _iconColor = Color.White;
        private bool _tintIcon = true;
        private int _iconOffsetX = 0;
        private int _iconOffsetY = 0;
        private bool _showIcon = true;

        // Text
        private Color _textColor = Color.FromArgb(80, 80, 80);
        private Color _hoverTextColor = Color.FromArgb(50, 50, 50);
        private Color _pressedBackColor = Color.Empty;
        private Color _pressedTextColor = Color.Empty;
        private TextAlignment _textAlignment = TextAlignment.Left;

        // General animation
        private bool _isHovering = false;
        private float _hoverProgress = 0f;
        private CancellationTokenSource _hoverAnimationCts;

        // Separator animation (hover fade only)
        private float _separatorProgress = 0f; // used as alpha only
        private CancellationTokenSource _separatorAnimationCts;

        // Icon press scale
        private float _iconPressProgress = 0f;

        // Text press offset
        private float _textPressProgress = 0f;
        private int _textPressOffset = 1;

        // Separator press brightness
        private float _separatorPressProgress = 0f;
        private CancellationTokenSource _separatorPressAnimationCts;

        // Press state
        private bool _isPressed = false;
        private DateTime _pressStartTime = DateTime.MinValue;
        private const int MinimumPressDisplayTime = 80;

        // Layout
        private Padding _contentPadding = new Padding(15, 10, 15, 10);

        // Separator
        private bool _showSeparator = false;
        private Color _separatorColor = Color.FromArgb(255, 86, 156, 255);
        private int _separatorWidth = 3;
        private int _separatorRadius = 3;
        private int _separatorMargin = 6;
        private SidebarSeparatorAlignment _separatorAlignment = SidebarSeparatorAlignment.Left;
        private SeparatorVisibility _separatorVisibility = SeparatorVisibility.OnHover;

        // Text offsets
        private int _textOffsetX = 0;
        private int _textOffsetY = 0;

        // Loading
        private bool _isLoading = false;
        private bool _enableLoadingAnimation = false;
        private int _loadingDuration = 3000;
        private float _loadingProgress = 0f;
        private CancellationTokenSource _loadingCts;
        private CancellationTokenSource _loadingAnimationCts;
        private string _loadingText = "Loading...";
        private bool _showLoadingText = true;
        private Color _loadingProgressColor = Color.FromArgb(86, 156, 255);
        private Color _loadingBackgroundColor = Color.FromArgb(60, 86, 156, 255);
        private int _loadingThickness = 3;
        private int _loadingSize = 20;

        // Speeds
        private int _hoverAnimationSpeed = 120;
        private int _hoverOutAnimationSpeed = 180;
        private int _separatorAnimationSpeed = 140;   // faster fade in
        private int _separatorOutAnimationSpeed = 160; // fade out
        private int _separatorPressReleaseSpeed = 120;

        // Enable/disable
        private bool _enableHoverAnimation = true;
        private bool _enableSeparatorAnimation = true;
        private bool _enableIconPressAnimation = true;
        private bool _enableTextPressAnimation = true;
        private bool _enableSeparatorPressAnimation = true;

        // ImGui-like color darken (icon)
        private float _iconColorProgress = 0f;
        private float _iconHoverDarken = 0.18f;

        // ImGui-like color darken (text)
        private float _textColorProgress = 0f;
        private CancellationTokenSource _textColorAnimationCts;
        private int _textColorInSpeed = 100;
        private int _textColorOutSpeed = 120;
        private float _textHoverDarken = 0.15f;

        #endregion

        #region Properties

        [Category("Appearance")] public Color BackgroundColor { get => _backgroundColor; set { _backgroundColor = value; Invalidate(); } }
        [Category("Appearance")] public Color HoverBackColor { get => _hoverBackColor; set { _hoverBackColor = value; Invalidate(); } }
        [Category("Appearance")] public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        [Category("Appearance")] public int BorderRadius { get => _borderRadius; set { _borderRadius = Math.Max(0, value); Invalidate(); } }
        [Category("Appearance")] public int BorderWidth { get => _borderWidth; set { _borderWidth = Math.Max(0, value); Invalidate(); } }

        [Category("Icon")] public Image Icon { get => _icon; set { _icon = value; Invalidate(); } }
        [Category("Icon")] public Size IconSize { get => _iconSize; set { _iconSize = value; Invalidate(); } }
        [Category("Icon")] public SidebarIconPosition IconPosition { get => _iconPosition; set { _iconPosition = value; Invalidate(); } }
        [Category("Icon")] public int IconSpacing { get => _iconSpacing; set { _iconSpacing = Math.Max(0, value); Invalidate(); } }
        [Category("Icon")] public Color IconColor { get => _iconColor; set { _iconColor = value; Invalidate(); } }
        [Category("Icon")] public bool TintIcon { get => _tintIcon; set { _tintIcon = value; Invalidate(); } }
        [Category("Icon"), DefaultValue(0)] public int IconOffsetX { get => _iconOffsetX; set { _iconOffsetX = value; Invalidate(); } }
        [Category("Icon"), DefaultValue(0)] public int IconOffsetY { get => _iconOffsetY; set { _iconOffsetY = value; Invalidate(); } }
        [Category("Icon"), DefaultValue(true)] public bool ShowIcon { get => _showIcon; set { _showIcon = value; Invalidate(); } }

        [Category("Appearance")] public Color TextColor { get => _textColor; set { _textColor = value; Invalidate(); } }
        [Category("Appearance")] public Color HoverTextColor { get => _hoverTextColor; set { _hoverTextColor = value; Invalidate(); } }
        [Category("Appearance")] public Color PressedBackColor { get => _pressedBackColor; set { _pressedBackColor = value; Invalidate(); } }
        [Category("Appearance")] public Color PressedTextColor { get => _pressedTextColor; set { _pressedTextColor = value; Invalidate(); } }

        [Category("Text"), DefaultValue(TextAlignment.Left)]
        public TextAlignment TextAlignment { get => _textAlignment; set { _textAlignment = value; Invalidate(); } }

        [Category("Layout")] public Padding ContentPadding { get => _contentPadding; set { _contentPadding = value; Invalidate(); } }
        [Category("Layout"), DefaultValue(0)] public int TextOffsetX { get => _textOffsetX; set { _textOffsetX = value; Invalidate(); } }
        [Category("Layout"), DefaultValue(0)] public int TextOffsetY { get => _textOffsetY; set { _textOffsetY = value; Invalidate(); } }
        [Category("Text"), DefaultValue(1), Description("Pixels the text moves while pressed when EnableTextPressAnimation is enabled.")]
        public int TextPressOffset { get => _textPressOffset; set { _textPressOffset = Math.Max(0, Math.Min(4, value)); Invalidate(); } }

        [Category("Separator"), DefaultValue(false)] public bool ShowSeparator { get => _showSeparator; set { _showSeparator = value; Invalidate(); } }
        [Category("Separator")] public Color SeparatorColor { get => _separatorColor; set { _separatorColor = value; Invalidate(); } }
        [Category("Separator"), DefaultValue(3)] public int SeparatorWidth { get => _separatorWidth; set { _separatorWidth = Math.Max(1, value); Invalidate(); } }
        [Category("Separator"), DefaultValue(3)] public int SeparatorRadius { get => _separatorRadius; set { _separatorRadius = Math.Max(0, value); Invalidate(); } }
        [Category("Separator"), DefaultValue(6)] public int SeparatorMargin { get => _separatorMargin; set { _separatorMargin = Math.Max(0, value); Invalidate(); } }
        [Category("Separator"), DefaultValue(SidebarSeparatorAlignment.Left)] public SidebarSeparatorAlignment SeparatorAlignment { get => _separatorAlignment; set { _separatorAlignment = value; Invalidate(); } }
        [Category("Separator"), DefaultValue(SeparatorVisibility.OnHover)] public SeparatorVisibility SeparatorVisibilityMode { get => _separatorVisibility; set { _separatorVisibility = value; Invalidate(); } }

        [Category("Loading"), DefaultValue(false)] public bool EnableLoadingAnimation { get => _enableLoadingAnimation; set { _enableLoadingAnimation = value; } }
        [Category("Loading"), DefaultValue(3000)] public int LoadingDuration { get => _loadingDuration; set { _loadingDuration = Math.Max(100, value); } }
        [Category("Loading"), Browsable(false)] public bool IsLoading => _isLoading;
        [Category("Loading"), DefaultValue("Loading...")] public string LoadingText { get => _loadingText; set { _loadingText = value ?? "Loading..."; Invalidate(); } }
        [Category("Loading"), DefaultValue(true)] public bool ShowLoadingText { get => _showLoadingText; set { _showLoadingText = value; Invalidate(); } }
        [Category("Loading")] public Color LoadingProgressColor { get => _loadingProgressColor; set { _loadingProgressColor = value; Invalidate(); } }
        [Category("Loading")] public Color LoadingBackgroundColor { get => _loadingBackgroundColor; set { _loadingBackgroundColor = value; Invalidate(); } }
        [Category("Loading"), DefaultValue(3)] public int LoadingThickness { get => _loadingThickness; set { _loadingThickness = Math.Max(1, Math.Min(10, value)); Invalidate(); } }
        [Category("Loading"), DefaultValue(20)] public int LoadingSize { get => _loadingSize; set { _loadingSize = Math.Max(12, Math.Min(60, value)); Invalidate(); } }
        public event EventHandler LoadingComplete;

        [Category("Animation Speed"), DefaultValue(120)] public int HoverAnimationSpeed { get => _hoverAnimationSpeed; set => _hoverAnimationSpeed = Math.Max(1, value); }
        [Category("Animation Speed"), DefaultValue(180)] public int HoverOutAnimationSpeed { get => _hoverOutAnimationSpeed; set => _hoverOutAnimationSpeed = Math.Max(1, value); }
        [Category("Animation Speed"), DefaultValue(140)] public int SeparatorAnimationSpeed { get => _separatorAnimationSpeed; set => _separatorAnimationSpeed = Math.Max(1, value); }
        [Category("Animation Speed"), DefaultValue(160)] public int SeparatorOutAnimationSpeed { get => _separatorOutAnimationSpeed; set => _separatorOutAnimationSpeed = Math.Max(1, value); }
        [Category("Animation Speed"), DefaultValue(120)] public int SeparatorPressReleaseSpeed { get => _separatorPressReleaseSpeed; set => _separatorPressReleaseSpeed = Math.Max(1, value); }

        [Category("Animation"), DefaultValue(true)] public bool EnableHoverAnimation { get => _enableHoverAnimation; set => _enableHoverAnimation = value; }
        [Category("Animation"), DefaultValue(true)] public bool EnableSeparatorAnimation { get => _enableSeparatorAnimation; set => _enableSeparatorAnimation = value; }
        [Category("Animation"), DefaultValue(true)]
        public bool EnableIconPressAnimation
        {
            get => _enableIconPressAnimation;
            set
            {
                if (_enableIconPressAnimation == value)
                    return;

                _enableIconPressAnimation = value;
                if (!_enableIconPressAnimation)
                    _iconPressProgress = 0f;

                Invalidate();
            }
        }
        [Category("Animation"), DefaultValue(true)]
        public bool EnableTextPressAnimation
        {
            get => _enableTextPressAnimation;
            set
            {
                if (_enableTextPressAnimation == value)
                    return;

                _enableTextPressAnimation = value;
                if (!_enableTextPressAnimation)
                {
                    _textPressProgress = 0f;
                }

                Invalidate();
            }
        }
        [Category("Animation"), DefaultValue(true)] public bool EnableSeparatorPressAnimation { get => _enableSeparatorPressAnimation; set => _enableSeparatorPressAnimation = value; }

        // Icon color hover
        [Category("Icon"), DefaultValue(0.18f), Description("How much the icon darkens on hover (0-1)")]
        public float IconHoverDarken { get => _iconHoverDarken; set { _iconHoverDarken = Math.Max(0f, Math.Min(1f, value)); Invalidate(); } }

        // Text color hover
        [Category("Text"), DefaultValue(0.15f), Description("How much the text darkens on hover (0-1)")]
        public float TextHoverDarken { get => _textHoverDarken; set { _textHoverDarken = Math.Max(0f, Math.Min(1f, value)); Invalidate(); } }
        [Category("Animation Speed"), DefaultValue(100)] public int TextColorInSpeed { get => _textColorInSpeed; set => _textColorInSpeed = Math.Max(1, value); }
        [Category("Animation Speed"), DefaultValue(120)] public int TextColorOutSpeed { get => _textColorOutSpeed; set => _textColorOutSpeed = Math.Max(1, value); }

        #endregion

        #region Constructor
        public VortexSidebarButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable, true);

            BackColor = Color.Transparent;
            Size = new Size(200, 45);
            Cursor = Cursors.Hand;
            TabStop = true;

            try { Font = new Font("Segoe UI", 9f, FontStyle.Regular); }
            catch { Font = SystemFonts.DefaultFont; }
        }
        #endregion

        #region Easing
        private float EaseOutCubic(float t) => 1 - (float)Math.Pow(1 - t, 3);
        private float EaseInOutCubic(float t) => t < 0.5f ? 4 * t * t * t : 1 - (float)Math.Pow(-2 * t + 2, 3) / 2;
        #endregion

        #region Animations

        private async Task AnimateHoverAsync(bool hovering, CancellationToken ct)
        {
            try
            {
                float start = _hoverProgress, target = hovering ? 1f : 0f;
                float duration = hovering ? _hoverAnimationSpeed : _hoverOutAnimationSpeed;
                int frame = 16; float elapsed = 0f;

                _hoverProgress = start + (target - start) * 0.2f; Invalidate();

                while (!ct.IsCancellationRequested && elapsed < duration)
                {
                    elapsed += frame;
                    float p = Math.Min(elapsed / duration, 1f);
                    _hoverProgress = start + (target - start) * EaseOutCubic(p);
                    if (IsHandleCreated && !IsDisposed) Invalidate();
                    await Task.Delay(frame, ct);
                }

                if (!ct.IsCancellationRequested) { _hoverProgress = target; Invalidate(); }
            }
            catch (TaskCanceledException) { }
        }

        private async Task AnimateSeparatorAsync(bool visible, CancellationToken ct)
        {
            try
            {
                float start = _separatorProgress, target = visible ? 1f : 0f;
                float duration = visible ? _separatorAnimationSpeed : _separatorOutAnimationSpeed;
                int frame = 16; float elapsed = 0f;

                while (!ct.IsCancellationRequested && elapsed < duration)
                {
                    elapsed += frame;
                    float p = Math.Min(elapsed / duration, 1f);
                    _separatorProgress = start + (target - start) * EaseInOutCubic(p);
                    if (IsHandleCreated && !IsDisposed) Invalidate();
                    await Task.Delay(frame, ct);
                }

                if (!ct.IsCancellationRequested) { _separatorProgress = target; Invalidate(); }
            }
            catch (TaskCanceledException) { }
        }

        private Rectangle SnapScaleRect(Rectangle baseRect, float scale)
        {
            // الحجم الأساسي
            int bw = baseRect.Width;
            int bh = baseRect.Height;

            // نحسب انكماش بالبكسل بدل كسور (10% max انكماش زي قبل)
            // مثال: لو bw=18 والضغط 50% → انكماش ~0.9px → نكمّمه لـ 1px
            float maxShrinkW = bw * 0.10f; // نفس 10% اللي كنت عاملها
            float maxShrinkH = bh * 0.10f;

            // من progress بدل scale: هنا داخل الدالة جالك scale جاهز، فحوّله لانكماش
            // shrink = (1 - scale) * base
            float shrinkWF = (1f - scale) * bw;
            float shrinkHF = (1f - scale) * bh;

            // نكمّم لعدد صحيح
            int shrinkW = (int)Math.Round(shrinkWF);
            int shrinkH = (int)Math.Round(shrinkHF);

            // نتأكد ما نعدّيش الحد الأقصى 10%
            shrinkW = Math.Max(0, Math.Min((int)Math.Round(maxShrinkW), shrinkW));
            shrinkH = Math.Max(0, Math.Min((int)Math.Round(maxShrinkH), shrinkH));

            int iw = bw - shrinkW;
            int ih = bh - shrinkH;

            // نحافظ على نفس الـparity بتاعة المقاس الأصلي عشان المركز ما يتحركش
            if ((iw & 1) != (bw & 1)) iw--; // لو اختلفت الفردي/الزوجي، نقص بكسل
            if ((ih & 1) != (bh & 1)) ih--;

            // ضمان الحد الأدنى
            iw = Math.Max(1, iw);
            ih = Math.Max(1, ih);

            // مركز ثابت 100%
            int ix = baseRect.X + (bw - iw) / 2;
            int iy = baseRect.Y + (bh - ih) / 2;

            return new Rectangle(ix, iy, iw, ih);
        }


        private async Task AnimateSeparatorPressAsync(bool pressed, CancellationToken ct)
        {
            try
            {
                float start = _separatorPressProgress, target = pressed ? 1f : 0f;

                if (pressed)
                {
                    _separatorPressProgress = 1f; Invalidate();
                }
                else
                {
                    float duration = _separatorPressReleaseSpeed;
                    int frame = 16; float elapsed = 0f;

                    while (!ct.IsCancellationRequested && elapsed < duration)
                    {
                        elapsed += frame;
                        float p = Math.Min(elapsed / duration, 1f);
                        _separatorPressProgress = start + (target - start) * EaseOutCubic(p);
                        if (IsHandleCreated && !IsDisposed) Invalidate();
                        await Task.Delay(frame, ct);
                    }

                    if (!ct.IsCancellationRequested) { _separatorPressProgress = target; Invalidate(); }
                }
            }
            catch (TaskCanceledException) { }
        }

        private async Task AnimateTextColorAsync(bool hovering, CancellationToken ct)
        {
            try
            {
                float start = _textColorProgress, target = hovering ? 1f : 0f;
                float duration = hovering ? _textColorInSpeed : _textColorOutSpeed;
                const int frame = 16; float elapsed = 0f;

                _textColorProgress = start + (target - start) * 0.3f; Invalidate();

                while (!ct.IsCancellationRequested && elapsed < duration)
                {
                    elapsed += frame;
                    float t = Math.Min(elapsed / duration, 1f);
                    _textColorProgress = start + (target - start) * EaseOutCubic(t);
                    if (IsHandleCreated && !IsDisposed) Invalidate();
                    await Task.Delay(frame, ct);
                }

                if (!ct.IsCancellationRequested) { _textColorProgress = target; Invalidate(); }
            }
            catch (TaskCanceledException) { }
        }

        private async Task AnimateLoadingAsync(CancellationToken ct)
        {
            try
            {
                _loadingProgress = 0f;
                SafeInvalidate();

                const int frame = 8;
                float duration = Math.Max(1, _loadingDuration);
                Stopwatch stopwatch = Stopwatch.StartNew();

                while (!ct.IsCancellationRequested)
                {
                    float elapsed = (float)stopwatch.Elapsed.TotalMilliseconds;
                    _loadingProgress = Math.Min(elapsed / duration, 1f);
                    SafeInvalidate();

                    if (_loadingProgress >= 1f)
                        break;

                    await Task.Delay(frame, ct);
                }

                if (!ct.IsCancellationRequested)
                {
                    _loadingProgress = 1f;
                    SafeInvalidate();
                    await Task.Delay(100, ct);
                    _isLoading = false;
                    SafeInvalidate();
                    OnLoadingComplete(EventArgs.Empty);
                }
            }
            catch (TaskCanceledException)
            {
                _isLoading = false;
                _loadingProgress = 0f;
                SafeInvalidate();
            }
        }

        #endregion

        #region Mouse Events

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovering = true;

            _hoverAnimationCts?.Cancel(); _hoverAnimationCts?.Dispose();
            _hoverAnimationCts = new CancellationTokenSource();
            if (_enableHoverAnimation) _ = AnimateHoverAsync(true, _hoverAnimationCts.Token);

            if (_showSeparator && _separatorVisibility == SeparatorVisibility.OnHover)
            {
                _separatorAnimationCts?.Cancel(); _separatorAnimationCts?.Dispose();
                if (_enableSeparatorAnimation)
                {
                    _separatorAnimationCts = new CancellationTokenSource();
                    _ = AnimateSeparatorAsync(true, _separatorAnimationCts.Token);
                }
                else
                {
                    _separatorProgress = 1f;
                    Invalidate();
                }
            }

            _iconColorProgress = 1f;
            Invalidate();

            _textColorAnimationCts?.Cancel(); _textColorAnimationCts?.Dispose();
            _textColorAnimationCts = new CancellationTokenSource();
            _ = AnimateTextColorAsync(true, _textColorAnimationCts.Token);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovering = false;

            _hoverAnimationCts?.Cancel(); _hoverAnimationCts?.Dispose();
            _hoverAnimationCts = new CancellationTokenSource();
            if (_enableHoverAnimation) _ = AnimateHoverAsync(false, _hoverAnimationCts.Token);

            if (_showSeparator && _separatorVisibility == SeparatorVisibility.OnHover)
            {
                _separatorAnimationCts?.Cancel(); _separatorAnimationCts?.Dispose();
                if (_enableSeparatorAnimation)
                {
                    _separatorAnimationCts = new CancellationTokenSource();
                    _ = AnimateSeparatorAsync(false, _separatorAnimationCts.Token);
                }
                else
                {
                    _separatorProgress = 0f;
                    Invalidate();
                }
            }

            if (_isPressed)
            {
                _isPressed = false;
                _iconPressProgress = 0f;
                _textPressProgress = 0f;
                Invalidate();
            }

            _iconColorProgress = 0f;
            Invalidate();

            _textColorAnimationCts?.Cancel(); _textColorAnimationCts?.Dispose();
            _textColorAnimationCts = new CancellationTokenSource();
            _ = AnimateTextColorAsync(false, _textColorAnimationCts.Token);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                _isPressed = true;
                _pressStartTime = DateTime.Now;

                if (_enableIconPressAnimation && _icon != null && _showIcon)
                {
                    _iconPressProgress = 1f;
                    Invalidate();
                }

                if (_enableTextPressAnimation && !string.IsNullOrEmpty(Text))
                {
                    _textPressProgress = 1f;
                    Invalidate();
                }

                if (_showSeparator && _enableSeparatorPressAnimation)
                {
                    _separatorPressAnimationCts?.Cancel(); _separatorPressAnimationCts?.Dispose();
                    _separatorPressAnimationCts = new CancellationTokenSource();
                    _ = AnimateSeparatorPressAsync(true, _separatorPressAnimationCts.Token);
                }

                Focus();

                if (_enableLoadingAnimation && !_isLoading)
                    _ = StartLoadingAsync();
            }
        }

        protected override async void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left && _isPressed)
            {
                if (_enableTextPressAnimation && !string.IsNullOrEmpty(Text))
                {
                    _textPressProgress = 0f;
                    Invalidate();
                }

                if (_enableIconPressAnimation && _icon != null && _showIcon)
                {
                    _iconPressProgress = 0f;
                    Invalidate();
                }

                int elapsedMs = (int)(DateTime.Now - _pressStartTime).TotalMilliseconds;
                int remainingTime = MinimumPressDisplayTime - elapsedMs;
                if (remainingTime > 0) await Task.Delay(remainingTime);

                _isPressed = false;

                if (_showSeparator && _enableSeparatorPressAnimation)
                {
                    _separatorPressAnimationCts?.Cancel(); _separatorPressAnimationCts?.Dispose();
                    _separatorPressAnimationCts = new CancellationTokenSource();
                    _ = AnimateSeparatorPressAsync(false, _separatorPressAnimationCts.Token);
                }
            }
        }

        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }

        #endregion

        #region Public Loading Methods

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
            catch (OperationCanceledException) { /* ignore */ }
        }

        public void StopLoading()
        {
            _isLoading = false;
            _loadingAnimationCts?.Cancel();
            _loadingProgress = 0f;
            Invalidate();
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

        #endregion

        #region Helpers

        private void SafeInvalidate()
        {
            if (IsDisposed)
                return;

            if (!IsHandleCreated)
            {
                Invalidate();
                return;
            }

            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(Invalidate)); }
                catch (ObjectDisposedException) { }
                catch (InvalidOperationException) { }
            }
            else
            {
                Invalidate();
            }
        }

        private Color InterpolateColor(Color start, Color end, float t)
        {
            t = Math.Max(0, Math.Min(1, t));
            int r = (int)Math.Round(start.R + (end.R - start.R) * t);
            int g = (int)Math.Round(start.G + (end.G - start.G) * t);
            int b = (int)Math.Round(start.B + (end.B - start.B) * t);
            int a = (int)Math.Round(start.A + (end.A - start.A) * t);
            return Color.FromArgb(a, r, g, b);
        }

        private Color Darken(Color c, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            int r = (int)Math.Round(c.R * (1f - amount));
            int g = (int)Math.Round(c.G * (1f - amount));
            int b = (int)Math.Round(c.B * (1f - amount));
            return Color.FromArgb(c.A, r, g, b);
        }

        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(rect); return path; }

            int d = radius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(d, d));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - d; path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - d; path.AddArc(arc, 0, 90);
            arc.X = rect.Left; path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private GraphicsPath CreateRoundedPathF(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(rect); return path; }
            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);

            float d = radius * 2f;
            RectangleF arc = new RectangleF(rect.Location, new SizeF(d, d));

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - d; path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - d; path.AddArc(arc, 0, 90);
            arc.X = rect.Left; path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void DrawIconWithColor(Graphics g, Image icon, Rectangle rect, Color color)
        {
            if (icon == null) return;
            ColorMatrix cm = new ColorMatrix(new float[][]
            {
                new float[] {color.R/255f, 0, 0, 0, 0},
                new float[] {0, color.G/255f, 0, 0, 0},
                new float[] {0, 0, color.B/255f, 0, 0},
                new float[] {0, 0, 0, color.A/255f, 0},
                new float[] {0, 0, 0, 0, 1}
            });
            using (var ia = new ImageAttributes())
            {
                ia.SetColorMatrix(cm);
                g.DrawImage(icon, rect, 0, 0, icon.Width, icon.Height, GraphicsUnit.Pixel, ia);
            }
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle clientRect = new Rectangle(0, 0, Width, Height);

            // Base colors
            Color currentBackColor = _backgroundColor;
            Color currentTextColor = _textColor;

            if (_hoverProgress > 0)
            {
                currentBackColor = InterpolateColor(currentBackColor, _hoverBackColor, _hoverProgress);
                currentTextColor = InterpolateColor(currentTextColor, _hoverTextColor, _hoverProgress);
            }

            if (_isPressed)
            {
                if (_pressedBackColor.A > 0) currentBackColor = _pressedBackColor;
                if (_pressedTextColor.A > 0) currentTextColor = _pressedTextColor;
            }

            // Background
            if (currentBackColor.A > 0)
            {
                if (_borderWidth > 0)
                {
                    RectangleF bgRect = new RectangleF(_borderWidth, _borderWidth, Width - (_borderWidth * 2), Height - (_borderWidth * 2));
                    float bgRadius = Math.Max(0, _borderRadius - _borderWidth);
                    using (var p = CreateRoundedPathF(bgRect, bgRadius))
                    using (var b = new SolidBrush(currentBackColor)) g.FillPath(b, p);
                }
                else
                {
                    using (var p = CreateRoundedPath(clientRect, _borderRadius))
                    using (var b = new SolidBrush(currentBackColor)) g.FillPath(b, p);
                }
            }

            // Separator — FADE ONLY (fixed margins and height)
            if (_showSeparator)
            {
                bool drawSep = false;
                float alpha = 1f;

                switch (_separatorVisibility)
                {
                    case SeparatorVisibility.Always: drawSep = true; alpha = 1f; break;
                    case SeparatorVisibility.Never: drawSep = false; break;
                    case SeparatorVisibility.OnHover: drawSep = _separatorProgress > 0; alpha = _separatorProgress; break;
                }

                if (drawSep && alpha > 0f)
                {
                    Rectangle sepRect;
                    if (_separatorAlignment == SidebarSeparatorAlignment.Up || _separatorAlignment == SidebarSeparatorAlignment.Down)
                    {
                        int x = _separatorMargin;
                        int y = _separatorAlignment == SidebarSeparatorAlignment.Up ? 0 : Height - _separatorWidth;
                        int w = Math.Max(0, Width - (_separatorMargin * 2));
                        sepRect = new Rectangle(x, y, w, _separatorWidth);
                    }
                    else
                    {
                        int x = _separatorAlignment == SidebarSeparatorAlignment.Left ? 0 : Width - _separatorWidth;
                        int y = _separatorMargin;
                        int h = Math.Max(0, Height - (_separatorMargin * 2));
                        sepRect = new Rectangle(x, y, _separatorWidth, h);
                    }

                    if (_separatorPressProgress > 0f)
                    {
                        int pressGrow = (int)Math.Round(2 * _separatorPressProgress);
                        if (_separatorAlignment == SidebarSeparatorAlignment.Up || _separatorAlignment == SidebarSeparatorAlignment.Down)
                        {
                            sepRect.Height += pressGrow;
                            if (_separatorAlignment == SidebarSeparatorAlignment.Down)
                                sepRect.Y -= pressGrow;
                        }
                        else
                        {
                            sepRect.Width += pressGrow;
                            if (_separatorAlignment == SidebarSeparatorAlignment.Right)
                                sepRect.X -= pressGrow;
                        }
                    }

                    Color baseCol = _separatorColor;
                    Color bright = InterpolateColor(baseCol, Color.White, 0.45f * _separatorPressProgress);
                    Color final = Color.FromArgb((int)(bright.A * alpha), bright.R, bright.G, bright.B);

                    int r = Math.Min(_separatorRadius, Math.Min(sepRect.Width, sepRect.Height) / 2);

                    using (GraphicsPath sepPath = CreateRoundedPath(sepRect, r))
                    using (SolidBrush sepBrush = new SolidBrush(final))
                        g.FillPath(sepBrush, sepPath);
                }
            }

            // Border
            if (_borderWidth > 0 && _borderColor.A > 0)
            {
                float half = _borderWidth / 2f;
                RectangleF borderRect = new RectangleF(half, half, Width - _borderWidth, Height - _borderWidth);
                using (var path = CreateRoundedPathF(borderRect, _borderRadius))
                using (var pen = new Pen(_borderColor, _borderWidth) { LineJoin = LineJoin.Round })
                    g.DrawPath(pen, path);
            }

            // Content area
            Rectangle contentRect = new Rectangle(
                _contentPadding.Left,
                _contentPadding.Top,
                Width - _contentPadding.Horizontal,
                Height - _contentPadding.Vertical);

            // Layout
            Rectangle iconRect = Rectangle.Empty;
            Rectangle textRect = contentRect;

            if (_icon != null && _showIcon)
            {
                int iconX;
                int iconY = contentRect.Y + (contentRect.Height - _iconSize.Height) / 2 + _iconOffsetY;

                if (_iconPosition == SidebarIconPosition.Right)
                {
                    iconX = contentRect.Right - _iconSize.Width + _iconOffsetX;
                    int overlapComp = Math.Max(0, -_iconOffsetX);
                    textRect = new Rectangle(contentRect.X, contentRect.Y,
                        contentRect.Width - (_iconSize.Width + _iconSpacing) - overlapComp, contentRect.Height);
                }
                else if (_iconPosition == SidebarIconPosition.Top)
                {
                    Size textSize = TextRenderer.MeasureText(Text ?? "", Font);
                    int totalHeight = _iconSize.Height + (string.IsNullOrEmpty(Text) ? 0 : _iconSpacing + textSize.Height);
                    int startY = contentRect.Y + (contentRect.Height - totalHeight) / 2;

                    iconX = contentRect.X + (contentRect.Width - _iconSize.Width) / 2 + _iconOffsetX;
                    iconY = startY + _iconOffsetY;

                    int textY = startY + _iconSize.Height + _iconSpacing;
                    textRect = new Rectangle(contentRect.X, textY, contentRect.Width, Math.Max(0, contentRect.Bottom - textY));
                }
                else if (_iconPosition == SidebarIconPosition.Center)
                {
                    Size textSize = TextRenderer.MeasureText(Text ?? "", Font);
                    int totalWidth = _iconSize.Width + _iconSpacing + textSize.Width;
                    int startX = contentRect.X + (contentRect.Width - totalWidth) / 2;
                    iconX = startX + _iconOffsetX;
                    textRect = new Rectangle(startX + _iconSize.Width + _iconSpacing, contentRect.Y, textSize.Width, contentRect.Height);
                }
                else
                {
                    iconX = contentRect.X + _iconOffsetX;
                    int shift = Math.Max(0, _iconOffsetX);
                    textRect = new Rectangle(contentRect.X + (_iconSize.Width + _iconSpacing) + shift, contentRect.Y,
                        contentRect.Right - (contentRect.X + (_iconSize.Width + _iconSpacing) + shift), contentRect.Height);
                }

                iconRect = new Rectangle(iconX, iconY, _iconSize.Width, _iconSize.Height);

                if (!(_isLoading && _loadingProgress > 0))
                {
                    // Scale بسيط على press فقط (0..10%)
                    float pressScale = 1.0f - (_iconPressProgress * 0.10f);

                    // لون الأيقونة: تغميق hover + تغميق بسيط وقت press
                    Color darker = Darken(_iconColor, _iconHoverDarken);
                    Color iconFinal = InterpolateColor(_iconColor, darker, _iconColorProgress);
                    // مستطيل “كمّامي” يحافظ على المركز والـparity
                    Rectangle scaledRect = SnapScaleRect(iconRect, pressScale);

                    // للـraster الصغيرة، ده بيمنع shimmer
                    var oldPo = g.PixelOffsetMode;
                    var oldIm = g.InterpolationMode;

                    // جرب دول، ولو حسّيت بلور زيادة بدّل NearestNeighbor
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;

                    if (_tintIcon)
                        DrawIconWithColor(g, _icon, scaledRect, iconFinal);
                    else
                        g.DrawImage(_icon, scaledRect);

                    g.PixelOffsetMode = oldPo;
                    g.InterpolationMode = oldIm;
                }


            }

            // Text offsets
            textRect = new Rectangle(
                textRect.X + _textOffsetX,
                textRect.Y + _textOffsetY,
                textRect.Width - _textOffsetX,
                textRect.Height - _textOffsetY);

            // Final text color (hover darken)
            Color textFinalColor = InterpolateColor(currentTextColor, Darken(currentTextColor, _textHoverDarken), _textColorProgress);

            // Text
            if (!(_isLoading && _loadingProgress > 0) && !string.IsNullOrEmpty(Text))
            {
                TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;

                if (_icon == null || !_showIcon)
                {
                    switch (_textAlignment)
                    {
                        case TextAlignment.Left: flags |= TextFormatFlags.Left; break;
                        case TextAlignment.Center: flags |= TextFormatFlags.HorizontalCenter; break;
                        case TextAlignment.Right: flags |= TextFormatFlags.Right; break;
                    }
                }
                else if (_iconPosition == SidebarIconPosition.Top) flags |= TextFormatFlags.HorizontalCenter;
                else flags |= TextFormatFlags.Left;

                if (_enableTextPressAnimation && _textPressProgress > 0f && _textPressOffset > 0)
                {
                    int pressOffset = (int)Math.Round(_textPressOffset * _textPressProgress);
                    textRect.Offset(pressOffset, pressOffset);
                }

                TextRenderer.DrawText(g, Text, Font, textRect, textFinalColor, flags);
            }

            // Loading
            if (_isLoading && _loadingProgress > 0) DrawLoadingProgress(g, clientRect);
        }

        private void DrawLoadingProgress(Graphics g, Rectangle bounds)
        {
            if (_loadingProgress <= 0) return;

            var sm = g.SmoothingMode; var im = g.InterpolationMode; var cq = g.CompositingQuality; var po = g.PixelOffsetMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            int centerX = bounds.X + bounds.Width / 2;
            int centerY = bounds.Y + bounds.Height / 2;
            int diameter = _loadingSize;
            int radius = diameter / 2;

            if (_showLoadingText && !string.IsNullOrEmpty(_loadingText))
            {
                Size textSize = TextRenderer.MeasureText(g, _loadingText, Font);
                int spacing = 8;
                int totalWidth = diameter + spacing + textSize.Width;

                int startX = centerX - totalWidth / 2;

                RectangleF circleRect = new RectangleF(startX, centerY - radius, diameter, diameter);
                float inset = _loadingThickness / 2f;
                circleRect.Inflate(-inset, -inset);

                using (Pen backgroundPen = new Pen(_loadingBackgroundColor, _loadingThickness))
                { backgroundPen.StartCap = LineCap.Round; backgroundPen.EndCap = LineCap.Round; g.DrawEllipse(backgroundPen, circleRect); }

                if (_loadingProgress > 0.01f)
                {
                    float sweepAngle = Math.Min(360f * _loadingProgress, 359.99f);
                    using (Pen progressPen = new Pen(_loadingProgressColor, _loadingThickness))
                    { progressPen.StartCap = LineCap.Round; progressPen.EndCap = LineCap.Round; g.DrawArc(progressPen, circleRect, -90, sweepAngle); }
                }

                Rectangle textRect = new Rectangle(startX + diameter + spacing, centerY - textSize.Height / 2, textSize.Width, textSize.Height);
                TextRenderer.DrawText(g, _loadingText, Font, textRect, _textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
            else
            {
                RectangleF circleRect = new RectangleF(centerX - radius, centerY - radius, diameter, diameter);
                float inset = _loadingThickness / 2f;
                circleRect.Inflate(-inset, -inset);

                using (Pen backgroundPen = new Pen(_loadingBackgroundColor, _loadingThickness))
                { backgroundPen.StartCap = LineCap.Round; backgroundPen.EndCap = LineCap.Round; g.DrawEllipse(backgroundPen, circleRect); }

                if (_loadingProgress > 0.01f)
                {
                    float sweepAngle = Math.Min(360f * _loadingProgress, 359.99f);
                    using (Pen progressPen = new Pen(_loadingProgressColor, _loadingThickness))
                    { progressPen.StartCap = LineCap.Round; progressPen.EndCap = LineCap.Round; g.DrawArc(progressPen, circleRect, -90, sweepAngle); }
                }
            }

            g.SmoothingMode = sm; g.InterpolationMode = im; g.CompositingQuality = cq; g.PixelOffsetMode = po;
        }

        #endregion

        #region Disposal
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hoverAnimationCts?.Cancel(); _hoverAnimationCts?.Dispose();
                _separatorAnimationCts?.Cancel(); _separatorAnimationCts?.Dispose();
                _separatorPressAnimationCts?.Cancel(); _separatorPressAnimationCts?.Dispose();
                _loadingCts?.Cancel(); _loadingCts?.Dispose();
                _loadingAnimationCts?.Cancel(); _loadingAnimationCts?.Dispose();
                _textColorAnimationCts?.Cancel(); _textColorAnimationCts?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
