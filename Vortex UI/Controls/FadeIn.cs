using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vortex.Ui
{
    /// <summary>
    /// Direction of the entrance slide.
    /// </summary>
    public enum AnimationType
    {
        None = 0,
        SlideLeft = 1,
        SlideRight = 2,
        SlideUp = 3,
        SlideDown = 4
    }

    /// <summary>
    /// A tiny, time-based entrance animation used when switching pages.
    /// It is duration-driven (not frame-count driven), so on a slow machine it
    /// simply draws fewer frames instead of running longer — no lag, ever.
    /// </summary>
    public static class FadeIn
    {
        // One in-flight animation per control; a new request cancels the old one.
        private static readonly ConditionalWeakTable<Control, CancellationTokenSource> _running =
            new ConditionalWeakTable<Control, CancellationTokenSource>();

        /// <summary>
        /// Slides <paramref name="control"/> into its final position.
        /// </summary>
        /// <param name="control">The control (page) to bring in.</param>
        /// <param name="type">Slide direction.</param>
        /// <param name="durationMs">Total animation time in milliseconds.</param>
        /// <param name="distancePx">How far (in pixels) the control travels.</param>
        public static async Task AnimateAsync(Control control, AnimationType type = AnimationType.SlideRight,
            int durationMs = 170, int distancePx = 36)
        {
            if (control == null || control.IsDisposed)
                return;

            // Cancel any previous animation still running on this control.
            if (_running.TryGetValue(control, out var prev))
            {
                prev.Cancel();
                prev.Dispose();
                _running.Remove(control);
            }

            control.BringToFront();
            control.Visible = true;

            if (type == AnimationType.None || durationMs <= 0 || distancePx <= 0)
                return;

            var cts = new CancellationTokenSource();
            _running.Remove(control);
            _running.Add(control, cts);
            CancellationToken token = cts.Token;

            Point final = control.Location;

            int dx = 0, dy = 0;
            switch (type)
            {
                case AnimationType.SlideRight: dx = -distancePx; break; // start left,  move right
                case AnimationType.SlideLeft:  dx =  distancePx; break; // start right, move left
                case AnimationType.SlideDown:  dy = -distancePx; break; // start above, move down
                case AnimationType.SlideUp:    dy =  distancePx; break; // start below, move up
            }

            try
            {
                var sw = Stopwatch.StartNew();
                while (true)
                {
                    if (control.IsDisposed || token.IsCancellationRequested)
                        return;

                    double t = sw.Elapsed.TotalMilliseconds / durationMs;
                    if (t >= 1.0)
                        break;

                    float eased = EaseOutCubic((float)t);
                    int ox = (int)Math.Round(dx * (1f - eased));
                    int oy = (int)Math.Round(dy * (1f - eased));
                    control.Location = new Point(final.X + ox, final.Y + oy);

                    await Task.Delay(16, token); // ~60 FPS cap; drops frames on slow hardware
                }
            }
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                if (!control.IsDisposed && !token.IsCancellationRequested)
                    control.Location = final;

                if (_running.TryGetValue(control, out var current) && current == cts)
                    _running.Remove(control);
                cts.Dispose();
            }
        }

        private static float EaseOutCubic(float t)
        {
            t = t < 0f ? 0f : (t > 1f ? 1f : t);
            return 1f - (float)Math.Pow(1f - t, 3);
        }
    }
}
