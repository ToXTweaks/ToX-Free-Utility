using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexDataGridView : DataGridView
    {
        // Basic properties
        private int _cornerCutSize = 10;
        private bool _showBorder = true;
        private Color _borderColor = Color.Black;
        private float _borderWidth = 2f;
        private Color _vortexBackgroundColor = Color.FromArgb(240, 240, 240);

        #region Properties

        [Category("Vortex Appearance")]
        [Description("Size of the cut corner in pixels")]
        public int CornerCutSize
        {
            get { return _cornerCutSize; }
            set
            {
                _cornerCutSize = value;
                this.RecreateHandle();
                Invalidate();
            }
        }

        [Category("Vortex Appearance")]
        [Description("Determines whether to show custom border around the grid")]
        public bool ShowVortexBorder
        {
            get { return _showBorder; }
            set
            {
                _showBorder = value;
                Invalidate();
            }
        }

        [Category("Vortex Appearance")]
        [Description("Color of the custom grid border")]
        public Color VortexBorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        [Category("Vortex Appearance")]
        [Description("Width of the custom grid border")]
        public float VortexBorderWidth
        {
            get { return _borderWidth; }
            set
            {
                _borderWidth = value;
                Invalidate();
            }
        }

        [Category("Vortex Appearance")]
        [Description("Background color of the custom grid")]
        public Color VortexBackgroundColor
        {
            get { return _vortexBackgroundColor; }
            set
            {
                _vortexBackgroundColor = value;
                Invalidate();
            }
        }

        #endregion

        public VortexDataGridView()
        {
            // Set control styles for proper rendering
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            // Default styles
            this.BackgroundColor = Color.White;
            this.BorderStyle = BorderStyle.None;
            this.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            this.RowHeadersVisible = false;
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.AllowUserToResizeRows = false;

            // Apply default styling similar to modern UIs
            this.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            this.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            try
            {
                this.ColumnHeadersDefaultCellStyle.Font = new Font("Bahnschrift", 10F, FontStyle.Regular);
            }
            catch
            {
                this.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            }
            this.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.ColumnHeadersHeight = 35;

            this.DefaultCellStyle.BackColor = Color.White;
            this.DefaultCellStyle.ForeColor = Color.Black;
            try
            {
                this.DefaultCellStyle.Font = new Font("Bahnschrift", 10F, FontStyle.Regular);
            }
            catch
            {
                this.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            }
            this.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 140, 0); // Same orange as progress bar
            this.DefaultCellStyle.SelectionForeColor = Color.White;

            this.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 249, 249);

            this.RowTemplate.Height = 30;
            this.EnableHeadersVisualStyles = false;

            // Add some padding around the grid
            this.Padding = new Padding(5);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Create the base region for the control
            GraphicsPath path = CreateDataGridPath();

            // Set the region of the control to our custom shape
            this.Region = new Region(path);

            using (SolidBrush backgroundBrush = new SolidBrush(_vortexBackgroundColor))
            {
                e.Graphics.FillPath(backgroundBrush, path);
            }

            // Draw border if enabled
            if (_showBorder)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen borderPen = new Pen(_borderColor, _borderWidth))
                {
                    borderPen.LineJoin = LineJoin.Round;
                    e.Graphics.DrawPath(borderPen, path);
                }
            }

            // Let the base DataGridView paint itself
            base.OnPaint(e);
        }

        private GraphicsPath CreateDataGridPath()
        {
            GraphicsPath path = new GraphicsPath();

            // Determine points based on properties
            PointF[] points = new PointF[]
            {
                new PointF(0, 0),                                    // Top left
                new PointF(Width, 0),                                // Top right
                new PointF(Width, Height - _cornerCutSize),          // Right before cut
                new PointF(Width - _cornerCutSize, Height),          // Bottom after cut
                new PointF(0, Height)                                // Bottom left
            };

            // Create path
            path.AddPolygon(points);
            return path;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Ensure the control is repainted with the correct shape
            this.Region = new Region(CreateDataGridPath());
            Invalidate();
        }

        // Override WndProc to handle custom painting of the DataGridView
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // WM_PAINT = 0x000F
            if (m.Msg == 0x000F)
            {
                using (Graphics g = Graphics.FromHwnd(this.Handle))
                {
                    // Set high quality rendering
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;

                    // Draw border if enabled
                    if (_showBorder)
                    {
                        GraphicsPath path = CreateDataGridPath();
                        using (Pen borderPen = new Pen(_borderColor, _borderWidth))
                        {
                            borderPen.LineJoin = LineJoin.Round;
                            g.DrawPath(borderPen, path);
                        }
                    }
                }
            }
        }

        // Add a design-time renderer for the control
        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            base.OnCellPainting(e);

            // Handle cell painting for the cut corner area
            if (e.RowIndex == this.Rows.Count - 1 && e.ColumnIndex == this.Columns.Count - 1)
            {
                // If we're painting the bottom-right cell, handle it specially to accommodate the cut corner
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    e.Paint(e.ClipBounds, DataGridViewPaintParts.All);

                    // Create a GraphicsPath for the cut corner
                    using (GraphicsPath cutPath = new GraphicsPath())
                    {
                        int x = e.CellBounds.Right - _cornerCutSize;
                        int y = e.CellBounds.Bottom - _cornerCutSize;

                        // Add the triangle to represent the cut
                        PointF[] cutPoints = new PointF[]
                        {
                            new PointF(e.CellBounds.Right, y),
                            new PointF(x, e.CellBounds.Bottom),
                            new PointF(e.CellBounds.Right, e.CellBounds.Bottom)
                        };

                        cutPath.AddPolygon(cutPoints);

                        // Fill with background color
                        using (SolidBrush cutBrush = new SolidBrush(_vortexBackgroundColor))
                        {
                            e.Graphics.FillPath(cutBrush, cutPath);
                        }
                    }

                    e.Handled = true;
                }
            }
        }
    }
}