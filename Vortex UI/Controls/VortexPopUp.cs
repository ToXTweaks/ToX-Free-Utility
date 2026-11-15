using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public enum PopupPosition
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft
    }

    public enum PopupMode
    {
        InForm,      // Popup appears inside parent form
    }

    public class VortexPopup : Control
    {
        // Basic properties
        private int _cornerCutSize = 10;
        private bool _showBorder = true;
        private Color _borderColor = Color.Black;
        private float _borderWidth = 2f;
        private Color _backgroundColor = Color.FromArgb(255, 140, 0); // Orange color
        private bool _showProgressBar = true;
        private int _autoCloseDelay = 3000; // 3 seconds default

        // Position properties
        private PopupPosition _position = PopupPosition.TopRight;
        private int _offset = 20; // Distance from screen edge
        private Size _parentSize = new Size(800, 600); // Default screen size
        private PopupMode _popupMode = PopupMode.InForm; // Default to show inside form
        private Form _overlayForm = null; // Form used when in overlay mode

        // Animation properties
        private bool _isShowing = false;
        private float _showAnimationProgress = 0f;
        private float _hideAnimationProgress = 0f;
        private int _showAnimationDuration = 250;
        private int _hideAnimationDuration = 200;

        // Progress bar properties
        private bool _isCountingDown = false;
        private float _progressValue = 0f;
        private CancellationTokenSource _progressCts;
        private CancellationTokenSource _autoCloseCts;

        // Animation cancellation tokens
        private CancellationTokenSource _showAnimationCts;
        private CancellationTokenSource _hideAnimationCts;

        // Keep track of the form (even when inside panels)
        private Form _parentForm = null;

        #region Properties

        [Category("Appearance")]
        [Description("Size of the cut corner in pixels")]
        public int CornerCutSize
        {
            get { return _cornerCutSize; }
            set
            {
                _cornerCutSize = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines whether to show border around the popup")]
        public bool ShowBorder
        {
            get { return _showBorder; }
            set
            {
                _showBorder = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color of the popup border")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Width of the popup border")]
        public float BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                _borderWidth = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Background color of the popup")]
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                Invalidate();
            }
        }

        [Category("Position")]
        [Description("Position of the popup on the screen")]
        public PopupPosition Position
        {
            get { return _position; }
            set
            {
                _position = value;
                UpdatePosition();
                Invalidate();
            }
        }

        [Category("Position")]
        [Description("Offset from screen edge")]
        public int Offset
        {
            get { return _offset; }
            set
            {
                _offset = value;
                UpdatePosition();
                Invalidate();
            }
        }

        [Category("Parent")]
        [Description("Size of parent container or screen")]
        public Size ParentSize
        {
            get { return _parentSize; }
            set
            {
                _parentSize = value;
                UpdatePosition();
                Invalidate();
            }
        }

        [Category("Progress")]
        [Description("Show progress indicator")]
        public bool ShowProgressBar
        {
            get { return _showProgressBar; }
            set
            {
                _showProgressBar = value;
                Invalidate();
            }
        }

        [Category("Animation")]
        [Description("Duration of show animation in milliseconds")]
        public int ShowAnimationDuration
        {
            get { return _showAnimationDuration; }
            set
            {
                _showAnimationDuration = value;
            }
        }

        [Category("Animation")]
        [Description("Duration of hide animation in milliseconds")]
        public int HideAnimationDuration
        {
            get { return _hideAnimationDuration; }
            set
            {
                _hideAnimationDuration = value;
            }
        }

        [Category("Behavior")]
        [Description("Auto-close delay in milliseconds (0 to disable)")]
        public int AutoCloseDelay
        {
            get { return _autoCloseDelay; }
            set
            {
                _autoCloseDelay = Math.Max(0, value);
            }
        }

        [Category("Behavior")]
        [Description("Determines whether the popup appears inside the form or as a separate overlay")]
        public PopupMode PopupMode
        {
            get { return _popupMode; }
            set
            {
                if (_popupMode != value)
                {
                    _popupMode = value;

                    // Hide popup first if it's visible
                    if (_isShowing)
                    {
                        _ = HideAsync();
                    }

                }
            }
        }

        // Read-only property to check if the popup is visible and fully shown
        [Browsable(false)]
        public bool IsShown
        {
            get { return Visible && _showAnimationProgress >= 1.0f && _hideAnimationProgress <= 0f; }
        }

        // Event that fires when popup fully closes
        public event EventHandler PopupClosed;

        #endregion

        public VortexPopup()
        {
            // Set defaults
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(300, 60);
            Visible = false;
            ForeColor = Color.Black;

            try
            {
                Font = new Font("Bahnschrift", 10, FontStyle.Bold);
            }
            catch
            {
                Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }
        }

        // Helper to find the parent form at any level
        private Form FindParentForm()
        {
            Control current = this;
            while (current != null)
            {
                if (current is Form form)
                    return form;
                current = current.Parent;
            }
            return null;
        }

        // Override OnHandleCreated to properly subscribe to parent resize
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Find and track the parent form
            _parentForm = FindParentForm();

            if (_parentForm != null)
            {
                // Subscribe to form resize event
                _parentForm.Resize += ParentForm_Resize;

                // Update position based on actual form size
                _parentSize = _parentForm.ClientSize;
                UpdatePosition();
            }
            else if (Parent != null)
            {
                // Fallback to direct parent if no form found
                Parent.Resize += Parent_Resize;
                _parentSize = Parent.ClientSize;
                UpdatePosition();
            }
        }

        // Handle parent form resize
        private void ParentForm_Resize(object sender, EventArgs e)
        {
            if (_parentForm != null)
            {
                _parentSize = _parentForm.ClientSize;
                UpdatePosition();
            }
        }

        // Handle direct parent resize
        private void Parent_Resize(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                _parentSize = Parent.ClientSize;
                UpdatePosition();
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            // Unsubscribe from old parent form
            if (_parentForm != null)
            {
                _parentForm.Resize -= ParentForm_Resize;
                _parentForm = null;
            }

            // Unsubscribe from old parent
            if (e is EventArgsWithOldParent args && args.OldParent != null)
            {
                args.OldParent.Resize -= Parent_Resize;
            }

            // Find and subscribe to new parent form
            _parentForm = FindParentForm();
            if (_parentForm != null)
            {
                _parentForm.Resize += ParentForm_Resize;
                _parentSize = _parentForm.ClientSize;
            }
            else if (Parent != null)
            {
                // Fallback to direct parent if no form found
                Parent.Resize += Parent_Resize;
                _parentSize = Parent.ClientSize;
            }

            UpdatePosition();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelAnimations();

                // Unsubscribe from parent form events
                if (_parentForm != null)
                {
                    _parentForm.Resize -= ParentForm_Resize;
                    _parentForm = null;
                }

                // Unsubscribe from direct parent events
                if (Parent != null)
                {
                    Parent.Resize -= Parent_Resize;
                }
            }
            base.Dispose(disposing);
        }

        // Calculate absolute position relative to the form
        private Point GetAbsolutePosition(Control control)
        {
            if (control == null) return Point.Empty;
            Point position = control.Location;
            Control parent = control.Parent;

            while (parent != null && !(parent is Form))
            {
                position.Offset(parent.Location);
                parent = parent.Parent;
            }

            return position;
        }

        // Cancel all ongoing animations
        private void CancelAnimations()
        {
            _showAnimationCts?.Cancel();
            _showAnimationCts?.Dispose();
            _showAnimationCts = null;

            _hideAnimationCts?.Cancel();
            _hideAnimationCts?.Dispose();
            _hideAnimationCts = null;

            _progressCts?.Cancel();
            _progressCts?.Dispose();
            _progressCts = null;

            _autoCloseCts?.Cancel();
            _autoCloseCts?.Dispose();
            _autoCloseCts = null;
        }

        #region Animation Methods

        // Show popup with animation
        public async Task ShowAsync()
        {
            // Cancel any ongoing animations
            CancelAnimations();

            // Reset animation progress
            _showAnimationProgress = 0f;
            _hideAnimationProgress = 0f;
            _isShowing = true;
            _progressValue = 0f;

            // Create new cancellation tokens
            _showAnimationCts = new CancellationTokenSource();

            // Handle overlay mode
            if (_popupMode == PopupMode.InForm)
            {
                // Make sure position is updated in case parent hierarchy has changed
                UpdatePosition();

                // Make visible in the form
                Visible = true;

                // Bring to front
                BringToFront();

                // Start show animation
                await AnimateShowAsync(_showAnimationCts.Token);
            }

            // Start auto-close timer if enabled
            if (_autoCloseDelay > 0)
            {
                if (_showProgressBar)
                {
                    // Use progress bar countdown
                    _ = StartCountdownAsync(_autoCloseDelay);
                }
                else
                {
                    // Use simple timer for auto-close
                    _ = StartAutoCloseTimerAsync(_autoCloseDelay);
                }
            }
        }

        // Simple auto-close timer when progress bar is disabled
        private async Task StartAutoCloseTimerAsync(int delay)
        {
            try
            {
                _autoCloseCts = new CancellationTokenSource();
                await Task.Delay(delay, _autoCloseCts.Token);
                // Auto close after delay
                await HideAsync();
            }
            catch (TaskCanceledException)
            {
                // Timer was canceled
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-close timer error: {ex.Message}");
            }
        }

        // Hide popup with animation
        public async Task HideAsync()
        {
            // Cancel any ongoing animations
            CancelAnimations();

            // Create new cancellation tokens
            _hideAnimationCts = new CancellationTokenSource();

            // Start hide animation
            await AnimateHideAsync(_hideAnimationCts.Token);

            // Hide when done
            if (_popupMode == PopupMode.InForm)
            {
                Visible = false;
            }

            _isShowing = false;

            // Raise closed event
            OnPopupClosed(EventArgs.Empty);
        }

        // Animation for showing
        private async Task AnimateShowAsync(CancellationToken cancelToken)
        {
            try
            {
                float targetValue = 1.0f;
                float stepSize = 0.1f;
                int stepDuration = Math.Max(10, _showAnimationDuration / 10);

                while (!cancelToken.IsCancellationRequested && _showAnimationProgress < targetValue)
                {
                    _showAnimationProgress = Math.Min(_showAnimationProgress + stepSize, targetValue);

                    if (IsHandleCreated && !IsDisposed)
                    {
                        try
                        {
                            BeginInvoke(new Action(() => Invalidate()));
                        }
                        catch
                        {
                            // Handle possible cross-thread issues 
                            Invalidate();
                        }
                    }
                    else
                    {
                        break;
                    }

                    await Task.Delay(stepDuration, cancelToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Animation was canceled, this is expected behavior
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Show animation error: {ex.Message}");
            }
        }

        // Animation for hiding
        private async Task AnimateHideAsync(CancellationToken cancelToken)
        {
            try
            {
                float targetValue = 1.0f;
                float stepSize = 0.1f;
                int stepDuration = Math.Max(10, _hideAnimationDuration / 10);

                _hideAnimationProgress = 0f;

                while (!cancelToken.IsCancellationRequested && _hideAnimationProgress < targetValue)
                {
                    _hideAnimationProgress = Math.Min(_hideAnimationProgress + stepSize, targetValue);

                    if (IsHandleCreated && !IsDisposed)
                    {
                        try
                        {
                            BeginInvoke(new Action(() => Invalidate()));
                        }
                        catch
                        {
                            // Handle possible cross-thread issues
                            Invalidate();
                        }
                    }
                    else
                    {
                        break;
                    }

                    await Task.Delay(stepDuration, cancelToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Animation was canceled, this is expected behavior
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hide animation error: {ex.Message}");
            }
        }

        // Countdown timer with progress bar
        public async Task StartCountdownAsync(int duration)
        {
            if (_isCountingDown)
                return;

            _isCountingDown = true;
            _progressValue = 0f;

            // Cancel previous countdown if any
            _progressCts?.Cancel();
            _progressCts?.Dispose();
            _progressCts = new CancellationTokenSource();

            try
            {
                int steps = 100; // 100 steps for smoother animation
                int stepDuration = duration / steps;
                float stepSize = 1.0f / steps;

                for (int i = 0; i < steps; i++)
                {
                    if (_progressCts.Token.IsCancellationRequested)
                        break;

                    _progressValue = (i + 1) * stepSize;

                    if (IsHandleCreated && !IsDisposed)
                    {
                        try
                        {
                            BeginInvoke(new Action(() => Invalidate()));
                        }
                        catch
                        {
                            // Handle possible cross-thread issues
                            Invalidate();
                        }
                    }
                    else
                    {
                        break;
                    }

                    await Task.Delay(stepDuration, _progressCts.Token);
                }

                // Complete countdown
                _progressValue = 1.0f;

                Invalidate();

                // Short delay to show the completed progress
                await Task.Delay(100, _progressCts.Token);

                // Hide the popup when countdown completes
                if (!_progressCts.Token.IsCancellationRequested)
                {
                    _isCountingDown = false;
                    _ = HideAsync();
                }
            }
            catch (TaskCanceledException)
            {
                // Countdown was canceled
                _isCountingDown = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Countdown error: {ex.Message}");
                _isCountingDown = false;
            }
        }

        // Helper method to interpolate between values
        private float Interpolate(float start, float end, float progress)
        {
            return start + (end - start) * progress;
        }

        // Helper method to interpolate between colors
        private Color InterpolateColor(Color start, Color end, float ratio)
        {
            int r = (int)(start.R + (end.R - start.R) * ratio);
            int g = (int)(start.G + (end.G - start.G) * ratio);
            int b = (int)(start.B + (end.B - start.B) * ratio);
            return Color.FromArgb(r, g, b);
        }

        #endregion

        #region Event Handlers

        // Trigger the popup closed event
        protected virtual void OnPopupClosed(EventArgs e)
        {
            PopupClosed?.Invoke(this, e);
        }

        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            PaintPopup(e.Graphics);
        }

        // Method that handles the actual painting
        internal void PaintPopup(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Calculate animation effects
            float showProgress = _showAnimationProgress;
            float hideProgress = _hideAnimationProgress;

            // Create popup path with the special shape
            GraphicsPath path = CreatePopupPath(showProgress, hideProgress);

            // Set control shape in InForm mode only
            if (_popupMode == PopupMode.InForm)
            {
                using (Region region = new Region(path))
                {
                    Region = region;
                }
            }

            // Create semi-transparent background to create shadow effect
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(50, Color.Black)))
            {
                GraphicsPath shadowPath = CreatePopupPath(showProgress, hideProgress, 3);
                g.FillPath(shadowBrush, shadowPath);
            }

            // Draw popup background
            using (SolidBrush brush = new SolidBrush(_backgroundColor))
            {
                g.FillPath(brush, path);
            }

            // Draw border if enabled
            if (_showBorder)
            {
                using (Pen borderPen = new Pen(_borderColor, _borderWidth))
                {
                    borderPen.LineJoin = LineJoin.Round;
                    g.DrawPath(borderPen, path);
                }
            }

            // Calculate text rect (left side)
            Rectangle textRect = new Rectangle(
                10,
                0,
                Width - 70, // Leave room for progress circle
                Height
            );

            // Draw text
            TextRenderer.DrawText(
                g,
                Text,
                Font,
                textRect,
                ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left
            );

            // Draw progress indicator if enabled
            if (_showProgressBar)
            {
                DrawProgressCircle(g);
            }
        }

        // Draw circular progress indicator (right side)
        private void DrawProgressCircle(Graphics g)
        {
            // Calculate circle parameters
            int circleDiameter = Height - 20;
            int right = Width - 10;
            int centerX = right - circleDiameter / 2;
            int centerY = Height / 2;
            int radius = circleDiameter / 2;
            int thickness = Math.Max(2, circleDiameter / 10);

            Rectangle circleRect = new Rectangle(
                centerX - radius,
                centerY - radius,
                radius * 2,
                radius * 2);

            // Draw background circle
            using (Pen backgroundPen = new Pen(Color.FromArgb(80, ForeColor), thickness))
            {
                backgroundPen.StartCap = LineCap.Round;
                backgroundPen.EndCap = LineCap.Round;
                g.DrawEllipse(backgroundPen, circleRect);
            }

            // Draw progress arc
            float sweepAngle = 360 * _progressValue;
            using (Pen progressPen = new Pen(ForeColor, thickness))
            {
                progressPen.StartCap = LineCap.Round;
                progressPen.EndCap = LineCap.Round;
                g.DrawArc(progressPen, circleRect, -90, sweepAngle);
            }
        }

        // Create the path for the popup shape
        internal GraphicsPath CreatePopupPath(float showProgress = 1.0f, float hideProgress = 0.0f, float shadowOffset = 0)
        {
            GraphicsPath path = new GraphicsPath();

            // Calculate scale/position based on show/hide animation
            float scaleX, scaleY;
            float offsetX = 0, offsetY = 0;

            // Calculate show animation effect (grow from edge)
            if (showProgress < 1.0f)
            {
                switch (_position)
                {
                    case PopupPosition.TopLeft:
                    case PopupPosition.BottomLeft:
                        scaleX = showProgress;
                        scaleY = showProgress;
                        break;
                    case PopupPosition.TopRight:
                    case PopupPosition.BottomRight:
                        scaleX = showProgress;
                        scaleY = showProgress;
                        offsetX = Width * (1 - showProgress);
                        break;
                    default:
                        scaleX = scaleY = showProgress;
                        break;
                }
            }
            // Calculate hide animation effect (slide off screen)
            else if (hideProgress > 0.0f)
            {
                scaleX = 1.0f;
                scaleY = 1.0f;

                switch (_position)
                {
                    case PopupPosition.TopLeft:
                    case PopupPosition.BottomLeft:
                        offsetX = -Width * hideProgress;
                        break;
                    case PopupPosition.TopRight:
                    case PopupPosition.BottomRight:
                        offsetX = Width * hideProgress;
                        break;
                    default:
                        offsetX = 0;
                        break;
                }
            }
            else
            {
                scaleX = scaleY = 1.0f;
            }

            // Create rectangle based on animation state
            RectangleF rect = new RectangleF(
                offsetX + shadowOffset,
                offsetY + shadowOffset,
                (Width - shadowOffset * 2) * scaleX,
                (Height - shadowOffset * 2) * scaleY
            );

            // Scale cut size accordingly
            float scaledCutSize = _cornerCutSize * Math.Min(scaleX, scaleY);

            // Determine points based on properties
            PointF[] points = new PointF[]
            {
                new PointF(rect.Left, rect.Top),                             // Top left
                new PointF(rect.Right, rect.Top),                            // Top right
                new PointF(rect.Right, rect.Bottom - scaledCutSize),         // Right before cut
                new PointF(rect.Right - scaledCutSize, rect.Bottom),         // Bottom after cut
                new PointF(rect.Left, rect.Bottom)                           // Bottom left
            };

            // Create path
            path.AddPolygon(points);

            return path;
        }

        // Update position based on selected corner
        private void UpdatePosition()
        {
            // Calculate position relative to the form's client area
            Point formLocation = Point.Empty;
            Size formSize = _parentForm?.ClientSize ?? _parentSize;

            if (Parent != null && _parentForm != null && Parent != _parentForm)
            {
                // Get the parent's position relative to the form
                formLocation = GetAbsolutePosition(Parent);
            }

            // Calculate position based on selected corner
            Point position = new Point(0, 0);

            switch (_position)
            {
                case PopupPosition.TopLeft:
                    position = new Point(_offset, _offset);
                    break;
                case PopupPosition.TopRight:
                    position = new Point(formSize.Width - Width - _offset - formLocation.X, _offset);
                    break;
                case PopupPosition.BottomRight:
                    position = new Point(formSize.Width - Width - _offset - formLocation.X, formSize.Height - Height - _offset - formLocation.Y);
                    break;
                case PopupPosition.BottomLeft:
                    position = new Point(_offset, formSize.Height - Height - _offset - formLocation.Y);
                    break;
            }

            // Apply the new location
            if (IsHandleCreated && !IsDisposed && Parent != null)
            {
                try
                {
                    BeginInvoke(new Action(() => { Location = position; }));
                }
                catch
                {
                    // Handle possible cross-thread issues
                    Location = position;
                }
            }
            else
            {
                Location = position;
            }
        }

        // EventArgs with old parent reference
        private class EventArgsWithOldParent : EventArgs
        {
            public Control OldParent { get; }

            public EventArgsWithOldParent(Control oldParent)
            {
                OldParent = oldParent;
            }
        }
    }
}