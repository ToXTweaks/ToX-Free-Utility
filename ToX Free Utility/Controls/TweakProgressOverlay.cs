using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToX_Free_Utility_NEW_ERA.Controls
{
    public class TweakProgressOverlay : Control
    {
        private static readonly Color Accent = Color.FromArgb(4, 120, 251);
        private static readonly Color AccentGlow = Color.FromArgb(90, 170, 255);
        private static readonly Color CardTop = Color.FromArgb(32, 34, 40);
        private static readonly Color CardBottom = Color.FromArgb(12, 14, 18);
        private static readonly Color Success = Color.FromArgb(7, 181, 96);
        private static readonly Color SuccessGlow = Color.FromArgb(72, 210, 140);

        private readonly Timer _animTimer;
        private float _fadeAlpha;
        private bool _isClosing;
        private Form _parentForm;

        private string _actionTitle = "APPLYING TWEAK";
        private string _tweakTitle = "";
        private string _statusText = "Applying system optimizations...";
        private Color _accentColor = Accent;
        private Color _accentGlow = AccentGlow;

        private Rectangle _cardBounds = Rectangle.Empty;
        private Rectangle _progressTrackBounds = Rectangle.Empty;
        private float _targetProgress;
        private float _currentProgress;
        private bool _isCompleted;

        private GraphicsPath _cachedCardPath;
        private GraphicsPath _cachedShadowPath;
        private readonly Font _titleFont;
        private readonly Font _subFont;
        private readonly Font _statusFont;
        private readonly Font _percentFont;

        public TweakProgressOverlay()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Visible = false;

            _titleFont = CreateFont(12f, FontStyle.Bold);
            _subFont = CreateFont(9.5f, FontStyle.Bold);
            _statusFont = CreateFont(8.5f, FontStyle.Regular);
            _percentFont = CreateFont(9.5f, FontStyle.Bold);

            _animTimer = new Timer { Interval = 12 };
            _animTimer.Tick += OnAnimTimerTick;
        }

        public static async Task<bool> ExecuteWithOverlayAsync(Form parentForm, string tweakTitle, bool isEnabling, Func<Task> tweakAction)
        {
            if (parentForm == null || parentForm.IsDisposed)
            {
                if (tweakAction != null)
                    await tweakAction();
                return true;
            }

            foreach (Control child in parentForm.Controls)
            {
                if (child is TweakProgressOverlay existing)
                {
                    parentForm.Controls.Remove(existing);
                    existing.Dispose();
                    break;
                }
            }

            var overlay = new TweakProgressOverlay();
            overlay.Configure(tweakTitle, isEnabling);
            overlay.AttachToForm(parentForm);
            overlay.StartOpen();

            overlay.SetProgress(25f, "Preparing the tweak...");
            await Task.Delay(120);

            string workingText = isEnabling
                ? "Writing system settings..."
                : "Restoring the previous settings...";
            overlay.SetProgress(60f, workingText);

            try
            {
                if (tweakAction != null)
                {
                    Task actionTask = tweakAction();
                    var started = DateTime.UtcNow;
                    float crawl = 60f;

                    while (!actionTask.IsCompleted)
                    {
                        await Task.WhenAny(actionTask, Task.Delay(350));
                        if (actionTask.IsCompleted)
                            break;

                        int elapsed = (int)(DateTime.UtcNow - started).TotalSeconds;
                        crawl = Math.Min(84f, crawl + (elapsed < 8 ? 1.2f : 0.35f));
                        string status = elapsed < 4
                            ? workingText
                            : workingText + "  " + elapsed + "s";
                        overlay.SetProgress(crawl, status);
                    }

                    await actionTask;
                }
            }
            catch (Exception ex)
            {
                overlay.SetProgress(100f, "Error: " + ex.Message);
                await Task.Delay(700);
                overlay.CloseOverlay();
                return false;
            }

            overlay.SetProgress(88f, "Finishing up...");
            await Task.Delay(130);

            overlay.SetCompleted(isEnabling
                ? "Tweak applied successfully."
                : "Tweak reverted successfully.");
            await Task.Delay(420);

            overlay.CloseOverlay();
            return true;
        }

        public void Configure(string tweakTitle, bool isEnabling)
        {
            _tweakTitle = string.IsNullOrWhiteSpace(tweakTitle) ? "TWEAK" : tweakTitle.ToUpperInvariant();
            _actionTitle = isEnabling ? "APPLYING TWEAK" : "REVERTING TWEAK";
            _statusText = isEnabling ? "Applying system optimizations..." : "Restoring default configuration...";
            _accentColor = Accent;
            _accentGlow = AccentGlow;
            _currentProgress = 0f;
            _targetProgress = 0f;
            _isCompleted = false;
        }

        public void SetProgress(float progressPercent, string status)
        {
            _targetProgress = Math.Max(0f, Math.Min(100f, progressPercent));
            if (!string.IsNullOrEmpty(status))
                _statusText = status;
            Invalidate();
        }

        public void SetCompleted(string completedMessage)
        {
            _targetProgress = 100f;
            _currentProgress = 100f;
            _isCompleted = true;
            _statusText = completedMessage;
            _accentColor = Success;
            _accentGlow = SuccessGlow;
            Invalidate();
        }

        public void AttachToForm(Form form)
        {
            _parentForm = form;
            Bounds = new Rectangle(0, 0, form.ClientSize.Width, form.ClientSize.Height);
            form.Controls.Add(this);
            BringToFront();
            UpdateLayout();
            form.Resize += OnParentFormResize;
        }

        public void StartOpen()
        {
            _fadeAlpha = 0.35f;
            _isClosing = false;
            Visible = true;
            _animTimer.Start();
        }

        public void CloseOverlay()
        {
            if (_isClosing)
                return;

            _isClosing = true;
            _animTimer.Start();
        }

        private void OnParentFormResize(object sender, EventArgs e)
        {
            if (_parentForm == null)
                return;

            Bounds = new Rectangle(0, 0, _parentForm.ClientSize.Width, _parentForm.ClientSize.Height);
            UpdateLayout();
            Invalidate();
        }

        private void UpdateLayout()
        {
            if (Width <= 0 || Height <= 0)
                return;

            int cardW = Math.Min(480, Math.Max(320, Width - 80));
            int cardH = 196;
            _cardBounds = new Rectangle((Width - cardW) / 2, (Height - cardH) / 2, cardW, cardH);
            _progressTrackBounds = new Rectangle(_cardBounds.X + 28, _cardBounds.Y + 108, _cardBounds.Width - 56, 12);

            if (_cachedCardPath != null)
                _cachedCardPath.Dispose();
            _cachedCardPath = CreateRoundRectPath(_cardBounds, 16);

            if (_cachedShadowPath != null)
                _cachedShadowPath.Dispose();
            _cachedShadowPath = CreateRoundRectPath(
                new Rectangle(_cardBounds.X - 3, _cardBounds.Y + 4, _cardBounds.Width + 6, _cardBounds.Height),
                16);
        }

        private void OnAnimTimerTick(object sender, EventArgs e)
        {
            if (_currentProgress < _targetProgress)
            {
                _currentProgress += (_targetProgress - _currentProgress) * 0.35f + 1f;
                if (_currentProgress > _targetProgress)
                    _currentProgress = _targetProgress;
            }

            if (!_isClosing)
            {
                _fadeAlpha = Math.Min(1f, _fadeAlpha + 0.35f);
            }
            else
            {
                _fadeAlpha -= 0.4f;
                if (_fadeAlpha <= 0f)
                {
                    _animTimer.Stop();
                    RemoveFromForm();
                    return;
                }
            }

            Invalidate();
        }

        private void RemoveFromForm()
        {
            if (_parentForm != null)
            {
                _parentForm.Resize -= OnParentFormResize;
                _parentForm.Controls.Remove(this);
            }

            Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int backdropAlpha = (int)(210 * _fadeAlpha);
            using (var bg = new SolidBrush(Color.FromArgb(backdropAlpha, 5, 7, 10)))
                g.FillRectangle(bg, ClientRectangle);

            if (_cardBounds.Width <= 0 || _cachedCardPath == null)
                return;

            if (_cachedShadowPath != null)
            {
                using (var shadow = new SolidBrush(Color.FromArgb((int)(90 * _fadeAlpha), 0, 0, 0)))
                    g.FillPath(shadow, _cachedShadowPath);
            }

            using (var cardBrush = new LinearGradientBrush(_cardBounds, CardTop, CardBottom, LinearGradientMode.Vertical))
                g.FillPath(cardBrush, _cachedCardPath);

            using (var border = new Pen(Color.FromArgb((int)(180 * _fadeAlpha), _accentColor), 1.5f))
                g.DrawPath(border, _cachedCardPath);

            int dot = 8;
            int dotX = _cardBounds.Left + 24;
            int dotY = _cardBounds.Top + 26;
            using (var dotBrush = new SolidBrush(Color.FromArgb((int)(255 * _fadeAlpha), _accentColor)))
                g.FillEllipse(dotBrush, dotX, dotY, dot, dot);
            using (var glow = new Pen(Color.FromArgb((int)(90 * _fadeAlpha), _accentGlow), 2f))
                g.DrawEllipse(glow, dotX - 2, dotY - 2, dot + 4, dot + 4);

            using (var titleBrush = new SolidBrush(Color.FromArgb((int)(255 * _fadeAlpha), Color.White)))
                g.DrawString(_actionTitle, _titleFont, titleBrush, dotX + 16, _cardBounds.Top + 20);

            using (var subBrush = new SolidBrush(Color.FromArgb((int)(210 * _fadeAlpha), 176, 186, 204)))
                g.DrawString(_tweakTitle, _subFont, subBrush, dotX + 16, _cardBounds.Top + 46);

            string percentText = ((int)_currentProgress) + "%";
            SizeF percentSize = g.MeasureString(percentText, _percentFont);
            float percentX = _progressTrackBounds.Right - percentSize.Width;
            float percentY = _progressTrackBounds.Top - 22;

            using (var percentBrush = new SolidBrush(Color.FromArgb((int)(255 * _fadeAlpha), _accentGlow)))
                g.DrawString(percentText, _percentFont, percentBrush, percentX, percentY);

            using (var labelBrush = new SolidBrush(Color.FromArgb((int)(150 * _fadeAlpha), 150, 160, 176)))
                g.DrawString("PROGRESS", _statusFont, labelBrush, _progressTrackBounds.Left, percentY + 2);

            using (GraphicsPath trackPath = CreateRoundRectPath(_progressTrackBounds, 6))
            {
                using (var track = new SolidBrush(Color.FromArgb((int)(255 * _fadeAlpha), 18, 20, 26)))
                    g.FillPath(track, trackPath);
            }

            int fillWidth = (int)(_progressTrackBounds.Width * Math.Max(0f, Math.Min(1f, _currentProgress / 100f)));
            if (fillWidth > 6)
            {
                var fillRect = new Rectangle(_progressTrackBounds.X, _progressTrackBounds.Y, fillWidth, _progressTrackBounds.Height);
                using (GraphicsPath fillPath = CreateRoundRectPath(fillRect, 6))
                using (var fill = new LinearGradientBrush(fillRect, _accentColor, _accentGlow, LinearGradientMode.Horizontal))
                    g.FillPath(fill, fillPath);
            }

            Color statusColor = _isCompleted
                ? Color.FromArgb((int)(255 * _fadeAlpha), Success)
                : Color.FromArgb((int)(190 * _fadeAlpha), 168, 176, 190);

            using (var statusBrush = new SolidBrush(statusColor))
                g.DrawString(_statusText, _statusFont, statusBrush, _progressTrackBounds.Left, _progressTrackBounds.Bottom + 14);
        }

        private static Font CreateFont(float size, FontStyle style)
        {
            try
            {
                return new Font("Bahnschrift", size, style);
            }
            catch
            {
                return new Font("Segoe UI", size, style);
            }
        }

        private static GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0)
                return path;

            float d = Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height));
            float x = rect.X;
            float y = rect.Y;
            float right = rect.Right;
            float bottom = rect.Bottom;

            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(right - d, y, d, d, 270, 90);
            path.AddArc(right - d, bottom - d, d, d, 0, 90);
            path.AddArc(x, bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer.Stop();
                _animTimer.Dispose();
                if (_cachedCardPath != null)
                    _cachedCardPath.Dispose();
                if (_cachedShadowPath != null)
                    _cachedShadowPath.Dispose();
                _titleFont.Dispose();
                _subFont.Dispose();
                _statusFont.Dispose();
                _percentFont.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
