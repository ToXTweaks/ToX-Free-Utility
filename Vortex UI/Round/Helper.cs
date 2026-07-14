using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Vortex.Ui
{
    public static class Helper
    {
        public static int[] GetRefreshRates()
        {
            return Win32.GetRefreshRates();
        }

        public static int GetHighestRefreshRate()
        {
            return Win32.GetRefreshRate();
        }

        public static GraphicsPath RoundHexagon(Rectangle bounds, float rounding)
        {
            GraphicsPath path = new GraphicsPath();

            // so it doesnt crash (negative values also dont crash so thats cool)
            rounding = Math.Min(rounding, Math.Min(bounds.Width, bounds.Height) / 4f);

            PointF[] points = new PointF[6];
            float width = bounds.Width;
            float height = bounds.Height;

            points[0] = new PointF(bounds.X + width / 2, bounds.Y); // t
            points[1] = new PointF(bounds.X + width, bounds.Y + height / 4); // tr
            points[2] = new PointF(bounds.X + width, bounds.Y + 3 * height / 4); // br
            points[3] = new PointF(bounds.X + width / 2, bounds.Y + height); // b
            points[4] = new PointF(bounds.X, bounds.Y + 3 * height / 4); // bl
            points[5] = new PointF(bounds.X, bounds.Y + height / 4); // tl

            for (int i = 0; i < points.Length; i++)
            {
                // i did Beziers becuase Arcs are PAINN

                PointF current = points[i];
                PointF previous = points[(i - 1 + points.Length) % points.Length];
                PointF next = points[(i + 1) % points.Length];

                PointF dirToPrev = Normalize(new PointF(previous.X - current.X, previous.Y - current.Y));
                PointF dirToNext = Normalize(new PointF(next.X - current.X, next.Y - current.Y));

                PointF arcStart = new PointF(current.X + dirToPrev.X * rounding, current.Y + dirToPrev.Y * rounding);
                PointF arcEnd = new PointF(current.X + dirToNext.X * rounding, current.Y + dirToNext.Y * rounding);

                PointF control1 = new PointF(current.X + dirToPrev.X * (rounding / 2), current.Y + dirToPrev.Y * (rounding / 2));
                PointF control2 = new PointF(current.X + dirToNext.X * (rounding / 2), current.Y + dirToNext.Y * (rounding / 2));

                if (i == 0)
                    path.StartFigure();

                path.AddLine(previous, arcStart);
                path.AddBezier(arcStart, control1, control2, arcEnd);
            }

            path.CloseFigure();
            return path;
        }
        public static PointF Normalize(PointF point)
        {
            float length = (float)Math.Sqrt(point.X * point.X + point.Y * point.Y);
            return new PointF(point.X / length, point.Y / length);
        }

        public static GraphicsPath RoundRect(int x, int y, int width, int height, int borderRadius)
        {
            Rectangle rectangle = new Rectangle(x, y, width, height);

            return RoundRect(rectangle, new Padding(borderRadius));
        }

        public static GraphicsPath RoundRect(Rectangle rectangle, int borderRadius)
        {
            return RoundRect(rectangle, new Padding(borderRadius));
        }

        public static GraphicsPath RoundRect(RectangleF rectangle, int borderRadius)
        {
            return RoundRect(rectangle, new Padding(borderRadius));
        }

        public static GraphicsPath RoundRect(RectangleF rectangle, Padding borderRadius)
        {
            GraphicsPath path = new GraphicsPath();

            void AddArc(float x, float y, float diameter, float startAngle, float sweepAngle)
            {
                if (diameter > 0)
                {
                    RectangleF arc = new RectangleF(x, y, diameter, diameter);
                    path.AddArc(arc, startAngle, sweepAngle);
                }
                else
                {
                    float small = 0.000001f;
                    RectangleF arc = new RectangleF(x, y, small, small);
                    path.AddArc(arc, startAngle, sweepAngle);
                }
            }

            float topLeftDiameter = borderRadius.Top * 2;
            AddArc(rectangle.X, rectangle.Y, topLeftDiameter, 180, 90);

            float topRightDiameter = borderRadius.Left * 2;
            AddArc(rectangle.Right - topRightDiameter, rectangle.Y, topRightDiameter, 270, 90);

            float bottomRightDiameter = borderRadius.Bottom * 2;
            AddArc(rectangle.Right - bottomRightDiameter, rectangle.Bottom - bottomRightDiameter, bottomRightDiameter, 0, 90);

            float bottomLeftDiameter = borderRadius.Right * 2;
            AddArc(rectangle.X, rectangle.Bottom - bottomLeftDiameter, bottomLeftDiameter, 90, 90);

            path.CloseFigure();

            return path;
        }

        public static GraphicsPath Checkmark(Rectangle area)
        {
            GraphicsPath path = new GraphicsPath();

            Point[] points = new Point[]
            {
            new Point(area.Left + (int)(area.Width * 0.25), area.Top + (int)(area.Height * 0.5)),
            new Point(area.Left + (int)(area.Width * 0.45), area.Top + (int)(area.Height * 0.7)),
            new Point(area.Right - (int)(area.Width * 0.3), area.Top + (int)(area.Height * 0.3))
            };

            path.AddLines(points);

            return path;
        }

        public static GraphicsPath Checkmark(RectangleF area)
        {
            GraphicsPath path = new GraphicsPath();

            PointF[] points = new PointF[]
            {
            new PointF(area.Left + (int)(area.Width * 0.25), area.Top + (int)(area.Height * 0.5)),
            new PointF(area.Left + (int)(area.Width * 0.45), area.Top + (int)(area.Height * 0.7)),
            new PointF(area.Right - (int)(area.Width * 0.3), area.Top + (int)(area.Height * 0.3))
            };

            path.AddLines(points);

            return path;
        }

        public static GraphicsPath Checkmark(RectangleF area, Point symbolsOffset)
        {
            GraphicsPath path = new GraphicsPath();

            area.Offset(symbolsOffset);

            PointF[] points = new PointF[]
            {
            new PointF(area.Left + (int)(area.Width * 0.25), area.Top + (int)(area.Height * 0.5)),
            new PointF(area.Left + (int)(area.Width * 0.45), area.Top + (int)(area.Height * 0.7)),
            new PointF(area.Right - (int)(area.Width * 0.3), area.Top + (int)(area.Height * 0.3))
            };

            path.AddLines(points);

            return path;
        }

        public static GraphicsPath Crossmark(Rectangle rect)
        {
            Rectangle area = rect;
            int WidthBeforeScale = area.Width;
            area.Width = (int)Math.Round(area.Width * 0.7f, 0);
            area.Height = area.Width;

            int WidthAfterScale = area.Width;
            int WidthDifference = WidthBeforeScale - WidthAfterScale;

            area.Offset(WidthDifference / 2, 1 + (WidthDifference / 2));

            GraphicsPath path = new GraphicsPath();

            Point[] points = new Point[]
            {
            new Point(area.Left, area.Top),
            new Point(area.Right, area.Bottom)
            };

            path.AddLines(points);

            GraphicsPath path2 = new GraphicsPath();

            Point[] points2 = new Point[]
            {
            new Point(area.Left, area.Bottom),
            new Point(area.Right, area.Top)
            };

            path2.AddLines(points2);

            path.AddPath(path2, false);

            return path;
        }

        public static GraphicsPath Crossmark(Rectangle rect, Point symbolsOffset)
        {
            Rectangle area = rect;
            area.Offset(symbolsOffset);
            int WidthBeforeScale = area.Width;
            area.Width = (int)Math.Round(area.Width * 0.7f, 0);
            area.Height = area.Width;

            int WidthAfterScale = area.Width;
            int WidthDifference = WidthBeforeScale - WidthAfterScale;

            area.Offset(WidthDifference / 2, 1 + (WidthDifference / 2));

            GraphicsPath path = new GraphicsPath();

            Point[] points = new Point[]
            {
            new Point(area.Left, area.Top),
            new Point(area.Right, area.Bottom)
            };

            path.AddLines(points);

            GraphicsPath path2 = new GraphicsPath();

            Point[] points2 = new Point[]
            {
            new Point(area.Left, area.Bottom),
            new Point(area.Right, area.Top)
            };

            path2.AddLines(points2);

            path.AddPath(path2, false);

            return path;
        }

        public static GraphicsPath Crossmark(RectangleF rect, Point symbolsOffset)
        {
            RectangleF area = rect;
            area.Offset(symbolsOffset);
            float WidthBeforeScale = area.Width;
            area.Width = (int)Math.Round(area.Width * 0.7f, 0);
            area.Height = area.Width;

            float WidthAfterScale = area.Width;
            float WidthDifference = WidthBeforeScale - WidthAfterScale;

            area.Offset(WidthDifference / 2, 1 + (WidthDifference / 2));

            GraphicsPath path = new GraphicsPath();

            PointF[] points = new PointF[]
            {
            new PointF(area.Left, area.Top),
            new PointF(area.Right, area.Bottom)
            };

            path.AddLines(points);

            GraphicsPath path2 = new GraphicsPath();

            PointF[] points2 = new PointF[]
            {
            new PointF(area.Left, area.Bottom),
            new PointF(area.Right, area.Top)
            };

            path2.AddLines(points2);

            path.AddPath(path2, false);

            return path;
        }

        public static GraphicsPath Plus(Rectangle rect)
        {
            Rectangle area = rect;
            int widthBeforeScale = area.Width;
            area.Width = (int)Math.Round(area.Width * 0.7f, 0);
            area.Height = area.Width;

            int widthAfterScale = area.Width;
            int widthDifference = widthBeforeScale - widthAfterScale;

            area.Offset(widthDifference / 2, 1 + (widthDifference / 2));

            GraphicsPath path = new GraphicsPath();

            Point[] horizontalPoints = new Point[]
{
        new Point(area.Left, area.Top + (area.Height / 2)),
        new Point(area.Right, area.Top + (area.Height / 2))
};

            path.AddLines(horizontalPoints);

            GraphicsPath path2 = new GraphicsPath();

            Point[] verticalPoints = new Point[]
{
        new Point(area.Left + (area.Width / 2), area.Top),
        new Point(area.Left + (area.Width / 2), area.Bottom)
};

            path2.AddLines(verticalPoints);
            path.AddPath(path2, false);

            return path;
        }

        private static int arrowThicknessHardcoded = 2;

        public static GraphicsPath LeftArrow(Rectangle rect)
        {
            rect.Height = rect.Width;
            rect.Width = rect.Width / 2;

            GraphicsPath path = new GraphicsPath();
            path.AddLine(rect.Right, rect.Top, rect.Left, rect.Bottom / 2);
            path.AddLine(rect.Left, rect.Bottom / 2, rect.Right, rect.Bottom);

            Matrix translateMatrix = new Matrix();
            translateMatrix.Translate(3 + (arrowThicknessHardcoded * 2), 4 + (arrowThicknessHardcoded + rect.Height / 2));
            translateMatrix.Scale(1, 0.9f);
            translateMatrix.Scale(0.6f, 0.6f);
            path.Transform(translateMatrix);

            return path;
        }

        public static GraphicsPath RightArrow(Rectangle rect)
        {
            rect.Height = rect.Width;
            rect.Width = rect.Width / 2;

            rect.Offset(rect.Width - arrowThicknessHardcoded, 0);

            GraphicsPath path = new GraphicsPath();
            path.AddLine(rect.Left, rect.Top, rect.Right, rect.Bottom / 2);
            path.AddLine(rect.Right, rect.Bottom / 2, rect.Left, rect.Bottom);

            Matrix translateMatrix = new Matrix();
            translateMatrix.Translate(13 + (-arrowThicknessHardcoded * 2), 4 + (arrowThicknessHardcoded + rect.Height / 2));
            translateMatrix.Scale(1, 0.9f);
            translateMatrix.Scale(0.6f, 0.6f);
            path.Transform(translateMatrix);

            return path;
        }

        public static GraphicsPath LeftArrowtest(Rectangle rectangle)
        {
            GraphicsPath path = new GraphicsPath();

            Point[] points =
            {
            new Point(rectangle.Right, rectangle.Top),
            new Point(rectangle.Left, rectangle.Top + rectangle.Height / 2),
            new Point(rectangle.Right, rectangle.Bottom)
        };

            path.AddPolygon(points);

            return path;
        }

        public static GraphicsPath DownArrow(Rectangle rect)
        {
            GraphicsPath path = new GraphicsPath();

            Point[] points =
            {
            new Point(rect.Left, rect.Top),
            new Point(rect.Left + rect.Width / 2, rect.Bottom),
            new Point(rect.Right, rect.Top)
        };

            path.AddPolygon(points);

            return path;
        }

        public static void CopyProperties(this object source, object destination)
        {
            if (source == null || destination == null)
                throw new Exception("Source or/and Destination Objects are null");

            Type typeDest = destination.GetType();
            Type typeSrc = source.GetType();

            PropertyInfo[] srcProps = typeSrc.GetProperties();
            foreach (PropertyInfo srcProp in srcProps)
            {
                if (!srcProp.CanRead)
                {
                    continue;
                }
                PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
                if (targetProperty == null)
                {
                    continue;
                }
                if (!targetProperty.CanWrite)
                {
                    continue;
                }
                if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate)
                {
                    continue;
                }
                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    continue;
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
                {
                    continue;
                }
                targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
            }
        }

        public static GraphicsPath Star(float centerX, float centerY, float outerRadius, float innerRadius, int numPoints)
        {
            if (numPoints % 2 == 0 || numPoints < 5)
            {
                throw new ArgumentException("Number of points must be an odd number and greater than or equal to 5.");
            }

            var path = new GraphicsPath();
            float angleIncrement = 360f / numPoints;
            float currentAngle = -90f;
            PointF[] points = new PointF[numPoints * 2];

            for (int i = 0; i < numPoints * 2; i += 2)
            {
                points[i] = PointOnCircle(centerX, centerY, outerRadius, currentAngle);
                points[i + 1] = PointOnCircle(centerX, centerY, innerRadius, currentAngle + angleIncrement / 2);
                currentAngle += angleIncrement;
            }

            path.AddPolygon(points);

            return path;
        }

        private static PointF PointOnCircle(float centerX, float centerY, float radius, float angleInDegrees)
        {
            float angleInRadians = (float)(angleInDegrees * Math.PI / 180.0);
            float x = centerX + radius * (float)Math.Cos(angleInRadians);
            float y = centerY + radius * (float)Math.Sin(angleInRadians);
            return new PointF(x, y);
        }

        public static class Win32
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetDC(IntPtr hwnd);

            [DllImport("user32.dll")]
            public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

            [DllImport("gdi32.dll")]
            public static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

            [DllImport("user32.dll")]
            public static extern bool GetCursorPos(out POINT lpPoint);

            [DllImport("user32.dll")]
            public static extern short GetAsyncKeyState(int vKey);

            public static bool isClickingLeftMouse()
            {
                return (GetAsyncKeyState(0x01) & 0x8000) != 0;
            }


            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int X;
                public int Y;
            }

            [DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
          

            public const int WM_SYSCOMMAND = 0x0112;
            public const int SC_CLOSE = 0xF060;

            [StructLayout(LayoutKind.Sequential)]
            public struct DEVMODE
            {
                private const int CCHDEVICENAME = 32;
                private const int CCHFORMNAME = 32;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
                public string dmDeviceName;
                public short dmSpecVersion;
                public short dmDriverVersion;
                public short dmSize;
                public short dmDriverExtra;
                public int dmFields;
                public int dmPositionX;
                public int dmPositionY;
                public int dmDisplayOrientation;
                public int dmDisplayFixedOutput;
                public short dmColor;
                public short dmDuplex;
                public short dmYResolution;
                public short dmTTOption;
                public short dmCollate;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
                public string dmFormName;
                public short dmLogPixels;
                public int dmBitsPerPel;
                public int dmPelsWidth;
                public int dmPelsHeight;
                public int dmDisplayFlags;
                public int dmDisplayFrequency;
                public int dmICMMethod;
                public int dmICMIntent;
                public int dmMediaType;
                public int dmDitherType;
                public int dmReserved1;
                public int dmReserved2;
                public int dmPanningWidth;
                public int dmPanningHeight;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct DISPLAY_DEVICE
            {
                public int cb;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string DeviceName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string DeviceString;
                public int StateFlags;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string DeviceID;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string DeviceKey;
            }

            [DllImport("user32.dll")]
            private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

            [DllImport("user32.dll")]
            private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

            public static bool REFRESH_RATE_OVERRIDE = false;
            public static int SPOOFED_REFRESH_RATE = 60;

            public static int GetRefreshRate()
            {
                if (REFRESH_RATE_OVERRIDE)
                {
                    return SPOOFED_REFRESH_RATE;
                }

                DISPLAY_DEVICE d = new DISPLAY_DEVICE();
                d.cb = Marshal.SizeOf(d);
                DEVMODE vDevMode = new DEVMODE();
                vDevMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

                uint deviceIndex = 0;
                int maxRefreshRate = 1;

                while (EnumDisplayDevices(null, deviceIndex, ref d, 0))
                {
                    if (EnumDisplaySettings(d.DeviceName, -1, ref vDevMode))
                    {
                        int refreshRate = vDevMode.dmDisplayFrequency;
                        if (refreshRate > maxRefreshRate)
                        {
                            maxRefreshRate = refreshRate;
                        }
                    }
                    deviceIndex++;
                }

                return maxRefreshRate;
            }

            public static int[] GetRefreshRates()
            {
                List<int> refreshRates = new List<int>();
                DISPLAY_DEVICE d = new DISPLAY_DEVICE();
                d.cb = Marshal.SizeOf(d);
                DEVMODE vDevMode = new DEVMODE();
                vDevMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

                uint deviceIndex = 0;
                while (EnumDisplayDevices(null, deviceIndex, ref d, 0))
                {
                    if (EnumDisplaySettings(d.DeviceName, -1, ref vDevMode))
                    {
                        refreshRates.Add(vDevMode.dmDisplayFrequency);
                    }
                    deviceIndex++;
                }

                return refreshRates.ToArray();
            }

            internal static class NativeMethods
            {
                public const int GWL_EXSTYLE = -20;
                public const int WS_EX_LAYERED = 0x80000;

                [System.Runtime.InteropServices.DllImport("user32.dll")]
                public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

                [System.Runtime.InteropServices.DllImport("user32.dll")]
                public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

                [DllImport("user32.dll", CharSet = CharSet.Auto)]
                internal static extern bool SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);

                internal const int LWA_ALPHA = 0x2;
            }

            internal static class PerPixelAlphaBlend
            {
                public static void SetBitmap(Bitmap bitmap, int left, int top, IntPtr handle)
                {
                    SetBitmap(bitmap, 255, left, top, handle);
                }

                public unsafe static void SetBitmap(Bitmap bitmap, byte opacity, int left, int top, IntPtr handle)
                {
                    if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                        throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");


                    IntPtr screenDc = Win32.GetDC(IntPtr.Zero);
                    IntPtr memDc = Win32.CreateCompatibleDC(screenDc);
                    IntPtr hBitmap = IntPtr.Zero;
                    IntPtr oldBitmap = IntPtr.Zero;

                    try
                    {
                        hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                        oldBitmap = Win32.SelectObject(memDc, hBitmap);

                        Win32.Size size = new Win32.Size(bitmap.Width, bitmap.Height);
                        Win32.Point topPos = new Win32.Point(left, top);
                        Win32.Point pointSource = new Win32.Point(0, 0);
                        Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION
                        {
                            BlendOp = Win32.AC_SRC_OVER,
                            BlendFlags = 0,
                            SourceConstantAlpha = opacity,
                            AlphaFormat = Win32.AC_SRC_ALPHA
                        };

                        Win32.UpdateLayeredWindow(handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, Win32.ULW_ALPHA);

                    }
                    finally
                    {
                        Win32.ReleaseDC(IntPtr.Zero, screenDc);
                        if (hBitmap != IntPtr.Zero)
                        {
                            Win32.SelectObject(memDc, oldBitmap);
                            Win32.DeleteObject(hBitmap);
                        }

                        Win32.DeleteDC(memDc);
                    }
                }

                internal static class Win32
                {
                    public enum Bool
                    {
                        False = 0,
                        True
                    };


                    [StructLayout(LayoutKind.Sequential)]
                    public struct Point
                    {
                        public Int32 x;
                        public Int32 y;

                        public Point(Int32 x, Int32 y)
                        {
                            this.x = x;
                            this.y = y;
                        }
                    }


                    [StructLayout(LayoutKind.Sequential)]
                    public struct Size
                    {
                        public Int32 cx;
                        public Int32 cy;

                        public Size(Int32 cx, Int32 cy)
                        {
                            this.cx = cx;
                            this.cy = cy;
                        }
                    }


                    [StructLayout(LayoutKind.Sequential, Pack = 1)]
                    struct ARGB
                    {
                        public byte Blue;
                        public byte Green;
                        public byte Red;
                        public byte Alpha;
                    }


                    [StructLayout(LayoutKind.Sequential, Pack = 1)]
                    public struct BLENDFUNCTION
                    {
                        public byte BlendOp;
                        public byte BlendFlags;
                        public byte SourceConstantAlpha;
                        public byte AlphaFormat;
                    }


                    public const Int32 ULW_COLORKEY = 0x00000001;
                    public const Int32 ULW_ALPHA = 0x00000002;
                    public const Int32 ULW_OPAQUE = 0x00000004;

                    public const byte AC_SRC_OVER = 0x00;
                    public const byte AC_SRC_ALPHA = 0x01;


                    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
                    public static extern Bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

                    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
                    public static extern IntPtr GetDC(IntPtr hWnd);

                    [DllImport("user32.dll", ExactSpelling = true)]
                    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

                    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                    public static extern Bool DeleteDC(IntPtr hdc);

                    [DllImport("gdi32.dll", ExactSpelling = true)]
                    public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

                    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                    public static extern Bool DeleteObject(IntPtr hObject);
                }
            }
        }
    }
}

