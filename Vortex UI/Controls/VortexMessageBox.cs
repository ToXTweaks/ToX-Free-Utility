using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Vortex.Ui
{
    /// <summary>
    /// A lightweight, dark-themed replacement for <see cref="System.Windows.Forms.MessageBox"/>.
    /// </summary>
    public static class VortexMessageBox
    {
        public enum MessageBoxButtons
        {
            OK = 0,
            OKCancel = 1,
            YesNo = 2,
            YesNoCancel = 3
        }

        public static DialogResult Show(string message, string title = "Message",
            MessageBoxButtons buttons = MessageBoxButtons.OK, bool darkTheme = true)
        {
            using (var form = new VortexMessageForm(message, title, buttons, darkTheme))
            {
                Form owner = Form.ActiveForm;
                return owner != null ? form.ShowDialog(owner) : form.ShowDialog();
            }
        }

        private sealed class VortexMessageForm : Form
        {
            private readonly Color _backColor;
            private readonly Color _textColor;
            private readonly Color _accent = Color.FromArgb(4, 120, 251);
            private readonly int _radius = 12;

            public VortexMessageForm(string message, string title, MessageBoxButtons buttons, bool darkTheme)
            {
                _backColor = darkTheme ? Color.FromArgb(32, 32, 36) : Color.FromArgb(245, 245, 247);
                _textColor = darkTheme ? Color.White : Color.FromArgb(30, 30, 30);

                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.CenterParent;
                ShowInTaskbar = false;
                BackColor = _backColor;
                Font = new Font("Segoe UI", 9f, FontStyle.Regular);
                MinimumSize = new Size(320, 150);
                Padding = new Padding(1);
                DoubleBuffered = true;

                int width = 380;

                var titleLabel = new Label
                {
                    Text = title ?? string.Empty,
                    ForeColor = _textColor,
                    Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                    AutoSize = false,
                    Location = new Point(20, 18),
                    Size = new Size(width - 40, 24)
                };

                var messageLabel = new Label
                {
                    Text = message ?? string.Empty,
                    ForeColor = _textColor,
                    AutoSize = false,
                    Location = new Point(20, 52),
                    Size = new Size(width - 40, 0),
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Regular)
                };

                Size textNeeded = TextRenderer.MeasureText(messageLabel.Text, messageLabel.Font,
                    new Size(width - 40, 0), TextFormatFlags.WordBreak);
                messageLabel.Height = Math.Max(40, textNeeded.Height);

                int buttonsTop = messageLabel.Bottom + 24;

                var buttonPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.RightToLeft,
                    Location = new Point(20, buttonsTop),
                    Size = new Size(width - 40, 40),
                    WrapContents = false
                };

                foreach (var def in GetButtons(buttons))
                {
                    var btn = CreateButton(def.Item1, def.Item2, def.Item3);
                    buttonPanel.Controls.Add(btn);
                    if (def.Item3) AcceptButton = btn;
                    if (def.Item2 == DialogResult.Cancel || def.Item2 == DialogResult.No)
                        CancelButton = btn;
                }

                Controls.Add(titleLabel);
                Controls.Add(messageLabel);
                Controls.Add(buttonPanel);

                ClientSize = new Size(width, buttonPanel.Bottom + 20);
            }

            private IEnumerable<Tuple<string, DialogResult, bool>> GetButtons(MessageBoxButtons buttons)
            {
                // Returned in display order; FlowDirection is RightToLeft so the first added sits rightmost.
                switch (buttons)
                {
                    case MessageBoxButtons.OKCancel:
                        yield return Tuple.Create("OK", DialogResult.OK, true);
                        yield return Tuple.Create("Cancel", DialogResult.Cancel, false);
                        break;
                    case MessageBoxButtons.YesNo:
                        yield return Tuple.Create("Yes", DialogResult.Yes, true);
                        yield return Tuple.Create("No", DialogResult.No, false);
                        break;
                    case MessageBoxButtons.YesNoCancel:
                        yield return Tuple.Create("Yes", DialogResult.Yes, true);
                        yield return Tuple.Create("No", DialogResult.No, false);
                        yield return Tuple.Create("Cancel", DialogResult.Cancel, false);
                        break;
                    default:
                        yield return Tuple.Create("OK", DialogResult.OK, true);
                        break;
                }
            }

            private Button CreateButton(string text, DialogResult result, bool primary)
            {
                var btn = new Button
                {
                    Text = text,
                    DialogResult = result,
                    FlatStyle = FlatStyle.Flat,
                    Size = new Size(96, 32),
                    Margin = new Padding(8, 0, 0, 0),
                    ForeColor = primary ? Color.White : _textColor,
                    BackColor = primary ? _accent : Color.FromArgb(60, 60, 66),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = primary
                    ? ControlPaint.Light(_accent, 0.1f)
                    : Color.FromArgb(78, 78, 84);
                return btn;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Accent bar at the top
                using (var brush = new SolidBrush(_accent))
                    e.Graphics.FillRectangle(brush, 0, 0, Width, 3);

                // Border
                using (var pen = new Pen(Color.FromArgb(70, 70, 76)))
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }

            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);
                Region = System.Drawing.Region.FromHrgn(
                    CreateRoundRectRgn(0, 0, Width, Height, _radius, _radius));
            }

            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            private static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int cx, int cy);

            // Allow dragging the borderless dialog.
            protected override void WndProc(ref Message m)
            {
                const int WM_NCHITTEST = 0x84;
                const int HTCLIENT = 1;
                const int HTCAPTION = 2;
                base.WndProc(ref m);
                if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)
                {
                    Point pos = PointToClient(Cursor.Position);
                    if (pos.Y <= 46)
                        m.Result = (IntPtr)HTCAPTION;
                }
            }
        }
    }
}
