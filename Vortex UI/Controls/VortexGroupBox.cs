using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexGroupBox : ContainerControl
    {
        private string _text = "GroupBox";
        private Font _headerFont;
        private Color _borderColor = Color.FromArgb(60, 60, 60);
        private Color _headerBackColor = Color.FromArgb(45, 45, 45);
        private Color _backgroundColor = Color.FromArgb(35, 35, 35);
        private Color _headerTextColor = Color.FromArgb(200, 200, 200);
        private int _borderRadius = 8;
        private int _headerHeight = 35;
        private float _borderWidth = 1f;

        // Child title properties
        private bool _showChildTitle = false;
        private string _childTitle = "Child Title";
        private Font _childTitleFont;
        private Color _childTitleColor = Color.FromArgb(150, 150, 150);
        private int _childTitleHeight = 13;
        private int _childTitleSpacing = 1;

        public VortexGroupBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
         ControlStyles.UserPaint |
         ControlStyles.OptimizedDoubleBuffer |
         ControlStyles.ResizeRedraw |
         ControlStyles.SupportsTransparentBackColor |
         ControlStyles.ContainerControl, true);

            _headerFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            _childTitleFont = new Font("Segoe UI", 7.5F, FontStyle.Regular);
            BackColor = Color.Transparent;
            ForeColor = Color.FromArgb(200, 200, 200);
            UpdatePadding();

            // Enable auto-scrolling for child controls
            AutoScroll = true;
        }

        public override string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        public Color HeaderBackColor
        {
            get { return _headerBackColor; }
            set
            {
                _headerBackColor = value;
                Invalidate();
            }
        }

        public Color GroupBackColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                //BackColor = value;
                Invalidate();
            }
        }

        public Color HeaderTextColor
        {
            get { return _headerTextColor; }
            set
            {
                _headerTextColor = value;
                Invalidate();
            }
        }

        public int BorderRadius
        {
            get { return _borderRadius; }
            set
            {
                _borderRadius = Math.Max(0, value);
                Invalidate();
            }
        }

        public bool ShowChildTitle
        {
            get { return _showChildTitle; }
            set
            {
                _showChildTitle = value;
                UpdatePadding();
                Invalidate();
            }
        }

        public string ChildTitle
        {
            get { return _childTitle; }
            set
            {
                _childTitle = value ?? "";
                Invalidate();
            }
        }

        public Font ChildTitleFont
        {
            get { return _childTitleFont; }
            set
            {
                _childTitleFont = value ?? new Font("Segoe UI", 7.5F);
                Invalidate();
            }
        }

        public Color ChildTitleColor
        {
            get { return _childTitleColor; }
            set
            {
                _childTitleColor = value;
                Invalidate();
            }
        }

        public int ChildTitleHeight
        {
            get { return _childTitleHeight; }
            set
            {
                _childTitleHeight = Math.Max(0, value);
                UpdatePadding();
                Invalidate();
            }
        }

        public int ChildTitleSpacing
        {
            get { return _childTitleSpacing; }
            set
            {
                _childTitleSpacing = Math.Max(0, value);
                UpdatePadding();
                Invalidate();
            }
        }

        public int HeaderHeight
        {
            get { return _headerHeight; }
            set
            {
                _headerHeight = Math.Max(20, value);
                UpdatePadding();
                Invalidate();
            }
        }

        private void UpdatePadding()
        {
            int totalHeaderHeight = _headerHeight + (_showChildTitle ? _childTitleSpacing + _childTitleHeight : 0);
            Padding = new Padding(10, totalHeaderHeight + 10, 10, 10);
        }

        public float BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                _borderWidth = Math.Max(0.5f, value);
                Invalidate();
            }
        }

        public Font HeaderFont
        {
            get { return _headerFont; }
            set
            {
                _headerFont = value ?? new Font("Segoe UI", 9F);
                Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // If fully opaque background color, fill normally
            if (BackColor != Color.Transparent || Parent == null)
            {
                base.OnPaintBackground(e);
                return;
            }

            // Design-time: skip the expensive parent render, use a cheap solid fill.
            if (VortexDesign.IsDesignerProcess)
            {
                using (var brush = new SolidBrush(Parent.BackColor.A > 0 ? Parent.BackColor : System.Drawing.SystemColors.Control))
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
                return;
            }

            // Paint parent's background onto this control so transparency looks correct
            var g = e.Graphics;
            var state = g.Save();

            // Translate so parent's painting aligns with child location
            g.TranslateTransform(-Left, -Top);

            // Let parent draw itself into our Graphics
            var pea = new PaintEventArgs(g, new Rectangle(Point.Empty, Parent.ClientSize));
            // First background then foreground of parent
            InvokePaintBackground(Parent, pea);
            InvokePaint(Parent, pea);

            g.Restore(state);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Use float for smoother rendering - adjust for border width
            float offset = _borderWidth / 2f;
            RectangleF bounds = new RectangleF(offset, offset, Width - _borderWidth, Height - _borderWidth);

            int totalHeaderHeight = _headerHeight + (_showChildTitle ? _childTitleSpacing + _childTitleHeight : 0);
            RectangleF headerBounds = new RectangleF(offset, offset, Width - _borderWidth, totalHeaderHeight);

            // Create the main rounded rectangle path
            using (GraphicsPath mainPath = CreateRoundedRectangleF(bounds, _borderRadius))
            {
                // Fill the entire background first
                using (SolidBrush bgBrush = new SolidBrush(_backgroundColor))
                {
                    g.FillPath(bgBrush, mainPath);
                }
            }

            // Create header path with only top corners rounded
            using (GraphicsPath headerPath = CreateHeaderPath(headerBounds, _borderRadius))
            {
                // Fill header background
                using (SolidBrush headerBrush = new SolidBrush(_headerBackColor))
                {
                    g.FillPath(headerBrush, headerPath);
                }
            }

            // Draw the separator line connected to the borders
            float separatorY = totalHeaderHeight + offset;
            using (Pen separatorPen = new Pen(_borderColor, _borderWidth))
            {
                // Draw full width separator line
                g.DrawLine(separatorPen, offset, separatorY, Width - offset, separatorY);
            }

            // Draw the main border last
            using (GraphicsPath mainPath = CreateRoundedRectangleF(bounds, _borderRadius))
            {
                using (Pen borderPen = new Pen(_borderColor, _borderWidth))
                {
                    g.DrawPath(borderPen, mainPath);
                }
            }

            // Draw header text - center it vertically within the main header height only
            if (!string.IsNullOrEmpty(_text))
            {
                Rectangle textBounds = new Rectangle(12, 0, Width - 24, _headerHeight);
                TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;

                TextRenderer.DrawText(g, _text, _headerFont, textBounds, _headerTextColor, flags);
            }

            // Draw child title if enabled - position it directly below the main title
            if (_showChildTitle && !string.IsNullOrEmpty(_childTitle) && _childTitleHeight > 0)
            {
                // Calculate the actual height of the main title text
                Size mainTextSize = TextRenderer.MeasureText(_text ?? "", _headerFont);

                // Position child title right below the main text with minimal spacing
                int mainTextCenterY = _headerHeight / 2;
                int mainTextBottomY = mainTextCenterY + (mainTextSize.Height / 2);
                int childTitleY = mainTextBottomY + _childTitleSpacing;

                Rectangle childTextBounds = new Rectangle(12, childTitleY, Width - 24, _childTitleHeight);
                TextFormatFlags childFlags = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis;

                TextRenderer.DrawText(g, _childTitle, _childTitleFont, childTextBounds, _childTitleColor, childFlags);
            }

            // Don't call base.OnPaint(e) for ContainerControl to avoid double painting
        }

        private GraphicsPath CreateRoundedRectangleF(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            float diameter = radius * 2f;
            RectangleF arc = new RectangleF(bounds.Location, new SizeF(diameter, diameter));

            // Top-left corner
            path.AddArc(arc, 180, 90);

            // Top edge
            path.AddLine(bounds.X + radius, bounds.Y, bounds.Right - radius, bounds.Y);

            // Top-right corner
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Right edge
            path.AddLine(bounds.Right, bounds.Y + radius, bounds.Right, bounds.Bottom - radius);

            // Bottom-right corner
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom edge
            path.AddLine(bounds.Right - radius, bounds.Bottom, bounds.X + radius, bounds.Bottom);

            // Bottom-left corner
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            // Left edge
            path.AddLine(bounds.X, bounds.Bottom - radius, bounds.X, bounds.Y + radius);

            path.CloseFigure();
            return path;
        }

        private GraphicsPath CreateHeaderPath(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            float diameter = radius * 2f;
            RectangleF arc = new RectangleF(bounds.Location, new SizeF(diameter, diameter));

            // Start from bottom-left
            path.AddLine(bounds.X, bounds.Bottom, bounds.X, bounds.Y + radius);

            // Top-left corner
            path.AddArc(arc, 180, 90);

            // Top edge
            path.AddLine(bounds.X + radius, bounds.Y, bounds.Right - radius, bounds.Y);

            // Top-right corner
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Right edge to bottom
            path.AddLine(bounds.Right, bounds.Y + radius, bounds.Right, bounds.Bottom);

            // Bottom edge
            path.AddLine(bounds.Right, bounds.Bottom, bounds.X, bounds.Bottom);

            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _headerFont?.Dispose();
                _childTitleFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}