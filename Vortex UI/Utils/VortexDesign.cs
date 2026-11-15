using System;
using System.Diagnostics;

namespace Vortex.Ui
{
    /// <summary>
    /// Shared helpers for detecting the design-time (Visual Studio) host.
    /// </summary>
    internal static class VortexDesign
    {
        /// <summary>
        /// True when the current process is the Visual Studio shell hosting the WinForms designer.
        /// Reliable for deeply-nested child controls, where the per-control DesignMode property
        /// returns false at paint time.
        /// </summary>
        public static readonly bool IsDesignerProcess = DetectDesignerProcess();

        private static bool DetectDesignerProcess()
        {
            try
            {
                string name = Process.GetCurrentProcess().ProcessName;
                return string.Equals(name, "devenv", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(name, "DesignToolsServer", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(name, "Blend", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
