using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static Vortex.Ui.Helper.Win32;

namespace Vortex.Ui
{
    public partial class RoundedForm : Form
    {
        private Color privateBackgroundColor = Color.White;
        private Color privateBorderColor = Color.White;
        private Bitmap backImage;
        private Graphics backGraphics;
        private Form targetFormReference = null;
        
        // Cache pens to avoid creating them every frame
        private Pen cachedBackgroundPen;
        private Pen cachedBorderPen;

        private bool privateInvalidateNextDrawCall = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool InvalidateNextDrawCall
        {
            get
            {
                return privateInvalidateNextDrawCall;
            }
            set
            {
                if (value == true)
                {
                    DrawForm(null, EventArgs.Empty);
                    privateInvalidateNextDrawCall = false;
                }
            }
        }

        private Image privateBackgroundImage;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Image BackgroundImageOfTargetForm
        {
            get => privateBackgroundImage;
            set
            {
                privateBackgroundImage?.Dispose();
                privateBackgroundImage = value;
                DrawForm(null, EventArgs.Empty);
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BackgroundColor
        {
            get => privateBackgroundColor;
            set
            {
                if (privateBackgroundColor != value)
                {
                    privateBackgroundColor = value;
                    // Invalidate cached pen when color changes
                    cachedBackgroundPen?.Dispose();
                    cachedBackgroundPen = null;
                    Invalidate();
                }
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BorderColor
        {
            get => privateBorderColor;
            set
            {
                if (privateBorderColor != value)
                {
                    privateBorderColor = value;
                    // Invalidate cached pen when color changes
                    cachedBorderPen?.Dispose();
                    cachedBorderPen = null;
                    Invalidate();
                }
            }
        }

        public int Rounding = 8;

        public RoundedForm(Color init_backgroundColor, Color init_borderColor, ref int RoundValue, bool show = true, Form targetForm = null)
        {
            Visible = show;
            InitializeComponent();
            SetStyles();
            BackgroundColor = init_backgroundColor;
            BorderColor = init_borderColor;
            Rounding = RoundValue;
            targetFormReference = targetForm;
        }

        private void SetStyles()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            UpdateStyles();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }
        public void DrawForm(object sender, EventArgs e)
        {
            if (stop || Width <= 0 || Height <= 0)
            {
                return;
            }

            SuspendLayout();
            try
            {
                if (backImage == null || backImage.Size != Size || InvalidateNextDrawCall)
                {
                    InvalidateNextDrawCall = false;
                    backImage?.Dispose();
                    backGraphics?.Dispose();
                    
                    backImage = new Bitmap(Math.Max(1, Width), Math.Max(1, Height));
                    backGraphics = Graphics.FromImage(backImage);
                    backGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                    backGraphics.CompositingQuality = CompositingQuality.HighSpeed;
                    backGraphics.InterpolationMode = InterpolationMode.Low;
                }
                else
                {
                    backGraphics.Clear(Color.Transparent);
                }

                Rectangle gradientRectangle = new Rectangle(0, 0, Width - 2, Height - 2);
                GraphicsPath roundedRectangle = Helper.RoundRect(gradientRectangle, Rounding);

                Rectangle subtractRectangle = gradientRectangle;
                subtractRectangle.Offset(1, 1);
                subtractRectangle.Inflate(-1, -1);

                Rectangle fillinoutlineRectangle = subtractRectangle;
                fillinoutlineRectangle.Offset(-1, -1);

                GraphicsPath subtractPath = Helper.RoundRect(subtractRectangle, Rounding);
                GraphicsPath fillinoutlinePath = Helper.RoundRect(fillinoutlineRectangle, Rounding);

                // Fill with transparent first to ensure corners are transparent
                backGraphics.Clear(Color.Transparent);
                
                // Create or reuse cached pens
                if (cachedBackgroundPen == null)
                {
                    cachedBackgroundPen = new Pen(BackgroundColor, 1.6f);
                }
                if (cachedBorderPen == null)
                {
                    cachedBorderPen = new Pen(BorderColor);
                }
                
                using (SolidBrush brush = new SolidBrush(BackgroundColor))
                {
                    backGraphics.DrawPath(cachedBackgroundPen, fillinoutlinePath);

                    if (BackgroundImageOfTargetForm == null)
                    {
                        backGraphics.FillPath(brush, roundedRectangle);
                    }
                    else
                    {
                        try
                        {
                            if (backgroundImageTextureBrush == null)
                            {
                                backgroundImageTextureBrush = new TextureBrush(BackgroundImageOfTargetForm)
                                {
                                    WrapMode = WrapMode.Clamp
                                };
                            }
                            backGraphics.FillPath(backgroundImageTextureBrush, roundedRectangle);
                        }
                        catch
                        {
                            // Fallback to solid color if texture fails
                            backGraphics.FillPath(brush, roundedRectangle);
                        }
                    }

                    backGraphics.DrawPath(cachedBorderPen, roundedRectangle);
                }

                // Dispose paths
                roundedRectangle?.Dispose();
                subtractPath?.Dispose();
                fillinoutlinePath?.Dispose();

                double normalisedOpacity = 1;

                double.TryParse(Tag.ToString(), out normalisedOpacity);
                normalisedOpacity *= 255;

                byte opacity = Convert.ToByte(normalisedOpacity);

                PerPixelAlphaBlend.SetBitmap(backImage, opacity, Left, Top, Handle);

            }
            finally
            {
                ResumeLayout();
            }
        }

        TextureBrush backgroundImageTextureBrush = null;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!DesignMode)
            {
                var cp = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE);
                cp |= NativeMethods.WS_EX_LAYERED;
                NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_EXSTYLE, cp);
            }
            DrawForm(this, EventArgs.Empty);
        }

        private void RoundedForm_PaddingChanged(object sender, EventArgs e)
        {
            DrawForm(this, e);
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            DrawForm(this, e);
        }

        private bool stop = false;

        internal void Stop()
        {
            if (stop) return;
            
            stop = true;
            
            try
            {
                // Dispose cached pens
                cachedBackgroundPen?.Dispose();
                cachedBackgroundPen = null;
                
                cachedBorderPen?.Dispose();
                cachedBorderPen = null;
                
                backgroundImageTextureBrush?.Dispose();
                backgroundImageTextureBrush = null;
                
                backGraphics?.Dispose();
                backGraphics = null;
                
                backImage?.Dispose();
                backImage = null;
                
                privateBackgroundImage?.Dispose();
                privateBackgroundImage = null;
                
                targetFormReference = null;
            }
            catch { }
        }
    }
}
