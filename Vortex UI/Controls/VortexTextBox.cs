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
    // Custom TextBox that supports transparent backgrounds (TextBox بدل RichTextBox للتوافق مع AOT)
    internal class TransparentTextBox : TextBox
    {
        private const int WM_LBUTTONDBLCLK = 0x0203;

        // شفافية حقيقية للـ TextBox الـ native عبر hollow brush على WM_CTLCOLOR
        [DllImport("gdi32.dll")] private static extern int SetBkMode(IntPtr hdc, int mode);
        [DllImport("gdi32.dll")] private static extern IntPtr GetStockObject(int fnObject);
        private const int WM_REFLECT = 0x2000;
        private const int WM_CTLCOLOREDIT = 0x0133;
        private const int WM_CTLCOLORSTATIC = 0x0138;
        private const int BK_TRANSPARENT = 1;
        private const int NULL_BRUSH = 5;
        private static readonly IntPtr HollowBrush = GetStockObject(NULL_BRUSH);
        private bool _transparentBackground = false;

        public void SetTransparentBackground(bool enabled)
        {
            if (_transparentBackground == enabled) return;
            _transparentBackground = enabled;
            if (IsHandleCreated) Invalidate();
        }

        private char _passwordChar = '\0';
        private bool _usePasswordChar = false;
        private string _actualText = string.Empty;
        private bool _isUpdatingText = false;
        private bool _isPlaceholderMode = false;

        public new char PasswordChar
        {
            get { return _passwordChar; }
            set
            {
                _passwordChar = value;
                if (_usePasswordChar && !string.IsNullOrEmpty(_actualText))
                {
                    _isUpdatingText = true;
                    base.Text = new string(_passwordChar, _actualText.Length);
                    _isUpdatingText = false;
                }
            }
        }

        public new bool UseSystemPasswordChar
        {
            get { return _usePasswordChar; }
            set
            {
                _usePasswordChar = value;
                if (value && _passwordChar == '\0')
                {
                    _passwordChar = '●';
                }
                
                if (value && !string.IsNullOrEmpty(_actualText))
                {
                    _isUpdatingText = true;
                    base.Text = new string(_passwordChar, _actualText.Length);
                    _isUpdatingText = false;
                }
                else if (!value)
                {
                    _isUpdatingText = true;
                    base.Text = _actualText;
                    _isUpdatingText = false;
                }
            }
        }

        public string ActualText
        {
            get { return _actualText; }
        }

        public TransparentTextBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BorderStyle = BorderStyle.None;
            BackColor = Color.Transparent;
            ScrollBars = ScrollBars.None;
            WordWrap = false;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            // Don't process text changes when in placeholder mode or updating
            if (_isPlaceholderMode || _isUpdatingText)
            {
                base.OnTextChanged(e);
                return;
            }
            
            if (_usePasswordChar && _passwordChar != '\0')
            {
                _isUpdatingText = true;
                int selStart = SelectionStart;
                string currentText = base.Text.TrimEnd('\n', '\r');
                
                // Update actual text based on changes
                if (currentText.Length > _actualText.Length)
                {
                    int insertPos = Math.Max(0, selStart - (currentText.Length - _actualText.Length));
                    if (insertPos <= _actualText.Length)
                    {
                        string addedText = currentText.Substring(insertPos, currentText.Length - _actualText.Length);
                        _actualText = _actualText.Insert(insertPos, addedText);
                    }
                }
                else if (currentText.Length < _actualText.Length)
                {
                    int removeCount = _actualText.Length - currentText.Length;
                    int removePos = Math.Max(0, selStart);
                    if (removePos < _actualText.Length)
                    {
                        _actualText = _actualText.Remove(removePos, Math.Min(removeCount, _actualText.Length - removePos));
                    }
                }
                
                base.Text = new string(_passwordChar, _actualText.Length);
                SelectionStart = Math.Min(selStart, base.Text.Length);
                _isUpdatingText = false;
            }
            else
            {
                string cleanText = base.Text;
                cleanText = cleanText.TrimEnd('\n', '\r', '\0');
                _actualText = cleanText;
            }

            base.OnTextChanged(e);

            // في وضع الشفافية، أعد رسم الخلفية ورا النص عشان مايفضلش أثر للنص القديم
            if (_transparentBackground && Parent != null && IsHandleCreated)
                Parent.Invalidate(Bounds, false);
        }

        protected override void WndProc(ref Message m)
        {
            // خلفية شفافة: نخلّي الـ edit مايرسمش خلفيته (hollow) ونخلّي خلفية النص شفافة
            if (_transparentBackground &&
                (m.Msg == (WM_REFLECT + WM_CTLCOLOREDIT) || m.Msg == (WM_REFLECT + WM_CTLCOLORSTATIC)))
            {
                SetBkMode(m.WParam, BK_TRANSPARENT);
                m.Result = HollowBrush;
                return;
            }

            // Intercept double-click to select all actual text only
            if (m.Msg == WM_LBUTTONDBLCLK)
            {
                int actualLength = _actualText.Length;

                if (actualLength > 0)
                {
                    SelectionStart = 0;
                    SelectionLength = actualLength;
                }
                else
                {
                    SelectionStart = 0;
                    SelectionLength = 0;
                }

                return;
            }

            base.WndProc(ref m);
        }

        public void SetTextWithoutMask(string text)
        {
            _isUpdatingText = true;
            _actualText = text ?? string.Empty;
            
            if (_usePasswordChar && _passwordChar != '\0' && !string.IsNullOrEmpty(_actualText))
            {
                base.Text = new string(_passwordChar, _actualText.Length);
            }
            else
            {
                base.Text = text ?? string.Empty;
            }
            
            if (base.Text.EndsWith("\n"))
            {
                base.Text = base.Text.TrimEnd('\n', '\r');
            }
            
            _isUpdatingText = false;
        }

        public void ClearActualText()
        {
            _actualText = string.Empty;
        }

        public void SetPlaceholderMode(bool isPlaceholder)
        {
            _isPlaceholderMode = isPlaceholder;
            if (isPlaceholder)
            {
                _actualText = string.Empty;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT
                return cp;
            }
        }
    }

    [ToolboxItem(true)]
    public class VortexTextBox : Control
    {
        // Inner TextBox
        private TransparentTextBox _textBox;

        // Basic properties
        private int _cornerCutSize = 10;
        private bool _showBorder = true;
        private Color _borderColor = Color.FromArgb(64, 158, 255);
        private float _borderWidth = 2f;
        private Color _focusColor = Color.FromArgb(44, 115, 200);
        private Color _hoverColor = Color.FromArgb(54, 138, 230);
        private HorizontalAlignment _textAlign = HorizontalAlignment.Left; // Default text alignment
        
        // Border radius properties
        private int _borderRadius = 0;
        private bool _useRoundedCorners = false;

        // Placeholder text properties
        private string _placeholderText = "";
        private Color _placeholderColor = Color.Gray;
        private bool _isPlaceholderActive = false;

        // Animation properties
        private bool _isHovering = false;
        private bool _isFocused = false;
        private float _hoverAnimationProgress = 0f;
        private float _focusAnimationProgress = 0f;
        private int _hoverAnimationDuration = 150;
        private int _focusAnimationDuration = 150;

        // Animation cancellation tokens
        private CancellationTokenSource _hoverAnimationCts;
        private CancellationTokenSource _focusAnimationCts;

        // Flag to track if the control is being disposed
        private bool _isDisposing = false;

        // Store the forecolor to ensure it persists
        private Color _customForeColor;

        #region Properties

        [Category("Appearance")]
        [Description("Size of the cut corner in pixels")]
        public int CornerCutSize
        {
            get { return _cornerCutSize; }
            set
            {
                _cornerCutSize = value;
                UpdateControlRegion();
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Determines whether to show border around the textbox")]
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
        [Description("Color of the textbox border")]
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
        [Description("Width of the textbox border")]
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
        [Description("Color when textbox is focused")]
        public Color FocusColor
        {
            get { return _focusColor; }
            set
            {
                _focusColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color when mouse hovers over the textbox")]
        public Color HoverColor
        {
            get { return _hoverColor; }
            set
            {
                _hoverColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Text alignment within the textbox")]
        public HorizontalAlignment TextAlign
        {
            get { return _textAlign; }
            set
            {
                _textAlign = value;
                if (_textBox != null)
                {
                    _textBox.TextAlign = value;
                }
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Border radius in pixels (only applies when UseRoundedCorners is true)")]
        [DefaultValue(0)]
        public int BorderRadius
        {
            get { return _borderRadius; }
            set
            {
                _borderRadius = Math.Max(0, value);
                UpdateControlRegion();
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Use rounded corners instead of cut corner design")]
        [DefaultValue(false)]
        public bool UseRoundedCorners
        {
            get { return _useRoundedCorners; }
            set
            {
                _useRoundedCorners = value;
                UpdateControlRegion();
                Invalidate();
            }
        }

        [Category("Animation")]
        [Description("Duration of hover animation in milliseconds")]
        public int HoverAnimationDuration
        {
            get { return _hoverAnimationDuration; }
            set
            {
                _hoverAnimationDuration = value;
            }
        }

        [Category("Animation")]
        [Description("Duration of focus animation in milliseconds")]
        public int FocusAnimationDuration
        {
            get { return _focusAnimationDuration; }
            set
            {
                _focusAnimationDuration = value;
            }
        }

        [Category("Appearance")]
        [Description("The text displayed in the textbox")]
        public override string Text
        {
            get
            {
                // Don't return placeholder text as actual text
                if (_isPlaceholderActive && _textBox != null)
                    return string.Empty;
                return _textBox?.ActualText ?? string.Empty;
            }
            set
            {
                if (_textBox != null)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _isPlaceholderActive = false;
                        _textBox.ForeColor = _customForeColor;
                        _textBox.UseSystemPasswordChar = this.UseSystemPasswordChar;
                        _textBox.PasswordChar = this.PasswordChar;
                        _textBox.SetTextWithoutMask(value);
                    }
                    else
                    {
                        _textBox.SetTextWithoutMask(string.Empty);
                        SetPlaceholderIfEmpty();
                    }
                }
            }
        }

        [Category("Appearance")]
        [Description("The placeholder text displayed when the textbox is empty")]
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set
            {
                _placeholderText = value;
                SetPlaceholderIfEmpty();
            }
        }

        [Category("Appearance")]
        [Description("The color of the placeholder text")]
        public Color PlaceholderColor
        {
            get { return _placeholderColor; }
            set
            {
                _placeholderColor = value;
                if (_isPlaceholderActive && _textBox != null)
                {
                    _textBox.ForeColor = value;
                }
            }
        }

        [Category("Appearance")]
        [Description("The font used to display text in the textbox")]
        public override Font Font
        {
            get { return _textBox?.Font ?? base.Font; }
            set
            {
                base.Font = value;
                if (_textBox != null)
                    _textBox.Font = value;
            }
        }

        [Category("Appearance")]
        [Description("The foreground color used to display text")]
        public override Color ForeColor
        {
            get { return _customForeColor; }
            set
            {
                _customForeColor = value;
                base.ForeColor = value;
                if (_textBox != null && !_isPlaceholderActive)
                    _textBox.ForeColor = value;
            }
        }

        [Category("Behavior")]
        [Description("Indicates if the text in the textbox is read-only")]
        public bool ReadOnly
        {
            get { return _textBox?.ReadOnly ?? false; }
            set
            {
                if (_textBox != null)
                    _textBox.ReadOnly = value;
            }
        }

        [Category("Behavior")]
        [Description("Indicates if the textbox is multiline")]
        public bool Multiline
        {
            get { return _textBox?.Multiline ?? false; }
            set
            {
                if (_textBox != null)
                {
                    _textBox.Multiline = value;
                    UpdateTextBoxBounds();
                }
            }
        }

        [Category("Behavior")]
        [Description("Indicates if the textbox should use a password character")]
        public bool UseSystemPasswordChar
        {
            get { return _textBox?.UseSystemPasswordChar ?? false; }
            set
            {
                if (_textBox != null && !_isPlaceholderActive)
                    _textBox.UseSystemPasswordChar = value;
            }
        }

        [Category("Behavior")]
        [Description("The character used to mask password")]
        public char PasswordChar
        {
            get { return _textBox?.PasswordChar ?? '\0'; }
            set
            {
                if (_textBox != null && !_isPlaceholderActive)
                    _textBox.PasswordChar = value;
            }
        }

        [Category("Behavior")]
        [Description("Maximum length of text")]
        public int MaxLength
        {
            get { return _textBox?.MaxLength ?? 32767; }
            set
            {
                if (_textBox != null)
                    _textBox.MaxLength = value;
            }
        }

        [Category("Appearance")]
        [Description("Padding inside the textbox")]
        public new Padding Padding
        {
            get { return base.Padding; }
            set
            {
                base.Padding = value;
                UpdateTextBoxBounds();
            }
        }

        // Forward TextChanged event from inner TextBox
        public new event EventHandler TextChanged
        {
            add
            {
                if (_textBox != null && !_isDisposing)
                    _textBox.TextChanged += value;
            }
            remove
            {
                if (_textBox != null && !_isDisposing)
                    _textBox.TextChanged -= value;
            }
        }

        #endregion

        public VortexTextBox()
        {
            // Enable custom painting and transparency support FIRST
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            // Set default properties
            _customForeColor = Color.White; // Initialize with default color
            Size = new Size(260, 32);
            Padding = new Padding(10, 8, 10, 5);
            BackColor = Color.FromArgb(11, 14, 19); // Default background color

            // Create inner TextBox
            _textBox = new TransparentTextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = GetInnerBackColor(), // نفس لون خلفية الكنترول بدل Transparent (الـ TextBox الـ native مايدعمش شفافية)
                ForeColor = _customForeColor,
                Location = new Point(Padding.Left, Padding.Top),
                Size = new Size(Width - Padding.Left - Padding.Right, Height - Padding.Top - Padding.Bottom),
                TextAlign = _textAlign, // Set initial text alignment
                Multiline = false
            };

            try
            {
                _textBox.Font = new Font("Bahnschrift", 9);
            }
            catch
            {
                _textBox.Font = new Font("Segoe UI", 9);
            }

            // Hook up events
            _textBox.GotFocus += TextBox_GotFocus;
            _textBox.LostFocus += TextBox_LostFocus;
            _textBox.TextChanged += TextBox_TextChanged;
            _textBox.KeyDown += TextBox_KeyDown;
            _textBox.Click += TextBox_Click;

            // Add the TextBox to our control
            Controls.Add(_textBox);

            // اضبط شفافية الـ TextBox الداخلي حسب خلفية الكنترول
            RefreshInnerBackColor();

            // مخفي افتراضياً: بنرسم النص بإيدنا (شفاف). يظهر بس وقت الكتابة.
            _textBox.Visible = false;

            // Initialize the control region
            UpdateControlRegion();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateControlRegion();

            // Ensure ForeColor and TextAlign are applied
            if (_textBox != null)
            {
                if (!_isPlaceholderActive)
                    _textBox.ForeColor = _customForeColor;
                _textBox.TextAlign = _textAlign;

                // Set placeholder if needed
                SetPlaceholderIfEmpty();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateControlRegion();
            UpdateTextBoxBounds();
        }

        private void UpdateTextBoxBounds()
        {
            if (_textBox != null && !_isDisposing)
            {
                _textBox.Location = new Point(Padding.Left, Padding.Top);
                _textBox.Size = new Size(Width - Padding.Left - Padding.Right, Height - Padding.Top - Padding.Bottom);
            }
        }

        private bool IsDesign()
        {
            return DesignMode || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;
        }

        // الـ TextBox الـ native مش بيدعم خلفية شفافة فعلاً (بيرسم نفسه أبيض)،
        // فبنخلّي خلفيته لون غامق متناسق بدل الأبيض. (بنتجنّب المشي للفورم الأبيض)
        private Color GetInnerBackColor()
        {
            if (BackColor.A > 0)
                return BackColor;

            // دوّر على VortexPanel وخُد لونه المخصص الغامق
            Control p = Parent;
            while (p != null)
            {
                if (p is VortexPanel vp && vp.BackgroundColor.A > 0)
                    return vp.BackgroundColor;
                p = p.Parent;
            }
            // افتراضي غامق (مش بنرجّع لون الفورم عشان ميطلعش أبيض)
            return Color.FromArgb(11, 14, 19);
        }

        private void RefreshInnerBackColor()
        {
            if (_textBox == null || _isDisposing) return;

            // شفافية حقيقية (hollow brush) بتشتغل وقت التشغيل فقط؛ في الديزاينر بتطلع بيضا
            // فبنستخدم لون صلب غامق في الديزاينر، والشفافية في الـ runtime لما الخلفية شفافة.
            if (BackColor.A == 0 && !IsDesign())
            {
                _textBox.SetTransparentBackground(true);
                _textBox.BackColor = Color.Transparent;
            }
            else
            {
                _textBox.SetTransparentBackground(false);
                _textBox.BackColor = GetInnerBackColor();
            }
        }

        private void UpdateControlRegion()
        {
            if (Width > 0 && Height > 0 && IsHandleCreated && !IsDisposed && !_isDisposing)
            {
                try
                {
                    // Don't set region for transparent backgrounds - it interferes with transparency
                    if (BackColor.A == 0 || BackColor == Color.Transparent)
                    {
                        Region = null;
                    }
                    else
                    {
                        using (GraphicsPath path = CreateControlPath())
                        {
                            Region = new Region(path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating control region: {ex.Message}");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_isDisposing)
            {
                _isDisposing = true;

                // Cancel animations first
                CancelAnimations();

                // Remove event handlers
                if (_textBox != null)
                {
                    _textBox.GotFocus -= TextBox_GotFocus;
                    _textBox.LostFocus -= TextBox_LostFocus;
                    _textBox.TextChanged -= TextBox_TextChanged;
                    _textBox.KeyDown -= TextBox_KeyDown;
                    _textBox.Click -= TextBox_Click;

                    // Remove from controls collection before disposing
                    if (Controls.Contains(_textBox))
                    {
                        Controls.Remove(_textBox);
                    }

                    _textBox.Dispose();
                    _textBox = null;
                }
            }

            base.Dispose(disposing);
        }

        // Cancel any ongoing animations
        private void CancelAnimations()
        {
            try
            {
                if (_hoverAnimationCts != null)
                {
                    _hoverAnimationCts.Cancel();
                    _hoverAnimationCts.Dispose();
                    _hoverAnimationCts = null;
                }

                if (_focusAnimationCts != null)
                {
                    _focusAnimationCts.Cancel();
                    _focusAnimationCts.Dispose();
                    _focusAnimationCts = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling animations: {ex.Message}");
            }
        }

        #region Placeholder Methods

        // Method to set placeholder text if the textbox is empty
        private void SetPlaceholderIfEmpty()
        {
            if (_textBox == null || _isDisposing || _isPlaceholderActive)
                return;

            string actualText = _textBox.ActualText ?? string.Empty;
            bool isDesignMode = this.DesignMode || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;

            if (string.IsNullOrEmpty(actualText) && !string.IsNullOrEmpty(_placeholderText) && (isDesignMode || !_isFocused))
            {
                _isPlaceholderActive = true;
                _textBox.SetPlaceholderMode(true);
                _textBox.UseSystemPasswordChar = false;
                _textBox.PasswordChar = '\0';
                _textBox.ForeColor = _placeholderColor;
                _textBox.SetTextWithoutMask(_placeholderText);
            }
        }

        // Method to clear the placeholder text
        private void ClearPlaceholder()
        {
            if (_textBox == null || _isDisposing || !_isPlaceholderActive)
                return;

            _isPlaceholderActive = false;
            _textBox.SetPlaceholderMode(false);
            _textBox.ClearActualText();
            _textBox.ForeColor = _customForeColor;
            _textBox.UseSystemPasswordChar = this.UseSystemPasswordChar;
            _textBox.PasswordChar = this.PasswordChar;
            _textBox.SetTextWithoutMask(string.Empty);
        }

        #endregion

        #region Animation Methods

        private async Task AnimateHoverAsync(bool hovering, CancellationToken cancelToken)
        {
            try
            {
                float targetValue = hovering ? 1.0f : 0.0f;
                float stepSize = 0.1f;
                int stepDuration = Math.Max(10, _hoverAnimationDuration / 10);

                while (!cancelToken.IsCancellationRequested && !_isDisposing)
                {
                    if ((hovering && _hoverAnimationProgress >= targetValue) ||
                        (!hovering && _hoverAnimationProgress <= targetValue))
                    {
                        break;
                    }

                    _hoverAnimationProgress = hovering
                        ? Math.Min(_hoverAnimationProgress + stepSize, targetValue)
                        : Math.Max(_hoverAnimationProgress - stepSize, targetValue);

                    if (IsHandleCreated && !IsDisposed && !_isDisposing)
                    {
                        try
                        {
                            BeginInvoke(new Action(() =>
                            {
                                if (!IsDisposed && !_isDisposing)
                                    Invalidate();
                            }));
                        }
                        catch
                        {
                            // Control might be disposed between the check and the BeginInvoke
                            break;
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
                System.Diagnostics.Debug.WriteLine($"Hover animation error: {ex.Message}");
            }
        }

        private async Task AnimateFocusAsync(bool focused, CancellationToken cancelToken)
        {
            try
            {
                float targetValue = focused ? 1.0f : 0.0f;
                float stepSize = 0.1f;
                int stepDuration = Math.Max(10, _focusAnimationDuration / 10);

                while (!cancelToken.IsCancellationRequested && !_isDisposing)
                {
                    if ((focused && _focusAnimationProgress >= targetValue) ||
                        (!focused && _focusAnimationProgress <= targetValue))
                    {
                        break;
                    }

                    _focusAnimationProgress = focused
                        ? Math.Min(_focusAnimationProgress + stepSize, targetValue)
                        : Math.Max(_focusAnimationProgress - stepSize, targetValue);

                    if (IsHandleCreated && !IsDisposed && !_isDisposing)
                    {
                        try
                        {
                            BeginInvoke(new Action(() =>
                            {
                                if (!IsDisposed && !_isDisposing)
                                    Invalidate();
                            }));
                        }
                        catch
                        {
                            // Control might be disposed between the check and the BeginInvoke
                            break;
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
                System.Diagnostics.Debug.WriteLine($"Focus animation error: {ex.Message}");
            }
        }

        // Helper method to interpolate between colors
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

        private void TextBox_GotFocus(object sender, EventArgs e)
        {
            if (_isDisposing) return;

            _isFocused = true;

            // Clear placeholder when focused
            if (_isPlaceholderActive)
            {
                ClearPlaceholder();
            }

            try
            {
                // Cancel previous animation if running
                if (_focusAnimationCts != null)
                {
                    _focusAnimationCts.Cancel();
                    _focusAnimationCts.Dispose();
                }

                _focusAnimationCts = new CancellationTokenSource();

                // Start new animation
                _ = AnimateFocusAsync(true, _focusAnimationCts.Token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GotFocus: {ex.Message}");
            }

            OnGotFocus(e);
        }

        private void TextBox_LostFocus(object sender, EventArgs e)
        {
            if (_isDisposing) return;

            _isFocused = false;

            // Show placeholder if needed when focus is lost
            SetPlaceholderIfEmpty();

            // خلّصنا كتابة -> اخفي التكست بوكس وارسم النص بإيدنا (شفاف فوق الجراديانت)
            HideEditor();

            try
            {
                // Cancel previous animation if running
                if (_focusAnimationCts != null)
                {
                    _focusAnimationCts.Cancel();
                    _focusAnimationCts.Dispose();
                }

                _focusAnimationCts = new CancellationTokenSource();

                // Start new animation
                _ = AnimateFocusAsync(false, _focusAnimationCts.Token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LostFocus: {ex.Message}");
            }

            OnLostFocus(e);
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (_isDisposing || _textBox == null) return;

            // حدّث الرسم اليدوي للنص
            Invalidate();

            // Only raise TextChanged event if we're not showing placeholder text
            if (!_isPlaceholderActive)
            {
                OnTextChanged(e);
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isDisposing || _textBox == null) return;

            // Clear placeholder when user starts typing
            if (_isPlaceholderActive)
            {
                // Only clear if it's an input key (not Ctrl, Alt, Shift, etc.)
                if (!e.Control && !e.Alt &&
                    e.KeyCode != Keys.ControlKey && e.KeyCode != Keys.ShiftKey &&
                    e.KeyCode != Keys.Menu && e.KeyCode != Keys.CapsLock)
                {
                    ClearPlaceholder();
                }
            }
        }

        private void TextBox_Click(object sender, EventArgs e)
        {
            if (_isDisposing || _textBox == null) return;

            // Clear placeholder when user clicks the textbox
            if (_isPlaceholderActive)
            {
                ClearPlaceholder();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (_isDisposing) return;

            _isHovering = true;

            try
            {
                // Cancel previous animation if running
                if (_hoverAnimationCts != null)
                {
                    _hoverAnimationCts.Cancel();
                    _hoverAnimationCts.Dispose();
                }

                _hoverAnimationCts = new CancellationTokenSource();

                // Start new animation
                _ = AnimateHoverAsync(true, _hoverAnimationCts.Token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MouseEnter: {ex.Message}");
            }

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (_isDisposing) return;

            _isHovering = false;

            try
            {
                // Cancel previous animation if running
                if (_hoverAnimationCts != null)
                {
                    _hoverAnimationCts.Cancel();
                    _hoverAnimationCts.Dispose();
                }

                _hoverAnimationCts = new CancellationTokenSource();

                // Start new animation
                _ = AnimateHoverAsync(false, _hoverAnimationCts.Token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MouseLeave: {ex.Message}");
            }

            base.OnMouseLeave(e);
        }

        // Forward focus to the inner TextBox
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (!_isDisposing && _textBox != null && !_textBox.Focused)
            {
                try
                {
                    ShowEditor();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error focusing textbox: {ex.Message}");
                }
            }
        }

        // لما الكنترول ياخد فوكس بالكيبورد (Tab) نظهر محرر الكتابة
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            if (!_isDisposing) ShowEditor();
        }

        // Override background color to also update the inner TextBox
        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                base.BackColor = value;
                RefreshInnerBackColor();
                // Update region based on transparency
                UpdateControlRegion();
                Invalidate();
            }
        }

        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_isDisposing) return;

            try
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

                // If background is transparent, paint parent background first
                if (BackColor.A == 0 && Parent != null && VortexDesign.IsDesignerProcess)
                {
                    using (var brush = new SolidBrush(Parent.BackColor.A > 0 ? Parent.BackColor : System.Drawing.SystemColors.Control))
                        e.Graphics.FillRectangle(brush, ClientRectangle);
                }
                else if (BackColor.A == 0 && Parent != null)
                {
                    using (var bmp = new Bitmap(Width, Height))
                    {
                        using (var g = Graphics.FromImage(bmp))
                        {
                            g.TranslateTransform(-Left, -Top);
                            var pe = new PaintEventArgs(g, e.ClipRectangle);
                            InvokePaintBackground(Parent, pe);
                            InvokePaint(Parent, pe);
                            g.TranslateTransform(Left, Top);
                        }
                        e.Graphics.DrawImageUnscaled(bmp, 0, 0);
                    }
                }

                // Create textbox path with the special shape
                using (GraphicsPath path = CreateControlPath())
                {
                    // Calculate current border color based on animation progress
                    Color currentBorderColor = _borderColor;

                    if (_hoverAnimationProgress > 0)
                    {
                        currentBorderColor = InterpolateColor(currentBorderColor, _hoverColor, _hoverAnimationProgress);
                    }

                    if (_focusAnimationProgress > 0)
                    {
                        currentBorderColor = InterpolateColor(currentBorderColor, _focusColor, _focusAnimationProgress);
                    }

                    // Draw background (only if not fully transparent)
                    if (BackColor.A > 0)
                    {
                        using (SolidBrush backgroundBrush = new SolidBrush(BackColor))
                        {
                            e.Graphics.FillPath(backgroundBrush, path);
                        }
                    }

                    // Draw border if enabled
                    if (_showBorder && _borderWidth > 0)
                    {
                        using (Pen borderPen = new Pen(currentBorderColor, _borderWidth))
                        {
                            borderPen.LineJoin = LineJoin.Round;
                            e.Graphics.DrawPath(borderPen, path);
                        }
                    }
                }

                // owner-draw للنص لما التكست بوكس مخفي (مش بنكتب) عشان يبان شفاف فوق الجراديانت
                DrawDisplayText(e.Graphics);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnPaint: {ex.Message}");
            }
        }

        // يرسم النص/الـ placeholder يدوياً فوق الخلفية الشفافة (بدل الاعتماد على خلفية الـ TextBox)
        private void DrawDisplayText(Graphics g)
        {
            if (_textBox == null || _textBox.Visible) return;

            string display;
            Color color;
            if (_isPlaceholderActive)
            {
                display = _placeholderText;
                color = _placeholderColor;
            }
            else
            {
                display = _textBox.ActualText ?? string.Empty;
                if (_textBox.UseSystemPasswordChar && !string.IsNullOrEmpty(display))
                {
                    char pc = _textBox.PasswordChar == '\0' ? '●' : _textBox.PasswordChar;
                    display = new string(pc, display.Length);
                }
                color = _customForeColor;
            }

            if (string.IsNullOrEmpty(display)) return;

            Rectangle r = new Rectangle(_textBox.Left, _textBox.Top, _textBox.Width, _textBox.Height);
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter |
                                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
            if (_textAlign == HorizontalAlignment.Center) flags |= TextFormatFlags.HorizontalCenter;
            else if (_textAlign == HorizontalAlignment.Right) flags |= TextFormatFlags.Right;

            TextRenderer.DrawText(g, display, _textBox.Font, r, color, flags);
        }

        // إظهار/إخفاء الـ TextBox: مخفي = بنرسم النص بإيدنا (شفاف)، ظاهر = وضع الكتابة
        private void ShowEditor()
        {
            if (_textBox == null || _isDisposing) return;
            if (!_textBox.Visible) _textBox.Visible = true;
            try { _textBox.Focus(); } catch { }
        }

        private void HideEditor()
        {
            if (_textBox == null || _isDisposing) return;
            if (_textBox.Visible) _textBox.Visible = false;
            Invalidate();
        }

        private GraphicsPath CreateControlPath()
        {
            GraphicsPath path = new GraphicsPath();

            try
            {
                if (_useRoundedCorners)
                {
                    // Use rounded corners
                    path = CreateRoundedPath(new Rectangle(0, 0, Width, Height), _borderRadius);
                }
                else
                {
                    // Use original cut corner design
                    PointF[] points = new PointF[]
                    {
                        new PointF(0, 0),                                // Top left
                        new PointF(Width, 0),                            // Top right
                        new PointF(Width, Height - _cornerCutSize),      // Right before cut
                        new PointF(Width - _cornerCutSize, Height),      // Bottom after cut
                        new PointF(0, Height)                            // Bottom left
                    };

                    path.AddPolygon(points);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating control path: {ex.Message}");

                // Create a simple rectangle as fallback
                path.AddRectangle(new Rectangle(0, 0, Width, Height));
            }

            return path;
        }

        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            // Ensure radius doesn't exceed half of the smallest dimension
            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);

            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(rect.Location, size);

            // Top left arc
            path.AddArc(arc, 180, 90);

            // Top right arc
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right arc
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left arc
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        // Override CreateParams - only use WS_EX_TRANSPARENT when background is transparent
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Only set transparent flag when actually using transparent background
                if (BackColor.A == 0 || BackColor == Color.Transparent)
                {
                    cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT
                }
                return cp;
            }
        }

        // Override OnParentChanged to handle parent changes safely
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            // Update the control region when parent changes
            if (!_isDisposing)
            {
                UpdateControlRegion();
            }
        }

        // Override OnParentBackColorChanged to handle parent color changes
        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);

            // Redraw when parent background changes
            if (!_isDisposing)
            {
                // لو الكنترول شفاف، حدّث لون الـ TextBox الداخلي حسب الـ parent الجديد
                if (BackColor.A == 0)
                    RefreshInnerBackColor();

                Invalidate();
                // If we have a transparent background, also invalidate parent
                if (BackColor.A == 0 && Parent != null)
                {
                    Parent.Invalidate(Bounds, true);
                }
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            
            // If background becomes transparent, invalidate parent
            if (BackColor.A == 0 && Parent != null)
            {
                Parent.Invalidate(Bounds, true);
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
        }

        // Override OnForeColorChanged to ensure the TextBox ForeColor is updated
        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);

            // Update the TextBox ForeColor only if we're not showing placeholder
            if (_textBox != null && !_isDisposing && !_isPlaceholderActive)
            {
                _textBox.ForeColor = ForeColor;
            }
        }

        // Override SetBoundsCore to ensure minimum size
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // Ensure minimum size
            int minWidth = 50;
            int minHeight = 20;

            if (width < minWidth) width = minWidth;
            if (height < minHeight) height = minHeight;

            base.SetBoundsCore(x, y, width, height, specified);
        }
    }
}