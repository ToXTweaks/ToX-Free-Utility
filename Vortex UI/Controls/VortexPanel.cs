using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Timer = System.Windows.Forms.Timer;

namespace Vortex.Ui
{
    public enum PatternStyle { None, Dots, Grid, Diagonal, Crosses, Gradient, Hexagons, Stars, AdvancedGradient }
    public enum GradientBlendMode { Linear, Smooth, Radial, Soft }

    [ToolboxItem(true)]
    public class VortexPanel : Panel, IMessageFilter
    {
        private class ParentInfo { public Bitmap Snapshot; public Size SnapshotSize = Size.Empty; public Point SnapshotLocation = Point.Empty; public int LastSnapshotTick = 0; public readonly object Lock = new object(); }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, ParentInfo> _parentInfos = new System.Runtime.CompilerServices.ConditionalWeakTable<Control, ParentInfo>();
        private class CompositedInfo { public int RequestCount; public bool Initialized; public bool WasCompositedInitially; public readonly object Lock = new object(); }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Form, CompositedInfo> _compositedInfos = new System.Runtime.CompilerServices.ConditionalWeakTable<Form, CompositedInfo>();

        private sealed class ScrollOverlay : Control
        {
            private readonly VortexPanel _owner;

            public ScrollOverlay(VortexPanel owner)
            {
                _owner = owner;
                TabStop = false;
                Visible = false;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            }

            protected override void OnPaintBackground(PaintEventArgs e) { }

            protected override void OnPaint(PaintEventArgs e)
            {
                _owner.PaintScrollOverlay(e.Graphics, Location);
            }
        }

        [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] private static extern IntPtr BeginDeferWindowPos(int nNumWindows);
        [DllImport("user32.dll")] private static extern IntPtr DeferWindowPos(IntPtr hWinPosInfo, IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")] private static extern bool EndDeferWindowPos(IntPtr hWinPosInfo);
        [DllImport("user32.dll")] private static extern int ScrollWindowEx(IntPtr hWnd, int dx, int dy, ref RECT prcScroll, ref RECT prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, uint flags);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] private static extern IntPtr WindowFromPoint(Point point);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(Rectangle rectangle)
            {
                Left = rectangle.Left;
                Top = rectangle.Top;
                Right = rectangle.Right;
                Bottom = rectangle.Bottom;
            }
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_COMPOSITED = 0x02000000;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_NOOWNERZORDER = 0x0200;
        private const uint SW_SCROLLCHILDREN = 0x0001;
        private const uint SW_INVALIDATE = 0x0002;
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;

        private class ScrollSnapshot
        {
            public Control Control;
            public Bitmap Bitmap;
            public Point BaseLocation;

            public ScrollSnapshot(Control control, Bitmap bitmap, Point baseLocation)
            {
                Control = control;
                Bitmap = bitmap;
                BaseLocation = baseLocation;
            }
        }

        [Category("Appearance")] public Color BackgroundColor { get => _backgroundColor; set { _backgroundColor = value; InvalidateRenderCache(); Invalidate(); } }
        [Category("Appearance")] public Color PatternColor { get => _patternColor; set { _patternColor = value; InvalidateRenderCache(); Invalidate(); } }
        [Category("Appearance"), DefaultValue(PatternStyle.None)] public PatternStyle PatternStyle { get => _patternStyle; set { _patternStyle = value; InvalidateRenderCache(); Invalidate(); } }
        [Category("Appearance"), DefaultValue(1)]
        public int PatternSize
        {
            get => _patternSize;
            set
            {
                int v = Math.Max(1, value);
                if (_patternSize == v) return;
                _patternSize = v;
                InvalidatePatternCache();
                Invalidate();
            }
        }
        [Category("Appearance"), DefaultValue(24)] public int PatternSpacing { get => _patternSpacing; set { _patternSpacing = Math.Max(5, value); InvalidateRenderCache(); Invalidate(); } }
        [Category("Appearance"), DefaultValue(0.2f)] public float PatternOpacity { get => _patternOpacity; set { _patternOpacity = Clamp(value, 0f, 1f); InvalidateRenderCache(); Invalidate(); } }
        [Category("Appearance"), DefaultValue(true)] public bool HighQuality { get => _highQuality; set { _highQuality = value; InvalidateRenderCache(); } }
        [Category("Appearance"), DefaultValue(false)] public bool RoundCorners { get => _roundCorners; set { if (_roundCorners == value) return; _roundCorners = value; UpdateRegion(); InvalidateRenderCache(); Invalidate(); } }
        [Category("Appearance"), DefaultValue(10)] public int CornerRadius { get => _cornerRadius; set { value = Math.Max(0, value); if (_cornerRadius == value) return; _cornerRadius = value; UpdateRegion(); InvalidateRenderCache(); Invalidate(); } }
        [Category("Appearance"), DefaultValue(false)] public bool ShowBorders { get => _showBorders; set { _showBorders = value; Invalidate(); } }
        [Category("Appearance")] public Color BorderColor { get => _borderColor; set { _borderColor = value; Invalidate(); } }
        [Category("Appearance"), DefaultValue(1)] public int BorderSize { get => _borderSize; set { _borderSize = Math.Max(1, value); Invalidate(); } }

        private bool _useComposited = false;
        [Category("Behavior"), DefaultValue(false)]
        public bool UseComposited { get => _useComposited; set { if (_useComposited == value) return; _useComposited = value; UpdateComposited(); } }

        private bool _useCompositedForScrolling = false;
        [Category("Behavior"), DefaultValue(false)]
        public bool UseCompositedForScrolling { get => _useCompositedForScrolling; set { if (_useCompositedForScrolling == value) return; _useCompositedForScrolling = value; UpdateComposited(); } }

        [Category("Gradient")] public Color GradientStartColor { get => _gradientStartColor; set { _gradientStartColor = value; InvalidateGradientCache(); InvalidateRenderCache(); Invalidate(); } }
        [Category("Gradient")] public Color GradientEndColor { get => _gradientEndColor; set { _gradientEndColor = value; InvalidateGradientCache(); InvalidateRenderCache(); Invalidate(); } }
        [Category("Gradient"), DefaultValue(LinearGradientMode.Vertical)] public LinearGradientMode GradientDirection { get => _gradientDirection; set { _gradientDirection = value; InvalidateRenderCache(); Invalidate(); } }
        [Category("Gradient")] public Color GradientTopLeft { get => _gradientTopLeft; set { _gradientTopLeft = value; InvalidateGradientCache(); InvalidateRenderCache(); Invalidate(); } }
        [Category("Gradient")] public Color GradientTopRight { get => _gradientTopRight; set { _gradientTopRight = value; InvalidateGradientCache(); InvalidateRenderCache(); Invalidate(); } }
        [Category("Gradient")] public Color GradientBottomLeft { get => _gradientBottomLeft; set { _gradientBottomLeft = value; InvalidateGradientCache(); InvalidateRenderCache(); Invalidate(); } }
        [Category("Gradient")] public Color GradientBottomRight { get => _gradientBottomRight; set { _gradientBottomRight = value; InvalidateGradientCache(); InvalidateRenderCache(); Invalidate(); } }
        [Category("Gradient"), DefaultValue(2.0f)] public float GradientSmoothness { get => _gradientSmoothness; set { _gradientSmoothness = Math.Max(0.5f, Math.Min(5f, value)); InvalidateGradientCache(); InvalidateRenderCache(); Invalidate(); } }
        [Category("Gradient"), DefaultValue(GradientBlendMode.Smooth)] public GradientBlendMode GradientBlendMode { get => _gradientBlendMode; set { _gradientBlendMode = value; InvalidateGradientCache(); InvalidateRenderCache(); Invalidate(); } }

        [Category("Scrollbar"), DefaultValue(false)]
        public bool EnableCustomScrollbar
        {
            get => _enableCustomScrollbar;
            set
            {
                if (_enableCustomScrollbar == value) return;
                _enableCustomScrollbar = value;

                if (value)
                {
                    EnsureScrollTimers();
                    AttachChildHandlers();
                    RegisterWheelMessageFilter();
                }
                else
                {
                    EndBitmapScroll(false);
                    ResetScrollOffsets();
                    DetachChildHandlers();
                    _childBaseLocations.Clear();
                    UnregisterWheelMessageFilter();
                    ReleaseScrollTimers();
                }

                AutoScroll = !value;
                UpdateScrollbars();
                UpdateComposited();
                Invalidate();
            }
        }
        [Category("Scrollbar"), DefaultValue(true)] public bool AutoHideScrollbar { get => _autoHideScrollbar; set { _autoHideScrollbar = value; UpdateScrollbars(); Invalidate(); } }
        [Category("Scrollbar"), DefaultValue(12)] public int ScrollbarThickness { get => _scrollbarThickness; set { _scrollbarThickness = Math.Max(6, Math.Min(50, value)); UpdateScrollbars(); Invalidate(); } }
        [Category("Scrollbar"), DefaultValue(2)] public int ScrollbarOffset { get => _scrollbarOffset; set { _scrollbarOffset = Math.Max(0, value); UpdateScrollbars(); Invalidate(); } }
        [Category("Scrollbar")] public Color ScrollbarTrackColor { get => _scrollbarTrackColor; set { _scrollbarTrackColor = value; Invalidate(); } }
        [Category("Scrollbar")] public Color ScrollbarThumbColor { get => _scrollbarThumbColor; set { _scrollbarThumbColor = value; Invalidate(); } }
        [Category("Scrollbar")] public Color ScrollbarThumbHoverColor { get => _scrollbarThumbHoverColor; set { _scrollbarThumbHoverColor = value; Invalidate(); } }
        [Category("Scrollbar")] public Color ScrollbarThumbActiveColor { get => _scrollbarThumbActiveColor; set { _scrollbarThumbActiveColor = value; Invalidate(); } }
        [Category("Scrollbar"), DefaultValue(true)] public bool ScrollbarRoundedCorners { get => _scrollbarRoundedCorners; set { _scrollbarRoundedCorners = value; Invalidate(); } }
        [Category("Scrollbar"), DefaultValue(6)] public int ScrollbarCornerRadius { get => _scrollbarCornerRadius; set { _scrollbarCornerRadius = Math.Max(0, value); Invalidate(); } }
        [Category("Scrollbar"), DefaultValue(0.8f)] public float ScrollbarOpacity { get => _scrollbarOpacity; set { _scrollbarOpacity = Clamp(value, 0.1f, 1f); Invalidate(); } }
        [Category("Scrollbar"), DefaultValue(true)]
        public bool UseBitmapScrollSnapshots
        {
            get => _useBitmapScrollSnapshots;
            set
            {
                if (_useBitmapScrollSnapshots == value) return;
                _useBitmapScrollSnapshots = value;
                if (!_useBitmapScrollSnapshots) EndBitmapScroll(false);
            }
        }
        [Category("Scrollbar"), DefaultValue(200)] public int FadeInTime { get => _fadeInTime; set { _fadeInTime = Math.Max(10, Math.Min(2000, value)); } }
        [Category("Scrollbar"), DefaultValue(800)] public int FadeOutTime { get => _fadeOutTime; set { _fadeOutTime = Math.Max(10, Math.Min(5000, value)); } }
        [Category("Scrollbar"), DefaultValue(1500)] public int AutoHideDelay { get => _autoHideDelay; set { _autoHideDelay = Math.Max(100, Math.Min(10000, value)); } }
        [Category("Scrollbar"), DefaultValue(typeof(Size), "0, 0")] public new Size AutoScrollMargin { get => _autoScrollMargin; set { _autoScrollMargin = new Size(Math.Max(0, value.Width), Math.Max(0, value.Height)); UpdateScrollbars(); Invalidate(); } }
        [Category("Scrollbar"), DefaultValue(0)] public int VerticalScrollbarLength { get => _verticalScrollbarLength; set { _verticalScrollbarLength = Math.Max(0, value); UpdateScrollbars(); Invalidate(); } }
        [Category("Scrollbar"), DefaultValue(0)] public int HorizontalScrollbarLength { get => _horizontalScrollbarLength; set { _horizontalScrollbarLength = Math.Max(0, value); UpdateScrollbars(); Invalidate(); } }
        [Browsable(false)] public int VerticalScrollValue { get => _verticalOffset; set => SetVerticalOffset(value); }
        [Browsable(false)] public int HorizontalScrollValue { get => _horizontalOffset; set => SetHorizontalOffset(value); }


        private Bitmap _parentBackgroundSnapshot;
        private Size _parentSnapshotSize = Size.Empty;
        private Point _parentSnapshotLocation = Point.Empty;
        private Bitmap _cachedRenderBitmap;
        private Size _cachedRenderSize = Size.Empty;
        private Bitmap? _bitmapViewportSnapshot;
        private Rectangle _bitmapViewportSnapshotScreenBounds = Rectangle.Empty;

        private float _gradientSmoothness = 2.0f;
        private GradientBlendMode _gradientBlendMode = GradientBlendMode.Smooth;
        private Color _backgroundColor = Color.FromArgb(11, 14, 19);
        private Color _patternColor = Color.FromArgb(30, 30, 35);
        private PatternStyle _patternStyle = PatternStyle.None;
        private int _patternSize = 1;
        private int _patternSpacing = 24;
        private float _patternOpacity = 0.2f;
        private bool _highQuality = true;
        private bool _roundCorners = false;
        private int _cornerRadius = 10;
        private bool _showBorders = false;
        private Color _borderColor = Color.White;
        private int _borderSize = 1;
        private Form? _compositedForm;
        private bool _hasCompositedRequest = false;
        private Color _gradientStartColor = Color.FromArgb(5, 7, 10);
        private Color _gradientEndColor = Color.FromArgb(11, 14, 19);
        private LinearGradientMode _gradientDirection = LinearGradientMode.Vertical;
        private Color _gradientTopLeft = Color.FromArgb(43, 43, 43);
        private Color _gradientTopRight = Color.FromArgb(20, 20, 20);
        private Color _gradientBottomLeft = Color.FromArgb(23, 23, 23);
        private Color _gradientBottomRight = Color.FromArgb(27, 27, 27);

        private bool _enableCustomScrollbar = false;
        private bool _autoHideScrollbar = true;
        private int _scrollbarThickness = 12;
        private int _scrollbarOffset = 2;
        private Color _scrollbarTrackColor = Color.FromArgb(25, 25, 30);
        private Color _scrollbarThumbColor = Color.FromArgb(60, 60, 65);
        private Color _scrollbarThumbHoverColor = Color.FromArgb(80, 80, 85);
        private Color _scrollbarThumbActiveColor = Color.FromArgb(100, 100, 105);
        private bool _scrollbarRoundedCorners = true;
        private int _scrollbarCornerRadius = 6;
        private float _scrollbarOpacity = 0.8f;
        private bool _useBitmapScrollSnapshots = false;
        private int _fadeInTime = 200;
        private int _fadeOutTime = 800;
        private int _autoHideDelay = 1500;
        private Size _autoScrollMargin = new Size(0, 0);
        private int _verticalScrollbarLength = 0;
        private int _horizontalScrollbarLength = 0;

        private Rectangle _vTrack, _vThumb, _hTrack, _hThumb;
        private bool _vVisible, _hVisible;
        private bool _dragV, _dragH;
        private int _dragVOffset, _dragHOffset;
        private int _verticalOffset, _horizontalOffset;
        private readonly Dictionary<Control, Point> _childBaseLocations = new Dictionary<Control, Point>();
        private bool _applyingContentOffsets = false;
        private float _currentOpacity = 0f;
        private bool _shouldBeVisible = false;
        private Timer? _fadeTimer;
        private Timer? _autoHideTimer;
        private Timer? _scrollRefreshTimer;
        private Timer? _bitmapScrollTimer;
        private Timer? _dragScrollTimer;
        private Timer? _smoothScrollTimer;
        private Point _lastMouse;
        private bool _hoverV, _hoverH;
        private readonly List<ScrollSnapshot> _scrollSnapshots = new List<ScrollSnapshot>();
        private ScrollOverlay _scrollOverlay;
        private bool _bitmapScrollActive = false;
        private bool _paintingTransparentScrollSnapshot = false;
        private bool _messageFilterRegistered = false;
        private int _bitmapTargetVerticalOffset, _bitmapTargetHorizontalOffset;
        private int _targetVerticalOffset, _targetHorizontalOffset;
        private bool _updatingSmoothScroll = false;
        private bool _hasPendingDragVerticalOffset, _hasPendingDragHorizontalOffset;
        private int _pendingDragVerticalOffset, _pendingDragHorizontalOffset;

        private Bitmap _cachedGradientBitmap;
        private Size _cachedGradientSize;
        private Color _cachedTL, _cachedTR, _cachedBL, _cachedBR;
        private float _cachedSmoothness;
        private GradientBlendMode _cachedBlendMode;

        private Bitmap _cachedPatternBitmap;
        private Size _cachedPatternSize = Size.Empty;
        private PatternStyle _cachedPatternStyle;
        private int _cachedPatternElementSize;
        private Color _cachedPatternColor;
        private float _cachedPatternOpacity;
        private float _lastParentSnapshotTime = 0f;
        private const int ParentSnapshotThrottleMs = 200;
        private const int BitmapScrollIntervalMs = 16;
        private const int BitmapScrollMaxPixelsPerTick = 52;
        private const float BitmapScrollEase = 0.45f;
        private const int DragScrollIntervalMs = 16;
        private const int SmoothScrollIntervalMs = 16;
        private const int SmoothScrollMaxPixelsPerTick = 160;
        private const int SmoothScrollMinPixelsPerTick = 16;
        private const int SmoothScrollSnapPixels = 8;
        private const float SmoothScrollEase = 0.68f;
        private const int ScrollChildRefreshDelayMs = 90;

        private bool _isDraggingScrollbar = false;
        private bool _isScrollingContent = false;
        private bool _suppressParentSnapshotDuringDrag = true;

        private void InvalidatePatternCache()
        {
            try { _cachedPatternBitmap?.Dispose(); } catch { }
            _cachedPatternBitmap = null;
            _cachedPatternSize = Size.Empty;
            _cachedPatternStyle = PatternStyle.None;
            _cachedPatternElementSize = 0;
            _cachedPatternColor = Color.Empty;
            _cachedPatternOpacity = -1f;
            InvalidateRenderCache(); // لو أردت تحديث الrender الكلي أيضاً
        }

        private static void Parent_Paint_Static(object sender, PaintEventArgs e)
        {
            if (!(sender is Control parent)) return;
            if (parent is VortexPanel scrollingParent && scrollingParent._isScrollingContent) return;
            if (_parentInfos.TryGetValue(parent, out ParentInfo pinfo)) System.Threading.Interlocked.Exchange(ref pinfo.LastSnapshotTick, Environment.TickCount);
            foreach (Control c in parent.Controls) if (c is VortexPanel vp) vp.ThrottledInvalidateParentSnapshot();
        }

        private static void Parent_MoveOrResize_Static(object sender, EventArgs e)
        {
            if (!(sender is Control parent)) return;
            if (_parentInfos.TryGetValue(parent, out ParentInfo pinfo)) System.Threading.Interlocked.Exchange(ref pinfo.LastSnapshotTick, Environment.TickCount);
            foreach (Control c in parent.Controls) if (c is VortexPanel vp) vp.ThrottledInvalidateParentSnapshot();
        }

        private static void Parent_Disposed_Static(object sender, EventArgs e)
        {
            if (sender is Control parent)
            {
                if (_parentInfos.TryGetValue(parent, out ParentInfo pi))
                {
                    lock (pi.Lock) { pi.Snapshot?.Dispose(); pi.Snapshot = null; pi.SnapshotSize = Size.Empty; pi.SnapshotLocation = Point.Empty; }
                }
                try { _parentInfos.Remove(parent); } catch { }
            }
        }

        public VortexPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            DoubleBuffered = true;
            TabStop = true;
            AutoScroll = false;
            MouseEnter += (s, e) => { FocusIfPossible(); ShowScrollbars(); };
            MouseMove += (s, e) => { _lastMouse = e.Location; UpdateHoverState(e.Location); };
            MouseLeave += (s, e) => StartAutoHide();
            MouseDown += OnMouseDownDrag;
            MouseUp += OnMouseUpDrag;
            AttachChildHandlers();
            UpdateRegion();
        }

        private void EnsureScrollTimers()
        {
            if (_fadeTimer == null)
            {
                _fadeTimer = new Timer { Interval = 30 };
                _fadeTimer.Tick += (s, e) => AnimateOpacityStep();
            }

            if (_autoHideTimer == null)
            {
                _autoHideTimer = new Timer();
                _autoHideTimer.Tick += (s, e) => BeginFadeOut();
            }

            if (_scrollRefreshTimer == null)
            {
                _scrollRefreshTimer = new Timer { Interval = ScrollChildRefreshDelayMs };
                _scrollRefreshTimer.Tick += (s, e) => { _scrollRefreshTimer?.Stop(); RefreshScrolledChildren(); };
            }

            if (_bitmapScrollTimer == null)
            {
                _bitmapScrollTimer = new Timer { Interval = BitmapScrollIntervalMs };
                _bitmapScrollTimer.Tick += (s, e) => AnimateBitmapScrollStep();
            }

            if (_dragScrollTimer == null)
            {
                _dragScrollTimer = new Timer { Interval = DragScrollIntervalMs };
                _dragScrollTimer.Tick += (s, e) => ApplyPendingDragScroll();
            }

            if (_smoothScrollTimer == null)
            {
                _smoothScrollTimer = new Timer { Interval = SmoothScrollIntervalMs };
                _smoothScrollTimer.Tick += (s, e) => AnimateSmoothScrollStep();
            }
        }

        private void ReleaseScrollTimers()
        {
            try { _fadeTimer?.Stop(); _autoHideTimer?.Stop(); _scrollRefreshTimer?.Stop(); _bitmapScrollTimer?.Stop(); _dragScrollTimer?.Stop(); _smoothScrollTimer?.Stop(); } catch { }

            _fadeTimer?.Dispose(); _fadeTimer = null;
            _autoHideTimer?.Dispose(); _autoHideTimer = null;
            _scrollRefreshTimer?.Dispose(); _scrollRefreshTimer = null;
            _bitmapScrollTimer?.Dispose(); _bitmapScrollTimer = null;
            _dragScrollTimer?.Dispose(); _dragScrollTimer = null;
            _smoothScrollTimer?.Dispose(); _smoothScrollTimer = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnregisterWheelMessageFilter();
                ReleaseCompositedRequest();
                ReleaseScrollTimers();
                RestoreSnapshotControls();
                DisposeScrollSnapshots();
                if (_scrollOverlay != null)
                {
                    try { Controls.Remove(_scrollOverlay); } catch { }
                    try { _scrollOverlay.Dispose(); } catch { }
                    _scrollOverlay = null;
                }
                try { _parentBackgroundSnapshot?.Dispose(); } catch { }
                _parentBackgroundSnapshot = null; _parentSnapshotSize = Size.Empty; _parentSnapshotLocation = Point.Empty;
                _childBaseLocations.Clear();
                _cachedGradientBitmap?.Dispose(); _cachedRenderBitmap?.Dispose(); _cachedPatternBitmap?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void UpdateRegion() { if (!RoundCorners) try { this.Region = null; } catch { } }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RegisterWheelMessageFilter();
            UpdateComposited();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            UnregisterWheelMessageFilter();
            base.OnHandleDestroyed(e);
        }

        private void RegisterWheelMessageFilter()
        {
            if (_messageFilterRegistered || !EnableCustomScrollbar || IsInDesignMode() || !IsHandleCreated) return;
            Application.AddMessageFilter(this);
            _messageFilterRegistered = true;
        }

        private void UnregisterWheelMessageFilter()
        {
            if (!_messageFilterRegistered) return;
            try { Application.RemoveMessageFilter(this); } catch { }
            _messageFilterRegistered = false;
        }

        public void UpdateComposited()
        {
            try
            {
                Form? targetForm = ShouldUseComposited() ? FindForm() : null;

                if (_hasCompositedRequest && !ReferenceEquals(_compositedForm, targetForm)) ReleaseCompositedRequest();

                if (targetForm != null)
                {
                    if (_hasCompositedRequest) ApplyFormCompositedState(targetForm);
                    else RequestComposited(targetForm);
                }

                InvalidateParentSnapshot(); Invalidate();
            }
            catch { }
        }

        private bool ShouldUseComposited()
        {
            return !IsInDesignMode() && (_useComposited || (_enableCustomScrollbar && _useCompositedForScrolling));
        }

        private void RequestComposited(Form form)
        {
            if (form == null || form.IsDisposed) return;

            _compositedForm = form;
            _hasCompositedRequest = true;

            form.HandleCreated -= Form_HandleCreated;
            form.HandleCreated += Form_HandleCreated;
            form.Disposed -= Form_Disposed;
            form.Disposed += Form_Disposed;

            CompositedInfo info = _compositedInfos.GetValue(form, _ => new CompositedInfo());
            lock (info.Lock)
            {
                info.RequestCount++;
                ApplyFormCompositedState(form, info);
            }
        }

        private void ReleaseCompositedRequest()
        {
            Form? form = _compositedForm;
            bool hadRequest = _hasCompositedRequest;
            _hasCompositedRequest = false;
            _compositedForm = null;

            if (!hadRequest || form == null) return;

            try
            {
                form.HandleCreated -= Form_HandleCreated;
                form.Disposed -= Form_Disposed;
            }
            catch { }

            try
            {
                if (_compositedInfos.TryGetValue(form, out CompositedInfo? info) && info != null)
                {
                    bool removeInfo;
                    lock (info.Lock)
                    {
                        if (info.RequestCount > 0) info.RequestCount--;
                        ApplyFormCompositedState(form, info);
                        removeInfo = info.RequestCount == 0;
                    }

                    if (removeInfo) _compositedInfos.Remove(form);
                }
            }
            catch { }
        }

        private void Form_HandleCreated(object? sender, EventArgs e)
        {
            if (sender is Form form && _hasCompositedRequest && ReferenceEquals(form, _compositedForm)) ApplyFormCompositedState(form);
        }

        private void Form_Disposed(object? sender, EventArgs e)
        {
            if (sender is Form form && ReferenceEquals(form, _compositedForm)) ReleaseCompositedRequest();
        }

        private static void ApplyFormCompositedState(Form form)
        {
            if (form == null) return;
            if (_compositedInfos.TryGetValue(form, out CompositedInfo? info) && info != null)
            {
                lock (info.Lock) ApplyFormCompositedState(form, info);
            }
        }

        private static void ApplyFormCompositedState(Form form, CompositedInfo info)
        {
            if (form == null || form.IsDisposed || !form.IsHandleCreated) return;

            int ex = GetWindowLong(form.Handle, GWL_EXSTYLE);
            if (!info.Initialized)
            {
                info.WasCompositedInitially = (ex & WS_EX_COMPOSITED) != 0;
                info.Initialized = true;
            }

            bool shouldBeComposited = info.RequestCount > 0 || info.WasCompositedInitially;
            int desired = shouldBeComposited ? (ex | WS_EX_COMPOSITED) : (ex & ~WS_EX_COMPOSITED);
            if (desired != ex) SetWindowLong(form.Handle, GWL_EXSTYLE, desired);
        }

        private void InvalidateParentSnapshot()
        {
            try { _parentBackgroundSnapshot?.Dispose(); } catch { }
            _parentBackgroundSnapshot = null; _parentSnapshotSize = Size.Empty; _parentSnapshotLocation = Point.Empty;
            if (Parent != null && _parentInfos.TryGetValue(Parent, out ParentInfo pi)) { lock (pi.Lock) { try { pi.Snapshot?.Dispose(); } catch { } pi.Snapshot = null; pi.SnapshotSize = Size.Empty; pi.SnapshotLocation = Point.Empty; } }
        }

        private void EnsureParentSnapshot()
        {
            if (HasUsableParentSnapshot()) return;
            if (_isDraggingScrollbar && _suppressParentSnapshotDuringDrag && _parentBackgroundSnapshot == null) return;
            if (Parent == null || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            if (!Parent.Visible) return;
            var pinfo = _parentInfos.GetValue(Parent, k => new ParentInfo());
            lock (pinfo.Lock)
            {
                if (pinfo.Snapshot != null && pinfo.SnapshotSize == ClientSize && pinfo.SnapshotLocation == Location)
                {
                    try { _parentBackgroundSnapshot = (Bitmap)pinfo.Snapshot.Clone(); _parentSnapshotSize = pinfo.SnapshotSize; _parentSnapshotLocation = pinfo.SnapshotLocation; return; } catch { _parentBackgroundSnapshot = null; _parentSnapshotSize = Size.Empty; _parentSnapshotLocation = Point.Empty; }
                }
            }

            var bmp = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppArgb);
            using (var pg = Graphics.FromImage(bmp))
            {
                pg.TranslateTransform(-Left, -Top);
                var pea = new PaintEventArgs(pg, new Rectangle(Left, Top, Width, Height));
                try { InvokePaintBackground(Parent, pea); InvokePaint(Parent, pea); } catch { } finally { pg.ResetTransform(); }

                // InvokePaint(Parent) بيرسم خلفية الـ parent بس، مش الكنترولز الشقيقة.
                // لو فيه siblings ورا البانل (أقل في الـ z-order) ومتداخلين معاها،
                // نرسمهم كمان عشان الشفافية توريهم بدل خلفية الـ parent الفاضية.
                PaintSiblingsBehindInto(pg);
            }
            lock (pinfo.Lock)
            {
                try { pinfo.Snapshot?.Dispose(); } catch { }
                pinfo.Snapshot = bmp; pinfo.SnapshotSize = ClientSize; pinfo.SnapshotLocation = Location;
                try { _parentBackgroundSnapshot = (Bitmap)bmp.Clone(); _parentSnapshotSize = ClientSize; _parentSnapshotLocation = Location; } catch { _parentBackgroundSnapshot = null; _parentSnapshotSize = Size.Empty; _parentSnapshotLocation = Point.Empty; }
            }
        }

        // يرسم الكنترولز الشقيقة اللي ورا هذا البانل (في الـ z-order) والمتداخلة معاه،
        // داخل صورة الـ snapshot، عشان الخلفية الشفافة توريهم.
        private void PaintSiblingsBehindInto(Graphics g)
        {
            Control parent = Parent;
            if (parent == null) return;

            int myIndex;
            try { myIndex = parent.Controls.GetChildIndex(this, false); }
            catch { return; }
            if (myIndex < 0) return;

            Rectangle myBounds = Bounds; // بإحداثيات الـ parent

            // في WinForms: أعلى index = أبعد للخلف، وبيترسم الأول.
            // نرسم من الأبعد (Count-1) لحد اللي قبلي مباشرة (myIndex+1) عشان الأقرب يغطّي الأبعد.
            for (int i = parent.Controls.Count - 1; i > myIndex; i--)
            {
                Control sib = parent.Controls[i];
                if (sib == null || !sib.Visible || sib.Width <= 0 || sib.Height <= 0) continue;
                if (!sib.Bounds.IntersectsWith(myBounds)) continue;

                try
                {
                    using (var sbmp = new Bitmap(sib.Width, sib.Height, PixelFormat.Format32bppArgb))
                    {
                        sib.DrawToBitmap(sbmp, new Rectangle(0, 0, sib.Width, sib.Height));
                        // من إحداثيات الـ parent لإحداثيات هذا البانل (client)
                        g.DrawImage(sbmp, sib.Left - Left, sib.Top - Top, sib.Width, sib.Height);
                    }
                }
                catch { }
            }
        }

        private void ThrottledInvalidateParentSnapshot()
        {
            if (Parent is VortexPanel scrollingParent && scrollingParent._isScrollingContent) return;
            if (_isDraggingScrollbar && _suppressParentSnapshotDuringDrag) { InvalidateScrollbarRegions(); return; }
            float now = Environment.TickCount;
            if (now - _lastParentSnapshotTime < ParentSnapshotThrottleMs) { Invalidate(new Rectangle(0, 0, Math.Min(ClientSize.Width, 16), Math.Min(ClientSize.Height, 16))); return; }
            _lastParentSnapshotTime = now;
            InvalidateParentSnapshot(); Invalidate();
        }

        private bool HasUsableParentSnapshot()
        {
            return _parentBackgroundSnapshot != null
                && _parentSnapshotSize == ClientSize
                && _parentSnapshotLocation == Location
                && _parentBackgroundSnapshot.Width > 0
                && _parentBackgroundSnapshot.Height > 0;
        }

        private Color GetParentBackColor()
        {
            Control p = this.Parent;
            while (p != null)
            {
                if (p.BackColor.A > 0) return p.BackColor;
                p = p.Parent;
            }
            return SystemColors.Control;
        }

        private bool IsInDesignMode()
        {
            return DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime || Site?.DesignMode == true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            bool isDesignMode = IsInDesignMode();
            if (HighQuality)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.CompositingMode = CompositingMode.SourceOver;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            }
            else
            {
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.None;
            }

            if (_paintingTransparentScrollSnapshot)
            {
                PaintTransparentScrollSnapshot(g);
                DrawScrollSnapshots(g);
                if (EnableCustomScrollbar) DrawScrollbars(g);
                return;
            }

            if (_isDraggingScrollbar && _suppressParentSnapshotDuringDrag)
            {
                if (HasUsableParentSnapshot())
                {
                    try { g.DrawImageUnscaled(_parentBackgroundSnapshot, 0, 0); }
                    catch (ArgumentException) { using (var sb = new SolidBrush(GetParentBackColor())) g.FillRectangle(sb, ClientRectangle); _parentBackgroundSnapshot = null; _parentSnapshotSize = Size.Empty; _parentSnapshotLocation = Point.Empty; }
                }
                else using (var sb = new SolidBrush(GetParentBackColor())) g.FillRectangle(sb, ClientRectangle);

                if (RoundCorners && _cachedRenderBitmap != null) g.DrawImageUnscaled(_cachedRenderBitmap, 0, 0);
                else
                {
                    DrawBackground(g, null);
                    if (PatternStyle != PatternStyle.None && PatternStyle != PatternStyle.Gradient && PatternStyle != PatternStyle.AdvancedGradient && PatternOpacity > 0f) DrawPattern(g, null);
                }

                if (ShowBorders && BorderSize > 0) DrawBorder(g);
            }
            else
            {
                if (RoundCorners && CornerRadius > 0)
                {
                    EnsureCachedRenderBitmap();

                    if (Parent != null) EnsureParentSnapshot();

                    if (HasUsableParentSnapshot())
                    {
                        try { g.DrawImageUnscaled(_parentBackgroundSnapshot, 0, 0); }
                        catch (ArgumentException) { using (var sb = new SolidBrush(GetParentBackColor())) g.FillRectangle(sb, ClientRectangle); _parentBackgroundSnapshot = null; _parentSnapshotSize = Size.Empty; _parentSnapshotLocation = Point.Empty; }
                    }
                    else using (var sb = new SolidBrush(GetParentBackColor())) g.FillRectangle(sb, ClientRectangle);

                    if (_cachedRenderBitmap != null) g.DrawImageUnscaled(_cachedRenderBitmap, 0, 0);
                    if (ShowBorders && BorderSize > 0) DrawBorder(g);
                }
                else
                {
                    // لو الخلفية شبه/كامل الشفافية، اعرض محتوى الـ parent الحقيقي ورا الكنترول
                    // (snapshot) بدل لون صلب، عشان الشفافية تشتغل فعلاً زي مسار الأركان المدوّرة.
                    bool transparentBackground = BackgroundColor.A < 255 && PatternStyle != PatternStyle.Gradient && PatternStyle != PatternStyle.AdvancedGradient;
                    if (transparentBackground && Parent != null)
                    {
                        EnsureParentSnapshot();
                        if (HasUsableParentSnapshot())
                        {
                            try { g.DrawImageUnscaled(_parentBackgroundSnapshot, 0, 0); }
                            catch (ArgumentException) { using (var sb = new SolidBrush(GetParentBackColor())) g.FillRectangle(sb, ClientRectangle); _parentBackgroundSnapshot = null; _parentSnapshotSize = Size.Empty; _parentSnapshotLocation = Point.Empty; }
                        }
                        else using (var sb = new SolidBrush(GetParentBackColor())) g.FillRectangle(sb, ClientRectangle);
                    }
                    else
                    {
                        using (var sb = new SolidBrush(GetParentBackColor())) g.FillRectangle(sb, ClientRectangle);
                    }

                    DrawBackground(g, null);
                    if (PatternStyle != PatternStyle.None && PatternStyle != PatternStyle.Gradient && PatternStyle != PatternStyle.AdvancedGradient && PatternOpacity > 0f) DrawPattern(g, null);
                    if (ShowBorders && BorderSize > 0) DrawBorder(g);
                }
            }

            DrawScrollSnapshots(g);
            if (EnableCustomScrollbar) DrawScrollbars(g);
        }

        private void PaintTransparentScrollSnapshot(Graphics g)
        {
            if (RoundCorners && CornerRadius > 0)
            {
                EnsureCachedRenderBitmap();
                if (_cachedRenderBitmap != null)
                {
                    g.DrawImageUnscaled(_cachedRenderBitmap, 0, 0);
                }
                else
                {
                    using (GraphicsPath path = CreateRoundedRectanglePath(new RectangleF(0, 0, Width, Height), CornerRadius))
                    {
                        DrawBackground(g, path);
                        if (PatternStyle != PatternStyle.None && PatternStyle != PatternStyle.Gradient && PatternStyle != PatternStyle.AdvancedGradient && PatternOpacity > 0f)
                        {
                            GraphicsState state = g.Save();
                            try
                            {
                                g.SetClip(path, CombineMode.Replace);
                                DrawPattern(g, null);
                            }
                            finally
                            {
                                g.Restore(state);
                            }
                        }
                    }
                }

                if (ShowBorders && BorderSize > 0) DrawBorder(g);
                return;
            }

            DrawBackground(g, null);
            if (PatternStyle != PatternStyle.None && PatternStyle != PatternStyle.Gradient && PatternStyle != PatternStyle.AdvancedGradient && PatternOpacity > 0f) DrawPattern(g, null);
            if (ShowBorders && BorderSize > 0) DrawBorder(g);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (this.Parent == null) { InvalidateRenderCache(); InvalidateParentSnapshot(); UpdateComposited(); Invalidate(); return; }
            try
            {
                _parentInfos.GetValue(this.Parent, parent =>
                {
                    parent.Paint -= Parent_Paint_Static; parent.Paint += Parent_Paint_Static;
                    parent.Move -= Parent_MoveOrResize_Static; parent.Move += Parent_MoveOrResize_Static;
                    parent.Resize -= Parent_MoveOrResize_Static; parent.Resize += Parent_MoveOrResize_Static;
                    parent.Disposed -= Parent_Disposed_Static; parent.Disposed += Parent_Disposed_Static;
                    return new ParentInfo();
                });
            }
            catch { }
            InvalidateRenderCache(); InvalidateParentSnapshot(); Invalidate(); UpdateComposited();
        }

        private void EnsureCachedRenderBitmap()
        {
            if (_cachedRenderBitmap != null && _cachedRenderSize == ClientSize) return;
            _cachedRenderBitmap?.Dispose(); _cachedRenderBitmap = null;
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

            var bmp = new Bitmap(ClientSize.Width, ClientSize.Height, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.CompositingMode = CompositingMode.SourceOver;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(Color.Transparent);

                using (GraphicsPath path = CreateRoundedRectanglePath(new RectangleF(0, 0, bmp.Width, bmp.Height), CornerRadius))
                {
                    using (Brush bgBrush = new SolidBrush(BackgroundColor)) g.FillPath(bgBrush, path);

                    if (PatternStyle == PatternStyle.Gradient)
                    {
                        using (var lg = new LinearGradientBrush(new Rectangle(0, 0, bmp.Width, bmp.Height), GradientStartColor, GradientEndColor, GradientDirection))
                            g.FillPath(lg, path);
                    }
                    else if (PatternStyle == PatternStyle.AdvancedGradient)
                    {
                        var grad = GenerateAdvancedGradientBitmap(new Size(bmp.Width, bmp.Height));
                        using (var tb = new TextureBrush(grad))
                        {
                            g.SetClip(path);
                            g.FillRectangle(tb, 0, 0, bmp.Width, bmp.Height);
                            g.ResetClip();
                        }
                        grad.Dispose();
                    }

                    if (PatternStyle != PatternStyle.None && PatternStyle != PatternStyle.Gradient && PatternStyle != PatternStyle.AdvancedGradient && PatternOpacity > 0f)
                    {
                        g.SetClip(path, CombineMode.Replace);
                        var patternBmp = GetPatternBitmap(PatternSize, PatternSpacing, PatternStyle, PatternColor, PatternOpacity);
                        using (var tb = new TextureBrush(patternBmp, WrapMode.Tile)) g.FillRectangle(tb, 0, 0, bmp.Width, bmp.Height);
                        g.ResetClip();
                    }
                }
            }

            _cachedRenderBitmap = bmp; _cachedRenderSize = ClientSize;
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            InvalidateGradientCache(); UpdateScrollbars();
            _cachedRenderBitmap?.Dispose(); _cachedRenderBitmap = null; _cachedRenderSize = Size.Empty;
            UpdateRegion(); Invalidate();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (ReferenceEquals(e.Control, _scrollOverlay)) return;
            if (!EnableCustomScrollbar) return;
            StopSmoothScrolling();
            EndBitmapScroll(false);
            AttachScrollableChildHandlers(e.Control);
            UpdateScrollbars();
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            if (ReferenceEquals(e.Control, _scrollOverlay)) return;
            if (!EnableCustomScrollbar) { _childBaseLocations.Remove(e.Control); return; }
            StopSmoothScrolling();
            EndBitmapScroll(false);
            DetachScrollableChildHandlers(e.Control);
            _childBaseLocations.Remove(e.Control);
            UpdateScrollbars();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!EnableCustomScrollbar) { base.OnMouseWheel(e); return; }
            if (ProcessWheelDelta(e.Delta, false))
            {
                if (e is HandledMouseEventArgs handled) handled.Handled = true;
                return;
            }

            base.OnMouseWheel(e);
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != WM_MOUSEWHEEL && m.Msg != WM_MOUSEHWHEEL) return false;
            if (!EnableCustomScrollbar || IsDisposed || !Visible || !IsHandleCreated) return false;

            Point screenPoint = GetWheelMessagePoint(m.LParam);
            VortexPanel? target = FindWheelTarget(screenPoint);
            if (!ReferenceEquals(target, this)) return false;

            int delta = GetWheelMessageDelta(m.WParam);
            bool handled = ProcessWheelDelta(delta, m.Msg == WM_MOUSEHWHEEL);
            return handled;
        }

        private bool ProcessWheelDelta(int delta, bool forceHorizontal)
        {
            if (!EnableCustomScrollbar || delta == 0) return false;
            int scrollAmount = GetWheelScrollAmount(delta);

            bool horizontal = forceHorizontal || ((ModifierKeys & Keys.Shift) == Keys.Shift && _hVisible);
            if (horizontal && !_hVisible) return false;
            if (!horizontal && !_vVisible) return false;

            if (!horizontal && TryStartBitmapWheelScroll(scrollAmount, false))
            {
                ShowScrollbars();
                return true;
            }

            if (horizontal) SetHorizontalOffsetAnimated(GetHorizontalScrollTargetBase() - scrollAmount);
            else SetVerticalOffsetAnimated(GetVerticalScrollTargetBase() - scrollAmount);
            ShowScrollbars();
                return true;
        }

        private static int GetWheelMessageDelta(IntPtr wParam)
        {
            return unchecked((short)((wParam.ToInt64() >> 16) & 0xffff));
        }

        private static Point GetWheelMessagePoint(IntPtr lParam)
        {
            long value = lParam.ToInt64();
            int x = unchecked((short)(value & 0xffff));
            int y = unchecked((short)((value >> 16) & 0xffff));
            if (x == 0 && y == 0) return Cursor.Position;
            return new Point(x, y);
        }

        private static VortexPanel? FindWheelTarget(Point screenPoint)
        {
            IntPtr hwnd = WindowFromPoint(screenPoint);
            Control? control = hwnd == IntPtr.Zero ? null : Control.FromChildHandle(hwnd);
            while (control != null)
            {
                if (control is VortexPanel panel && panel.EnableCustomScrollbar && panel.ClientRectangle.Contains(panel.PointToClient(screenPoint))) return panel;
                control = control.Parent;
            }

            return null;
        }

        private int _scrollSpeed = 0;
        [Category("Scrollbar"), DefaultValue(0), Description("Pixels scrolled per wheel notch. 0 = automatic based on font size.")]
        public int ScrollSpeed { get => _scrollSpeed; set { _scrollSpeed = Math.Max(0, value); } }

        private int GetWheelScrollAmount(int wheelDelta)
        {
            if (wheelDelta == 0) return 0;

            float notches = wheelDelta / (float)SystemInformation.MouseWheelScrollDelta;
            int step;
            if (_scrollSpeed > 0)
            {
                step = _scrollSpeed;
            }
            else
            {
                int lineHeight = Math.Max(12, Font?.Height ?? 16);
                step = Math.Max(42, Math.Min(64, lineHeight * 4));
            }
            int amount = (int)Math.Round(step * notches);
            return amount == 0 ? Math.Sign(wheelDelta) : amount;
        }

        private int GetVerticalScrollTargetBase()
        {
            return _smoothScrollTimer != null && _smoothScrollTimer.Enabled ? _targetVerticalOffset : _verticalOffset;
        }

        private int GetHorizontalScrollTargetBase()
        {
            return _smoothScrollTimer != null && _smoothScrollTimer.Enabled ? _targetHorizontalOffset : _horizontalOffset;
        }

        private void SetVerticalOffsetAnimated(int value)
        {
            EnsureScrollTimers();
            Timer smoothScrollTimer = _smoothScrollTimer!;
            Rectangle content = GetContentBounds();
            int max = Math.Max(0, content.Height - ClientSize.Height);
            _targetVerticalOffset = Math.Max(0, Math.Min(max, value));
            if (!smoothScrollTimer.Enabled) _targetHorizontalOffset = _horizontalOffset;

            if (_targetVerticalOffset == _verticalOffset)
            {
                if (_targetHorizontalOffset == _horizontalOffset) smoothScrollTimer.Stop();
                return;
            }

            _isScrollingContent = true;
            smoothScrollTimer.Start();
        }

        private void SetHorizontalOffsetAnimated(int value)
        {
            EnsureScrollTimers();
            Timer smoothScrollTimer = _smoothScrollTimer!;
            Rectangle content = GetContentBounds();
            int max = Math.Max(0, content.Width - ClientSize.Width);
            _targetHorizontalOffset = Math.Max(0, Math.Min(max, value));
            if (!smoothScrollTimer.Enabled) _targetVerticalOffset = _verticalOffset;

            if (_targetHorizontalOffset == _horizontalOffset)
            {
                if (_targetVerticalOffset == _verticalOffset) smoothScrollTimer.Stop();
                return;
            }

            _isScrollingContent = true;
            smoothScrollTimer.Start();
        }

        private void AnimateSmoothScrollStep()
        {
            if (IsDisposed || !EnableCustomScrollbar)
            {
                StopSmoothScrolling();
                return;
            }

            int nextVertical = GetNextSmoothScrollOffset(_verticalOffset, _targetVerticalOffset);
            int nextHorizontal = GetNextSmoothScrollOffset(_horizontalOffset, _targetHorizontalOffset);

            if (nextVertical == _verticalOffset && nextHorizontal == _horizontalOffset)
            {
                StopSmoothScrolling();
                return;
            }

            _updatingSmoothScroll = true;
            try
            {
                if (nextVertical != _verticalOffset) SetVerticalOffset(nextVertical);
                if (nextHorizontal != _horizontalOffset) SetHorizontalOffset(nextHorizontal);
            }
            finally
            {
                _updatingSmoothScroll = false;
            }

            if (_verticalOffset == _targetVerticalOffset && _horizontalOffset == _targetHorizontalOffset) _smoothScrollTimer?.Stop();
        }

        private int GetNextSmoothScrollOffset(int current, int target)
        {
            int delta = target - current;
            if (delta == 0) return current;
            if (Math.Abs(delta) <= SmoothScrollSnapPixels) return target;

            int step = (int)Math.Round(delta * SmoothScrollEase);
            if (step == 0) step = Math.Sign(delta);
            if (Math.Abs(step) < SmoothScrollMinPixelsPerTick) step = Math.Sign(delta) * Math.Min(Math.Abs(delta), SmoothScrollMinPixelsPerTick);
            if (Math.Abs(step) > SmoothScrollMaxPixelsPerTick) step = Math.Sign(step) * SmoothScrollMaxPixelsPerTick;
            return current + step;
        }

        private void StopSmoothScrolling()
        {
            _smoothScrollTimer?.Stop();
            _targetVerticalOffset = _verticalOffset;
            _targetHorizontalOffset = _horizontalOffset;
        }

        private bool TryStartBitmapWheelScroll(int scrollAmount, bool horizontal)
        {
            if (!CanUseBitmapWheelScroll(horizontal)) return false;
            EnsureScrollTimers();
            StopSmoothScrolling();

            int targetHorizontal = _bitmapScrollActive ? _bitmapTargetHorizontalOffset : _horizontalOffset;
            int targetVertical = _bitmapScrollActive ? _bitmapTargetVerticalOffset : _verticalOffset;

            if (horizontal)
            {
                int max = GetMaxHorizontalOffset();
                targetHorizontal = Math.Max(0, Math.Min(max, targetHorizontal - scrollAmount));
            }
            else
            {
                int max = GetMaxVerticalOffset();
                targetVertical = Math.Max(0, Math.Min(max, targetVertical - scrollAmount));
            }

            if (!_bitmapScrollActive && targetHorizontal == _horizontalOffset && targetVertical == _verticalOffset) return false;
            if (!_bitmapScrollActive && !BeginBitmapScroll()) return false;

            _bitmapTargetHorizontalOffset = targetHorizontal;
            _bitmapTargetVerticalOffset = targetVertical;

            _isScrollingContent = true;
            _bitmapScrollTimer!.Start();
                return true;
        }

        private bool CanUseBitmapWheelScroll(bool horizontal)
        {
            if (!_useBitmapScrollSnapshots) return false;
            if (horizontal || IsInDesignMode() || _dragV || _dragH || !HasNestedChildControls()) return false;
            return horizontal ? _hVisible : _vVisible;
        }

        private bool BeginBitmapScroll()
        {
            if (_bitmapScrollActive) return true;
            if (Controls.Count == 0 || ClientSize.Width <= 0 || ClientSize.Height <= 0) return false;

            DisposeScrollSnapshots();
            CaptureBitmapViewportSnapshot();

            foreach (Control c in Controls)
            {
                if (IsScrollOverlayControl(c)) continue;
                if (!c.Visible || c.Width <= 0 || c.Height <= 0) continue;

                try
                {
                    Bitmap bmp = CaptureScrollSnapshot(c);
                    _scrollSnapshots.Add(new ScrollSnapshot(c, bmp, GetChildBaseLocation(c)));
                }
                catch
                {
                    DisposeScrollSnapshots();
                    return false;
                }
            }

            if (_scrollSnapshots.Count == 0) return false;

            _bitmapTargetVerticalOffset = _verticalOffset;
            _bitmapTargetHorizontalOffset = _horizontalOffset;
            _bitmapScrollActive = true;
            ShowScrollOverlay();
            InvalidateScrollbarRegions();
                return true;
        }

        private Bitmap CaptureScrollSnapshot(Control control)
        {
            var bmp = new Bitmap(control.Width, control.Height, PixelFormat.Format32bppPArgb);
            try
            {
                using (Graphics g = Graphics.FromImage(bmp)) g.Clear(Color.Transparent);

                if (!TryCaptureControlToBitmapForScrollSnapshot(control, bmp) &&
                    !TryPaintControlToBitmapForScrollSnapshot(control, bmp))
                {
                    throw new InvalidOperationException("Unable to capture scroll snapshot.");
                }

                if (!TryCopyVisibleControlPixelsForScrollSnapshot(control, bmp))
                {
                    TryCopyVisibleDescendantPixelsForScrollSnapshot(control, bmp);
                }

                return bmp;
            }
            catch
            {
                bmp.Dispose();
                throw;
            }
        }

        private void CaptureVortexPanelScrollSnapshot(VortexPanel panel, Bitmap bitmap)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                ConfigureScrollSnapshotGraphics(g);
                g.Clear(Color.Transparent);

                panel._paintingTransparentScrollSnapshot = true;
                try
                {
                    panel.PaintTransparentScrollSnapshot(g);
                    PaintChildControlsForScrollSnapshot(panel, g);
                }
                finally
                {
                    panel._paintingTransparentScrollSnapshot = false;
                }
            }
        }

        private void ConfigureScrollSnapshotGraphics(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.CompositingMode = CompositingMode.SourceOver;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        }

        private void PaintChildControlsForScrollSnapshot(Control parent, Graphics g)
        {
            for (int i = parent.Controls.Count - 1; i >= 0; i--)
            {
                Control child = parent.Controls[i];
                if (child == null || !child.Visible || child.Width <= 0 || child.Height <= 0) continue;
                if (parent is VortexPanel parentPanel && parentPanel.IsScrollOverlayControl(child)) continue;
                PaintControlForScrollSnapshot(child, g);
            }
        }

        private void PaintControlForScrollSnapshot(Control control, Graphics g)
        {
            if (ShouldPaintControlDirectForScrollSnapshot(control) && TryPaintControlDirectForScrollSnapshot(control, g)) return;
            TryDrawControlBitmapForScrollSnapshot(control, g);
        }

        private static bool ShouldPaintControlDirectForScrollSnapshot(Control control)
        {
            return control is VortexPanel;
        }

        private bool TryPaintControlDirectForScrollSnapshot(Control control, Graphics g)
        {
            GraphicsState state = g.Save();
            try
            {
                g.TranslateTransform(control.Left, control.Top);
                g.SetClip(new Rectangle(Point.Empty, control.Size), CombineMode.Intersect);

                if (control is VortexPanel panel)
                {
                    PaintVortexPanelForScrollSnapshot(panel, g);
                }
                else
                {
                    Rectangle clip = new Rectangle(Point.Empty, control.Size);
                    using (var pea = new PaintEventArgs(g, clip))
                    {
                        if (control.BackColor.A > 0) InvokePaintBackground(control, pea);
                        InvokePaint(control, pea);
                    }

                    PaintChildControlsForScrollSnapshot(control, g);
                }

                return true;
            }
            catch { return false; }
            finally
            {
                g.Restore(state);
            }
        }

        private void PaintVortexPanelForScrollSnapshot(VortexPanel panel, Graphics g)
        {
            panel._paintingTransparentScrollSnapshot = true;
            try
            {
                panel.PaintTransparentScrollSnapshot(g);
                PaintChildControlsForScrollSnapshot(panel, g);
            }
            finally
            {
                panel._paintingTransparentScrollSnapshot = false;
            }
        }

        private bool TryDrawControlBitmapForScrollSnapshot(Control control, Graphics g)
        {
            try
            {
                using (Bitmap childBitmap = new Bitmap(control.Width, control.Height, PixelFormat.Format32bppPArgb))
                {
                    using (Graphics childGraphics = Graphics.FromImage(childBitmap)) childGraphics.Clear(Color.Transparent);

                    if (!TryCaptureControlToBitmapForScrollSnapshot(control, childBitmap) &&
                        !TryPaintControlToBitmapForScrollSnapshot(control, childBitmap))
                    {
                        return false;
                    }

                    g.DrawImageUnscaled(childBitmap, control.Left, control.Top);
                }

                return true;
            }
            catch { return false; }
        }

        private bool TryCaptureControlToBitmapForScrollSnapshot(Control control, Bitmap bitmap)
        {
            if (control is VortexLabel label)
            {
                try
                {
                    label.DrawTransparentContentToBitmap(bitmap);
                    return true;
                }
                catch { return false; }
            }

            if (control is VortexPanel panel)
            {
                try
                {
                    CaptureVortexPanelScrollSnapshot(panel, bitmap);
                    return true;
                }
                catch { return false; }
            }

            if (control.BackColor.A < 255) return false;

            try
            {
                control.DrawToBitmap(bitmap, new Rectangle(Point.Empty, control.Size));

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryPaintControlToBitmapForScrollSnapshot(Control control, Bitmap bitmap)
        {
            try
            {
                using (Graphics childGraphics = Graphics.FromImage(bitmap))
                {
                    ConfigureScrollSnapshotGraphics(childGraphics);
                    childGraphics.Clear(Color.Transparent);

                    Rectangle clip = new Rectangle(Point.Empty, control.Size);
                    using (var pea = new PaintEventArgs(childGraphics, clip))
                    {
                        bool transparentBackColor = control.BackColor.A == 0;
                        if (!transparentBackColor || control is VortexPanel) InvokePaintBackground(control, pea);
                        InvokePaint(control, pea);
                    }

                    PaintChildControlsForScrollSnapshot(control, childGraphics);
                }

                return true;
            }
            catch { return false; }
        }

        private bool TryCopyVisibleControlPixelsForScrollSnapshot(Control control, Bitmap bitmap)
        {
            try
            {
                if (!control.IsHandleCreated || control.IsDisposed || bitmap.Width <= 0 || bitmap.Height <= 0) return false;
                if (_bitmapViewportSnapshot == null || _bitmapViewportSnapshotScreenBounds.IsEmpty) return false;

                var controlScreen = new Rectangle(control.PointToScreen(Point.Empty), control.Size);
                if (!_bitmapViewportSnapshotScreenBounds.Contains(controlScreen)) return false;

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    var source = new Rectangle(
                        controlScreen.Left - _bitmapViewportSnapshotScreenBounds.Left,
                        controlScreen.Top - _bitmapViewportSnapshotScreenBounds.Top,
                        controlScreen.Width,
                        controlScreen.Height);

                    g.DrawImage(_bitmapViewportSnapshot, new Rectangle(Point.Empty, controlScreen.Size), source, GraphicsUnit.Pixel);
                }

                return true;
            }
            catch { return false; }
        }

        private bool TryCopyVisibleDescendantPixelsForScrollSnapshot(Control root, Bitmap bitmap)
        {
            bool copiedAny = false;

            try
            {
                if (!root.IsHandleCreated || root.IsDisposed || bitmap.Width <= 0 || bitmap.Height <= 0) return false;
                if (_bitmapViewportSnapshot == null || _bitmapViewportSnapshotScreenBounds.IsEmpty) return false;

                var rootScreen = new Rectangle(root.PointToScreen(Point.Empty), root.Size);

                CopyVisibleDescendantPixels(root, root, bitmap, rootScreen, ref copiedAny);
            }
            catch { }

            return copiedAny;
        }

        private void CopyVisibleDescendantPixels(Control root, Control parent, Bitmap bitmap, Rectangle rootScreen, ref bool copiedAny)
        {
            foreach (Control child in parent.Controls)
            {
                if (child == null || IsScrollOverlayControl(child) || !child.Visible || child.Width <= 0 || child.Height <= 0) continue;

                if (ShouldCopyDescendantPixels(child))
                {
                    try
                    {
                        var childScreen = new Rectangle(child.PointToScreen(Point.Empty), child.Size);
                        Rectangle visibleScreen = Rectangle.Intersect(_bitmapViewportSnapshotScreenBounds, childScreen);
                        if (!visibleScreen.IsEmpty)
                        {
                            var destination = new Point(visibleScreen.Left - rootScreen.Left, visibleScreen.Top - rootScreen.Top);
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                g.CompositingMode = CompositingMode.SourceCopy;
                                var source = new Rectangle(
                                    visibleScreen.Left - _bitmapViewportSnapshotScreenBounds.Left,
                                    visibleScreen.Top - _bitmapViewportSnapshotScreenBounds.Top,
                                    visibleScreen.Width,
                                    visibleScreen.Height);

                                g.DrawImage(_bitmapViewportSnapshot, new Rectangle(destination, visibleScreen.Size), source, GraphicsUnit.Pixel);
                            }

                            copiedAny = true;
                        }
                    }
                    catch { }
                }

                if (child.Controls.Count > 0) CopyVisibleDescendantPixels(root, child, bitmap, rootScreen, ref copiedAny);
            }
        }

        private static bool ShouldCopyDescendantPixels(Control control)
        {
            return control.Controls.Count == 0 || !string.IsNullOrEmpty(control.Text) || control.BackColor.A < 255;
        }

        private void AnimateBitmapScrollStep()
        {
            if (IsDisposed || !_bitmapScrollActive || !EnableCustomScrollbar)
            {
                EndBitmapScroll(false);
                return;
            }

            int nextVertical = GetNextBitmapOffset(_verticalOffset, _bitmapTargetVerticalOffset);
            int nextHorizontal = GetNextBitmapOffset(_horizontalOffset, _bitmapTargetHorizontalOffset);

            if (nextVertical == _verticalOffset && nextHorizontal == _horizontalOffset)
            {
                EndBitmapScroll(true);
                return;
            }

            SetDisplayedScrollOffsets(nextHorizontal, nextVertical);
        }

        private int GetNextBitmapOffset(int current, int target)
        {
            int delta = target - current;
            if (delta == 0) return current;
            if (Math.Abs(delta) <= 2) return target;

            int step = (int)Math.Round(delta * BitmapScrollEase);
            if (step == 0) step = Math.Sign(delta);
            if (Math.Abs(step) > BitmapScrollMaxPixelsPerTick) step = Math.Sign(step) * BitmapScrollMaxPixelsPerTick;
            return current + step;
        }

        private void SetDisplayedScrollOffsets(int horizontalOffset, int verticalOffset)
        {
            _horizontalOffset = Math.Max(0, Math.Min(GetMaxHorizontalOffset(), horizontalOffset));
            _verticalOffset = Math.Max(0, Math.Min(GetMaxVerticalOffset(), verticalOffset));

            Rectangle content = GetContentBounds();
            if (_vVisible) CalculateVerticalScrollbar(content, ClientSize.Height, ClientSize.Width);
            if (_hVisible) CalculateHorizontalScrollbar(content, ClientSize.Width, ClientSize.Height);

            if (_scrollOverlay != null && _scrollOverlay.Visible) _scrollOverlay.Invalidate();
            else Invalidate(GetContentViewportRectangle());
            InvalidateScrollbarRegions();
        }

        private void EndBitmapScroll(bool commitToTarget)
        {
            if (!_bitmapScrollActive) return;

            _bitmapScrollTimer?.Stop();
            if (commitToTarget) SetDisplayedScrollOffsets(_bitmapTargetHorizontalOffset, _bitmapTargetVerticalOffset);

            try { ApplyContentOffsets(true); } catch { }
            _bitmapScrollActive = false;
            HideScrollOverlay();
            DisposeScrollSnapshots();
            _isScrollingContent = false;
            RefreshScrolledChildren();
        }

        private void HideSnapshotControls()
        {
            foreach (ScrollSnapshot snapshot in _scrollSnapshots)
            {
                try { if (!snapshot.Control.IsDisposed && snapshot.Control.IsHandleCreated) ShowWindow(snapshot.Control.Handle, SW_HIDE); } catch { }
            }
        }

        private void RestoreSnapshotControls()
        {
            foreach (ScrollSnapshot snapshot in _scrollSnapshots)
            {
                try { if (!snapshot.Control.IsDisposed && snapshot.Control.IsHandleCreated) ShowWindow(snapshot.Control.Handle, SW_SHOW); } catch { }
            }
        }

        private void DisposeScrollSnapshots()
        {
            foreach (ScrollSnapshot snapshot in _scrollSnapshots)
            {
                try { snapshot.Bitmap?.Dispose(); } catch { }
            }

            _scrollSnapshots.Clear();
            DisposeBitmapViewportSnapshot();
        }

        private void DisposeBitmapViewportSnapshot()
        {
            try { _bitmapViewportSnapshot?.Dispose(); } catch { }
            _bitmapViewportSnapshot = null;
            _bitmapViewportSnapshotScreenBounds = Rectangle.Empty;
        }

        private bool CaptureBitmapViewportSnapshot()
        {
            DisposeBitmapViewportSnapshot();

            try
            {
                Rectangle viewport = GetContentViewportRectangle();
                if (viewport.Width <= 0 || viewport.Height <= 0) return false;

                var snapshot = new Bitmap(viewport.Width, viewport.Height, PixelFormat.Format32bppPArgb);
                var screenBounds = new Rectangle(PointToScreen(viewport.Location), viewport.Size);
                using (Graphics g = Graphics.FromImage(snapshot))
                {
                    g.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size, CopyPixelOperation.SourceCopy);
                }

                _bitmapViewportSnapshot = snapshot;
                _bitmapViewportSnapshotScreenBounds = screenBounds;
                return true;
            }
            catch
            {
                DisposeBitmapViewportSnapshot();
                return false;
            }
        }

        private bool IsScrollOverlayControl(Control control)
        {
            return _scrollOverlay != null && ReferenceEquals(control, _scrollOverlay);
        }

        private ScrollOverlay EnsureScrollOverlay()
        {
            if (_scrollOverlay == null || _scrollOverlay.IsDisposed)
            {
                _scrollOverlay = new ScrollOverlay(this);
                Controls.Add(_scrollOverlay);
            }

            return _scrollOverlay;
        }

        private void ShowScrollOverlay()
        {
            ScrollOverlay overlay = EnsureScrollOverlay();
            Rectangle viewport = GetContentViewportRectangle();
            if (viewport.Width <= 0 || viewport.Height <= 0) return;

            overlay.Bounds = viewport;
            overlay.Visible = true;
            overlay.BringToFront();
            overlay.Invalidate();
        }

        private void HideScrollOverlay()
        {
            if (_scrollOverlay == null) return;
            _scrollOverlay.Visible = false;
            _scrollOverlay.Region = null;
        }

        private void PaintScrollOverlay(Graphics g, Point overlayLocation)
        {
            Size overlaySize = _scrollOverlay?.Size ?? Size.Empty;
            var overlayRect = new Rectangle(Point.Empty, overlaySize);

            if (RoundCorners && CornerRadius > 0)
            {
                EnsureCachedRenderBitmap();
                if (Parent != null && !IsInDesignMode()) EnsureParentSnapshot();

                if (HasUsableParentSnapshot())
                {
                    try { g.DrawImageUnscaled(_parentBackgroundSnapshot, -overlayLocation.X, -overlayLocation.Y); }
                    catch (ArgumentException) { using (var sb = new SolidBrush(GetParentBackColor())) g.FillRectangle(sb, overlayRect); }
                }
                else using (var sb = new SolidBrush(GetParentBackColor())) g.FillRectangle(sb, overlayRect);

                if (_cachedRenderBitmap != null)
                {
                    try { g.DrawImageUnscaled(_cachedRenderBitmap, -overlayLocation.X, -overlayLocation.Y); }
                    catch { using (var sb = new SolidBrush(BackgroundColor)) g.FillRectangle(sb, overlayRect); }
                }
                else using (var sb = new SolidBrush(BackgroundColor)) g.FillRectangle(sb, overlayRect);
            }
            else
            {
                using (var sb = new SolidBrush(GetParentBackColor())) g.FillRectangle(sb, overlayRect);
                using (var sb = new SolidBrush(BackgroundColor)) g.FillRectangle(sb, overlayRect);
            }

            if (!_bitmapScrollActive || _scrollSnapshots.Count == 0) return;

            foreach (ScrollSnapshot snapshot in _scrollSnapshots)
            {
                if (snapshot.Bitmap == null) continue;
                int x = snapshot.BaseLocation.X - _horizontalOffset - overlayLocation.X;
                int y = snapshot.BaseLocation.Y - _verticalOffset - overlayLocation.Y;
                var bounds = new Rectangle(x, y, snapshot.Bitmap.Width, snapshot.Bitmap.Height);
                if (!bounds.IntersectsWith(overlayRect)) continue;
                g.DrawImageUnscaled(snapshot.Bitmap, x, y);
            }
        }

        private void DrawScrollSnapshots(Graphics g)
        {
            if ((_scrollOverlay != null && _scrollOverlay.Visible) || !_bitmapScrollActive || _scrollSnapshots.Count == 0) return;

            var state = g.Save();
            try
            {
                Rectangle viewport = GetContentViewportRectangle();
                g.SetClip(viewport, CombineMode.Intersect);

                foreach (ScrollSnapshot snapshot in _scrollSnapshots)
                {
                    if (snapshot.Bitmap == null) continue;
                    int x = snapshot.BaseLocation.X - _horizontalOffset;
                    int y = snapshot.BaseLocation.Y - _verticalOffset;
                    var bounds = new Rectangle(x, y, snapshot.Bitmap.Width, snapshot.Bitmap.Height);
                    if (!bounds.IntersectsWith(viewport)) continue;
                    g.DrawImageUnscaled(snapshot.Bitmap, x, y);
                }
            }
            finally
            {
                g.Restore(state);
            }
        }

        private int GetMaxVerticalOffset()
        {
            Rectangle content = GetContentBounds();
            return Math.Max(0, content.Height - ClientSize.Height);
        }

        private int GetMaxHorizontalOffset()
        {
            Rectangle content = GetContentBounds();
            return Math.Max(0, content.Width - ClientSize.Width);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!EnableCustomScrollbar) return;
            if (_dragV && _vVisible) { HandleVerticalDrag(e.Y); return; }
            if (_dragH && _hVisible) { HandleHorizontalDrag(e.X); return; }
            UpdateHoverState(e.Location);
        }

        private void DrawBackground(Graphics g, GraphicsPath clipPath)
        {
            Rectangle rect = ClientRectangle;
            if (PatternStyle == PatternStyle.AdvancedGradient) { DrawAdvancedGradient(g, rect, clipPath); return; }
            if (PatternStyle == PatternStyle.Gradient)
            {
                using (var lg = new LinearGradientBrush(rect, GradientStartColor, GradientEndColor, GradientDirection))
                    if (clipPath != null) g.FillPath(lg, clipPath); else g.FillRectangle(lg, rect);
                return;
            }
            using (var bg = new SolidBrush(BackgroundColor)) if (clipPath != null) g.FillPath(bg, clipPath); else g.FillRectangle(bg, rect);
        }

        private void DrawAdvancedGradient(Graphics g, Rectangle rect, GraphicsPath clipPath)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            bool needs = _cachedGradientBitmap == null || _cachedGradientSize != rect.Size || _cachedTL != GradientTopLeft || _cachedTR != GradientTopRight || _cachedBL != GradientBottomLeft || _cachedBR != GradientBottomRight || _cachedSmoothness != GradientSmoothness || _cachedBlendMode != GradientBlendMode;
            if (needs)
            {
                _cachedGradientBitmap?.Dispose();
                _cachedGradientBitmap = GenerateAdvancedGradientBitmap(rect.Size);
                _cachedGradientSize = rect.Size; _cachedTL = GradientTopLeft; _cachedTR = GradientTopRight; _cachedBL = GradientBottomLeft; _cachedBR = GradientBottomRight; _cachedSmoothness = GradientSmoothness; _cachedBlendMode = GradientBlendMode;
            }
            if (_cachedGradientBitmap == null) return;
            if (clipPath != null) using (var tb = new TextureBrush(_cachedGradientBitmap)) g.FillPath(tb, clipPath); else g.DrawImageUnscaled(_cachedGradientBitmap, 0, 0);
        }

        private void DrawBorder(Graphics g)
        {
            if (BorderSize <= 0) return;
            float half = BorderSize / 2f;
            var bounds = new RectangleF(0.5f, 0.5f, Width - BorderSize, Height - BorderSize);
            float radius = Math.Max(0, CornerRadius - half);
            using (var pen = new Pen(BorderColor, BorderSize)) { pen.LineJoin = LineJoin.Round; pen.EndCap = LineCap.Round; pen.StartCap = LineCap.Round; if (RoundCorners && radius > 0) using (var path = CreateRoundedRectanglePath(bounds, radius)) g.DrawPath(pen, path); else g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width, bounds.Height); }
        }

        private void DrawPattern(Graphics g, Brush b)
        {
            if (PatternStyle == PatternStyle.None || PatternOpacity <= 0f) return;
            var patternBmp = GetPatternBitmap(PatternSize, PatternSpacing, PatternStyle, PatternColor, PatternOpacity);
            using (var tb = new TextureBrush(patternBmp, WrapMode.Tile)) g.FillRectangle(tb, ClientRectangle);
        }

        private void UpdateScrollbars()
        {
            if (!EnableCustomScrollbar) { _vVisible = _hVisible = false; return; }
            Rectangle content = GetContentBounds(); int viewW = ClientSize.Width, viewH = ClientSize.Height;
            _vVisible = content.Height > viewH; _hVisible = content.Width > viewW;
            if (_vVisible) CalculateVerticalScrollbar(content, viewH, viewW); else SetVerticalOffset(0);
            if (_hVisible) CalculateHorizontalScrollbar(content, viewW, viewH); else SetHorizontalOffset(0);
            ApplyContentOffsets(); InvalidateScrollbarRegions();
        }

        private void CalculateVerticalScrollbar(Rectangle content, int viewH, int viewW)
        {
            int availableH = viewH - 2 * ScrollbarOffset - (_hVisible ? (ScrollbarThickness + ScrollbarOffset) : 0);
            int trackH = (_verticalScrollbarLength > 0) ? Math.Min(_verticalScrollbarLength, availableH) : availableH;
            int trackY = ClientRectangle.Top + ScrollbarOffset + (availableH - trackH) / 2;
            _vTrack = new Rectangle(ClientRectangle.Right - ScrollbarThickness - ScrollbarOffset, trackY, ScrollbarThickness, trackH);
            int scrollRange = Math.Max(1, content.Height - viewH);
            int thumbH = Math.Max(20, (int)(trackH * (viewH / (float)content.Height)));
            int thumbY = trackY;
            if (scrollRange > 0) { float t = _verticalOffset / (float)scrollRange; thumbY = trackY + (int)((trackH - thumbH) * t); }
            _vThumb = new Rectangle(_vTrack.X, thumbY, _vTrack.Width, thumbH);
        }

        private void CalculateHorizontalScrollbar(Rectangle content, int viewW, int viewH)
        {
            int availableW = viewW - 2 * ScrollbarOffset - (_vVisible ? (ScrollbarThickness + ScrollbarOffset) : 0);
            int trackW = (_horizontalScrollbarLength > 0) ? Math.Min(_horizontalScrollbarLength, availableW) : availableW;
            int trackX = ClientRectangle.Left + ScrollbarOffset + (availableW - trackW) / 2;
            _hTrack = new Rectangle(trackX, ClientRectangle.Bottom - ScrollbarThickness - ScrollbarOffset, trackW, ScrollbarThickness);
            int scrollRange = Math.Max(1, content.Width - viewW);
            int thumbW = Math.Max(20, (int)(trackW * (viewW / (float)content.Width)));
            int thumbX = trackX;
            if (scrollRange > 0) { float t = _horizontalOffset / (float)scrollRange; thumbX = trackX + (int)((trackW - thumbW) * t); }
            _hThumb = new Rectangle(thumbX, _hTrack.Y, thumbW, _hTrack.Height);
        }

        private void DrawScrollbars(Graphics g)
        {
            float alphaFactor = (AutoHideScrollbar ? _currentOpacity : ScrollbarOpacity);
            if (alphaFactor <= 0f) return;
            Color trackCol = Color.FromArgb((int)(255 * alphaFactor), ScrollbarTrackColor);
            if (_vVisible) { FillScrollbarRect(g, _vTrack, trackCol); Color thumbCol = GetScrollbarThumbColor(_dragV, _hoverV, alphaFactor); FillScrollbarRect(g, _vThumb, thumbCol); }
            if (_hVisible) { FillScrollbarRect(g, _hTrack, trackCol); Color thumbCol = GetScrollbarThumbColor(_dragH, _hoverH, alphaFactor); FillScrollbarRect(g, _hThumb, thumbCol); }
        }

        private Color GetScrollbarThumbColor(bool isDragging, bool isHovering, float alphaFactor) { Color baseColor = isDragging ? ScrollbarThumbActiveColor : (isHovering ? ScrollbarThumbHoverColor : ScrollbarThumbColor); return Color.FromArgb((int)(255 * alphaFactor), baseColor); }

        private void FillScrollbarRect(Graphics g, Rectangle rect, Color color)
        {
            using (SolidBrush b = new SolidBrush(color))
            {
                if (ScrollbarRoundedCorners && ScrollbarCornerRadius > 0) using (GraphicsPath p = CreateRoundedRectanglePath(rect, ScrollbarCornerRadius)) g.FillPath(b, p); else g.FillRectangle(b, rect);
            }
        }

        private void InvalidateScrollbarRegions() { if (_vVisible) Invalidate(_vTrack); if (_hVisible) Invalidate(_hTrack); }

        private void OnMouseDownDrag(object sender, MouseEventArgs e)
        {
            if (!EnableCustomScrollbar) return;
            StopSmoothScrolling();
            EndBitmapScroll(false);
            FocusIfPossible();
            if (_vVisible && _vThumb.Contains(e.Location)) { _dragV = true; _dragVOffset = e.Y - _vThumb.Y; Capture = true; _isDraggingScrollbar = true; BeginBitmapDragScroll(); ShowScrollbars(); return; }
            if (_hVisible && _hThumb.Contains(e.Location)) { _dragH = true; _dragHOffset = e.X - _hThumb.X; Capture = true; _isDraggingScrollbar = true; BeginBitmapDragScroll(); ShowScrollbars(); return; }
        }

        private void OnMouseUpDrag(object sender, MouseEventArgs e)
        {
            if (!EnableCustomScrollbar) return;
            ApplyPendingDragScroll();
            _dragScrollTimer?.Stop();
            if (_bitmapScrollActive) EndBitmapScroll(true);
            _dragV = _dragH = false; Capture = false; _isDraggingScrollbar = false; StartAutoHide();
            InvalidateParentSnapshot(); Invalidate();
        }

        private void BeginBitmapDragScroll()
        {
            if (!_useBitmapScrollSnapshots) return;
            if (IsInDesignMode() || !HasNestedChildControls()) return;
            BeginBitmapScroll();
        }

        private void HandleVerticalDrag(int mouseY)
        {
            int trackY = _vTrack.Y, trackH = _vTrack.Height, thumbH = _vThumb.Height;
            int newThumbY = Math.Max(trackY, Math.Min(trackY + trackH - thumbH, mouseY - _dragVOffset));
            float t = (trackH - thumbH) == 0 ? 0f : (newThumbY - trackY) / (float)(trackH - thumbH);
            int maxOffset = Math.Max(0, GetContentBounds().Height - ClientSize.Height);
            int targetOffset = (int)(t * maxOffset);
            QueueVerticalDragOffset(targetOffset);
            ShowScrollbars();
        }

        private void HandleHorizontalDrag(int mouseX)
        {
            int trackX = _hTrack.X, trackW = _hTrack.Width, thumbW = _hThumb.Width;
            int newThumbX = Math.Max(trackX, Math.Min(trackX + trackW - thumbW, mouseX - _dragHOffset));
            float t = (trackW - thumbW) == 0 ? 0f : (newThumbX - trackX) / (float)(trackW - thumbW);
            int maxOffset = Math.Max(0, GetContentBounds().Width - ClientSize.Width);
            int targetOffset = (int)(t * maxOffset);
            QueueHorizontalDragOffset(targetOffset);
            ShowScrollbars();
        }

        private void QueueVerticalDragOffset(int targetOffset)
        {
            EnsureScrollTimers();
            _pendingDragVerticalOffset = targetOffset;
            _hasPendingDragVerticalOffset = true;
            if (!_dragScrollTimer!.Enabled) _dragScrollTimer.Start();
        }

        private void QueueHorizontalDragOffset(int targetOffset)
        {
            EnsureScrollTimers();
            _pendingDragHorizontalOffset = targetOffset;
            _hasPendingDragHorizontalOffset = true;
            if (!_dragScrollTimer!.Enabled) _dragScrollTimer.Start();
        }

        private void ApplyPendingDragScroll()
        {
            if (!_hasPendingDragVerticalOffset && !_hasPendingDragHorizontalOffset)
            {
                _dragScrollTimer?.Stop();
                return;
            }

            bool hasVertical = _hasPendingDragVerticalOffset;
            bool hasHorizontal = _hasPendingDragHorizontalOffset;
            int vertical = _pendingDragVerticalOffset;
            int horizontal = _pendingDragHorizontalOffset;

            _hasPendingDragVerticalOffset = false;
            _hasPendingDragHorizontalOffset = false;

            if (_bitmapScrollActive)
            {
                int targetVertical = hasVertical ? vertical : _verticalOffset;
                int targetHorizontal = hasHorizontal ? horizontal : _horizontalOffset;
                _bitmapTargetVerticalOffset = targetVertical;
                _bitmapTargetHorizontalOffset = targetHorizontal;
                SetDisplayedScrollOffsets(targetHorizontal, targetVertical);
                return;
            }

            if (hasVertical) SetVerticalOffset(vertical);
            if (hasHorizontal) SetHorizontalOffset(horizontal);
        }

        private void UpdateHoverState(Point pt)
        {
            bool prevHoverV = _hoverV, prevHoverH = _hoverH;
            _hoverV = _vVisible && (_vThumb.Contains(pt) || _vTrack.Contains(pt));
            _hoverH = _hVisible && (_hThumb.Contains(pt) || _hTrack.Contains(pt));
            if (prevHoverV != _hoverV || prevHoverH != _hoverH) InvalidateScrollbarRegions();
            if (_hoverV || _hoverH) ShowScrollbars();
        }

        private void SetVerticalOffset(int value)
        {
            Rectangle content = GetContentBounds();
            int max = Math.Max(0, content.Height - ClientSize.Height);
            int v = Math.Max(0, Math.Min(max, value));
            if (v == _verticalOffset) { if (!_updatingSmoothScroll) _targetVerticalOffset = _verticalOffset; return; }
            int oldHorizontalOffset = _horizontalOffset;
            int oldVerticalOffset = _verticalOffset;
            Rectangle oldThumb = _vThumb; _verticalOffset = v;
            CalculateVerticalScrollbar(content, ClientSize.Height, ClientSize.Width);
            ApplyContentOffsets(false, oldHorizontalOffset, oldVerticalOffset);
            Invalidate(Rectangle.Union(oldThumb, _vThumb));
            if (!_updatingSmoothScroll) _targetVerticalOffset = _verticalOffset;

        }

        private void SetHorizontalOffset(int value)
        {
            Rectangle content = GetContentBounds();
            int max = Math.Max(0, content.Width - ClientSize.Width);
            int v = Math.Max(0, Math.Min(max, value));
            if (v == _horizontalOffset) { if (!_updatingSmoothScroll) _targetHorizontalOffset = _horizontalOffset; return; }
            int oldHorizontalOffset = _horizontalOffset;
            int oldVerticalOffset = _verticalOffset;
            Rectangle oldThumb = _hThumb; _horizontalOffset = v;
            CalculateHorizontalScrollbar(content, ClientSize.Width, ClientSize.Height);
            ApplyContentOffsets(false, oldHorizontalOffset, oldVerticalOffset);
            Invalidate(Rectangle.Union(oldThumb, _hThumb));
            if (!_updatingSmoothScroll) _targetHorizontalOffset = _horizontalOffset;

        }

        private void ResetScrollOffsets()
        {
            int oldHorizontalOffset = _horizontalOffset;
            int oldVerticalOffset = _verticalOffset;
            _verticalOffset = 0; _horizontalOffset = 0;
            StopSmoothScrolling();
            ApplyContentOffsets(true, oldHorizontalOffset, oldVerticalOffset);
        }

        private void ShowScrollbars() { if (!EnableCustomScrollbar) return; EnsureScrollTimers(); _shouldBeVisible = true; _fadeTimer!.Start(); _autoHideTimer!.Stop(); }
        private void StartAutoHide() { if (!EnableCustomScrollbar || !AutoHideScrollbar) return; EnsureScrollTimers(); _autoHideTimer!.Interval = AutoHideDelay; _autoHideTimer.Start(); }
        private void BeginFadeOut() { if (!EnableCustomScrollbar) return; EnsureScrollTimers(); _autoHideTimer!.Stop(); _shouldBeVisible = false; _fadeTimer!.Start(); }

        private void AnimateOpacityStep()
        {
            Timer? fadeTimer = _fadeTimer;
            if (fadeTimer == null) return;
            float target = ScrollbarOpacity;
            float step = target / Math.Max(1, (_shouldBeVisible ? FadeInTime : FadeOutTime) / (float)fadeTimer.Interval);
            float previous = _currentOpacity;
            if (_shouldBeVisible) { _currentOpacity = Math.Min(target, _currentOpacity + step); if (_currentOpacity >= target) fadeTimer.Stop(); }
            else { _currentOpacity = Math.Max(0f, _currentOpacity - step); if (_currentOpacity <= 0f) fadeTimer.Stop(); }
            if (Math.Abs(previous - _currentOpacity) > 0.005f) InvalidateScrollbarRegions();
        }

        private void AttachChildHandlers()
        {
            if (!EnableCustomScrollbar) return;
            foreach (Control c in Controls)
            {
                if (IsScrollOverlayControl(c)) continue;
                AttachScrollableChildHandlers(c);
            }
        }

        private void DetachChildHandlers()
        {
            foreach (Control c in Controls)
            {
                if (IsScrollOverlayControl(c)) continue;
                DetachScrollableChildHandlers(c);
            }
        }

        private void AttachScrollableChildHandlers(Control control)
        {
            if (!EnableCustomScrollbar) return;
            TrackChildBaseLocation(control);
            control.LocationChanged -= ChildChanged; control.LocationChanged += ChildChanged;
            control.SizeChanged -= ChildChanged; control.SizeChanged += ChildChanged;
            control.VisibleChanged -= ChildChanged; control.VisibleChanged += ChildChanged;

            control.MouseEnter -= ChildMouseEnter; control.MouseEnter += ChildMouseEnter;
            control.MouseLeave -= ChildMouseLeave; control.MouseLeave += ChildMouseLeave;
            TryEnableDoubleBuffering(control);
        }

        private void DetachScrollableChildHandlers(Control control)
        {
            control.LocationChanged -= ChildChanged;
            control.SizeChanged -= ChildChanged;
            control.VisibleChanged -= ChildChanged;

            control.MouseEnter -= ChildMouseEnter;
            control.MouseLeave -= ChildMouseLeave;
        }

        private void ChildMouseEnter(object sender, EventArgs e)
        {
            FocusIfPossible();
            ShowScrollbars();
        }

        private void ChildMouseLeave(object sender, EventArgs e)
        {
            StartAutoHide();
        }

        private void TryEnableDoubleBuffering(Control c)
        {
            try
            {
                var prop = c.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (prop != null) prop.SetValue(c, true, null);
            }
            catch { }
        }

        private void TrackChildBaseLocation(Control control)
        {
            if (!_childBaseLocations.ContainsKey(control)) _childBaseLocations[control] = control.Location;
        }

        private Point GetChildBaseLocation(Control control)
        {
            if (!_childBaseLocations.TryGetValue(control, out Point baseLocation))
            {
                baseLocation = control.Location;
                _childBaseLocations[control] = baseLocation;
            }

            return baseLocation;
        }

        private void ChildChanged(object sender, EventArgs e)
        {
            var ctrl = sender as Control;
            if (_applyingContentOffsets) return;
            if (ctrl != null && _verticalOffset == 0 && _horizontalOffset == 0) _childBaseLocations[ctrl] = ctrl.Location;
            UpdateScrollbars();
        }

        private Rectangle GetContentBounds()
        {
            int minX = 0, minY = 0, maxX = 0, maxY = 0; bool any = false;
            foreach (Control c in Controls)
            {
                if (IsScrollOverlayControl(c)) continue;
                if (!c.Visible) continue;
                any = true;
                Point basePos = GetChildBaseLocation(c);
                Size s = c.Size;
                minX = Math.Min(minX, basePos.X); minY = Math.Min(minY, basePos.Y);
                maxX = Math.Max(maxX, basePos.X + s.Width); maxY = Math.Max(maxY, basePos.Y + s.Height);
            }
            if (!any) return new Rectangle(Point.Empty, Size);
            int w = (maxX - minX) + _autoScrollMargin.Width; int h = (maxY - minY) + _autoScrollMargin.Height;
            return new Rectangle(minX, minY, Math.Max(0, w), Math.Max(0, h));
        }

        private void ApplyContentOffsets(bool force = false, int oldHorizontalOffset = int.MinValue, int oldVerticalOffset = int.MinValue)
        {
            if (!force && !EnableCustomScrollbar) return;
            bool isScrollMove = !force && oldHorizontalOffset != int.MinValue && oldVerticalOffset != int.MinValue;

            if (isScrollMove)
            {
                int dx = oldHorizontalOffset - _horizontalOffset;
                int dy = oldVerticalOffset - _verticalOffset;
                if (TryScrollContentWindow(dx, dy))
                {
                    SyncChildControlLocations();
                    InvalidateScrollbarRegions();
                    ScheduleScrolledChildRefresh();
                    return;
                }
            }

            var moves = new List<KeyValuePair<Control, Point>>();
            foreach (Control c in Controls)
            {
                if (IsScrollOverlayControl(c)) continue;
                if (!c.Visible) continue;
                Point basePos = GetChildBaseLocation(c);
                Point newLoc = new Point(basePos.X - _horizontalOffset, basePos.Y - _verticalOffset);
                if (c.Location != newLoc) moves.Add(new KeyValuePair<Control, Point>(c, newLoc));
            }

            if (moves.Count == 0) return;

            SuspendLayout();
            _applyingContentOffsets = true;
            try
            {
                MoveChildControls(moves);
            }
            finally
            {
                _applyingContentOffsets = false;
                ResumeLayout(false);
                // Repaint the whole panel (including children) so faux-transparent controls
                // don't leave ghosts of their previous position after a scroll move.
                Invalidate(true);
                if (isScrollMove) InvalidateScrollbarRegions();
                ScheduleScrolledChildRefresh();
            }
        }

        private void SyncChildControlLocations()
        {
            var moves = GetPendingChildMoves();
            if (moves.Count == 0) return;

            SuspendLayout();
            _applyingContentOffsets = true;
            try
            {
                MoveChildControls(moves);
            }
            finally
            {
                _applyingContentOffsets = false;
                ResumeLayout(false);
            }
        }

        private List<KeyValuePair<Control, Point>> GetPendingChildMoves()
        {
            var moves = new List<KeyValuePair<Control, Point>>();
            foreach (Control c in Controls)
            {
                if (IsScrollOverlayControl(c)) continue;
                if (!c.Visible) continue;
                Point basePos = GetChildBaseLocation(c);
                Point newLoc = new Point(basePos.X - _horizontalOffset, basePos.Y - _verticalOffset);
                if (c.Location != newLoc) moves.Add(new KeyValuePair<Control, Point>(c, newLoc));
            }

            return moves;
        }

        private bool TryScrollContentWindow(int dx, int dy)
        {
            // Disabled: ScrollWindowEx bit-blits the viewport, but controls with faux
            // transparency keep showing the previously-scrolled pixels in their transparent
            // areas, producing torn/ghosted text. Always fall back to the move + full-repaint
            // path, which repaints cleanly.
            return false;
#pragma warning disable CS0162
            if (!IsHandleCreated || IsDisposed || (dx == 0 && dy == 0)) return false;
            if (HasNestedChildControls()) return false;

            Rectangle scrollRectangle = GetContentViewportRectangle();
            if (scrollRectangle.Width <= 0 || scrollRectangle.Height <= 0) return false;

            RECT scrollRect = new RECT(scrollRectangle);
            RECT clipRect = new RECT(scrollRectangle);

            _applyingContentOffsets = true;
            try
            {
                int result = ScrollWindowEx(Handle, dx, dy, ref scrollRect, ref clipRect, IntPtr.Zero, IntPtr.Zero, SW_SCROLLCHILDREN | SW_INVALIDATE);
                return result != 0;
            }
            finally
            {
                _applyingContentOffsets = false;
            }
#pragma warning restore CS0162
        }

        private bool HasNestedChildControls()
        {
            foreach (Control c in Controls)
            {
                if (IsScrollOverlayControl(c)) continue;
                if (c.Visible && c.Controls.Count > 0) return true;
            }

            return false;
        }

        private Rectangle GetContentViewportRectangle()
        {
            Rectangle rect = ClientRectangle;
            if (_vVisible && !_vTrack.IsEmpty) rect.Width = Math.Max(0, _vTrack.Left - rect.Left);
            if (_hVisible && !_hTrack.IsEmpty) rect.Height = Math.Max(0, _hTrack.Top - rect.Top);
            return rect;
        }

        private void MoveChildControls(List<KeyValuePair<Control, Point>> moves)
        {
            if (TryMoveChildControlsNative(moves)) return;

            foreach (var move in moves)
            {
                Control control = move.Key;
                Point location = move.Value;
                if (!control.IsDisposed && control.Location != location) control.Location = location;
            }
        }

        private bool TryMoveChildControlsNative(List<KeyValuePair<Control, Point>> moves)
        {
            if (!IsHandleCreated || moves.Count == 0) return false;

            IntPtr deferHandle = BeginDeferWindowPos(moves.Count);
            if (deferHandle == IntPtr.Zero) return false;

            bool movedWindow = false;
            foreach (var move in moves)
            {
                Control control = move.Key;
                Point location = move.Value;

                if (control.IsDisposed) continue;
                if (!control.IsHandleCreated)
                {
                    control.Location = location;
                    continue;
                }

                IntPtr nextHandle = DeferWindowPos(deferHandle, control.Handle, IntPtr.Zero, location.X, location.Y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_NOOWNERZORDER);
                if (nextHandle == IntPtr.Zero) return false;

                deferHandle = nextHandle;
                movedWindow = true;
            }

            return movedWindow && EndDeferWindowPos(deferHandle);
        }

        private void ScheduleScrolledChildRefresh()
        {
            if (!EnableCustomScrollbar || IsInDesignMode()) return;
            EnsureScrollTimers();
            _isScrollingContent = true;
            _scrollRefreshTimer!.Stop();
            _scrollRefreshTimer.Start();
        }

        private void RefreshScrolledChildren()
        {
            if (IsDisposed || !EnableCustomScrollbar) return;
            _isScrollingContent = false;

            foreach (Control c in Controls)
            {
                RefreshScrolledControl(c);
            }

            Invalidate(true);
        }

        private void RefreshScrolledControl(Control control)
        {
            if (IsScrollOverlayControl(control)) return;
            if (!control.Visible || control.IsDisposed) return;

            if (control is VortexPanel panel) panel.InvalidateParentSnapshot();
            if (control is VortexLabel label) label.ResetTransparentBackgroundCache();

            foreach (Control child in control.Controls)
            {
                RefreshScrolledControl(child);
            }

            control.Invalidate(true);
        }

        private void InvalidateGradientCache() { _cachedGradientBitmap?.Dispose(); _cachedGradientBitmap = null; }

        // 8x8 Bayer ordered-dither matrix, normalized to [0,1). Breaks up 8-bit banding
        // on low-contrast dark gradients without adding visible random noise.
        private static readonly float[] _bayer8 = BuildBayer8();
        private static float[] BuildBayer8()
        {
            int[] m =
            {
                 0, 32,  8, 40,  2, 34, 10, 42,
                48, 16, 56, 24, 50, 18, 58, 26,
                12, 44,  4, 36, 14, 46,  6, 38,
                60, 28, 52, 20, 62, 30, 54, 22,
                 3, 35, 11, 43,  1, 33,  9, 41,
                51, 19, 59, 27, 49, 17, 57, 25,
                15, 47,  7, 39, 13, 45,  5, 37,
                63, 31, 55, 23, 61, 29, 53, 21
            };
            var r = new float[64];
            for (int i = 0; i < 64; i++) r[i] = (m[i] + 0.5f) / 64f;
            return r;
        }

        private static byte ToByteDithered(float value, float dither)
        {
            int v = (int)(value + dither);
            return v < 0 ? (byte)0 : v > 255 ? (byte)255 : (byte)v;
        }

        private Bitmap GenerateAdvancedGradientBitmap(Size size)
        {
            Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                float tlA = GradientTopLeft.A, tlR = GradientTopLeft.R, tlG = GradientTopLeft.G, tlB = GradientTopLeft.B;
                float trA = GradientTopRight.A, trR = GradientTopRight.R, trG = GradientTopRight.G, trB = GradientTopRight.B;
                float blA = GradientBottomLeft.A, blR = GradientBottomLeft.R, blG = GradientBottomLeft.G, blB = GradientBottomLeft.B;
                float brA = GradientBottomRight.A, brR = GradientBottomRight.R, brG = GradientBottomRight.G, brB = GradientBottomRight.B;
                float heightMinusOne = size.Height > 1 ? size.Height - 1 : 1f;
                float widthMinusOne = size.Width > 1 ? size.Width - 1 : 1f;

                GradientBlendMode mode = GradientBlendMode;
                bool isRadial = mode == GradientBlendMode.Radial;
                float invSmoothness = 1f / GradientSmoothness;

                // Precompute the per-axis interpolation factors once. For every mode except
                // Radial, sx depends only on x and sy only on y, so computing them inside the
                // inner loop repeated the same SmoothStep/Pow work width*height times.
                float[] sxArr = null, syArr = null;
                if (!isRadial)
                {
                    sxArr = new float[size.Width];
                    syArr = new float[size.Height];
                    for (int x = 0; x < size.Width; x++)
                    {
                        float vx = x / widthMinusOne;
                        sxArr[x] = mode == GradientBlendMode.Smooth ? SmoothStep(vx)
                                 : mode == GradientBlendMode.Soft ? (float)Math.Pow(vx, invSmoothness)
                                 : vx;
                    }
                    for (int y = 0; y < size.Height; y++)
                    {
                        float vy = y / heightMinusOne;
                        syArr[y] = mode == GradientBlendMode.Smooth ? SmoothStep(vy)
                                 : mode == GradientBlendMode.Soft ? (float)Math.Pow(vy, invSmoothness)
                                 : vy;
                    }
                }

                for (int y = 0; y < size.Height; y++)
                {
                    float vy = y / heightMinusOne;
                    int rowBase = y * stride;
                    int ditherRow = (y & 7) << 3;
                    for (int x = 0; x < size.Width; x++)
                    {
                        float sx, sy;
                        if (isRadial)
                        {
                            float vx = x / widthMinusOne;
                            float dx = vx - 0.5f, dy = vy - 0.5f;
                            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                            float radialFactor = SmoothStep(Math.Min(1f, dist * 1.414f));
                            sx = Clamp(vx + dx * radialFactor * 0.3f, 0f, 1f);
                            sy = Clamp(vy + dy * radialFactor * 0.3f, 0f, 1f);
                        }
                        else
                        {
                            sx = sxArr[x];
                            sy = syArr[y];
                        }

                        float ivx = 1f - sx, ivy = 1f - sy;
                        float w1 = ivx * ivy, w2 = sx * ivy, w3 = ivx * sy, w4 = sx * sy;
                        float d = _bayer8[ditherRow + (x & 7)];
                        byte A = ToByteDithered(tlA * w1 + trA * w2 + blA * w3 + brA * w4, d);
                        byte R = ToByteDithered(tlR * w1 + trR * w2 + blR * w3 + brR * w4, d);
                        byte G = ToByteDithered(tlG * w1 + trG * w2 + blG * w3 + brG * w4, d);
                        byte B = ToByteDithered(tlB * w1 + trB * w2 + blB * w3 + brB * w4, d);
                        int idx = rowBase + x * 4;
                        ptr[idx] = B; ptr[idx + 1] = G; ptr[idx + 2] = R; ptr[idx + 3] = A;
                    }
                }
            }
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        private float SmoothStep(float t)
        {
            t = Clamp(t, 0f, 1f);
            float s = GradientSmoothness;
            if (s <= 1f) return t;

            // Continuous smoothness: apply N full smootherstep passes, then blend the
            // fractional pass so values like 2.5 land halfway between 2 and 3 passes
            // instead of snapping to integers.
            int full = (int)s;
            float frac = s - full;
            float result = t;
            for (int i = 1; i < full; i++) result = result * result * (3f - 2f * result);
            if (frac > 0f)
            {
                float next = result * result * (3f - 2f * result);
                result += (next - result) * frac;
            }
            return result;
        }

        private GraphicsPath CreateRoundedRectanglePath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0f) { path.AddRectangle(rect); path.CloseFigure(); return path; }
            float diameter = radius * 2f;
            SizeF size = new SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(rect.Location, size);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter; path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter; path.AddArc(arc, 0, 90);
            arc.X = rect.Left; path.AddArc(arc, 90, 90);
            path.CloseFigure(); return path;
        }

        private static float Clamp(float v, float min, float max) => Math.Min(max, Math.Max(min, v));
        private void FocusIfPossible() { if (CanFocus && !Focused && !ContainsFocus) Focus(); }
        private void InvalidateRenderCache() { _cachedRenderBitmap?.Dispose(); _cachedRenderBitmap = null; _cachedRenderSize = Size.Empty; }

        private Bitmap GetPatternBitmap(int elementSize, int spacing, PatternStyle style, Color color, float opacity)
        {
            // compute tile size (use spacing as base but ensure it accounts for element size to avoid clipping)
            int tileW = Math.Max(16, spacing);
            int tileH = Math.Max(16, spacing);
            Size desired = new Size(tileW, tileH);

            // if cache matches exactly return
            if (_cachedPatternBitmap != null &&
                _cachedPatternSize == desired &&
                _cachedPatternStyle == style &&
                _cachedPatternElementSize == elementSize &&
                _cachedPatternColor.ToArgb() == color.ToArgb() &&
                Math.Abs(_cachedPatternOpacity - opacity) < 0.001f)
            {
                return _cachedPatternBitmap;
            }

            // dispose old
            _cachedPatternBitmap?.Dispose();
            _cachedPatternBitmap = null;

            // Use premultiplied pixel format to match cachedRender (prevents over-bright blending)
            var bmp = new Bitmap(desired.Width, desired.Height, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                // For tiled patterns prefer crisp pixels (no AA inside tile)
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.None;
                g.CompositingMode = CompositingMode.SourceOver;
                g.Clear(Color.Transparent);

                // compute effective alpha taking into account original color alpha and requested opacity
                int effectiveAlpha = (int)Math.Round((color.A / 255.0f) * Clamp(opacity, 0f, 1f) * 255f);
                effectiveAlpha = Math.Max(0, Math.Min(255, effectiveAlpha));

                // create color with that alpha
                Color brushColor = Color.FromArgb(effectiveAlpha, color.R, color.G, color.B);

                using (var sb = new SolidBrush(brushColor))
                {
                    switch (style)
                    {
                        case PatternStyle.Dots:
                            {
                                int cx = desired.Width / 2;
                                int cy = desired.Height / 2;
                                g.FillEllipse(sb, cx - elementSize / 2f, cy - elementSize / 2f, elementSize, elementSize);
                            }
                            break;

                        case PatternStyle.Grid:
                            {
                                float half = elementSize / 2f;
                                g.FillRectangle(sb, 0, desired.Height / 2f - half, desired.Width, elementSize);
                                g.FillRectangle(sb, desired.Width / 2f - half, 0, elementSize, desired.Height);
                            }
                            break;

                        case PatternStyle.Diagonal:
                            {
                                using (var pen = new Pen(brushColor, elementSize))
                                {
                                    pen.StartCap = LineCap.Square;
                                    pen.EndCap = LineCap.Square;
                                    float ext = Math.Max(desired.Width, desired.Height);
                                    g.DrawLine(pen, -ext, desired.Height + ext, desired.Width + ext, -ext);
                                }
                            }
                            break;

                        case PatternStyle.Crosses:
                            {
                                int cx = desired.Width / 2;
                                int cy = desired.Height / 2;
                                g.FillRectangle(sb, cx - elementSize, cy - elementSize / 2f, elementSize * 2, elementSize);
                                g.FillRectangle(sb, cx - elementSize / 2f, cy - elementSize, elementSize, elementSize * 2);
                            }
                            break;

                        case PatternStyle.Hexagons:
                            {
                                float unit = elementSize * 2f;
                                float cx = desired.Width / 2f;
                                float cy = desired.Height / 2f;
                                PointF[] hex = new PointF[6];
                                for (int i = 0; i < 6; i++)
                                {
                                    double a = Math.PI / 3 * i;
                                    hex[i] = new PointF(cx + unit * 0.6f * (float)Math.Cos(a), cy + unit * 0.6f * (float)Math.Sin(a));
                                }
                                g.FillPolygon(sb, hex);
                            }
                            break;

                        case PatternStyle.Stars:
                            {
                                float cx = desired.Width / 2f;
                                float cy = desired.Height / 2f;
                                PointF[] star = new PointF[10];
                                for (int k = 0; k < 10; k++)
                                {
                                    float r = (k % 2 == 0) ? elementSize * 2f : elementSize * 0.8f;
                                    double a = k * Math.PI / 5.0 - Math.PI / 2.0;
                                    star[k] = new PointF(cx + r * (float)Math.Cos(a), cy + r * (float)Math.Sin(a));
                                }
                                g.FillPolygon(sb, star);
                            }
                            break;

                        default:
                            {
                                int cx = desired.Width / 2;
                                int cy = desired.Height / 2;
                                g.FillEllipse(sb, cx - elementSize / 2f, cy - elementSize / 2f, elementSize, elementSize);
                            }
                            break;
                    }
                }
            }

            // cache metadata
            _cachedPatternBitmap = bmp;
            _cachedPatternSize = desired;
            _cachedPatternStyle = style;
            _cachedPatternElementSize = elementSize;
            _cachedPatternColor = color;
            _cachedPatternOpacity = opacity;

            return _cachedPatternBitmap;
        }
    }
}
