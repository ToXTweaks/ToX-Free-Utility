using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ToX_Free_Utility_NEW_ERA.Settings
{
    public sealed class ActionLogForm : Form
    {
        private readonly TextBox _logBox;

        public ActionLogForm()
        {
            Text = "ToX Action Log";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(780, 540);
            MinimumSize = new Size(520, 360);
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            Font = new Font("Bahnschrift", 9F);

            _logBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(18, 18, 18),
                ForeColor = Color.FromArgb(220, 220, 220),
                WordWrap = false,
                HideSelection = false
            };

            var bar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            var refresh = CreateButton("Refresh", 12);
            refresh.Click += (_, __) => LoadLog();

            var openFolder = CreateButton("Open log folder", 110);
            openFolder.Click += (_, __) => OpenFolder();

            bar.Controls.Add(refresh);
            bar.Controls.Add(openFolder);

            Controls.Add(_logBox);
            Controls.Add(bar);
            LoadLog();
        }

        private static Button CreateButton(string text, int left)
        {
            return new Button
            {
                Text = text,
                Left = left,
                Top = 8,
                Width = text.Length > 8 ? 130 : 90,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(4, 120, 251),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
        }

        private void LoadLog()
        {
            _logBox.Text = ActionLog.ReadAll();
            _logBox.SelectionStart = _logBox.TextLength;
            _logBox.ScrollToCaret();
        }

        private static void OpenFolder()
        {
            try
            {
                DirectoryEx.Ensure(ActionLog.LogFolder);
                Process.Start(new ProcessStartInfo
                {
                    FileName = ActionLog.LogFolder,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not open the log folder." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Action Log",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private static class DirectoryEx
        {
            public static void Ensure(string path)
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }
    }
}
