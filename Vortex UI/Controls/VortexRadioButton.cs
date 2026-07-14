using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    [DefaultProperty("Text")]
    [DefaultEvent("CheckedChanged")]
    public partial class VortexRadioButton : UserControl
    {
        private bool _checked = false;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private Timer _animationTimer;
        private float _animationProgress = 0f;
        private const int ANIMATION_STEPS = 8; // نفس سرعة الـ checkbox
        private const int ANIMATION_INTERVAL = 12;
        private int _circleSize = 20;
        private float _dotScale = 0.5f; // حجم النقطة الداخلية كنسبة من الدائرة
        private string _text = "";
        private bool _autoSize = true;

        private Color _checkedColor = Color.FromArgb(0, 123, 255);
        private Color _uncheckedColor = Color.FromArgb(240, 240, 240);
        private Color _disabledColor = Color.FromArgb(180, 180, 180);
        private Color _dotColor = Color.White;
        private Color _textColor = Color.White;
        private Color _borderColor = Color.Transparent;

        [Category("Appearance")]
        [Description("Gets or sets whether the radio button is checked")]
        [DefaultValue(false)]
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    if (_checked)
                        UncheckSiblings();
                    StartAnimation();
                    OnCheckedChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance")]
        [Description("The text displayed next to the radio button")]
        [DefaultValue("")]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    if (_autoSize)
                        AdjustSize();
                    Invalidate();
                    OnTextChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Layout")]
        [Description("Automatically sizes the control to fit its contents")]
        [DefaultValue(true)]
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override bool AutoSize
        {
            get { return _autoSize; }
            set
            {
                if (_autoSize != value)
                {
                    _autoSize = value;
                    if (_autoSize)
                        AdjustSize();
                    OnAutoSizeChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance")]
        [Description("The diameter of the radio circle")]
        [DefaultValue(20)]
        public int CircleSize
        {
            get { return _circleSize; }
            set
            {
                _circleSize = Math.Max(12, Math.Min(32, value));
                if (_autoSize)
                    AdjustSize();
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The scale of the inner dot (0.2 to 0.8)")]
        [DefaultValue(0.5f)]
        public float DotScale
        {
            get { return _dotScale; }
            set
            {
                _dotScale = Math.Max(0.2f, Math.Min(0.8f, value));
                Invalidate();
            }
        }

        [Category("Appearance")]
        public Color CheckedColor
        {
            get { return _checkedColor; }
            set { _checkedColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color UncheckedColor
        {
            get { return _uncheckedColor; }
            set { _uncheckedColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color DotColor
        {
            get { return _dotColor; }
            set { _dotColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; Invalidate(); }
        }

        public event EventHandler CheckedChanged;

        public VortexRadioButton()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            InitializeAnimation();
            AdjustSize();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "VortexRadioButton";
            this.ResumeLayout(false);
        }

        private void InitializeAnimation()
        {
            _animationTimer = new Timer();
            _animationTimer.Interval = ANIMATION_INTERVAL;
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        // إلغاء تحديد باقي الـ radio buttons في نفس الحاوية (سلوك المجموعة)
        private void UncheckSiblings()
        {
            var parent = this.Parent;
            if (parent == null) return;

            foreach (Control c in parent.Controls)
            {
                if (!ReferenceEquals(c, this) && c is VortexRadioButton rb && rb._checked)
                {
                    rb._checked = false;
                    rb.StartAnimation();
                    rb.OnCheckedChanged(EventArgs.Empty);
                }
            }
        }

        private void AdjustSize()
        {
            if (!_autoSize) return;

            Size textSize = Size.Empty;
            if (!string.IsNullOrEmpty(_text))
            {
                textSize = TextRenderer.MeasureText(_text, this.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
            }

            int textSpacing = string.IsNullOrEmpty(_text) ? 0 : 8;
            int leftPadding = 2;
            int rightPadding = 2;
            int verticalPadding = 4;

            int totalWidth = leftPadding + _circleSize + textSpacing + textSize.Width + rightPadding;
            int totalHeight = Math.Max(_circleSize, textSize.Height) + verticalPadding;

            totalHeight = Math.Max(totalHeight, _circleSize + 4);

            this.Size = new Size(totalWidth, totalHeight);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            if (_autoSize)
                AdjustSize();
            Invalidate();
            base.OnFontChanged(e);
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (_autoSize && (specified & BoundsSpecified.Size) != 0)
            {
                Size preferredSize = GetPreferredSize(Size.Empty);
                width = preferredSize.Width;
                height = preferredSize.Height;
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (!_autoSize)
                return base.GetPreferredSize(proposedSize);

            Size textSize = Size.Empty;
            if (!string.IsNullOrEmpty(_text))
            {
                textSize = TextRenderer.MeasureText(_text, this.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
            }

            int textSpacing = string.IsNullOrEmpty(_text) ? 0 : 8;
            int totalWidth = 2 + _circleSize + textSpacing + textSize.Width + 2;
            int totalHeight = Math.Max(_circleSize + 4, textSize.Height + 4);

            return new Size(totalWidth, totalHeight);
        }

        private void StartAnimation()
        {
            _animationProgress = 0f;
            _animationTimer.Start();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            _animationProgress += 1f / ANIMATION_STEPS;
            if (_animationProgress >= 1f)
            {
                _animationProgress = 1f;
                _animationTimer.Stop();
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int circleY = (this.Height - _circleSize) / 2;
            Rectangle circleRect = new Rectangle(2, circleY, _circleSize, _circleSize);

            DrawCircleBackground(g, circleRect);

            if (_checked || _animationTimer.Enabled)
            {
                DrawDot(g, circleRect);
            }

            if (!string.IsNullOrEmpty(_text))
            {
                DrawText(g, circleRect);
            }
        }

        private void DrawCircleBackground(Graphics g, Rectangle rect)
        {
            // 3x Supersampling لإخراج حواف ناعمة جداً وحد 1px دقيق بعد التصغير (نفس أسلوب الـ checkbox)
            const int SS = 3;

            int w = Math.Max(1, rect.Width);
            int h = Math.Max(1, rect.Height);

            using (var bmp = new Bitmap(w * SS, h * SS, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
            using (var g2 = Graphics.FromImage(bmp))
            {
                g2.SmoothingMode = SmoothingMode.AntiAlias;
                g2.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g2.CompositingQuality = CompositingQuality.HighQuality;
                g2.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // تعبئة الخلفية (دائرة)
                Color backgroundColor = GetBackgroundColor();
                RectangleF fillRect = new RectangleF(0f, 0f, w * SS, h * SS);
                using (SolidBrush brush = new SolidBrush(backgroundColor))
                {
                    g2.FillEllipse(brush, fillRect);
                }

                // حد دقيق 1px بعد التصغير
                Color borderColor = _checked ? ColorHelper.Darken(_checkedColor, 0.2f) : _borderColor;
                if (borderColor.A > 0)
                {
                    borderColor = Color.FromArgb(255, borderColor); // لون معتم لمنع هالة

                    RectangleF strokeRect = new RectangleF(0.5f * SS, 0.5f * SS, w * SS - 1f * SS, h * SS - 1f * SS);
                    using (Pen pen = new Pen(borderColor, SS))
                    {
                        pen.LineJoin = LineJoin.Round;
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        g2.DrawEllipse(pen, strokeRect);
                    }
                }

                // إسقاط النتيجة المصغّرة بجودة عالية إلى رسومات التحكم
                var oldInterp = g.InterpolationMode;
                var oldPixel = g.PixelOffsetMode;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(bmp, rect);
                g.InterpolationMode = oldInterp;
                g.PixelOffsetMode = oldPixel;
            }
        }

        private void DrawDot(Graphics g, Rectangle rect)
        {
            float progress = _animationProgress;
            if (_animationTimer.Enabled && !_checked)
                progress = 1f - _animationProgress;
            else if (!_animationTimer.Enabled && _checked)
                progress = 1f;

            if (progress <= 0) return;

            float diameter = rect.Width * _dotScale * progress;
            if (diameter < 0.5f) return;

            float x = rect.X + (rect.Width - diameter) / 2f;
            float y = rect.Y + (rect.Height - diameter) / 2f;

            using (SolidBrush dotBrush = new SolidBrush(this.Enabled ? _dotColor : _disabledColor))
            {
                g.FillEllipse(dotBrush, x, y, diameter, diameter);
            }
        }

        private void DrawText(Graphics g, Rectangle circleRect)
        {
            int textX = circleRect.Right + 8;

            int availableWidth = Math.Max(0, this.Width - textX - 2);
            Rectangle textRect = new Rectangle(textX, 0, availableWidth, this.Height);
            Color textColor = this.Enabled ? _textColor : _disabledColor;

            TextFormatFlags flags = TextFormatFlags.NoPadding |
                                     TextFormatFlags.VerticalCenter |
                                     TextFormatFlags.SingleLine |
                                     TextFormatFlags.EndEllipsis;

            TextRenderer.DrawText(g, _text, this.Font, textRect, textColor, flags);
        }

        private Color GetBackgroundColor()
        {
            if (!this.Enabled)
                return _disabledColor;

            if (_checked)
                return _isHovered ? ColorHelper.Lighten(_checkedColor, 0.1f) : _checkedColor;
            else
                return _isHovered ? ColorHelper.Darken(_uncheckedColor, 0.05f) : _uncheckedColor;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovered = false;
            _isPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.Enabled)
            {
                _isPressed = true;
                this.Focus();
                Invalidate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _isPressed && this.Enabled)
            {
                _isPressed = false;
                // الـ radio لا يُلغى بالضغط عليه؛ الضغط يحدّده فقط
                if (this.ClientRectangle.Contains(e.Location))
                {
                    this.Checked = true;
                }
                Invalidate();
            }
            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) && this.Enabled)
            {
                this.Checked = true;
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            CheckedChanged?.Invoke(this, e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
