using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Vortex.Ui
{
    // Custom RichTextBox for transparent background support
    internal class TransparentRichTextBox : RichTextBox
    {

        private const int WM_LBUTTONDBLCLK = 0x0203;

        private char _passwordChar = '\0';
        private const char DefaultPasswordChar = '•';
        private bool _usePasswordChar = false;
        private string _actualText = string.Empty;
        private bool _isUpdatingText = false;
        private bool _isPlaceholderMode = false;

        public char PasswordChar
        {
            get { return _passwordChar; }
            set
            {
                _passwordChar = value == '\0' ? DefaultPasswordChar : value;
                if (_usePasswordChar && !string.IsNullOrEmpty(_actualText))
                {
                    _isUpdatingText = true;
                    base.Text = new string(_passwordChar, _actualText.Length);
                    _isUpdatingText = false;
                }
            }
        }

        public bool UseSystemPasswordChar
        {
            get { return _usePasswordChar; }
            set
            {
                _usePasswordChar = value;
                if (value && _passwordChar == '\0')
                {
                    _passwordChar = DefaultPasswordChar;
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

        private HorizontalAlignment _textAlignment = HorizontalAlignment.Left;

        public new HorizontalAlignment SelectionAlignment
        {
            get
            {
                return _textAlignment;
            }
            set
            {
                _textAlignment = value;
                this.SelectAll();
                base.SelectionAlignment = value;
                this.Select(this.TextLength, 0);
            }
        }

        public TransparentRichTextBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BorderStyle = BorderStyle.None;
            BackColor = Color.Transparent;
            ScrollBars = RichTextBoxScrollBars.None;
            WordWrap = false;
        }

        protected override void WndProc(ref Message m)
        {
            // Intercept double-click to select all actual text only
            if (m.Msg == WM_LBUTTONDBLCLK)
            {
                // Don't let base handle it - we'll do it ourselves
                int actualLength = _actualText.Length;
                
                if (actualLength > 0)
                {
                    // Select all actual text (not including RichTextBox formatting)
                    SelectionStart = 0;
                    SelectionLength = actualLength;
                }
                else
                {
                    // Empty - no selection
                    SelectionStart = 0;
                    SelectionLength = 0;
                }
                
                // Don't call base.WndProc for double-click
                return;
            }
            
            base.WndProc(ref m);
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
                string currentText = base.Text.TrimEnd('\n', '\r'); // Remove trailing newlines
                
                // Update actual text based on changes
                if (currentText.Length > _actualText.Length)
                {
                    // Text was added
                    int insertPos = Math.Max(0, selStart - (currentText.Length - _actualText.Length));
                    if (insertPos <= _actualText.Length)
                    {
                        string addedText = currentText.Substring(insertPos, currentText.Length - _actualText.Length);
                        _actualText = _actualText.Insert(insertPos, addedText);
                    }
                }
                else if (currentText.Length < _actualText.Length)
                {
                    // Text was removed
                    int removeCount = _actualText.Length - currentText.Length;
                    int removePos = Math.Max(0, selStart);
                    if (removePos < _actualText.Length)
                    {
                        _actualText = _actualText.Remove(removePos, Math.Min(removeCount, _actualText.Length - removePos));
                    }
                }
                
                // Update display
                base.Text = new string(_passwordChar, _actualText.Length);
                SelectionStart = Math.Min(selStart, base.Text.Length);
                _isUpdatingText = false;
            }
            else
            {
                // Update actual text - remove any RichTextBox formatting characters
                string cleanText = base.Text;
                // Remove trailing newlines and any other control characters
                cleanText = cleanText.TrimEnd('\n', '\r', '\0');
                _actualText = cleanText;
            }
            
            base.OnTextChanged(e);
        }



        public void SetTextWithoutMask(string text)
        {
            _isUpdatingText = true;
            _actualText = text ?? string.Empty;
            
            // If password mode is active, show masked text, otherwise show actual text
            if (_usePasswordChar && _passwordChar != '\0' && !string.IsNullOrEmpty(_actualText))
            {
                base.Text = new string(_passwordChar, _actualText.Length);
            }
            else
            {
                base.Text = text ?? string.Empty;
            }
            
            // Remove any trailing newline that RichTextBox adds
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
    public class VortexRoundTextBox : Control
    {
        private const int DefaultMaxLength = int.MaxValue;

        [Category("Appearance")]
        [Description("Radius of the rounded corners in pixels")]
        public int CornerRadius
        {
            get
            {
                return this._cornerRadius;
            }
            set
            {
                this._cornerRadius = Math.Max(0, value);
                this.UpdateControlRegion();
                base.Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Background color of the inner TextBox area")]
        public Color FillColor
        {
            get
            {
                return this._fillColor;
            }
            set
            {
                this._fillColor = value;
                if (this._textBox != null && !this._isDisposing)
                {
                    // RichTextBox supports transparent background
                    this._textBox.BackColor = Color.Transparent;
                }
                base.Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Text alignment within the textbox")]
        public HorizontalAlignment TextAlign
        {
            get
            {
                return this._textAlign;
            }
            set
            {
                this._textAlign = value;
                if (this._textBox != null)
                {
                    this._textBox.SelectionAlignment = value;
                }
                base.Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The text content of the control.")]
        public override string Text
        {
            get
            {
                if (this._textBox == null)
                {
                    return string.Empty;
                }
                
                // Always return empty if placeholder is active
                if (this._isPlaceholderActive)
                {
                    return string.Empty;
                }
                
                // Return actual text (handles password mode internally)
                return this._textBox.ActualText ?? string.Empty;
            }
            set
            {
                if (this._textBox != null && !this._isDisposing)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        this._isPlaceholderActive = false;
                        this._textBox.ForeColor = this._customForeColor;
                        this._textBox.UseSystemPasswordChar = this.UseSystemPasswordChar;
                        this._textBox.PasswordChar = this.PasswordChar;
                        this._textBox.SetTextWithoutMask(value ?? string.Empty);
                        return;
                    }
                    this._textBox.SetTextWithoutMask(string.Empty);
                    if (!this._isFocused && !string.IsNullOrEmpty(this._placeholderText))
                    {
                        this.SetPlaceholderIfEmpty();
                    }
                }
            }
        }

        [Category("Appearance")]
        [Description("The placeholder text displayed when the textbox is empty")]
        public string PlaceholderText
        {
            get
            {
                return this._placeholderText;
            }
            set
            {
                this._placeholderText = value;
                if (this._textBox != null && !this._isDisposing)
                {
                    if (this._isPlaceholderActive)
                    {
                        // Update the placeholder text if it's currently showing
                        this._suppressTextEvents = true;
                        this._textBox.SetPlaceholderMode(true);
                        this._textBox.SetTextWithoutMask(this._placeholderText);
                        this._textBox.ForeColor = this._placeholderColor;
                        this._suppressTextEvents = false;
                    }
                    else if (string.IsNullOrEmpty(this._textBox.ActualText))
                    {
                        // Show placeholder if textbox is empty (for design mode)
                        this.SetPlaceholderIfEmpty();
                    }
                }
                base.Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The color of the placeholder text")]
        public Color PlaceholderColor
        {
            get
            {
                return this._placeholderColor;
            }
            set
            {
                this._placeholderColor = value;
                if (this._isPlaceholderActive && this._textBox != null)
                {
                    this._textBox.ForeColor = value;
                }
            }
        }

        [Category("Appearance")]
        [Description("The font used to display text in the textbox")]
        public override Font Font
        {
            get
            {
                TransparentRichTextBox textBox = this._textBox;
                return ((textBox != null) ? textBox.Font : null) ?? base.Font;
            }
            set
            {
                base.Font = value;
                if (this._textBox != null)
                {
                    this._textBox.Font = value;
                    this._textBox.SelectAll();
                    this._textBox.SelectionFont = value;
                    this._textBox.Select(this._textBox.TextLength, 0);
                }
            }
        }

        [Category("Appearance")]
        [Description("The foreground color used to display text")]
        public override Color ForeColor
        {
            get
            {
                return this._customForeColor;
            }
            set
            {
                this._customForeColor = value;
                base.ForeColor = value;
                if (this._textBox != null && !this._isPlaceholderActive)
                {
                    this._textBox.ForeColor = value;
                }
            }
        }

        [Category("Behavior")]
        [Description("Indicates if the text in the textbox is read-only")]
        public bool ReadOnly
        {
            get
            {
                TransparentRichTextBox textBox = this._textBox;
                return textBox != null && textBox.ReadOnly;
            }
            set
            {
                if (this._textBox != null)
                {
                    this._textBox.ReadOnly = value;
                }
            }
        }

        [Category("Behavior")]
        [Description("Indicates if the textbox is multiline")]
        [DefaultValue(false)]
        public bool Multiline
        {
            get
            {
                return this._multiline;
            }
            set
            {
                this._multiline = value;
                if (this._textBox != null)
                {
                    this._textBox.Multiline = value;
                    this._textBox.WordWrap = value;
                    if (value)
                    {
                        this._textBox.ScrollBars = RichTextBoxScrollBars.Vertical;
                    }
                    else
                    {
                        this._textBox.ScrollBars = RichTextBoxScrollBars.None;
                    }
                    this.UpdateTextBoxBounds();
                }
            }
        }

        public bool ShouldSerializeMultiline()
        {
            return this._multiline;
        }

        public void ResetMultiline()
        {
            Multiline = false;
        }

        [Category("Behavior")]
        [Description("Indicates if the textbox should use a password character")]
        public bool UseSystemPasswordChar
        {
            get
            {
                return this._savedPasswordState;
            }
            set
            {
                this._savedPasswordState = value;
                if (this._textBox != null && !this._isPlaceholderActive)
                {
                    this._textBox.UseSystemPasswordChar = value;
                }
            }
        }

        [Category("Behavior")]
        [Description("The character used to mask password")]
        public char PasswordChar
        {
            get
            {
                return this._savedPasswordChar;
            }
            set
            {
                this._savedPasswordChar = value;
                if (this._textBox != null && !this._isPlaceholderActive)
                {
                    this._textBox.PasswordChar = value;
                }
            }
        }

        [Category("Behavior")]
        [Description("Maximum length of text")]
        [DefaultValue(DefaultMaxLength)]
        public int MaxLength
        {
            get
            {
                return this._maxLength;
            }
            set
            {
                this._maxLength = value;
                if (this._textBox != null)
                {
                    this._textBox.MaxLength = value;
                }
            }
        }

        public bool ShouldSerializeMaxLength()
        {
            return this._maxLength != DefaultMaxLength;
        }

        public void ResetMaxLength()
        {
            MaxLength = DefaultMaxLength;
        }

        [Category("Appearance")]
        [Description("Padding inside the textbox")]
        public new Padding Padding
        {
            get
            {
                return base.Padding;
            }
            set
            {
                base.Padding = value;
                this.UpdateTextBoxBounds();
            }
        }

        [Category("Appearance")]
        [Description("Icon to display in the textbox")]
        public Image Icon
        {
            get
            {
                return this._icon;
            }
            set
            {
                if (this._icon != null)
                {
                    this._icon.Dispose();
                }
                this._icon = ((value != null) ? new Bitmap(value) : null);
                this.UpdateTextBoxBounds();
                base.Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Alignment of the icon within the textbox")]
        public ContentAlignment IconAlignment
        {
            get
            {
                return this._iconAlignment;
            }
            set
            {
                this._iconAlignment = value;
                this.UpdateTextBoxBounds();
                base.Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Horizontal offset for the icon position")]
        public int IconOffsetX
        {
            get
            {
                return this._iconOffsetX;
            }
            set
            {
                this._iconOffsetX = value;
                this.UpdateTextBoxBounds();
                base.Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Vertical offset for the icon position")]
        public int IconOffsetY
        {
            get
            {
                return this._iconOffsetY;
            }
            set
            {
                this._iconOffsetY = value;
                this.UpdateTextBoxBounds();
                base.Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Horizontal offset for the text position")]
        public int TextOffsetX
        {
            get
            {
                return this._textOffsetX;
            }
            set
            {
                this._textOffsetX = value;
                this.UpdateTextBoxBounds();
                base.Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Vertical offset for the text position")]
        public int TextOffsetY
        {
            get
            {
                return this._textOffsetY;
            }
            set
            {
                this._textOffsetY = value;
                this.UpdateTextBoxBounds();
                base.Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Size of the icon in pixels (Width, Height)")]
        public Size IconSize
        {
            get
            {
                return new Size(this._iconWidth, this._iconHeight);
            }
            set
            {
                this._iconWidth = Math.Max(1, value.Width);
                this._iconHeight = Math.Max(1, value.Height);
                this.UpdateTextBoxBounds();
                base.Invalidate();
            }
        }

        public new event EventHandler TextChanged
        {
            add
            {
                if (this._textBox != null && !this._isDisposing)
                {
                    this._textBox.TextChanged += value;
                }
            }
            remove
            {
                if (this._textBox != null && !this._isDisposing)
                {
                    this._textBox.TextChanged -= value;
                }
            }
        }

        public VortexRoundTextBox()
        {
            this.BackColor = Color.Transparent;
            this._customForeColor = Color.White;
            base.Size = new Size(260, 34);
            this.Padding = new Padding(10, 8, 10, 5);
            base.TabStop = false;
            this._textBox = new TransparentRichTextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.Transparent,
                ForeColor = this._customForeColor,
                Location = new Point(this.Padding.Left, this.Padding.Top),
                Size = new Size(base.Width - this.Padding.Left - this.Padding.Right, base.Height - this.Padding.Top - this.Padding.Bottom),
                TabStop = true,
                Multiline = this._multiline,
                MaxLength = this._maxLength
            };
            try
            {
                this._textBox.Font = new Font("Bahnschrift", 9f);
            }
            catch
            {
                this._textBox.Font = new Font("Segoe UI", 9f);
            }
            this._textBox.GotFocus += this.TextBox_GotFocus;
            this._textBox.LostFocus += this.TextBox_LostFocus;
            this._textBox.TextChanged += this.TextBox_TextChanged;
            this._textBox.KeyDown += this.TextBox_KeyDown;
            this._textBox.KeyPress += this.TextBox_KeyPress;
            this._textBox.KeyUp += this.TextBox_KeyUp;
            this._textBox.Click += this.TextBox_Click;
            base.Click += this.VortexRoundTextBox_Click;
            base.MouseDown += this.VortexRoundTextBox_MouseDown;
            base.Controls.Add(this._textBox);
            base.SetStyle(ControlStyles.UserPaint, true);
            base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            base.SetStyle(ControlStyles.ResizeRedraw, true);
            base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.UpdateControlRegion();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.UpdateControlRegion();
            if (this._textBox != null)
            {
                this._textBox.SelectionAlignment = this._textAlign;
                
                // Only show placeholder if there's no actual text
                if (string.IsNullOrEmpty(this._textBox.ActualText) && 
                    !string.IsNullOrEmpty(this._placeholderText))
                {
                    this.SetPlaceholderIfEmpty();
                }
                else if (!this._isPlaceholderActive)
                {
                    // Set normal text color only if placeholder is not active
                    this._textBox.ForeColor = this._customForeColor;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.UpdateControlRegion();
            this.UpdateTextBoxBounds();
        }

        private void UpdateTextBoxBounds()
        {
            if (this._textBox != null && !this._isDisposing)
            {
                int num = 16;
                Font font = this._textBox.Font;
                int num2 = Math.Max(num, ((font != null) ? font.Height : this.Font.Height) + 2);
                int num3 = 0;
                if (this._icon != null)
                {
                    num3 = this._iconWidth + Math.Abs(this._iconOffsetX) + 5;
                }
                int num4 = base.Width - this.Padding.Left - this.Padding.Right - num3;
                int num5 = Math.Max(0, num4 - Math.Abs(this._textOffsetX));
                int num6 = base.Height - this.Padding.Top - this.Padding.Bottom;
                int num7 = Math.Max(num2, num6 - Math.Abs(this._textOffsetY));
                int num8 = this.Padding.Left + this._textOffsetX;
                int num9 = this.Padding.Top + this._textOffsetY + (num6 - num7) / 2;
                if (this._icon != null)
                {
                    ContentAlignment iconAlignment = this._iconAlignment;
                    if (iconAlignment <= ContentAlignment.MiddleLeft)
                    {
                        if (iconAlignment != ContentAlignment.TopLeft)
                        {
                            if (iconAlignment == ContentAlignment.TopRight)
                            {
                                goto IL_0156;
                            }
                            if (iconAlignment != ContentAlignment.MiddleLeft)
                            {
                                goto IL_0156;
                            }
                        }
                    }
                    else
                    {
                        if (iconAlignment == ContentAlignment.MiddleRight)
                        {
                            goto IL_0156;
                        }
                        if (iconAlignment != ContentAlignment.BottomLeft)
                        {
                            if (iconAlignment != ContentAlignment.BottomRight)
                            {
                                goto IL_0156;
                            }
                            goto IL_0156;
                        }
                    }
                    num8 += num3;
                }
            IL_0156:
                this._textBox.Location = new Point(num8, num9);
                this._textBox.Size = new Size(num5, num7);
                this._textBox.BringToFront();
            }
        }

        private void UpdateControlRegion()
        {
            base.Region = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !this._isDisposing)
            {
                this._isDisposing = true;
                if (this._textBox != null)
                {
                    this._textBox.GotFocus -= this.TextBox_GotFocus;
                    this._textBox.LostFocus -= this.TextBox_LostFocus;
                    this._textBox.TextChanged -= this.TextBox_TextChanged;
                    this._textBox.KeyDown -= this.TextBox_KeyDown;
                    this._textBox.KeyPress -= this.TextBox_KeyPress;
                    this._textBox.KeyUp -= this.TextBox_KeyUp;
                    this._textBox.Click -= this.TextBox_Click;
                    if (base.Controls.Contains(this._textBox))
                    {
                        base.Controls.Remove(this._textBox);
                    }
                    this._textBox.Dispose();
                    this._textBox = null;
                }
                base.Click -= this.VortexRoundTextBox_Click;
                base.MouseDown -= this.VortexRoundTextBox_MouseDown;
            }
            base.Dispose(disposing);
        }

        private void SetPlaceholderIfEmpty()
        {
            if (this._textBox == null || this._isDisposing || this._isPlaceholderActive)
            {
                return;
            }
            
            // Double check: only show placeholder if ActualText is truly empty
            string actualText = this._textBox.ActualText ?? string.Empty;
            
            // In design mode, always show placeholder if text is empty
            bool isDesignMode = this.DesignMode || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;
            
            if (string.IsNullOrEmpty(actualText) && 
                !string.IsNullOrEmpty(this._placeholderText) && 
                (isDesignMode || !this._isFocused))
            {
                this._suppressTextEvents = true;
                this._isPlaceholderActive = true;
                this._textBox.SetPlaceholderMode(true);
                this._textBox.UseSystemPasswordChar = false;
                this._textBox.PasswordChar = '\0';
                this._textBox.ForeColor = this._placeholderColor;
                this._textBox.SetTextWithoutMask(this._placeholderText);
                this._suppressTextEvents = false;
            }
        }

        private void ClearPlaceholder()
        {
            if (this._textBox == null || this._isDisposing || !this._isPlaceholderActive)
            {
                return;
            }
            
            this._suppressTextEvents = true;
            this._isPlaceholderActive = false;
            
            // Disable placeholder mode and clear everything
            this._textBox.SetPlaceholderMode(false);
            this._textBox.ClearActualText();
            
            // Restore password mode and color BEFORE clearing text
            this._textBox.ForeColor = this._customForeColor;
            this._textBox.UseSystemPasswordChar = this.UseSystemPasswordChar;
            this._textBox.PasswordChar = this.PasswordChar;
            
            // Clear the displayed text
            this._textBox.SetTextWithoutMask(string.Empty);
            
            this._suppressTextEvents = false;
        }

        private Color InterpolateColor(Color color1, Color color2, float ratio)
        {
            ratio = Math.Max(0f, Math.Min(1f, ratio));
            int num = (int)((float)color1.R + (float)(color2.R - color1.R) * ratio);
            int num2 = (int)((float)color1.G + (float)(color2.G - color1.G) * ratio);
            int num3 = (int)((float)color1.B + (float)(color2.B - color1.B) * ratio);
            return Color.FromArgb(num, num2, num3);
        }

        private void TextBox_GotFocus(object sender, EventArgs e)
        {
            this._isFocused = true;
            
            // Clear placeholder when textbox gets focus
            if (this._isPlaceholderActive)
            {
                this.ClearPlaceholder();
            }
            
            base.Invalidate();
        }

        private void TextBox_LostFocus(object sender, EventArgs e)
        {
            this._isFocused = false;
            base.Invalidate();
            
            // Show placeholder if textbox is empty
            if (string.IsNullOrEmpty(this._textBox.ActualText) && !this._isPlaceholderActive)
            {
                this.SetPlaceholderIfEmpty();
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (this._isDisposing || this._suppressTextEvents)
            {
                return;
            }
            
            // If placeholder is active and text changed, clear it
            if (this._isPlaceholderActive)
            {
                // Check if the text is different from placeholder
                string currentText = this._textBox.Text;
                if (currentText != this._placeholderText)
                {
                    this.ClearPlaceholder();
                }
                return;
            }
            
            // Set placeholder if text becomes empty (whether focused or not)
            if (string.IsNullOrEmpty(this._textBox.ActualText))
            {
                if (!this._isFocused)
                {
                    this.SetPlaceholderIfEmpty();
                }
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (this._isDisposing || this._textBox == null)
            {
                return;
            }
            
            // Clear placeholder when user starts typing (but not for modifier keys)
            if (this._isPlaceholderActive)
            {
                // Check if it's a character key or backspace/delete
                bool isCharacterKey = !e.Control && !e.Alt && 
                                     e.KeyCode != Keys.ControlKey && 
                                     e.KeyCode != Keys.ShiftKey && 
                                     e.KeyCode != Keys.Menu && 
                                     e.KeyCode != Keys.Capital &&
                                     e.KeyCode != Keys.Tab;
                
                if (isCharacterKey)
                {
                    // Clear placeholder immediately before the key is processed
                    this._suppressTextEvents = true;
                    this._isPlaceholderActive = false;
                    this._textBox.Text = string.Empty;
                    this._textBox.ClearActualText();
                    this._textBox.ForeColor = this._customForeColor;
                    this._textBox.UseSystemPasswordChar = this.UseSystemPasswordChar;
                    this._textBox.PasswordChar = this.PasswordChar;
                    this._suppressTextEvents = false;
                }
            }
            
            this.OnKeyDown(e);
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (this._isDisposing || this._textBox == null)
            {
                return;
            }
            this.OnKeyPress(e);
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (this._isDisposing || this._textBox == null)
            {
                return;
            }
            this.OnKeyUp(e);
        }

        private void TextBox_Click(object sender, EventArgs e)
        {
            // Placeholder will be cleared when user starts typing (in KeyDown)
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
        }

        private void VortexRoundTextBox_Click(object sender, EventArgs e)
        {
            if (this._textBox != null && !this._isDisposing)
            {
                this._textBox.Focus();
            }
        }

        private void VortexRoundTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (this._textBox != null && !this._isDisposing)
            {
                this._textBox.Focus();
            }
        }

        public new bool Focus()
        {
            if (this._textBox != null && !this._isDisposing)
            {
                return this._textBox.Focus();
            }
            return base.Focus();
        }

        public override Color BackColor
        {
            get
            {
                return this._backColor;
            }
            set
            {
                this._backColor = value;
                if (value != Color.Transparent)
                {
                    base.BackColor = value;
                }
                if (!this._isDisposing)
                {
                    base.Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this._isDisposing)
            {
                return;
            }
            try
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                
                int width = Math.Max(1, base.Width);
                int height = Math.Max(1, base.Height);
                
                if (width > 0 && height > 0 && this._fillColor.A > 0)
                {
                    // Draw directly without creating a large bitmap
                    RectangleF rect = new RectangleF(0f, 0f, width, height);
                    
                    using (GraphicsPath path = this.CreateRoundedRectanglePath(rect, this._cornerRadius))
                    {
                        using (SolidBrush brush = new SolidBrush(this._fillColor))
                        {
                            e.Graphics.FillPath(brush, path);
                        }
                    }
                    
                    if (this._icon != null)
                    {
                        this.DrawIcon(e.Graphics, width, height);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void DrawIcon(Graphics g, int controlWidth, int controlHeight)
        {
            if (this._icon == null)
            {
                return;
            }
            try
            {
                int num = 0;
                int num2 = 0;
                int iconWidth = this._iconWidth;
                int iconHeight = this._iconHeight;
                ContentAlignment iconAlignment = this._iconAlignment;
                if (iconAlignment <= ContentAlignment.MiddleCenter)
                {
                    switch (iconAlignment)
                    {
                        case ContentAlignment.TopLeft:
                            num = this.Padding.Left + this._iconOffsetX;
                            num2 = this.Padding.Top + this._iconOffsetY;
                            break;
                        case ContentAlignment.TopCenter:
                            num = (controlWidth - iconWidth) / 2 + this._iconOffsetX;
                            num2 = this.Padding.Top + this._iconOffsetY;
                            break;
                        case (ContentAlignment)3:
                            break;
                        case ContentAlignment.TopRight:
                            num = controlWidth - this.Padding.Right - iconWidth + this._iconOffsetX;
                            num2 = this.Padding.Top + this._iconOffsetY;
                            break;
                        default:
                            if (iconAlignment != ContentAlignment.MiddleLeft)
                            {
                                if (iconAlignment == ContentAlignment.MiddleCenter)
                                {
                                    num = (controlWidth - iconWidth) / 2 + this._iconOffsetX;
                                    num2 = (controlHeight - iconHeight) / 2 + this._iconOffsetY;
                                }
                            }
                            else
                            {
                                num = this.Padding.Left + this._iconOffsetX;
                                num2 = (controlHeight - iconHeight) / 2 + this._iconOffsetY;
                            }
                            break;
                    }
                }
                else if (iconAlignment <= ContentAlignment.BottomLeft)
                {
                    if (iconAlignment != ContentAlignment.MiddleRight)
                    {
                        if (iconAlignment == ContentAlignment.BottomLeft)
                        {
                            num = this.Padding.Left + this._iconOffsetX;
                            num2 = controlHeight - this.Padding.Bottom - iconHeight + this._iconOffsetY;
                        }
                    }
                    else
                    {
                        num = controlWidth - this.Padding.Right - iconWidth + this._iconOffsetX;
                        num2 = (controlHeight - iconHeight) / 2 + this._iconOffsetY;
                    }
                }
                else if (iconAlignment != ContentAlignment.BottomCenter)
                {
                    if (iconAlignment == ContentAlignment.BottomRight)
                    {
                        num = controlWidth - this.Padding.Right - iconWidth + this._iconOffsetX;
                        num2 = controlHeight - this.Padding.Bottom - iconHeight + this._iconOffsetY;
                    }
                }
                else
                {
                    num = (controlWidth - iconWidth) / 2 + this._iconOffsetX;
                    num2 = controlHeight - this.Padding.Bottom - iconHeight + this._iconOffsetY;
                }
                num = Math.Max(0, Math.Min(num, controlWidth - iconWidth));
                num2 = Math.Max(0, Math.Min(num2, controlHeight - iconHeight));
                g.DrawImage(this._icon, new Rectangle(num, num2, iconWidth, iconHeight));
            }
            catch (Exception)
            {
            }
        }

        private GraphicsPath CreateRoundedRectanglePath(RectangleF rect, float radius)
        {
            GraphicsPath graphicsPath = new GraphicsPath();
            try
            {
                float num = Math.Max(0f, Math.Min(radius, Math.Min(rect.Width / 2f, rect.Height / 2f)));
                if (num <= 0f)
                {
                    graphicsPath.AddRectangle(rect);
                    return graphicsPath;
                }
                float num2 = num * 2f;
                graphicsPath.StartFigure();
                graphicsPath.AddArc(rect.X, rect.Y, num2, num2, 180f, 90f);
                graphicsPath.AddLine(rect.X + num, rect.Y, rect.Right - num, rect.Y);
                graphicsPath.AddArc(rect.Right - num2, rect.Y, num2, num2, 270f, 90f);
                graphicsPath.AddLine(rect.Right, rect.Y + num, rect.Right, rect.Bottom - num);
                graphicsPath.AddArc(rect.Right - num2, rect.Bottom - num2, num2, num2, 0f, 90f);
                graphicsPath.AddLine(rect.Right - num, rect.Bottom, rect.X + num, rect.Bottom);
                graphicsPath.AddArc(rect.X, rect.Bottom - num2, num2, num2, 90f, 90f);
                graphicsPath.AddLine(rect.X, rect.Bottom - num, rect.X, rect.Y + num);
                graphicsPath.CloseFigure();
            }
            catch (Exception)
            {
                graphicsPath.Reset();
                graphicsPath.AddRectangle(rect);
            }
            return graphicsPath;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                return base.CreateParams;
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (!this._isDisposing)
            {
                this.UpdateControlRegion();
            }
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            if (!this._isDisposing)
            {
                base.Invalidate();
            }
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            if (this._textBox != null && !this._isDisposing && !this._isPlaceholderActive)
            {
                this._textBox.ForeColor = this.ForeColor;
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            int num = 50;
            int num2 = 20;
            if (width < num)
            {
                width = num;
            }
            if (height < num2)
            {
                height = num2;
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        private TransparentRichTextBox _textBox;

        private int _cornerRadius = 10;

        private Color _fillColor = Color.FromArgb(11, 14, 19);

        private HorizontalAlignment _textAlign;

        private bool _multiline;

        private int _maxLength = DefaultMaxLength;

        private string _placeholderText = "";

        private Color _placeholderColor = Color.Gray;

        private bool _isPlaceholderActive;

        private bool _isFocused;

        private bool _savedPasswordState;

        private char _savedPasswordChar;

        private bool _isDisposing;

        private bool _suppressTextEvents;

        private Color _customForeColor;

        private Image _icon;

        private ContentAlignment _iconAlignment = ContentAlignment.MiddleLeft;

        private int _iconOffsetX = 5;

        private int _iconOffsetY;

        private int _iconWidth = 16;

        private int _iconHeight = 16;

        private int _textOffsetX;

        private int _textOffsetY;

        private Color _backColor = Color.Transparent;
    }
}
