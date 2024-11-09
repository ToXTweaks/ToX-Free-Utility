using Guna.UI2.WinForms;
using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
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

            // Initialize performance counters
            InitializePerformanceCounters();

            // Create a single timer to update performance data
            updateTimer = new Timer
            {
                Interval = 1000 // 1 second interval
            };
            updateTimer.Tick += async (s, e) => await UpdatePerformanceData();
            updateTimer.Start();

            // Load system information asynchronously
            LoadSystemInfoAsync();
            welcomemsg.Text = "Welcome, " +Environment.UserName;
            guna2Separator1.Width = welcomemsg.Width - 12;
        }

        private void InitializePerformanceCounters()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
        }

        private async Task UpdatePerformanceData()
        {
            // Asynchronously retrieve CPU and RAM usage
            float cpuUsage = await Task.Run(() => cpuCounter.NextValue());
            float ramUsage = await Task.Run(() => ramCounter.NextValue());

            // Update UI with performance data
            UpdatePerformanceUI(cpuUsage, ramUsage);
        }

        private void UpdatePerformanceUI(float cpuUsage, float ramUsage)
        {
            // Update the labels and progress bars on the UI thread
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => UpdatePerformanceUI(cpuUsage, ramUsage)));
                return;
            }

            labelCPU.Text = $"{cpuUsage:0}%";
            labelRAM.Text = $"{ramUsage:0}%";
            guna2CircleProgressBarCPU.Value = (int)cpuUsage;
            guna2CircleProgressBar1.Value = (int)ramUsage;
            labelFreeCPU.Text = $"{100 - cpuUsage:0}%";
            labelFreeRAM.Text = $"{100 - ramUsage:0}%";

            // Update process and thread counts
            UpdateProcessAndThreadCounts();
        }

        private void UpdateProcessAndThreadCounts()
        {
            int processCount = Process.GetProcesses().Length;
            labelProcessCount.Text = $"Process Count: {processCount}";

            int threadCount = 0;
            // Use Task to avoid UI blocking while calculating thread count
            Task.Run(() =>
            {
                foreach (Process process in Process.GetProcesses())
                {
                    threadCount += process.Threads.Count;
                }
                // Update thread count on the UI thread
                UpdateThreadCountUI(threadCount);
            });
        }

        private void UpdateThreadCountUI(int threadCount)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => UpdateThreadCountUI(threadCount)));
                return;
            }

            labelThreadCount.Text = $"Thread Count: {threadCount}";
        }

        public async void LoadSystemInfoAsync()
        {
            // Asynchronously load system information
            await Task.Run(() =>
            {
                SetCpuName();
                SetGpuName();
                SetRamInfo();
                SetMotherboardInfo();
                SetDiskInfo();
                SetNicInfo();
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

        private void guna2PictureBox10_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox5_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox14_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox15_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox16_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void Home_Load(object sender, EventArgs e)
        {
            
        }
    }
}
