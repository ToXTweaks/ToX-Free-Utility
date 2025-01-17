using Guna.UI2.WinForms;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToX_Free_Utility
{
    public partial class Home : Form
    {
        public static Home Instance { get; private set; }

        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private Timer updateTimer;


        public Home()
        {
            InitializeComponent();
            InitializeComponents();
            UsedOptimization();
            // Initialize performance counters
            InitializePerformanceCounters();

            // Create a single timer to update performance data
            updateTimer = new Timer
            {
                Interval = 1000 // 1 second interval
            };

            updateTimer.Tick += async (s, e) =>
            {
                if (!hasErrorOccurred) 
                {
                    await UpdatePerformanceData();
                }
            };

            updateTimer.Start();

            // Load system information asynchronously
            LoadSystemInfoAsync();
            welcomemsg.Text = "Welcome, " + Environment.UserName;
            guna2Separator1.Width = welcomemsg.Width - 12;
            
        }
        private void UsedOptimization()
        {
            int checkedCount = 0;
            // v2.0
            if (Properties.Settings.Default.KBOpti == true) checkedCount++;
            if (Properties.Settings.Default.KBDelay == true) checkedCount++;
            if (Properties.Settings.Default.KBRate == true) checkedCount++;
            if (Properties.Settings.Default.KBDQS == true) checkedCount++;
            if (Properties.Settings.Default.MOpti == true) checkedCount++;
            if (Properties.Settings.Default.M1_1 == true) checkedCount++;
            if (Properties.Settings.Default.MAcc == true) checkedCount++;
            if (Properties.Settings.Default.MDQS == true) checkedCount++;
            if (Properties.Settings.Default.GeneralNvidia == true) checkedCount++;
            if (Properties.Settings.Default.NvHidden == true) checkedCount++;
            if (Properties.Settings.Default.NviPstate == true) checkedCount++;
            if (Properties.Settings.Default.NIP == true) checkedCount++;
            if (Properties.Settings.Default.GeneralAMD == true) checkedCount++;
            if (Properties.Settings.Default.AMDHidden == true) checkedCount++;
            if (Properties.Settings.Default.AMDPStates == true) checkedCount++;
            if (Properties.Settings.Default.AmdSC == true) checkedCount++;
            if (Properties.Settings.Default.BasicPrivacy == true) checkedCount++;
            if (Properties.Settings.Default.BasicPerf == true) checkedCount++;
            if (Properties.Settings.Default.GameBar == true) checkedCount++;
            if (Properties.Settings.Default.DFeatures == true) checkedCount++;
            if (Properties.Settings.Default.PowerPlan == true) checkedCount++;
            if (Properties.Settings.Default.DCortana == true) checkedCount++;
            if (Properties.Settings.Default.TweakCSRSS == true) checkedCount++;
            if (Properties.Settings.Default.TweakMMCSS == true) checkedCount++;
            if (Properties.Settings.Default.DisableDefender == true) checkedCount++;
            if (Properties.Settings.Default.FixJPEG == true) checkedCount++;
            if (Properties.Settings.Default.BasicServices == true) checkedCount++;
            if (Properties.Settings.Default.MicrosoftEdge == true) checkedCount++;
            if (Properties.Settings.Default.BCDedit == true) checkedCount++;
            if (Properties.Settings.Default.USBPSavings == true) checkedCount++;
            if (Properties.Settings.Default.SetSVCHost == true) checkedCount++;
            if (Properties.Settings.Default.Network == true) checkedCount++;
            if (Properties.Settings.Default.Memory == true) checkedCount++;
            if (Properties.Settings.Default.DUAC == true) checkedCount++;
            // v2.1
            if (Properties.Settings.Default.CoreParking == true) checkedCount++;
            if (Properties.Settings.Default.ClearDevices == true) checkedCount++;
            if (Properties.Settings.Default.BiosTweaks == true) checkedCount++;
            if (Properties.Settings.Default.Mitigations == true) checkedCount++;
            if (Properties.Settings.Default.AltTab == true) checkedCount++;
            if (Properties.Settings.Default.HyperV == true) checkedCount++;
            // v2.2
            if (Properties.Settings.Default.Services == true) checkedCount++;
            if (Properties.Settings.Default.Kernel == true) checkedCount++;
            if (Properties.Settings.Default.Power == true) checkedCount++;
            if (Properties.Settings.Default.FSE == true) checkedCount++;
            if (Properties.Settings.Default.SSD == true) checkedCount++;
            if (Properties.Settings.Default.Hiber == true) checkedCount++;
            Optis.Text = "Used Optimizations: " + checkedCount.ToString() + "/46";
        }



        private void InitializePerformanceCounters()
        {
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading system info1: " + ex.Message);
                return;
            }
        }
        private bool hasErrorOccurred = false;

        private async Task UpdatePerformanceData()
        {
            try
            {
                // Asynchronously retrieve CPU and RAM usage
                float cpuUsage = await Task.Run(() => cpuCounter.NextValue());
                float ramUsage = await Task.Run(() => ramCounter.NextValue());

                // Update UI with performance data
                UpdatePerformanceUI(cpuUsage, ramUsage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading system info2: " + ex.Message);
                hasErrorOccurred = true;
                return;
            }
        }

        private void UpdatePerformanceUI(float cpuUsage, float ramUsage)
        {
            try
            {
                // Update the labels and progress bars on the UI thread
                if (InvokeRequired) { Invoke((MethodInvoker)(() => UpdatePerformanceUI(cpuUsage, ramUsage))); return; }

            labelCPU.Text = $"{cpuUsage:0}%";
            labelRAM.Text = $"{ramUsage:0}%";
            guna2CircleProgressBarCPU.Value = (int)cpuUsage;
            guna2CircleProgressBar1.Value = (int)ramUsage;
            labelFreeCPU.Text = $"{100 - cpuUsage:0}%";
            labelFreeRAM.Text = $"{100 - ramUsage:0}%";
                UpdateProcessAndThreadCounts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading system info3: " + ex.Message);
                return;
            }
        }

        private void UpdateProcessAndThreadCounts()
        {
            int processCount = Process.GetProcesses().Length;
            labelProcessCount.Text = $"Process Count: {processCount}";

            Task.Run(() =>
            {
                int threadCount = 0;
                foreach (Process process in Process.GetProcesses())
                    threadCount += process.Threads.Count;
                UpdateThreadCountUI(threadCount);
            });
        }

        private void UpdateThreadCountUI(int threadCount)
        {
            if (InvokeRequired) { Invoke((MethodInvoker)(() => UpdateThreadCountUI(threadCount))); return; }
            labelThreadCount.Text = $"Thread Count: {threadCount}";
        }

        public async void LoadSystemInfoAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    SetCpuName();
                    SetGpuName();
                    SetRamInfo();
                    SetMotherboardInfo();
                    SetDiskInfo();
                    SetNicInfo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading system info4: " + ex.Message);
                    return;
                }
            });
        }

        private void SetCpuName()
        {
            string cpuName = "";
            using (var searcher = new ManagementObjectSearcher("select Name from Win32_Processor"))
            {
                foreach (var item in searcher.Get())
                {
                    cpuName = item["Name"].ToString();
                    break;
                }
            }

            // Clean up CPU name
            if (cpuName.Contains("Intel"))
            {
                cpuName = "Intel " + Regex.Replace(cpuName, @"Intel\(R\)\s*|\s*CPU\s*|\(TM\)|@.*GHz", "").Trim();
            }
            else if (cpuName.Contains("AMD"))
            {
                cpuName = "AMD " + Regex.Replace(cpuName, @"AMD\s*|\(TM\)|Processor\s*|@.*GHz", "").Trim();
            }

            // Ensure the UI is accessible before updating it
            UpdateCpuNameUI(cpuName);
        }

        private void UpdateCpuNameUI(string cpuName)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => UpdateCpuNameUI(cpuName)));
                return;
            }

            labelCpuName.Text = $"CPU: {cpuName}";
        }

        private void SetGpuName()
        {
            string gpuName = "";
            using (var searcher = new ManagementObjectSearcher("select Name from Win32_VideoController"))
            {
                foreach (var item in searcher.Get())
                {
                    gpuName = item["Name"].ToString();
                    break;
                }
            }

            // Clean up GPU name
            if (gpuName.Contains("NVIDIA"))
            {
                gpuName = "NVIDIA " + Regex.Replace(gpuName, @"NVIDIA\s*|\(R\)|\(TM\)|Graphics|Adapter|Video Card|GeForce\s*", "").Trim();
            }
            else if (gpuName.Contains("AMD"))
            {
                gpuName = "AMD " + Regex.Replace(gpuName, @"AMD\s*|\(R\)|\(TM\)|Graphics|Video Card|Radeon\s*", "").Trim();
            }
            else if (gpuName.Contains("Intel"))
            {
                gpuName = "Intel " + Regex.Replace(gpuName, @"Intel\(R\)\s*|\(TM\)|Graphics|Adapter|Video Card", "").Trim();
            }

            // Ensure the UI is accessible before updating it
            UpdateGpuNameUI(gpuName);
        }

        private void UpdateGpuNameUI(string gpuName)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => UpdateGpuNameUI(gpuName)));
                return;
            }

            labelGpuName.Text = $"GPU: {gpuName}";
        }

        private void SetRamInfo()
        {
            double totalMemoryInGB = 0;
            using (var searcher = new ManagementObjectSearcher("select Capacity from Win32_PhysicalMemory"))
            {
                foreach (var item in searcher.Get())
                {
                    totalMemoryInGB += Convert.ToDouble(item["Capacity"]) / (1024 * 1024 * 1024);
                }
            }

            // Ensure the UI is accessible before updating it
            UpdateRamInfoUI(totalMemoryInGB);
        }

        private void UpdateRamInfoUI(double totalMemoryInGB)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => UpdateRamInfoUI(totalMemoryInGB)));
                return;
            }

            labelRamInfo.Text = $"RAM: {totalMemoryInGB:F1} GB";
        }

        private void SetMotherboardInfo()
        {
            using (var searcher = new ManagementObjectSearcher("select Product, Manufacturer from Win32_BaseBoard"))
            {
                foreach (var item in searcher.Get())
                {
                    string manufacturer = item["Manufacturer"].ToString().Split(' ')[0];
                    string product = item["Product"].ToString();
                    // Ensure the UI is accessible before updating it
                    UpdateMotherboardInfoUI(manufacturer, product);
                    break;
                }
            }
        }
        #region -
        public void InitializeComponents()
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name;

            string exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

            if (!appName.Equals("ToX Free Utility", StringComparison.OrdinalIgnoreCase))
            {
                Environment.Exit(0);
            }

            if (!exeName.Equals("ToX Free Utility.exe", StringComparison.OrdinalIgnoreCase))
            {
                Environment.Exit(0);
            }

            if (!this.Text.Contains("ToX Free Utility"))
            {
                Environment.Exit(0);
            }
        }
        #endregion
        private void UpdateMotherboardInfoUI(string manufacturer, string product)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => UpdateMotherboardInfoUI(manufacturer, product)));
                return;
            }

            labelMotherboardInfo.Text = $"BIOS: {manufacturer} {product}";
        }

        private void SetDiskInfo()
        {
            using (var searcher = new ManagementObjectSearcher("select Model from Win32_DiskDrive where Index = 0"))
            {
                foreach (var item in searcher.Get())
                {
                    string[] modelParts = item["Model"].ToString().Split(' ');
                    string diskModel = string.Join(" ", modelParts[0], modelParts.Length > 1 ? modelParts[1] : "").Trim();
                    // Ensure the UI is accessible before updating it
                    UpdateDiskInfoUI(diskModel);
                    break;
                }
            }
        }

        private void UpdateDiskInfoUI(string diskModel)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => UpdateDiskInfoUI(diskModel)));
                return;
            }

            labelDiskInfo.Text = $"DISK: {diskModel}";
        }

        private void SetNicInfo()
        {
            using (var searcher = new ManagementObjectSearcher("select * from Win32_NetworkAdapterConfiguration where IPEnabled = true"))
            {
                foreach (var item in searcher.Get())
                {
                    string description = item["Description"]?.ToString() ?? "";
                    if (description.Contains("Ethernet") || description.Contains("Wi-Fi") || description.Contains("Wireless"))
                    {
                        string[] nameParts = description.Split(' ');
                        string nicName = string.Join(" ", nameParts[0], nameParts.Length > 1 ? nameParts[1] : "").Trim();
                        // Ensure the UI is accessible before updating it
                        UpdateNicInfoUI(nicName);
                        return;
                    }
                }
            }
            // Ensure the UI is accessible before updating it
            UpdateNicInfoUI("Not Found");
        }

        private void UpdateNicInfoUI(string nicName)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => UpdateNicInfoUI(nicName)));
                return;
            }

            labelNicInfo.Text = $"NIC: {nicName}";
        }

        private void Home_Load(object sender, EventArgs e)
        {
            
        }
    }
}
