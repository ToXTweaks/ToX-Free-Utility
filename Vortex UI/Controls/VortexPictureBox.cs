using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexPictureBox : PictureBox
    {
        // ------- Properties -------
        private int _cornerRadius = 10;
        private float _opacity = 1f;
        private float _dimOpacity = 0f;

        // Crop-on-demand (for large images like 4K)
        private bool _enableCropOnDemand = true;      // تقدر تغيّره حسب رغبتك
        private float _cropThreshold = 1.5f;          // لو Image.Width > dest.Width * threshold نعمل crop

        // interactive resize flag
        private bool _isInSizing = false;

        // Rounded-path cache (للإطار فقط)
        private GraphicsPath _cachedPath;
        private Size _cachedPathSize;
        private int _cachedPathRadius;

        // Glow cache
        private Bitmap _cachedGlow;
        private Size _cachedGlowForSize;
        private int _cachedGlowSize;
        private float _cachedGlowIntensity;
        private int _cachedGlowCornerRadius;
        private Color _cachedGlowColor;

        // --------- Reusable GDI objects (تقليل الـ allocations في كل frame) ---------
        private Pen _borderPen;
        private Color _borderPenColor = Color.Empty;
        private SolidBrush _dimBrush;
        private int _dimBrushAlpha = -1;

        // --------- Render cache (حل اللاج) ---------
        private Bitmap _cachedRender;                  // الصورة النهائية (مصغّرة + مقصوصة)
        private Image _cachedRenderSource;             // مرجع الصورة الأصلية
        private Size _cachedRenderSize;                // مقاس العرض النهائي (dest.Size أو Image.Size حسب Mode)
        private float _cachedRenderOpacity;            // الأوباسيتي المستخدم
        private int _cachedRenderRadius;               // الكورنر المستخدم
        private PictureBoxSizeMode _cachedRenderMode;  // المود المستخدم (Zoom/Stretch..)
        // إذا استخدمنا crop-on-demand فسنخزن أيضاً الــ srcRect الذي قُمنا بقصّه
        private Rectangle _cachedRenderSrcRect = Rectangle.Empty;
        private bool _cachedRenderIsCropped = false;

        [Category("Appearance"), DefaultValue(10)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                int v = Math.Max(0, value);
                if (_cornerRadius == v) return;
                _cornerRadius = v;
                InvalidatePath();
                InvalidateGlow();
                InvalidateRender();   // لازم نبني رندر جديد
                Invalidate();
            }
        }

        [Category("Appearance"), DefaultValue(1.0f)]
        public float Opacity
        {
            get => _opacity;
            set
            {
                float v = Math.Max(0f, Math.Min(1f, value));
                if (Math.Abs(_opacity - v) < 0.0001f) return;
                _opacity = v;
                InvalidateRender();   // لأننا بنطبّق الأوباسيتي داخل الصورة النهائية
                Invalidate();
            }
        }

        [Category("Appearance"), DefaultValue(0.0f)]
        public float DimOpacity
        {
            get => _dimOpacity;
            set
            {
                float v = Math.Max(0f, Math.Min(1f, value));
                if (Math.Abs(_dimOpacity - v) < 0.0001f) return;
                _dimOpacity = v;
                // مفيش داعي نرمي الرندر؛ ده Overlay يتعمل في OnPaint
                Invalidate();
            }
        }

      

        // ظل لخاصية Image (بدل OnImageChanged غير الموجودة)
        public new Image Image
        {
            get => base.Image;
            set
            {
                if (ReferenceEquals(base.Image, value)) return;
                base.Image = value;
                ApplyAutoSizeIfNeeded();
                InvalidatePath();
                InvalidateGlow();
                InvalidateRender();   // الصورة تغيّرت -> ابني رندر جديد عند اللزوم
                Invalidate();
            }
        }

        public VortexPictureBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
        }

        // AutoSize كسلوك PictureBox
        public override Size GetPreferredSize(Size proposedSize)
        {
            if (SizeMode == PictureBoxSizeMode.AutoSize && Image != null)
                return Image.Size;
            return base.GetPreferredSize(proposedSize);
        }

        protected override void OnSizeModeChanged(EventArgs e)
        {
            base.OnSizeModeChanged(e);
            ApplyAutoSizeIfNeeded();
            InvalidatePath();
            InvalidateGlow();
            InvalidateRender();   // المود يغيّر الـdest -> لازم نبني
            Invalidate();
        }

        private void ApplyAutoSizeIfNeeded()
        {
            if (SizeMode == PictureBoxSizeMode.AutoSize && Image != null)
            {
                var sz = Image.Size;
                if (Size != sz) Size = sz;
            }
        }

        // Capture size/move start/end to enable fast-path while resizing
        protected override void WndProc(ref Message m)
        {
            const int WM_ENTERSIZEMOVE = 0x0231;
            const int WM_EXITSIZEMOVE = 0x0232;

            if (m.Msg == WM_ENTERSIZEMOVE)
            {
                _isInSizing = true;
                Invalidate();
            }
            else if (m.Msg == WM_EXITSIZEMOVE)
            {
                _isInSizing = false;
                // rebuild heavy caches after interactive resize ends
                InvalidateGlow();
                InvalidateRender(); // يُبنى في OnPaint التالي
                Invalidate();
            }

            base.WndProc(ref m);
        }

        // -------- Reusable GDI helpers --------
        private Pen GetBorderPen()
        {
            var c = ForeColor.IsEmpty ? Color.Black : ForeColor;
            if (_borderPen == null || _borderPenColor != c)
            {
                _borderPen?.Dispose();
                _borderPen = new Pen(c, 1f) { Alignment = PenAlignment.Inset };
                _borderPenColor = c;
            }
            return _borderPen;
        }

        private SolidBrush GetDimBrush()
        {
            int alpha = (int)(_dimOpacity * 255);
            if (_dimBrush == null || _dimBrushAlpha != alpha)
            {
                _dimBrush?.Dispose();
                _dimBrush = new SolidBrush(Color.FromArgb(alpha, Color.Black));
                _dimBrushAlpha = alpha;
            }
            return _dimBrush;
        }

        // -------- Painting --------
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (BackColor == Color.Transparent && Parent != null && !VortexDesign.IsDesignerProcess)
            {
                var g = pevent.Graphics;
                var s = g.Save();
                try
                {
                    g.TranslateTransform(-Left, -Top);
                    var rect = new Rectangle(Left, Top, Width, Height);
                    using (var pe = new PaintEventArgs(g, rect))
                    {
                        InvokePaintBackground(Parent, pe);
                        InvokePaint(Parent, pe);
                    }
                }
                finally { g.Restore(s); }
            }
            else if (BackColor == Color.Transparent && Parent != null)
            {
                using (var brush = new SolidBrush(Parent.BackColor.A > 0 ? Parent.BackColor : System.Drawing.SystemColors.Control))
                    pevent.Graphics.FillRectangle(brush, pevent.ClipRectangle);
            }
            else
            {
                base.OnPaintBackground(pevent);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingMode = CompositingMode.SourceOver;

            var rect = new Rectangle(0, 0, Math.Max(1, Width), Math.Max(1, Height));
            int radius = Math.Min(_cornerRadius, Math.Min(rect.Width, rect.Height) / 2);

            var clipPath = GetRoundedPathCached(rect, radius);
            {
                // لو مفيش صورة: ارسم الخلفية + بوردر وخلاص (آمن للديزاينر)
                if (Image == null)
                {
                    using (var fill = new SolidBrush(BackColor == Color.Transparent ? Color.Transparent : BackColor))
                        g.FillPath(fill, clipPath);
                    if (BorderStyle == BorderStyle.FixedSingle)
                        g.DrawPath(GetBorderPen(), clipPath);
                    return;
                }

                // مستطيل العرض النهائي حسب SizeMode
                var dest = GetDestRectForSizeMode(rect, Image.Size, SizeMode);
                if (dest.Width <= 0 || dest.Height <= 0) return;

                // FAST PATH: during interactive sizing we avoid heavy bitmap rebuilds
                if (_isInSizing)
                {
                    var oldSmoothing = g.SmoothingMode;
                    var oldInterpolation = g.InterpolationMode;
                    var oldPixelOffset = g.PixelOffsetMode;
                    try
                    {
                        g.SmoothingMode = SmoothingMode.None;
                        g.InterpolationMode = InterpolationMode.Low;
                        g.PixelOffsetMode = PixelOffsetMode.None;

                        // Draw image directly (fast): handle Center/Normal/Zoom/Stretch
                        if (SizeMode == PictureBoxSizeMode.CenterImage)
                        {
                            // Image is positioned centered; may have negative coordinates
                            int offX = (rect.Width - Image.Width) / 2;
                            int offY = (rect.Height - Image.Height) / 2;
                            var positioned = new Rectangle(offX, offY, Image.Width, Image.Height);
                            var visible = Rectangle.Intersect(rect, positioned);
                            if (!visible.IsEmpty)
                            {
                                var srcRect = new Rectangle(visible.X - positioned.X, visible.Y - positioned.Y, visible.Width, visible.Height);
                                var destRect = new Rectangle(visible.X, visible.Y, visible.Width, visible.Height);
                                g.DrawImage(Image, destRect, srcRect, GraphicsUnit.Pixel);
                            }
                        }
                        else
                        {
                            // For Normal/Stretch/Zoom/AutoSize: draw using dest rect (GDI+ will scale/clipping as needed)
                            g.DrawImage(Image, dest, 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel);
                        }
                    }
                    finally
                    {
                        g.SmoothingMode = oldSmoothing;
                        g.InterpolationMode = oldInterpolation;
                        g.PixelOffsetMode = oldPixelOffset;
                    }

                    // dim overlay + border (cheap)
                    if (_dimOpacity > 0f)
                        g.FillPath(GetDimBrush(), clipPath);

                    if (BorderStyle == BorderStyle.FixedSingle)
                        g.DrawPath(GetBorderPen(), clipPath);

                    return; // done fast-path
                }

                // HEAVY PATH: use cached rendered bitmap (rounded + opacity applied) to draw.
                EnsureRenderCached(dest.Size, radius);

                if (_cachedRender != null)
                {
                    if (_cachedRenderIsCropped)
                    {
                        // If we built a cropped cache (visible portion only), draw it at dest.Location
                        g.DrawImage(_cachedRender, dest.Location);
                    }
                    else
                    {
                        if (SizeMode == PictureBoxSizeMode.Normal)
                        {
                            // draw only visible portion (no scaling) from full-image-cache
                            var srcRect = new Rectangle(0, 0, Math.Min(_cachedRender.Width, dest.Width), Math.Min(_cachedRender.Height, dest.Height));
                            var destRect = new Rectangle(dest.Location, srcRect.Size);
                            g.DrawImage(_cachedRender, destRect, srcRect, GraphicsUnit.Pixel);
                        }
                        else
                        {
                            // cached was created to match destSize for other modes
                            g.DrawImage(_cachedRender, dest.Location);
                        }
                    }
                }

                // تعتيم داخلي (داخل الشكل)
                if (_dimOpacity > 0f)
                    g.FillPath(GetDimBrush(), clipPath);

                // Border
                if (BorderStyle == BorderStyle.FixedSingle)
                    g.DrawPath(GetBorderPen(), clipPath);
            }
        }

        // يبني/يحدّث الصورة النهائية في الكاش عند الحاجة فقط
        private void EnsureRenderCached(Size destSize, int radius)
        {
            if (Image == null) return;

            // افتراضياً: cacheSize = destSize (ما يُعرض) أو Image.Size حسب Mode
            Size cacheSize;
            Rectangle? srcRectForCache = null;
            bool willCrop = false;

            if (SizeMode == PictureBoxSizeMode.Normal)
            {
                // Normal: by default PictureBox draws image at top-left, clipped.
                // If crop-on-demand enabled and image much bigger than dest, build cache only for visible destSize
                if (_enableCropOnDemand &&
                    (Image.Width > destSize.Width * _cropThreshold || Image.Height > destSize.Height * _cropThreshold))
                {
                    willCrop = true;
                    cacheSize = destSize; // build cache equal to visible portion
                    // source rect is from (0,0) normally for Normal (image positioned top-left)
                    srcRectForCache = new Rectangle(0, 0, cacheSize.Width, cacheSize.Height);
                }
                else
                {
                    willCrop = false;
                    cacheSize = Image.Size; // full image cached (as before)
                    srcRectForCache = null;
                }
            }
            else if (SizeMode == PictureBoxSizeMode.CenterImage)
            {
                // For CenterImage: visible part is the center portion of the source image
                if (_enableCropOnDemand &&
                    (Image.Width > destSize.Width * _cropThreshold || Image.Height > destSize.Height * _cropThreshold))
                {
                    willCrop = true;
                    cacheSize = destSize; // build only visible
                    int sx = Math.Max(0, (Image.Width - cacheSize.Width) / 2);
                    int sy = Math.Max(0, (Image.Height - cacheSize.Height) / 2);
                    srcRectForCache = new Rectangle(sx, sy, cacheSize.Width, cacheSize.Height);
                }
                else
                {
                    willCrop = false;
                    cacheSize = Image.Size;
                    srcRectForCache = null;
                }
            }
            else
            {
                // For Zoom/Stretch/AutoSize: we need the scaled destSize to match what will be drawn
                willCrop = false;
                cacheSize = destSize;
                srcRectForCache = null;
            }

            bool need =
                _cachedRender == null ||
                !ReferenceEquals(_cachedRenderSource, Image) ||
                _cachedRenderSize != cacheSize ||
                Math.Abs(_cachedRenderOpacity - _opacity) > 0.0001f ||
                _cachedRenderRadius != radius ||
                _cachedRenderMode != SizeMode ||
                _cachedRenderIsCropped != willCrop ||
                (_cachedRenderIsCropped && srcRectForCache.HasValue && _cachedRenderSrcRect != srcRectForCache.Value);

            if (!need) return;

            _cachedRender?.Dispose();
            _cachedRender = null;

            // ابني الكاش بالحجم المختار، مع تمرير srcRect لو في cropping
            _cachedRender = CreateRoundedImage(Image, cacheSize, radius, srcRectForCache);

            // حدّث مفاتيح الكاش
            _cachedRenderSource = Image;
            _cachedRenderSize = cacheSize;
            _cachedRenderOpacity = _opacity;
            _cachedRenderRadius = radius;
            _cachedRenderMode = SizeMode;
            _cachedRenderIsCropped = willCrop;
            _cachedRenderSrcRect = srcRectForCache ?? Rectangle.Empty;
        }

        private void InvalidateRender()
        {
            _cachedRender?.Dispose();
            _cachedRender = null;
            _cachedRenderSource = null;
            _cachedRenderSize = Size.Empty;
            _cachedRenderOpacity = 0f;
            _cachedRenderRadius = 0;
            _cachedRenderMode = PictureBoxSizeMode.Normal;
            _cachedRenderIsCropped = false;
            _cachedRenderSrcRect = Rectangle.Empty;
        }

        // يرسم الصورة بالأوباسيتي الحالي (helper موحّد بدل التكرار)
        private void DrawImageWithOpacity(Graphics g, Image src, Rectangle dest,
                                          int sx, int sy, int sw, int sh)
        {
            if (_opacity < 0.999f)
            {
                using (var ia = new ImageAttributes())
                {
                    var cm = new ColorMatrix { Matrix33 = Math.Max(0f, Math.Min(1f, _opacity)) };
                    ia.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    g.DrawImage(src, dest, sx, sy, sw, sh, GraphicsUnit.Pixel, ia);
                }
            }
            else
            {
                g.DrawImage(src, dest, sx, sy, sw, sh, GraphicsUnit.Pixel);
            }
        }

        // === CreateRoundedImage: accepts optional srcRect to draw only portion from source (crop-on-demand)
        private Bitmap CreateRoundedImage(Image src, Size destSize, int radius, Rectangle? srcRect = null)
        {
            int w = Math.Max(1, destSize.Width);
            int h = Math.Max(1, destSize.Height);

            // temporary canvas
            var temp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (var gtemp = Graphics.FromImage(temp))
            {
                gtemp.Clear(Color.Transparent);
                gtemp.CompositingMode = CompositingMode.SourceOver;
                gtemp.CompositingQuality = CompositingQuality.HighQuality;
                gtemp.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gtemp.SmoothingMode = SmoothingMode.HighQuality;
                gtemp.PixelOffsetMode = PixelOffsetMode.HighQuality;

                if (srcRect.HasValue)
                {
                    Rectangle s = srcRect.Value;
                    DrawImageWithOpacity(gtemp, src, new Rectangle(0, 0, w, h), s.X, s.Y, s.Width, s.Height);
                }
                else if (this.SizeMode == PictureBoxSizeMode.CenterImage)
                {
                    int drawX = (w - src.Width) / 2;
                    int drawY = (h - src.Height) / 2;
                    DrawImageWithOpacity(gtemp, src, new Rectangle(drawX, drawY, src.Width, src.Height),
                                         0, 0, src.Width, src.Height);
                }
                else
                {
                    // Other modes: draw scaled to temp size (Zoom/Stretch/AutoSize/Normal when not cropping)
                    DrawImageWithOpacity(gtemp, src, new Rectangle(0, 0, w, h), 0, 0, src.Width, src.Height);
                }
            }

            // apply rounded mask via filling path with TextureBrush(temp) -- يحافظ على نعومة الأركان (AA)
            var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(result))
            {
                g.Clear(Color.Transparent);
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var path = CreateRoundedPath(new Rectangle(0, 0, w, h), Math.Min(radius, Math.Min(w, h) / 2)))
                using (var tb = new TextureBrush(temp, WrapMode.Clamp))
                {
                    g.FillPath(tb, path);
                }
            }

            temp.Dispose();
            return result;
        }

        private static Rectangle GetDestRectForSizeMode(Rectangle bounds, Size imgSize, PictureBoxSizeMode mode)
        {
            int imgW = imgSize.Width, imgH = imgSize.Height;
            if (imgW <= 0 || imgH <= 0) return Rectangle.Empty;

            switch (mode)
            {
                case PictureBoxSizeMode.Normal:
                    return new Rectangle(bounds.X, bounds.Y,
                        Math.Min(bounds.Width, imgW),
                        Math.Min(bounds.Height, imgH));

                case PictureBoxSizeMode.StretchImage:
                    return bounds;

                case PictureBoxSizeMode.AutoSize:
                    return new Rectangle(0, 0, imgW, imgH);

                case PictureBoxSizeMode.CenterImage:
                    {
                        // ALWAYS center the image — even if larger than bounds (coords may be negative)
                        int dx = bounds.X + (bounds.Width - imgW) / 2;
                        int dy = bounds.Y + (bounds.Height - imgH) / 2;
                        return new Rectangle(dx, dy, imgW, imgH);
                    }

                case PictureBoxSizeMode.Zoom:
                default:
                    float scale = Math.Min(bounds.Width / (float)imgW, bounds.Height / (float)imgH);
                    int w = Math.Max(1, (int)Math.Round(imgW * scale));
                    int h = Math.Max(1, (int)Math.Round(imgH * scale));
                    int ddx = bounds.X + (bounds.Width - w) / 2;
                    int ddy = bounds.Y + (bounds.Height - h) / 2;
                    return new Rectangle(ddx, ddy, w, h);
            }
        }

        // -------- Rounded path cache / border --------
        private GraphicsPath GetRoundedPathCached(Rectangle rect, int radius)
        {
            if (_cachedPath != null &&
                _cachedPathSize == rect.Size &&
                _cachedPathRadius == radius)
                return _cachedPath;

            _cachedPath?.Dispose();
            _cachedPath = CreateRoundedPath(rect, radius);
            _cachedPathSize = rect.Size;
            _cachedPathRadius = radius;
            return _cachedPath;
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(rect); return path; }

            int d = radius * 2;
            int r = rect.Right, b = rect.Bottom;
            path.StartFigure();
            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddLine(rect.Left + radius, rect.Top, r - radius, rect.Top);
            path.AddArc(r - d, rect.Top, d, d, 270, 90);
            path.AddLine(r, rect.Top + radius, r, b - radius);
            path.AddArc(r - d, b - d, d, d, 0, 90);
            path.AddLine(r - radius, b, rect.Left + radius, b);
            path.AddArc(rect.Left, b - d, d, d, 90, 90);
            path.AddLine(rect.Left, b - radius, rect.Left, rect.Top + radius);
            path.CloseFigure();
            return path;
        }

        private void InvalidatePath()
        {
            _cachedPath?.Dispose();
            _cachedPath = null;
            _cachedPathSize = Size.Empty;
            _cachedPathRadius = 0;
        }

        // -------- Glow --------
        private void InvalidateGlow()
        {
            _cachedGlow?.Dispose();
            _cachedGlow = null;
            _cachedGlowForSize = Size.Empty;
            _cachedGlowSize = 0;
            _cachedGlowIntensity = 0f;
            _cachedGlowCornerRadius = 0;
            _cachedGlowColor = Color.Empty;
        }


        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            InvalidatePath();
            // do not force heavy rebuild immediately on every small resize; InvalidateRender will be called
            // on WM_EXITSIZEMOVE or when necessary
            InvalidateGlow();
            InvalidateRender();
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cachedPath?.Dispose();
                _cachedGlow?.Dispose();
                _cachedRender?.Dispose();
                _borderPen?.Dispose();
                _dimBrush?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
