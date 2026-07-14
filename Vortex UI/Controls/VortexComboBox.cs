using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Vortex.Ui
{
    public enum DropdownArrowStyle
    {
        Chevron,
        Triangle,
        Arrow,
        Custom
    }

    [ToolboxItem(true)]
    public class VortexComboBox : ComboBox
    {
        #region Win32 API for Dropdown Styling

        [DllImport("user32.dll")]
        private static extern IntPtr GetComboBoxInfo(IntPtr hWnd, ref COMBOBOXINFO pcbi);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct COMBOBOXINFO
        {
            public int cbSize;
            public RECT rcItem;
            public RECT rcButton;
            public int stateButton;
            public IntPtr hwndCombo;
            public IntPtr hwndItem;
            public IntPtr hwndList;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;

        #endregion

        #region Private Fields

        // Appearance
        private Color _backgroundColor = Color.FromArgb(45, 45, 48);
        private Color _borderColor = Color.FromArgb(45, 45, 48);
        private Color _hoverBorderColor = Color.FromArgb(45, 45, 48);
        private Color _focusBorderColor = Color.FromArgb(45, 45, 48);
        private int _borderRadius = 8;
        private int _borderWidth = 1;

        // Text
        private Color _textColor = Color.White;
        private Color _placeholderColor = Color.FromArgb(180, 180, 180);
        private string _placeholderText = "Select an item...";

        // Dropdown button
        private Color _dropdownButtonColor = Color.FromArgb(45, 45, 48);
        private Color _dropdownButtonHoverColor = Color.FromArgb(45, 45, 48);
        private Color _dropdownArrowColor = Color.FromArgb(255, 73, 82);
        private int _dropdownButtonWidth = 30;
        private DropdownArrowStyle _arrowStyle = DropdownArrowStyle.Chevron;
        private int _arrowSize = 8;

        // Dropdown list
        private Color _dropdownBackColor = Color.FromArgb(45, 45, 48);
        private Color _dropdownItemHoverColor = Color.FromArgb(65, 65, 68);
        private Color _dropdownItemSelectedColor = Color.FromArgb(255, 73, 82);
        private int _dropdownBorderRadius = 8;
        private int _dropdownMaxHeight = 150;
        private int _dropdownBorderWidth = 2;

        // Scrollbar
        private int _scrollBarWidth = 12;
        private Color _scrollBarTrackColor = Color.FromArgb(45, 45, 48);
        private Color _scrollBarThumbColor = Color.FromArgb(65, 65, 68);
        private Color _scrollBarThumbHoverColor = Color.FromArgb(85, 85, 88);
        private Color _scrollBarThumbActiveColor = Color.FromArgb(255, 73, 82);
        private int _scrollBarThumbRadius = 0;
        private int _scrollBarThumbWidth = 12;

        // Animation
        private bool _isHovering = false;
        private bool _isDropdownButtonHovered = false;
        private float _hoverProgress = 0f;
        private float _dropdownButtonHoverProgress = 0f;
        private CancellationTokenSource _hoverAnimationCts;
        private CancellationTokenSource _dropdownButtonHoverCts;

        // State
        private bool _isFocused = false;
        private Point _lastMousePosition;
        private bool _isDropdownOpen = false;

        #endregion

        #region Properties

        [Category("Appearance")]
        [Description("Background color of the combo box")]
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Border color")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Border color when hovering")]
        public Color HoverBorderColor
        {
            get { return _hoverBorderColor; }
            set { _hoverBorderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Border color when focused")]
        public Color FocusBorderColor
        {
            get { return _focusBorderColor; }
            set { _focusBorderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Border radius in pixels")]
        [DefaultValue(8)]
        public int BorderRadius
        {
            get { return _borderRadius; }
            set { _borderRadius = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Border width in pixels")]
        [DefaultValue(1f)]
        public float BorderWidth
        {
            get { return _borderWidth; }
            set { _borderWidth = (int)Math.Max(1, value); Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Text color")]
        public Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Placeholder text color")]
        public Color PlaceholderColor
        {
            get { return _placeholderColor; }
            set { _placeholderColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Placeholder text when no item is selected")]
        [DefaultValue("Select an item...")]
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set { _placeholderText = value; Invalidate(); }
        }

        [Category("Dropdown Button")]
        [Description("Background color of the dropdown button")]
        public Color DropdownButtonColor
        {
            get { return _dropdownButtonColor; }
            set { _dropdownButtonColor = value; Invalidate(); }
        }

        [Category("Dropdown Button")]
        [Description("Background color of the dropdown button when hovering")]
        public Color DropdownButtonHoverColor
        {
            get { return _dropdownButtonHoverColor; }
            set { _dropdownButtonHoverColor = value; Invalidate(); }
        }

        [Category("Dropdown Button")]
        [Description("Color of the dropdown arrow")]
        public Color DropdownArrowColor
        {
            get { return _dropdownArrowColor; }
            set { _dropdownArrowColor = value; Invalidate(); }
        }

        [Category("Dropdown Button")]
        [Description("Width of the dropdown button in pixels")]
        [DefaultValue(30)]
        public int DropdownButtonWidth
        {
            get { return _dropdownButtonWidth; }
            set { _dropdownButtonWidth = Math.Max(20, Math.Min(60, value)); Invalidate(); }
        }

        [Category("Dropdown Button")]
        [Description("Style of the dropdown arrow")]
        [DefaultValue(DropdownArrowStyle.Chevron)]
        public DropdownArrowStyle ArrowStyle
        {
            get { return _arrowStyle; }
            set { _arrowStyle = value; Invalidate(); }
        }

        [Category("Dropdown Button")]
        [Description("Size of the dropdown arrow in pixels")]
        [DefaultValue(8)]
        public int ArrowSize
        {
            get { return _arrowSize; }
            set { _arrowSize = Math.Max(4, Math.Min(20, value)); Invalidate(); }
        }

        [Category("Dropdown List")]
        [Description("Background color of the dropdown list")]
        public Color DropdownBackColor
        {
            get { return _dropdownBackColor; }
            set { _dropdownBackColor = value; }
        }

        [Category("Dropdown List")]
        [Description("Background color of items when hovering")]
        public Color DropdownItemHoverColor
        {
            get { return _dropdownItemHoverColor; }
            set { _dropdownItemHoverColor = value; }
        }

        [Category("Dropdown List")]
        [Description("Background color of selected item")]
        public Color DropdownItemSelectedColor
        {
            get { return _dropdownItemSelectedColor; }
            set { _dropdownItemSelectedColor = value; }
        }

        [Category("Dropdown List")]
        [Description("Border radius of the dropdown list")]
        [DefaultValue(8)]
        public int DropdownBorderRadius
        {
            get { return _dropdownBorderRadius; }
            set { _dropdownBorderRadius = Math.Max(0, value); }
        }

        [Category("Dropdown List")]
        [Description("Maximum height of the dropdown list")]
        [DefaultValue(200)]
        public int DropdownMaxHeight
        {
            get { return _dropdownMaxHeight; }
            set { _dropdownMaxHeight = Math.Max(50, value); MaxDropDownItems = 10; }
        }

        [Category("Dropdown List")]
        [Description("Border width of the dropdown list in pixels")]
        [DefaultValue(2)]
        public int DropdownBorderWidth
        {
            get { return _dropdownBorderWidth; }
            set { _dropdownBorderWidth = Math.Max(1, value); }
        }

        [Category("Scrollbar")]
        [Description("Width of the scrollbar in pixels")]
        [DefaultValue(12)]
        public int ScrollBarWidth
        {
            get { return _scrollBarWidth; }
            set { _scrollBarWidth = Math.Max(8, Math.Min(20, value)); }
        }

        [Category("Scrollbar")]
        [Description("Background color of the scrollbar track")]
        public Color ScrollBarTrackColor
        {
            get { return _scrollBarTrackColor; }
            set { _scrollBarTrackColor = value; }
        }

        [Category("Scrollbar")]
        [Description("Color of the scrollbar thumb")]
        public Color ScrollBarThumbColor
        {
            get { return _scrollBarThumbColor; }
            set { _scrollBarThumbColor = value; }
        }

        [Category("Scrollbar")]
        [Description("Color of the scrollbar thumb when hovering")]
        public Color ScrollBarThumbHoverColor
        {
            get { return _scrollBarThumbHoverColor; }
            set { _scrollBarThumbHoverColor = value; }
        }

        [Category("Scrollbar")]
        [Description("Color of the scrollbar thumb when active/dragging")]
        public Color ScrollBarThumbActiveColor
        {
            get { return _scrollBarThumbActiveColor; }
            set { _scrollBarThumbActiveColor = value; }
        }

        [Category("Scrollbar")]
        [Description("Border radius of the scrollbar thumb in pixels")]
        [DefaultValue(6)]
        public int ScrollBarThumbRadius
        {
            get { return _scrollBarThumbRadius; }
            set { _scrollBarThumbRadius = Math.Max(0, value); }
        }

        [Category("Scrollbar")]
        [Description("Width of the scrollbar thumb in pixels")]
        [DefaultValue(8)]
        public int ScrollBarThumbWidth
        {
            get { return _scrollBarThumbWidth; }
            set { _scrollBarThumbWidth = Math.Max(4, Math.Min(_scrollBarWidth, value)); }
        }

        // Additional appearance options
        private int _cornerCutSize = 0;
        private bool _showBorder = true;
        private bool _useCustomDropDown = true;

        [Category("Dropdown Button")]
        [Description("Color of the dropdown arrow (alias of DropdownArrowColor)")]
        public Color ArrowColor
        {
            get { return _dropdownArrowColor; }
            set { _dropdownArrowColor = value; Invalidate(); }
        }

        [Category("Dropdown Button")]
        [Description("Background color of the dropdown button (alias of DropdownButtonColor)")]
        public Color ButtonColor
        {
            get { return _dropdownButtonColor; }
            set { _dropdownButtonColor = value; Invalidate(); }
        }

        [Category("Dropdown List")]
        [Description("Background color of items when hovering (alias of DropdownItemHoverColor)")]
        public Color ListItemHoverColor
        {
            get { return _dropdownItemHoverColor; }
            set { _dropdownItemHoverColor = value; }
        }

        [Category("Appearance")]
        [Description("Size of the cut corner in pixels")]
        [DefaultValue(0)]
        public int CornerCutSize
        {
            get { return _cornerCutSize; }
            set { _cornerCutSize = Math.Max(0, value); Invalidate(); }
        }

        [Category("Appearance")]
        [Description("Whether to draw the border")]
        [DefaultValue(true)]
        public bool ShowBorder
        {
            get { return _showBorder; }
            set { _showBorder = value; Invalidate(); }
        }

        [Category("Behavior")]
        [Description("Whether to use the custom styled dropdown")]
        [DefaultValue(true)]
        public bool UseCustomDropDown
        {
            get { return _useCustomDropDown; }
            set { _useCustomDropDown = value; }
        }

        #endregion

        #region Constructor

        // Custom dropdown window
        private VortexComboBoxDropdownWindow _customDropdownWindow;

        public VortexComboBox()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
            IntegralHeight = false;
            
            try
            {
                Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            }
            catch
            {
                Font = SystemFonts.DefaultFont;
            }

            Size = new Size(200, 35);
            ItemHeight = 30;
            BackColor = Color.Transparent;
            ForeColor = _textColor;
        }

        #endregion

        #region Animation Methods

        private float EaseOutCubic(float t)
        {
            return 1 - (float)Math.Pow(1 - t, 3);
        }

        private async Task AnimateHoverAsync(bool hovering, CancellationToken cancelToken)
        {
            try
            {
                float startValue = _hoverProgress;
                float targetValue = hovering ? 1.0f : 0.0f;
                float duration = 150f;
                float elapsed = 0f;
                int frameTime = 16;

                while (!cancelToken.IsCancellationRequested && elapsed < duration)
                {
                    elapsed += frameTime;
                    float progress = Math.Min(elapsed / duration, 1.0f);
                    float easedProgress = EaseOutCubic(progress);

                    _hoverProgress = startValue + (targetValue - startValue) * easedProgress;

                    if (IsHandleCreated && !IsDisposed)
                        Invalidate();

                    await Task.Delay(frameTime, cancelToken);
                }

                if (!cancelToken.IsCancellationRequested)
                {
                    _hoverProgress = targetValue;
                    if (IsHandleCreated && !IsDisposed)
                        Invalidate();
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Hover animation error: {ex.Message}");
            }
        }

        private async Task AnimateDropdownButtonHoverAsync(bool hovering, CancellationToken cancelToken)
        {
            try
            {
                float startValue = _dropdownButtonHoverProgress;
                float targetValue = hovering ? 1.0f : 0.0f;
                float duration = 120f;
                float elapsed = 0f;
                int frameTime = 16;

                while (!cancelToken.IsCancellationRequested && elapsed < duration)
                {
                    elapsed += frameTime;
                    float progress = Math.Min(elapsed / duration, 1.0f);
                    float easedProgress = EaseOutCubic(progress);

                    _dropdownButtonHoverProgress = startValue + (targetValue - startValue) * easedProgress;

                    if (IsHandleCreated && !IsDisposed)
                        Invalidate();

                    await Task.Delay(frameTime, cancelToken);
                }

                if (!cancelToken.IsCancellationRequested)
                {
                    _dropdownButtonHoverProgress = targetValue;
                    if (IsHandleCreated && !IsDisposed)
                        Invalidate();
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dropdown button hover animation error: {ex.Message}");
            }
        }

        private Color InterpolateColor(Color start, Color end, float ratio)
        {
            ratio = Math.Max(0, Math.Min(1, ratio));
            int r = (int)(start.R + (end.R - start.R) * ratio);
            int g = (int)(start.G + (end.G - start.G) * ratio);
            int b = (int)(start.B + (end.B - start.B) * ratio);
            int a = (int)(start.A + (end.A - start.A) * ratio);
            return Color.FromArgb(a, r, g, b);
        }

        #endregion

        #region Event Handlers


        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovering = true;

            _hoverAnimationCts?.Cancel();
            _hoverAnimationCts?.Dispose();
            _hoverAnimationCts = new CancellationTokenSource();
            _ = AnimateHoverAsync(true, _hoverAnimationCts.Token);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovering = false;
            _isDropdownButtonHovered = false;

            _hoverAnimationCts?.Cancel();
            _hoverAnimationCts?.Dispose();
            _hoverAnimationCts = new CancellationTokenSource();
            _ = AnimateHoverAsync(false, _hoverAnimationCts.Token);

            _dropdownButtonHoverCts?.Cancel();
            _dropdownButtonHoverCts?.Dispose();
            _dropdownButtonHoverCts = new CancellationTokenSource();
            _ = AnimateDropdownButtonHoverAsync(false, _dropdownButtonHoverCts.Token);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Don't call base to prevent Windows dropdown
            if (e.Button == MouseButtons.Left)
            {
                OpenCustomDropdown();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _lastMousePosition = e.Location;

            // Check if mouse is over dropdown button
            Rectangle dropdownButtonRect = new Rectangle(
                Width - _dropdownButtonWidth - _borderWidth,
                _borderWidth,
                _dropdownButtonWidth,
                Height - (_borderWidth * 2));

            bool wasHovered = _isDropdownButtonHovered;
            _isDropdownButtonHovered = dropdownButtonRect.Contains(e.Location);

            if (wasHovered != _isDropdownButtonHovered)
            {
                _dropdownButtonHoverCts?.Cancel();
                _dropdownButtonHoverCts?.Dispose();
                _dropdownButtonHoverCts = new CancellationTokenSource();
                _ = AnimateDropdownButtonHoverAsync(_isDropdownButtonHovered, _dropdownButtonHoverCts.Token);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            _isFocused = true;
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _isFocused = false;
            Invalidate();
        }

        protected override void OnDropDown(EventArgs e)
        {
            // ما تسيبش النظام يتعامل، إحنا هندير الفتح بنفسنا
            DroppedDown = false;
            OpenCustomDropdown();
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);
            Invalidate();
        }

        private void OpenCustomDropdown()
        {
            // ✅ حماية مضاعفة ضد null
            if (this == null || IsDisposed)
                return;

            // ✅ لو القائمة مفتوحة، اقفلها (Toggle)
            if (_isDropdownOpen && _customDropdownWindow != null && _customDropdownWindow.Visible)
            {
                CloseCustomDropdown();
                return;
            }

            // ✅ لو النافذة القديمة لسه موجودة، اقفلها ونضفها
            if (_customDropdownWindow != null)
            {
                try
                {
                    if (!_customDropdownWindow.IsDisposed)
                        _customDropdownWindow.Close();

                    _customDropdownWindow.Dispose();
                }
                catch { /* تجاهل أي خطأ في الإغلاق */ }

                _customDropdownWindow = null;
            }

            // ✅ إنشاء نسخة جديدة ومضمونة الارتباط بالكومبو الحالي
            _customDropdownWindow = new VortexComboBoxDropdownWindow(this);

            if (_customDropdownWindow == null)
                return;

            _customDropdownWindow.ItemSelected += CustomDropdown_ItemSelected;
            _customDropdownWindow.Closed += CustomDropdown_Closed;

            _isDropdownOpen = true;
            Invalidate(); // لإعادة رسم الكومبو بزاوية علوية مستقيمة والسهم لفوق

            // ✅ تأكد إن الموقع صحيح وأن الفورم عندها حجم سليم
            Point screenLocation = PointToScreen(new Point(0, Height - (_borderRadius / 2)));

            try
            {
                _customDropdownWindow.ShowDropdown(screenLocation);
            }
            catch (Exception ex)
            {
                // حماية إضافية (لو حصل Null داخل ShowDropdown)
                Console.WriteLine("Dropdown open error: " + ex.Message);
                _customDropdownWindow?.Dispose();
                _customDropdownWindow = null;
                _isDropdownOpen = false;
            }
        }


        private void CloseCustomDropdown()
        {
            if (_customDropdownWindow != null && _customDropdownWindow.Visible)
            {
                _customDropdownWindow.Hide();
            }
            _isDropdownOpen = false;
            Invalidate(); // Redraw ComboBox with all corners rounded
        }

        private void CustomDropdown_ItemSelected(object sender, int index)
        {
            SelectedIndex = index;
            _isDropdownOpen = false;
            CloseCustomDropdown();
            Focus();
            Invalidate();
        }

        private void CustomDropdown_Closed(object sender, EventArgs e)
        {
            _isDropdownOpen = false;

            // تأكد من تنظيف الذاكرة بعد الإغلاق
            if (_customDropdownWindow != null)
            {
                _customDropdownWindow.Dispose();
                _customDropdownWindow = null;
            }

            Invalidate();
        }


        #endregion

        #region Paint Methods

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Calculate current border color
            Color currentBorderColor = _borderColor;
            if (_isFocused || DroppedDown || _isDropdownOpen)
            {
                currentBorderColor = _focusBorderColor;
            }
            else if (_hoverProgress > 0)
            {
                currentBorderColor = InterpolateColor(_borderColor, _hoverBorderColor, _hoverProgress);
            }

            // Draw background with conditional rounding
            if (_isDropdownOpen)
            {
                // When dropdown is open, only round top corners
                using (GraphicsPath bgPath = CreateRoundedRectangleTopOnly(ClientRectangle, _borderRadius))
                using (SolidBrush bgBrush = new SolidBrush(_backgroundColor))
                {
                    g.FillPath(bgBrush, bgPath);
                }
            }
            else
            {
                // Normal state - all corners rounded
                using (GraphicsPath bgPath = CreateRoundedRectangle(ClientRectangle, _borderRadius))
                using (SolidBrush bgBrush = new SolidBrush(_backgroundColor))
                {
                    g.FillPath(bgBrush, bgPath);
                }
            }

            // Draw dropdown button background
            Rectangle dropdownButtonRect = new Rectangle(
                Width - _dropdownButtonWidth - _borderWidth,
                _borderWidth,
                _dropdownButtonWidth,
                Height - (_borderWidth * 2));

            Color currentDropdownButtonColor = _dropdownButtonColor;
            if (_dropdownButtonHoverProgress > 0)
            {
                currentDropdownButtonColor = InterpolateColor(
                    _dropdownButtonColor,
                    _dropdownButtonHoverColor,
                    _dropdownButtonHoverProgress);
            }

            using (GraphicsPath buttonPath = CreateRoundedRectangle(dropdownButtonRect, _borderRadius - _borderWidth, true, _isDropdownOpen))
            using (SolidBrush buttonBrush = new SolidBrush(currentDropdownButtonColor))
            {
                g.FillPath(buttonBrush, buttonPath);
            }

            // Draw dropdown arrow
            DrawArrow(g, dropdownButtonRect);

            // Draw border with conditional rounding
            float halfBorder = _borderWidth / 2f;
            RectangleF borderRect = new RectangleF(
                halfBorder,
                halfBorder,
                Width - _borderWidth,
                Height - _borderWidth);

            if (!_showBorder)
            {
                // Border drawing disabled
            }
            else if (_isDropdownOpen)
            {
                // When dropdown is open, only draw top and sides (no bottom border)
                using (GraphicsPath borderPath = CreateBorderPathTopOnly(borderRect, _borderRadius))
                using (Pen borderPen = new Pen(currentBorderColor, _borderWidth))
                {
                    borderPen.LineJoin = LineJoin.Round;
                    g.DrawPath(borderPen, borderPath);
                }
            }
            else
            {
                // Normal state - full border
                using (GraphicsPath borderPath = CreateRoundedRectangle(borderRect, _borderRadius))
                using (Pen borderPen = new Pen(currentBorderColor, _borderWidth))
                {
                    borderPen.LineJoin = LineJoin.Round;
                    g.DrawPath(borderPen, borderPath);
                }
            }

            // Draw text
            DrawText(g);
        }

        private void DrawText(Graphics g)
        {
            Rectangle textRect = new Rectangle(
                10,
                0,
                Width - _dropdownButtonWidth - 20,
                Height);

            string displayText = SelectedIndex >= 0 ? GetItemText(SelectedItem) : _placeholderText;
            Color textColor = SelectedIndex >= 0 ? _textColor : _placeholderColor;

            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
            TextRenderer.DrawText(g, displayText, Font, textRect, textColor, flags);
        }

        private void DrawArrow(Graphics g, Rectangle buttonRect)
        {
            int centerX = buttonRect.X + buttonRect.Width / 2;
            int centerY = buttonRect.Y + buttonRect.Height / 2;

            using (Pen arrowPen = new Pen(_dropdownArrowColor, 2f))
            {
                arrowPen.StartCap = LineCap.Round;
                arrowPen.EndCap = LineCap.Round;
                arrowPen.LineJoin = LineJoin.Round;

                // لو القائمة مفتوحة، ارسم السهم لفوق، لو مقفولة ارسمه لتحت
                bool isUp = _isDropdownOpen;

                switch (_arrowStyle)
                {
                    case DropdownArrowStyle.Chevron:
                        if (isUp)
                        {
                            // Chevron up (^ shape)
                            g.DrawLine(arrowPen,
                                centerX - _arrowSize / 2, centerY + _arrowSize / 4,
                                centerX, centerY - _arrowSize / 4);
                            g.DrawLine(arrowPen,
                                centerX, centerY - _arrowSize / 4,
                                centerX + _arrowSize / 2, centerY + _arrowSize / 4);
                        }
                        else
                        {
                            // Chevron down (V shape)
                            g.DrawLine(arrowPen,
                                centerX - _arrowSize / 2, centerY - _arrowSize / 4,
                                centerX, centerY + _arrowSize / 4);
                            g.DrawLine(arrowPen,
                                centerX, centerY + _arrowSize / 4,
                                centerX + _arrowSize / 2, centerY - _arrowSize / 4);
                        }
                        break;

                    case DropdownArrowStyle.Triangle:
                        // Filled triangle
                        PointF[] trianglePoints;
                        if (isUp)
                        {
                            trianglePoints = new PointF[]
                            {
                                new PointF(centerX - _arrowSize / 2f, centerY + _arrowSize / 4f),
                                new PointF(centerX + _arrowSize / 2f, centerY + _arrowSize / 4f),
                                new PointF(centerX, centerY - _arrowSize / 2f)
                            };
                        }
                        else
                        {
                            trianglePoints = new PointF[]
                            {
                                new PointF(centerX - _arrowSize / 2f, centerY - _arrowSize / 4f),
                                new PointF(centerX + _arrowSize / 2f, centerY - _arrowSize / 4f),
                                new PointF(centerX, centerY + _arrowSize / 2f)
                            };
                        }
                        using (SolidBrush arrowBrush = new SolidBrush(_dropdownArrowColor))
                        {
                            g.FillPolygon(arrowBrush, trianglePoints);
                        }
                        break;

                    case DropdownArrowStyle.Arrow:
                        if (isUp)
                        {
                            // Arrow up
                            g.DrawLine(arrowPen,
                                centerX, centerY + _arrowSize / 2,
                                centerX, centerY - _arrowSize / 2);
                            g.DrawLine(arrowPen,
                                centerX, centerY - _arrowSize / 2,
                                centerX - _arrowSize / 3, centerY - _arrowSize / 6);
                            g.DrawLine(arrowPen,
                                centerX, centerY - _arrowSize / 2,
                                centerX + _arrowSize / 3, centerY - _arrowSize / 6);
                        }
                        else
                        {
                            // Arrow down
                            g.DrawLine(arrowPen,
                                centerX, centerY - _arrowSize / 2,
                                centerX, centerY + _arrowSize / 2);
                            g.DrawLine(arrowPen,
                                centerX, centerY + _arrowSize / 2,
                                centerX - _arrowSize / 3, centerY + _arrowSize / 6);
                            g.DrawLine(arrowPen,
                                centerX, centerY + _arrowSize / 2,
                                centerX + _arrowSize / 3, centerY + _arrowSize / 6);
                        }
                        break;
                }
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Determine background color
            Color itemBackColor = _dropdownBackColor;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                itemBackColor = _dropdownItemSelectedColor;
            }
            else if ((e.State & DrawItemState.HotLight) == DrawItemState.HotLight ||
                     (e.State & DrawItemState.Focus) == DrawItemState.Focus)
            {
                itemBackColor = _dropdownItemHoverColor;
            }

            // Expand bounds to cover the entire width including any borders
            Rectangle itemBounds = new Rectangle(
                0,
                e.Bounds.Y,
                e.Bounds.Width,
                e.Bounds.Height);

            bool isFirstItem = e.Index == 0;
            bool isLastItem = e.Index == Items.Count - 1;

            // Draw background with rounded corners for first and last items
            if (_dropdownBorderRadius > 0 && (isFirstItem || isLastItem))
            {
                using (GraphicsPath itemPath = new GraphicsPath())
                {
                    float radius = Math.Min(_dropdownBorderRadius, itemBounds.Height / 2f);
                    float diameter = radius * 2f;
                    
                    if (isFirstItem && !isLastItem)
                    {
                        // Round top corners only
                        itemPath.AddArc(itemBounds.Left, itemBounds.Top, diameter, diameter, 180, 90);
                        itemPath.AddArc(itemBounds.Right - diameter, itemBounds.Top, diameter, diameter, 270, 90);
                        itemPath.AddLine(itemBounds.Right, itemBounds.Top + radius, itemBounds.Right, itemBounds.Bottom);
                        itemPath.AddLine(itemBounds.Right, itemBounds.Bottom, itemBounds.Left, itemBounds.Bottom);
                        itemPath.AddLine(itemBounds.Left, itemBounds.Bottom, itemBounds.Left, itemBounds.Top + radius);
                    }
                    else if (isLastItem && !isFirstItem)
                    {
                        // Round bottom corners only
                        itemPath.AddLine(itemBounds.Left, itemBounds.Top, itemBounds.Right, itemBounds.Top);
                        itemPath.AddLine(itemBounds.Right, itemBounds.Top, itemBounds.Right, itemBounds.Bottom - radius);
                        itemPath.AddArc(itemBounds.Right - diameter, itemBounds.Bottom - diameter, diameter, diameter, 0, 90);
                        itemPath.AddArc(itemBounds.Left, itemBounds.Bottom - diameter, diameter, diameter, 90, 90);
                        itemPath.AddLine(itemBounds.Left, itemBounds.Bottom - radius, itemBounds.Left, itemBounds.Top);
                    }
                    else if (isFirstItem && isLastItem)
                    {
                        // Round all corners (single item)
                        itemPath.AddArc(itemBounds.Left, itemBounds.Top, diameter, diameter, 180, 90);
                        itemPath.AddArc(itemBounds.Right - diameter, itemBounds.Top, diameter, diameter, 270, 90);
                        itemPath.AddArc(itemBounds.Right - diameter, itemBounds.Bottom - diameter, diameter, diameter, 0, 90);
                        itemPath.AddArc(itemBounds.Left, itemBounds.Bottom - diameter, diameter, diameter, 90, 90);
                    }
                    
                    itemPath.CloseFigure();
                    
                    using (SolidBrush bgBrush = new SolidBrush(itemBackColor))
                    {
                        e.Graphics.FillPath(bgBrush, itemPath);
                    }
                }
            }
            else
            {
                // Always fill the entire background first to cover any white areas
                using (SolidBrush bgBrush = new SolidBrush(itemBackColor))
                {
                    e.Graphics.FillRectangle(bgBrush, itemBounds);
                }
            }

            // Draw text
            string itemText = GetItemText(Items[e.Index]);
            Rectangle textRect = new Rectangle(
                10,
                e.Bounds.Y,
                e.Bounds.Width - 20,
                e.Bounds.Height);

            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
            TextRenderer.DrawText(e.Graphics, itemText, Font, textRect, _textColor, flags);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_LBUTTONDBLCLK = 0x0203;
            const int CB_SHOWDROPDOWN = 0x014F;

            switch (m.Msg)
            {
                case CB_SHOWDROPDOWN:
                    // 👇 امنع القائمة الأصلية نهائيًا
                    return;

                case WM_LBUTTONDOWN:
                case WM_LBUTTONDBLCLK:
                    // 👇 لو ضغط المستخدم داخل حدود الكومبو، نفتح قائمتنا الخاصة
                    Point mousePoint = PointToClient(Cursor.Position);
                    Rectangle fullBounds = new Rectangle(0, 0, Width, Height);

                    if (fullBounds.Contains(mousePoint))
                    {
                        // امنع النظام من تنفيذ الضغط الأصلي
                        OpenCustomDropdown();
                        return;
                    }
                    break;
            }

            base.WndProc(ref m);
        }

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private const int WS_BORDER = 0x00800000;
        private const int WS_EX_CLIENTEDGE = 0x00000200;

        #endregion

        #region Helper Methods

        private GraphicsPath CreateRoundedRectangle(RectangleF rect, float radius, bool rightSideOnly = false, bool topOnly = false)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);
            float diameter = radius * 2f;

            if (rightSideOnly)
            {
                if (topOnly)
                {
                    // Only round top-right corner (for dropdown button when open)
                    path.AddLine(rect.Left, rect.Top, rect.Right - radius, rect.Top);
                    path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
                    path.AddLine(rect.Right, rect.Top + radius, rect.Right, rect.Bottom);
                    path.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
                    path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Top);
                }
                else
                {
                    // Round both right corners (for dropdown button normal)
                    path.AddLine(rect.Left, rect.Top, rect.Right - radius, rect.Top);
                    path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
                    path.AddLine(rect.Right, rect.Top + radius, rect.Right, rect.Bottom - radius);
                    path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                    path.AddLine(rect.Right - radius, rect.Bottom, rect.Left, rect.Bottom);
                    path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Top);
                }
            }
            else
            {
                // Round all corners
                path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            }

            path.CloseFigure();
            return path;
        }

        private GraphicsPath CreateRoundedRectangleTopOnly(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);
            float diameter = radius * 2f;

            // Only round top corners
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddLine(rect.Right, rect.Top + radius, rect.Right, rect.Bottom);
            path.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
            path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Top + radius);

            path.CloseFigure();
            return path;
        }

        private GraphicsPath CreateBorderPathTopOnly(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                // Just top and sides, no bottom
                path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Top);
                path.AddLine(rect.Left, rect.Top, rect.Right, rect.Top);
                path.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom);
                return path;
            }

            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);
            float diameter = radius * 2f;

            // Start from bottom-left, go up and around top, down to bottom-right
            path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Top + radius);
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddLine(rect.Right, rect.Top + radius, rect.Right, rect.Bottom);

            return path;
        }

        #endregion

        #region Disposal

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hoverAnimationCts?.Cancel();
                _hoverAnimationCts?.Dispose();
                _dropdownButtonHoverCts?.Cancel();
                _dropdownButtonHoverCts?.Dispose();
                
                if (_customDropdownWindow != null)
                {
                    _customDropdownWindow.ItemSelected -= CustomDropdown_ItemSelected;
                    _customDropdownWindow.Closed -= CustomDropdown_Closed;
                    _customDropdownWindow.Dispose();
                    _customDropdownWindow = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Nested Class - Custom Dropdown Window

        private class VortexComboBoxDropdownWindow : Form, IMessageFilter
        {
            private readonly VortexComboBox _parent;
            private int _hoveredIndex = -1;
            private VortexScrollBar _scrollBar;
            private int _scrollOffset = 0;

            public event EventHandler<int> ItemSelected;

            private const int WM_LBUTTONDOWN = 0x0201;
            private const int WM_RBUTTONDOWN = 0x0204;
            private const int WM_MBUTTONDOWN = 0x0207;

            public VortexComboBoxDropdownWindow(VortexComboBox parent)
            {
                if (parent == null)
                    throw new ArgumentNullException(nameof(parent));

                _parent = parent;

                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                TopMost = true;

                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                SetStyle(ControlStyles.ResizeRedraw, true);

                BackColor = _parent._dropdownBackColor;

                // Custom scrollbar setup
                int totalHeight = _parent.Items.Count * _parent.ItemHeight;
                if (totalHeight > _parent._dropdownMaxHeight)
                {
                    _scrollBar = new VortexScrollBar
                    {
                        Dock = DockStyle.Right,
                        Width = _parent._scrollBarWidth,
                        TrackColor = _parent._scrollBarTrackColor,
                        ThumbColor = _parent._scrollBarThumbColor,
                        ThumbHoverColor = _parent._scrollBarThumbHoverColor,
                        ThumbActiveColor = _parent._scrollBarThumbActiveColor,
                        ThumbRadius = _parent._scrollBarThumbRadius,
                        ThumbWidth = _parent._scrollBarThumbWidth
                    };
                    _scrollBar.Maximum = totalHeight - _parent._dropdownMaxHeight + _scrollBar.LargeChange - 1;
                    _scrollBar.LargeChange = _parent.ItemHeight * 3;
                    _scrollBar.SmallChange = _parent.ItemHeight;
                    _scrollBar.Scroll += (s, e) => { _scrollOffset = _scrollBar.Value; Invalidate(); };
                    Controls.Add(_scrollBar);
                }
            }

            public void ShowDropdown(Point location)
            {
                int totalHeight = _parent.Items.Count * _parent.ItemHeight;
                int dropdownHeight = Math.Min(totalHeight, _parent._dropdownMaxHeight);

                Width = _parent.Width;
                Height = dropdownHeight;
                Location = location;

                // إضافة message filter عشان نمسك الـ clicks في أي مكان
                Application.AddMessageFilter(this);

                Show();
            }

            public bool PreFilterMessage(ref Message m)
            {
                // لو في click حصل في أي مكان
                if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_RBUTTONDOWN || m.Msg == WM_MBUTTONDOWN)
                {
                    // تحويل موقع الماوس لـ screen coordinates
                    Point clickPoint = Control.MousePosition;
                    
                    // تحقق لو الـ click جوه القائمة أو الكومبو بوكس
                    Rectangle dropdownBounds = new Rectangle(Location, Size);
                    Rectangle parentBounds = _parent.RectangleToScreen(_parent.ClientRectangle);
                    
                    // لو الـ click بره القائمة والكومبو بوكس، اقفل القائمة
                    if (!dropdownBounds.Contains(clickPoint) && !parentBounds.Contains(clickPoint))
                    {
                        _parent._isDropdownOpen = false;
                        _parent.Invalidate();
                        Application.RemoveMessageFilter(this);
                        Hide();
                        return false;
                    }
                }
                
                return false;
            }

            protected override void OnDeactivate(EventArgs e)
            {
                // مش هنقفل القائمة هنا عشان ميختفيش لما الماوس يتحرك بره
                // القائمة هتقفل بس لما المستخدم يختار item أو يضغط Escape أو يضغط بره
                base.OnDeactivate(e);
            }

            protected override void OnFormClosing(FormClosingEventArgs e)
            {
                // شيل الـ message filter لما القائمة تقفل
                Application.RemoveMessageFilter(this);
                base.OnFormClosing(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                int index = (_scrollOffset + e.Y) / _parent.ItemHeight;
                if (index >= 0 && index < _parent.Items.Count && index != _hoveredIndex)
                {
                    _hoveredIndex = index;
                    Invalidate();
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                _hoveredIndex = -1;
                Invalidate();
            }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);
                int index = (_scrollOffset + e.Y) / _parent.ItemHeight;
                if (index >= 0 && index < _parent.Items.Count)
                    ItemSelected?.Invoke(this, index);
            }

            protected override void OnMouseWheel(MouseEventArgs e)
            {
                base.OnMouseWheel(e);
                
                // لو في scrollbar موجود، نعمل scroll
                if (_scrollBar != null)
                {
                    // كل notch في الـ wheel = 120
                    int delta = e.Delta / 120;
                    int newValue = _scrollBar.Value - (delta * _scrollBar.SmallChange);
                    
                    // تأكد إن القيمة في النطاق الصحيح
                    newValue = Math.Max(_scrollBar.Minimum, Math.Min(_scrollBar.Maximum, newValue));
                    
                    _scrollBar.Value = newValue;
                    _scrollOffset = _scrollBar.Value;
                    Invalidate();
                }
            }

            protected override void OnPaintBackground(PaintEventArgs pevent)
            {
                // منع النظام من مسح الخلفية الافتراضية
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;

                float borderWidth = _parent._borderWidth;
                float radius = _parent._borderRadius;
                int visibleWidth = _scrollBar != null ? Width - _scrollBar.Width : Width;

                // تعبئة الخلفية بلون القائمة
                using (SolidBrush bgBrush = new SolidBrush(_parent._dropdownBackColor))
                    g.FillRectangle(bgBrush, ClientRectangle);

                // إنشاء المسار بزوايا سفلية مدوّرة فقط (للشكل الكامل بما فيه الـ scrollbar)
                using (GraphicsPath path = new GraphicsPath())
                {
                    float diameter = radius * 2f;
                    RectangleF rect = new RectangleF(0, 0, Width - 1, Height - 1);

                    path.StartFigure();
                    path.AddLine(rect.Left, rect.Top, rect.Right, rect.Top); // علوي مستقيم
                    path.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom - radius);
                    path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                    path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
                    path.AddLine(rect.Left, rect.Bottom - radius, rect.Left, rect.Top);
                    path.CloseFigure();

                    // تعبئة الخلفية داخل الشكل المدوّر
                    using (SolidBrush bg = new SolidBrush(_parent._dropdownBackColor))
                        g.FillPath(bg, path);
                }

                // رسم العناصر داخل القائمة
                int y = -_scrollOffset;
                for (int i = 0; i < _parent.Items.Count; i++)
                {
                    if (y + _parent.ItemHeight < 0)
                    {
                        y += _parent.ItemHeight;
                        continue;
                    }
                    if (y > Height) break;

                    Rectangle itemRect = new Rectangle(0, y, visibleWidth, _parent.ItemHeight);
                    Color itemBackColor = _parent._dropdownBackColor;

                    if (i == _parent.SelectedIndex)
                        itemBackColor = _parent._dropdownItemSelectedColor;
                    else if (i == _hoveredIndex)
                        itemBackColor = _parent._dropdownItemHoverColor;

                    using (SolidBrush itemBrush = new SolidBrush(itemBackColor))
                        g.FillRectangle(itemBrush, itemRect);

                    Rectangle textRect = new Rectangle(10, y, visibleWidth - 20, _parent.ItemHeight);
                    TextRenderer.DrawText(
                        g,
                        _parent.GetItemText(_parent.Items[i]),
                        _parent.Font,
                        textRect,
                        _parent._textColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine
                    );

                    y += _parent.ItemHeight;
                }

                // رسم البوردر في النهاية فوق الكل ومُعوّض بدقة لتساوي السمك من الأربع جهات
                float half = borderWidth / 2f;
                float rectX = half;
                float rectY = half;
                float rectW = visibleWidth - borderWidth;
                float rectH = Height - borderWidth;

                using (Pen topBorder = new Pen(_parent._focusBorderColor, borderWidth))
                {
                    topBorder.Alignment = PenAlignment.Inset; // ممكن تغيّره إلى Center لو حبيت
                    topBorder.LineJoin = LineJoin.Round;

                    // نستخدم overload بأربعة floats لتجنب خطأ التحويل
                    g.DrawRectangle(topBorder, rectX, rectY, rectW, rectH);
                }
            }


            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                if (keyData == Keys.Escape)
                {
                    Hide();
                    return true;
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        #endregion
    }
}
