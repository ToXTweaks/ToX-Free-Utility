using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ToX_Free_Utility_NEW_ERA.Settings
{
    public sealed class DashboardMetrics : IDisposable
    {
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _threadCounter;
        private bool _countersInitialized;
        private string _cpuName;

        public void EnsureInitialized()
        {
            if (_cpuName == null)
                _cpuName = ReadCpuName();

            if (_countersInitialized)
                return;

            _countersInitialized = true;

            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue();
            }
            catch
            {
                _cpuCounter = null;
            }

            try
            {
                _threadCounter = new PerformanceCounter("Process", "Thread Count", "_Total");
                _threadCounter.NextValue();
            }
            catch
            {
                _threadCounter = null;
            }
        }

        public string CpuName => _cpuName ?? "CPU";

        public float GetCpuUsagePercent()
        {
            if (_cpuCounter == null)
                return 0f;

            try
            {
                return Math.Max(0f, Math.Min(100f, _cpuCounter.NextValue()));
            }
            catch
            {
                return 0f;
            }
        }

        public MemorySnapshot GetMemorySnapshot()
        {
            var status = new MemoryStatusEx();
            status.Length = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));

            if (!GlobalMemoryStatusEx(status))
                return MemorySnapshot.Empty;

            ulong total = status.TotalPhysical;
            ulong available = status.AvailablePhysical;
            ulong used = total > available ? total - available : 0;

            return new MemorySnapshot
            {
                LoadPercent = Math.Max(0f, Math.Min(100f, status.MemoryLoad)),
                TotalBytes = total,
                UsedBytes = used,
                AvailableBytes = available
            };
        }

        public int GetProcessCount()
        {
            try
            {
                return Process.GetProcesses().Length;
            }
            catch
            {
                return 0;
            }
        }

        public int GetThreadCount()
        {
            if (_threadCounter != null)
            {
                try
                {
                    return (int)_threadCounter.NextValue();
                }
                catch
                {
                }
            }

            try
            {
                int total = 0;
                foreach (Process process in Process.GetProcesses())
                {
                    try
                    {
                        total += process.Threads.Count;
                    }
                    catch
                    {
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }

                return total;
            }
            catch
            {
                return 0;
            }
        }

        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _threadCounter?.Dispose();
            _cpuCounter = null;
            _threadCounter = null;
        }

        private static string ReadCpuName()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
                    @"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
                {
                    string name = key?.GetValue("ProcessorNameString") as string;
                    if (!string.IsNullOrWhiteSpace(name))
                        return name.Trim();
                }
            }
            catch
            {
            }

            string fallback = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            return string.IsNullOrWhiteSpace(fallback) ? "CPU" : fallback.Trim();
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private sealed class MemoryStatusEx
        {
            public uint Length;
            public uint MemoryLoad;
            public ulong TotalPhysical;
            public ulong AvailablePhysical;
            public ulong TotalPageFile;
            public ulong AvailablePageFile;
            public ulong TotalVirtual;
            public ulong AvailableVirtual;
            public ulong AvailableExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx lpBuffer);

        public sealed class MemorySnapshot
        {
            public static readonly MemorySnapshot Empty = new MemorySnapshot();

            public float LoadPercent { get; set; }
            public ulong TotalBytes { get; set; }
            public ulong UsedBytes { get; set; }
            public ulong AvailableBytes { get; set; }

            public string GetSummaryText()
            {
                if (TotalBytes == 0)
                    return "RAM unavailable";

                return $"{FormatBytes(UsedBytes)} / {FormatBytes(TotalBytes)}";
            }

            private static string FormatBytes(ulong bytes)
            {
                double gb = bytes / (1024d * 1024d * 1024d);
                if (gb >= 1d)
                    return $"{gb:0.#} GB";

                double mb = bytes / (1024d * 1024d);
                return $"{mb:0} MB";
            }
        }
    }
}
