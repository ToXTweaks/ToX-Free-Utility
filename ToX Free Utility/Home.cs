using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Guna.UI2.Material.Animation.AnimationManager;

namespace ToX_Free_Utility
{

    public partial class Home : Form
    {
        private Timer timer;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        public Home()
        {
            InitializeComponent();
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

            // Timer to update the progress bars
            Timer timer = new Timer();
            timer.Interval = 1000; // 1 second interval
            timer.Tick += UpdatePerformanceData;
            timer.Start();
        }
        private void UpdatePerformanceData(object sender, EventArgs e)
        {
            // Get CPU and RAM usage
            float cpuUsage = cpuCounter.NextValue();
            float ramUsage = ramCounter.NextValue();

            // Calculate free percentages
            float freeCPU = 100 - cpuUsage;
            float freeRAM = 100 - ramUsage;

            // Update the circle progress bars
            guna2CircleProgressBarCPU.Value = (int)cpuUsage;
            guna2CircleProgressBar1.Value = (int)ramUsage;

            // Update the "Used" percentage labels
            labelCPU.Text = $"{cpuUsage:0}%";
            labelRAM.Text = $"{ramUsage:0}%";

            // Update the "Free" percentage labels
            labelFreeCPU.Text = $"{freeCPU:0}%";
            labelFreeRAM.Text = $"{freeRAM:0}%";
        }
        private void Home_Load(object sender, EventArgs e)
        {

        }
    }
}
