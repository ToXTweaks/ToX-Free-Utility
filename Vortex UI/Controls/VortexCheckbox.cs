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
    public partial class VortexCheckBox : UserControl
    {
        private bool _checked = false;
        private bool _isHovered = false;
        private bool _isPressed = false;
        private Timer _animationTimer;
        private float _animationProgress = 0f;
        private const int ANIMATION_STEPS = 8; // قللت عدد الخطوات عشان تكون أسرع
        private const int ANIMATION_INTERVAL = 12; // قللت الوقت عشان تكون أسرع
        private int _cornerRadius = 6;
        private int _checkboxSize = 20;
        private float _checkMarkScale = 0.28f; // حجم علامة الصح
        private string _text = "";
        private bool _autoSize = true;

        private Color _checkedColor = Color.FromArgb(0, 123, 255);
        private Color _uncheckedColor = Color.FromArgb(240, 240, 240);
        private Color _disabledColor = Color.FromArgb(180, 180, 180);
        private Color _checkMarkColor = Color.White;
        private Color _textColor = Color.White;
        private Color _borderColor = Color.Transparent;

        [Category("Appearance")]
        [Description("Gets or sets whether the checkbox is checked")]
        [DefaultValue(false)]
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    StartAnimation();
                    OnCheckedChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance")]
        [Description("The text displayed next to the checkbox")]
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
        [Description("The corner radius of the checkbox (0-20)")]
        [DefaultValue(6)]
        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                _cornerRadius = Math.Max(0, Math.Min(20, value));
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The size of the checkbox square")]
        [DefaultValue(18)]
        public int CheckboxSize
        {
            get { return _checkboxSize; }
            set
            {
                _checkboxSize = Math.Max(12, Math.Min(32, value));
                if (_autoSize)
                    AdjustSize();
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("The scale of the checkmark (0.1 to 0.8)")]
        [DefaultValue(0.35f)]
        public float CheckMarkScale
        {
            get { return _checkMarkScale; }
            set
            {
                _checkMarkScale = Math.Max(0.1f, Math.Min(0.8f, value));
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
        public Color CheckMarkColor
        {
            get { return _checkMarkColor; }
            set { _checkMarkColor = value; Invalidate(); }
        }

        [Category("Appearance")]
        public Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; Invalidate(); }
        }

        public event EventHandler CheckedChanged;

        public VortexCheckBox()
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
            this.Name = "SmoothCheckBox";
            this.ResumeLayout(false);
        }

        private void InitializeAnimation()
        {
            _animationTimer = new Timer();
            _animationTimer.Interval = ANIMATION_INTERVAL;
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AdjustSize()
        {
            if (!_autoSize) return;

            Size textSize = Size.Empty;
            if (!string.IsNullOrEmpty(_text))
            {
                // قياس أدق بدون حواف إضافية من TextRenderer
                textSize = TextRenderer.MeasureText(_text, this.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
            }

            int textSpacing = string.IsNullOrEmpty(_text) ? 0 : 8;
            int leftPadding = 2;
            int rightPadding = 2; // قللت المساحة من اليمين
            int verticalPadding = 4; // قللت المساحة العمودية

            int totalWidth = leftPadding + _checkboxSize + textSpacing + textSize.Width + rightPadding;
            int totalHeight = Math.Max(_checkboxSize, textSize.Height) + verticalPadding;

            totalHeight = Math.Max(totalHeight, _checkboxSize + 4);

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
                // قياس أدق بدون حواف إضافية من TextRenderer
                textSize = TextRenderer.MeasureText(_text, this.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
            }

            int textSpacing = string.IsNullOrEmpty(_text) ? 0 : 8;
            int totalWidth = 2 + _checkboxSize + textSpacing + textSize.Width + 2;
            int totalHeight = Math.Max(_checkboxSize + 4, textSize.Height + 4);

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
            g.PixelOffsetMode = PixelOffsetMode.HighQuality; // اجعلها عالية الجودة بشكل عام، ونستخدم Half فقط للحدود
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int checkboxY = (this.Height - _checkboxSize) / 2;
            Rectangle checkboxRect = new Rectangle(2, checkboxY, _checkboxSize, _checkboxSize);

            DrawCheckboxBackground(g, checkboxRect);

            if (_checked || _animationTimer.Enabled)
            {
                DrawCheckmark(g, checkboxRect);
            }

            if (!string.IsNullOrEmpty(_text))
            {
                DrawText(g, checkboxRect);
            }
        }

        private void DrawCheckboxBackground(Graphics g, Rectangle rect)
        {
            // 3x Supersampling لإخراج حواف ناعمة جداً وحد 1px دقيق بعد التصغير
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

                // تعبئة الخلفية
                Color backgroundColor = GetBackgroundColor();
                RectangleF fillRect = new RectangleF(0f, 0f, w * SS, h * SS);
                float rr = Math.Min(_cornerRadius * SS, Math.Min(fillRect.Width, fillRect.Height) / 2f);
                using (GraphicsPath fillPath = CreateRoundedRectangleF(fillRect, rr))
                using (SolidBrush brush = new SolidBrush(backgroundColor))
                {
                    g2.FillPath(brush, fillPath);
                }

                // حد دقيق 1px بعد التصغير: ارسم بعرض يساوي SS داخل الباث المحاذي لـ 0.5
                Color borderColor = _checked ? ColorHelper.Darken(_checkedColor, 0.2f) : _borderColor;
                if (borderColor.A > 0)
                {
                    borderColor = Color.FromArgb(255, borderColor); // لون معتم لمنع هالة

                    RectangleF strokeRect = new RectangleF(0.5f * SS, 0.5f * SS, w * SS - 1f * SS, h * SS - 1f * SS);
                    float sr = Math.Min(_cornerRadius * SS, Math.Min(strokeRect.Width, strokeRect.Height) / 2f);
                    using (GraphicsPath strokePath = CreateRoundedRectangleF(strokeRect, sr))
                    using (Pen pen = new Pen(borderColor, SS))
                    {
                        pen.LineJoin = LineJoin.Round;
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        g2.DrawPath(pen, strokePath);
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

        private void DrawPremiumBorder(Graphics g, Rectangle rect, Color borderColor, int radius)
        {
            // حساب مسار الحد بدقة رياضية
            RectangleF borderRect = new RectangleF(rect.X + 0.5f, rect.Y + 0.5f, rect.Width - 1f, rect.Height - 1f);
            float borderRadius = Math.Min(radius, Math.Min(borderRect.Width, borderRect.Height) / 2f);

            // تقنية Multi-pass للحصول على أنعم حد ممكن
            using (GraphicsPath borderPath = CreateSmoothRoundedPath(borderRect, borderRadius))
            {
                // Pass 1: طبقة خلفية ناعمة جداً للتنعيم
                using (Pen softPen = new Pen(Color.FromArgb(30, borderColor), 3f))
                {
                    softPen.LineJoin = LineJoin.Round;
                    softPen.StartCap = LineCap.Round;
                    softPen.EndCap = LineCap.Round;
                    g.DrawPath(softPen, borderPath);
                }

                // Pass 2: طبقة متوسطة للانتقال
                using (Pen mediumPen = new Pen(Color.FromArgb(80, borderColor), 1.8f))
                {
                    mediumPen.LineJoin = LineJoin.Round;
                    mediumPen.StartCap = LineCap.Round;
                    mediumPen.EndCap = LineCap.Round;
                    g.DrawPath(mediumPen, borderPath);
                }

                // Pass 3: الطبقة النهائية الحادة
                using (Pen sharpPen = new Pen(Color.FromArgb(255, borderColor), 0.8f))
                {
                    sharpPen.LineJoin = LineJoin.Round;
                    sharpPen.StartCap = LineCap.Round;
                    sharpPen.EndCap = LineCap.Round;
                    g.DrawPath(sharpPen, borderPath);
                }
            }
        }

        private GraphicsPath CreateSmoothRoundedPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            
            if (radius <= 0f)
            {
                path.AddRectangle(rect);
                return path;
            }

            // ضمان نصف قطر صحيح
            float maxRadius = Math.Min(rect.Width, rect.Height) / 2f;
            radius = Math.Min(radius, maxRadius);
            
            if (radius < 1f)
            {
                path.AddRectangle(rect);
                return path;
            }

            float diameter = radius * 2f;
            
            // رسم أقواس بدقة رياضية عالية مع نقاط انتقال ناعمة
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            
            return path;
        }

        private GraphicsPath CreateSmoothRoundedPath(Rectangle rect, int radius)
        {
            return CreateSmoothRoundedPath(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), radius);
        }

        private void DrawCheckmark(Graphics g, Rectangle rect)
        {
            float progress = _animationProgress;
            if (_animationTimer.Enabled && !_checked)
                progress = 1f - _animationProgress;
            else if (!_animationTimer.Enabled && _checked)
                progress = 1f;

            if (progress > 0)
            {
                float centerX = rect.X + rect.Width * 0.5f;
                float centerY = rect.Y + rect.Height * 0.5f;
                float scale = rect.Width * _checkMarkScale * progress; // استخدام الخاصية الجديدة

                PointF[] checkmarkPoints = new PointF[]
                {
                    new PointF(centerX - scale * 0.6f, centerY),
                    new PointF(centerX - scale * 0.1f, centerY + scale * 0.5f),
                    new PointF(centerX + scale * 0.8f, centerY - scale * 0.5f)
                };

                using (Pen checkPen = new Pen(_checkMarkColor, 2.5f))
                {
                    checkPen.LineJoin = LineJoin.Round;
                    checkPen.StartCap = LineCap.Round;
                    checkPen.EndCap = LineCap.Round;
                    g.DrawLines(checkPen, checkmarkPoints);
                }
            }
        }

        private void DrawText(Graphics g, Rectangle checkboxRect)
        {
            int textX = checkboxRect.Right + 8;

            // جعل المستطيل بارتفاع التحكم بالكامل لضمان تمركز رأسي دقيق مع أي خط
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

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int maxRadius = Math.Min(rect.Width, rect.Height) / 2;
            radius = Math.Min(radius, maxRadius);
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private GraphicsPath CreateRoundedRectangleF(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0f)
            {
                path.AddRectangle(rect);
                return path;
            }

            // تأكد من أن نصف القطر لا يتجاوز نصف أصغر بُعد
            float maxRadius = Math.Min(rect.Width, rect.Height) / 2f;
            radius = Math.Min(radius, maxRadius);
            float diameter = radius * 2f;

            // تأكد من وجود مساحة كافية لرسم الأقواس
            if (diameter > rect.Width || diameter > rect.Height)
            {
                path.AddRectangle(rect);
                return path;
            }

            // ارسم الأقواس الأربعة بدقة أكبر
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90); // زاوية علوية يسار
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90); // زاوية علوية يمين
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90); // زاوية سفلية يمين
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90); // زاوية سفلية يسار
            path.CloseFigure();

            return path;
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
                if (this.ClientRectangle.Contains(e.Location))
                {
                    this.Checked = !this.Checked;
                }
                Invalidate();
            }
            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) && this.Enabled)
            {
                this.Checked = !this.Checked;
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

    public static class ColorHelper
    {
        public static Color Lighten(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, (int)(color.R + (255 - color.R) * factor)),
                Math.Min(255, (int)(color.G + (255 - color.G) * factor)),
                Math.Min(255, (int)(color.B + (255 - color.B) * factor))
            );
        }

        public static Color Darken(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                Math.Max(0, (int)(color.R * (1 - factor))),
                Math.Max(0, (int)(color.G * (1 - factor))),
                Math.Max(0, (int)(color.B * (1 - factor)))
            );
        }
    }
}
