using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Vortex.Ui
{
    [ToolboxItem(true)]
    public class VortexControlBox : UserControl
    {
        private Color _closeIconColor = Color.White;
        public Color CloseIconColor
        {
            get => _closeIconColor;
            set
            {
                if (_closeIconColor != value)
                {
                    _closeIconColor = value;
                    Invalidate();
                }
            }
        }

        private Color _maximizeIconColor = Color.White;
        public Color MaximizeIconColor
        {
            get => _maximizeIconColor;
            set
            {
                if (_maximizeIconColor != value)
                {
                    _maximizeIconColor = value;
                    Invalidate();
                }
            }
        }

        private Color _minimizeIconColor = Color.White;
        public Color MinimizeIconColor
        {
            get => _minimizeIconColor;
            set
            {
                if (_minimizeIconColor != value)
                {
                    _minimizeIconColor = value;
                    Invalidate();
                }
            }
        }

        private float _iconThickness = 1.5f;
        public float IconThickness
        {
            get => _iconThickness;
            set
            {
                if (Math.Abs(_iconThickness - value) > float.Epsilon)
                {
                    _iconThickness = value;
                    Invalidate();
                }
            }
        }

        private Color _closeHoverColor = Color.OrangeRed;
        public Color CloseHoverColor
        {
            get => _closeHoverColor;
            set
            {
                if (_closeHoverColor != value)
                {
                    _closeHoverColor = value;
                    Invalidate();
                }
            }
        }

        private Color _maximizeHoverColor = Color.FromArgb(64, 158, 255);
        public Color MaximizeHoverColor
        {
            get => _maximizeHoverColor;
            set
            {
                if (_maximizeHoverColor != value)
                {
                    _maximizeHoverColor = value;
                    Invalidate();
                }
            }
        }

        private Color _minimizeHoverColor = Color.FromArgb(64, 158, 255);
        public Color MinimizeHoverColor
        {
            get => _minimizeHoverColor;
            set
            {
                if (_minimizeHoverColor != value)
                {
                    _minimizeHoverColor = value;
                    Invalidate();
                }
            }
        }

        private Color _closeHoverBackColor = Color.Transparent;
        public Color CloseHoverBackColor
        {
            get => _closeHoverBackColor;
            set
            {
                if (_closeHoverBackColor != value)
                {
                    _closeHoverBackColor = value;
                    Invalidate();
                }
            }
        }

        private Color _maximizeHoverBackColor = Color.Transparent;
        public Color MaximizeHoverBackColor
        {
            get => _maximizeHoverBackColor;
            set
            {
                if (_maximizeHoverBackColor != value)
                {
                    _maximizeHoverBackColor = value;
                    Invalidate();
                }
            }
        }

        private Color _minimizeHoverBackColor = Color.Transparent;
        public Color MinimizeHoverBackColor
        {
            get => _minimizeHoverBackColor;
            set
            {
                if (_minimizeHoverBackColor != value)
                {
                    _minimizeHoverBackColor = value;
                    Invalidate();
                }
            }
        }

        private int _buttonSize = 20;
        public int ButtonSize
        {
            get => _buttonSize;
            set
            {
                if (_buttonSize != value)
                {
                    _buttonSize = value;
                    UpdateSize();
                    Invalidate();
                }
            }
        }

        private int _buttonSpacing = 6;
        public int ButtonSpacing
        {
            get => _buttonSpacing;
            set
            {
                if (_buttonSpacing != value)
                {
                    _buttonSpacing = value;
                    UpdateSize();
                    Invalidate();
                }
            }
        }

        private bool _showMinimizeButton = true;
        public bool ShowMinimizeButton
        {
            get => _showMinimizeButton;
            set
            {
                if (_showMinimizeButton != value)
                {
                    _showMinimizeButton = value;
                    UpdateSize();
                    Invalidate();
                }
            }
        }

        private bool _showMaximizeButton = true;
        public bool ShowMaximizeButton
        {
            get => _showMaximizeButton;
            set
            {
                if (_showMaximizeButton != value)
                {
                    _showMaximizeButton = value;
                    UpdateSize();
                    Invalidate();
                }
            }
        }

        private bool _showCloseButton = true;
        public bool ShowCloseButton
        {
            get => _showCloseButton;
            set
            {
                if (_showCloseButton != value)
                {
                    _showCloseButton = value;
                    UpdateSize();
                    Invalidate();
                }
            }
        }

        private bool _closeExitsApplication;
        public bool CloseExitsApplication
        {
            get => _closeExitsApplication;
            set => _closeExitsApplication = value;
        }

        private bool _preventMaximizeAction;
        [Category("Behavior")]
        [Description("If true, clicking the maximize button does nothing.")]
        public bool PreventMaximizeAction
        {
            get => _preventMaximizeAction;
            set => _preventMaximizeAction = value;
        }

        public event EventHandler CloseClick;
        public event EventHandler MaximizeClick;
        public event EventHandler MinimizeClick;

        private Rectangle _closeRect;
        private Rectangle _maximizeRect;
        private Rectangle _minimizeRect;
        private ButtonHover _buttonHover = ButtonHover.None;

        private enum ButtonHover
        {
            None,
            Close,
            Maximize,
            Minimize
        }

        public VortexControlBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint, true);

            BackColor = Color.Transparent;
            UpdateSize();
        }

        private void UpdateSize()
        {
            int buttonCount = 0;
            if (_showCloseButton) buttonCount++;
            if (_showMaximizeButton) buttonCount++;
            if (_showMinimizeButton) buttonCount++;

            if (buttonCount <= 0)
            {
                Size = new Size(0, 0);
                return;
            }

            int width = (buttonCount * _buttonSize) + ((buttonCount - 1) * _buttonSpacing);
            Size = new Size(width, _buttonSize);

            CalculateButtonRectangles();
        }

        private void CalculateButtonRectangles()
        {
            int x = 0;

            if (_showMinimizeButton)
            {
                _minimizeRect = new Rectangle(x, 0, _buttonSize, _buttonSize);
                x += _buttonSize + _buttonSpacing;
            }

            if (_showMaximizeButton)
            {
                _maximizeRect = new Rectangle(x, 0, _buttonSize, _buttonSize);
                x += _buttonSize + _buttonSpacing;
            }

            if (_showCloseButton)
            {
                _closeRect = new Rectangle(x, 0, _buttonSize, _buttonSize);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Parent != null && VortexDesign.IsDesignerProcess)
            {
                using (var brush = new SolidBrush(Parent.BackColor.A > 0 ? Parent.BackColor : System.Drawing.SystemColors.Control))
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
            }
            else if (Parent != null)
            {
                Graphics g = e.Graphics;
                var state = g.Save();

                g.TranslateTransform(-Left, -Top);

                Rectangle rect = new Rectangle(Left, Top, Width, Height);
                using (var pe = new PaintEventArgs(g, rect))
                {
                    InvokePaintBackground(Parent, pe);
                    InvokePaint(Parent, pe);
                }

                g.Restore(state);
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_showCloseButton)
                DrawCloseButton(g);

            if (_showMaximizeButton)
                DrawMaximizeButton(g);

            if (_showMinimizeButton)
                DrawMinimizeButton(g);
        }

        private void DrawCloseButton(Graphics g)
        {
            Color iconColor = _buttonHover == ButtonHover.Close ? _closeHoverColor : _closeIconColor;

            if (_buttonHover == ButtonHover.Close && _closeHoverBackColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(_closeHoverBackColor))
                {
                    g.FillRectangle(brush, _closeRect);
                }
            }

            using (Pen pen = new Pen(iconColor, _iconThickness))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                int padding = _buttonSize / 4;

                float x1 = _closeRect.X + padding;
                float y1 = _closeRect.Y + padding;
                float x2 = _closeRect.Right - padding;
                float y2 = _closeRect.Bottom - padding;

                g.DrawLine(pen, x1, y1, x2, y2);
                g.DrawLine(pen, x1, y2, x2, y1);
            }
        }

        private void DrawMaximizeButton(Graphics g)
        {
            Color iconColor = _buttonHover == ButtonHover.Maximize ? _maximizeHoverColor : _maximizeIconColor;

            if (_buttonHover == ButtonHover.Maximize && _maximizeHoverBackColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(_maximizeHoverBackColor))
                {
                    g.FillRectangle(brush, _maximizeRect);
                }
            }

            using (Pen pen = new Pen(iconColor, _iconThickness))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                int padding = _buttonSize / 4;
                Rectangle rect = new Rectangle(
                    _maximizeRect.X + padding,
                    _maximizeRect.Y + padding,
                    _maximizeRect.Width - padding * 2,
                    _maximizeRect.Height - padding * 2);

                bool isMaximized = FindForm() != null && FindForm().WindowState == FormWindowState.Maximized;

                if (isMaximized)
                {
                    int offset = _buttonSize / 10;
                    Rectangle frontRect = new Rectangle(
                        rect.X + offset,
                        rect.Y + offset,
                        rect.Width - offset,
                        rect.Height - offset);

                    g.DrawRectangle(pen, frontRect);
                    g.DrawLine(pen, rect.X, rect.Y, rect.X + rect.Width - offset, rect.Y);
                    g.DrawLine(pen, rect.X, rect.Y, rect.X, rect.Y + rect.Height - offset);
                }
                else
                {
                    g.DrawRectangle(pen, rect);
                }
            }
        }

        private void DrawMinimizeButton(Graphics g)
        {
            Color iconColor = _buttonHover == ButtonHover.Minimize ? _minimizeHoverColor : _minimizeIconColor;

            if (_buttonHover == ButtonHover.Minimize && _minimizeHoverBackColor != Color.Transparent)
            {
                using (var brush = new SolidBrush(_minimizeHoverBackColor))
                {
                    g.FillRectangle(brush, _minimizeRect);
                }
            }

            using (Pen pen = new Pen(iconColor, _iconThickness))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                int padding = _buttonSize / 4;
                int y = _minimizeRect.Y + _minimizeRect.Height / 2;

                g.DrawLine(pen,
                    _minimizeRect.X + padding,
                    y,
                    _minimizeRect.X + _minimizeRect.Width - padding,
                    y);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            ButtonHover oldHover = _buttonHover;

            if (_showCloseButton && _closeRect.Contains(e.Location))
            {
                _buttonHover = ButtonHover.Close;
                Cursor = Cursors.Hand;
            }
            else if (_showMaximizeButton && _maximizeRect.Contains(e.Location))
            {
                _buttonHover = ButtonHover.Maximize;
                Cursor = Cursors.Hand;
            }
            else if (_showMinimizeButton && _minimizeRect.Contains(e.Location))
            {
                _buttonHover = ButtonHover.Minimize;
                Cursor = Cursors.Hand;
            }
            else
            {
                _buttonHover = ButtonHover.None;
                Cursor = Cursors.Default;
            }

            if (oldHover != _buttonHover)
            {
                if (oldHover != ButtonHover.None)
                    InvalidateButton(oldHover);

                if (_buttonHover != ButtonHover.None)
                    InvalidateButton(_buttonHover);
            }
        }

        private void InvalidateButton(ButtonHover button)
        {
            switch (button)
            {
                case ButtonHover.Close:
                    Invalidate(_closeRect);
                    break;
                case ButtonHover.Maximize:
                    Invalidate(_maximizeRect);
                    break;
                case ButtonHover.Minimize:
                    Invalidate(_minimizeRect);
                    break;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_buttonHover != ButtonHover.None)
            {
                InvalidateButton(_buttonHover);
                _buttonHover = ButtonHover.None;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button != MouseButtons.Left)
                return;

            if (_showCloseButton && _closeRect.Contains(e.Location))
            {
                CloseClick?.Invoke(this, EventArgs.Empty);

                if (CloseClick == null && FindForm() != null)
                {
                    if (_closeExitsApplication)
                        Environment.Exit(-1);
                    else
                        FindForm().Close();
                }
            }
            else if (_showMaximizeButton && _maximizeRect.Contains(e.Location))
            {
                MaximizeClick?.Invoke(this, EventArgs.Empty);

                if (!_preventMaximizeAction && MaximizeClick == null && FindForm() != null)
                {
                    Form form = FindForm();
                    form.WindowState = form.WindowState == FormWindowState.Maximized
                        ? FormWindowState.Normal
                        : FormWindowState.Maximized;
                }
            }
            else if (_showMinimizeButton && _minimizeRect.Contains(e.Location))
            {
                MinimizeClick?.Invoke(this, EventArgs.Empty);

                if (MinimizeClick == null && FindForm() != null)
                    FindForm().WindowState = FormWindowState.Minimized;
            }
        }
    }
}