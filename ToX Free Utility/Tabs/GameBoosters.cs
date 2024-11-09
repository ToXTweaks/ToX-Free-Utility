using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility.LoadingForms;

namespace ToX_Free_Utility.Tabs
{
    public partial class GameBoosters : Form
    {
        private string gamePath = string.Empty;
        private bool isClickHandled = false;

        public GameBoosters()
        {
            InitializeComponent();
            // Set up drag and drop for the panel
            DragNDrop.AllowDrop = true;
        }
        private void DragNDrop_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                gamePath = files[0];  // Store the game path
                UpdateGameLabel();     // Update the label
            }
        }

        private void DragNDrop_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && Path.GetExtension(files[0]).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void DragNDrop_Click(object sender, EventArgs e)
        {
            if (isClickHandled) return;

            isClickHandled = true;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executable files (*.exe)|*.exe";
                openFileDialog.Title = "Select the Game Executable";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    gamePath = openFileDialog.FileName;
                    UpdateGameLabel();
                }
            }

            isClickHandled = false;
        }

        private void UpdateGameLabel()
        {
            label1.Visible = true;
            label1.Text = Path.GetFileName(gamePath);
            label1.Font = new Font(label1.Font.FontFamily, 12);
            label1.TextAlign = ContentAlignment.MiddleCenter;

            int x = (DragNDrop.Width - label1.Width) / 2;
            int y = (DragNDrop.Height - label1.Height) / 2;
            label1.Location = new Point(x, y);
            guna2PictureBox7.Visible = false;
        }

        private async void OptimizeButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(gamePath))
            {
                // Create and configure the loading form to stay on top
                GameBoostersLoading loadingForm = new GameBoostersLoading
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true, // Make the form stay on top
                };

                // Access Main instance through the parent form, if this is within a child form.
                Main mainForm = (Main)this.ParentForm;

                // Center the loading form on MainPanel within Main
                CenterForm(loadingForm, mainForm.MainPanel);

                // Simulate some delay (e.g., applying tweaks)
                await Task.Delay(2000);

                // Proceed to optimize the game by running the registry commands
                await Task.Run(() => CustomGameBoosterTweaks(gamePath));

                loadingForm.Close();

                // Show the success form after applying the tweaks
                GameBoostersSuccess successForm = new GameBoostersSuccess
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true // Ensure the form stays on top
                };

                CenterForm(successForm, mainForm.MainPanel);
                mainForm.TopMost = false;
            }
            else
            {
                MessageBox.Show("Please select or drag and drop a game to optimize.");
            }

        }

        private void CustomGameBoosterTweaks(string gamePath)
        {
            string gameExeName = Path.GetFileName(gamePath);
            string gameName = Path.GetFileNameWithoutExtension(gamePath);

            // Add the necessary registry entries
            ExecuteBatchCommand($@"reg add ""HKCU\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"" /v ""{gamePath}"" /t REG_SZ /d ""~ RUNASADMIN HIGHDPIAWARE"" /f");
            ExecuteBatchCommand($@"reg add ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{gameExeName}\PerfOptions"" /v ""CpuPriorityClass"" /t REG_DWORD /d ""3"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Application Name"" /t REG_SZ /d ""{gameExeName}"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""DSCP value"" /t REG_SZ /d ""46"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Local IP"" /t REG_SZ /d ""*"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Local IP Prefix Length"" /t REG_SZ /d ""*"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Local Port"" /t REG_SZ /d ""*"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Protocol"" /t REG_SZ /d ""UDP"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Remote IP"" /t REG_SZ /d ""*"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Remote IP Prefix Length"" /t REG_SZ /d ""*"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""Remote Port"" /t REG_SZ /d ""*"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""throttle Rate"" /t REG_SZ /d ""-1"" /f");
            ExecuteBatchCommand($@"REG ADD ""HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\{gameName}"" /v ""version"" /t REG_SZ /d ""1.0"" /f");
        }

        private void ExecuteBatchCommand(string command)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command)
            {
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas"
            };

            using (Process process = new Process { StartInfo = procStartInfo })
            {
                process.Start();
                process.WaitForExit();
            }
        }

        public void CenterForm(Form newForm, Panel targetComponent)
        {
            // Get the target component's location on the screen
            Point targetLocationOnScreen = targetComponent.PointToScreen(Point.Empty);

            // Calculate centered position for the new form
            int centerX = targetLocationOnScreen.X + (targetComponent.Width - newForm.Width) / 2;
            int centerY = targetLocationOnScreen.Y + (targetComponent.Height - newForm.Height) / 2;

            // Set the new form's start position and location
            newForm.StartPosition = FormStartPosition.Manual;
            newForm.Location = new Point(centerX, centerY);

            // Show the new form
            newForm.Show();
        }

        private async void OptimizeFortnite_Click(object sender, EventArgs e)
        {
            // Create and configure the loading form
            GameBoostersLoading loadingForm = new GameBoostersLoading
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true
            };

            // Open a file dialog to let the user choose the Fortnite executable
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Fortnite Executable|FortniteClient-Win64-Shipping.exe",
                Title = "Select the Fortnite Executable",
                Multiselect = false
            };

            string selectedFilePath = string.Empty;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog.FileName;
            }
            else
            {
                MessageBox.Show("No file selected.");
                return;
            }

            // Access Main instance through the parent form, if this is within a child form.
            Main mainForm = (Main)this.ParentForm;

            // Center the loading form on MainPanel within Main
            CenterForm(loadingForm, mainForm.MainPanel);

            // Simulate some delay (e.g., applying tweaks)
            await Task.Delay(2000);

            // Proceed to optimize the game by running the registry commands
            await Task.Run(() => ApplyFortniteTweaks(selectedFilePath));

            loadingForm.Close();

            // Show the success form after applying the tweaks
            GameBoostersSuccess successForm = new GameBoostersSuccess
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true // Ensure the form stays on top
            };

            CenterForm(successForm, mainForm.MainPanel);
            mainForm.TopMost = false;
        }





        // Method to apply registry tweaks
        private void ApplyFortniteTweaks(string fortnitePath)
        {
            // Add registry settings for compatibility and performance

            // Compatibility settings
            Microsoft.Win32.Registry.SetValue(
                @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers",
                fortnitePath,
                "~ RUNASADMIN HIGHDPIAWARE"
            );

            // Cpu Priority to high
            Microsoft.Win32.Registry.SetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\FortniteClient-Win64-Shipping.exe\PerfOptions",
                "CpuPriorityClass",
                3, // Set priority class
                Microsoft.Win32.RegistryValueKind.DWord
            );

            // Network optimizations
            string qosKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\fortnite";
            Microsoft.Win32.Registry.SetValue(qosKey, "Application Name", "FortniteClient-Win64-Shipping.exe");
            Microsoft.Win32.Registry.SetValue(qosKey, "DSCP value", "46");
            Microsoft.Win32.Registry.SetValue(qosKey, "Local IP", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Local IP Prefix Length", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Local Port", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Protocol", "UDP");
            Microsoft.Win32.Registry.SetValue(qosKey, "Remote IP", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Remote IP Prefix Length", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Remote Port", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "throttle Rate", "-1");
            Microsoft.Win32.Registry.SetValue(qosKey, "version", "1.0");
        }

        private async void OptimizeValorant_Click(object sender, EventArgs e)
        {
            // Create and configure the loading form
            GameBoostersLoading loadingForm = new GameBoostersLoading
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true // Ensure the form stays on top
            };

            // Open a file dialog to let the user choose the Valorant executable
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Valorant Executable|VALORANT-Win64-Shipping.exe",
                Title = "Select the Valorant Executable",
                Multiselect = false
            };

            string selectedFilePath = string.Empty;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog.FileName;
            }
            else
            {
                MessageBox.Show("No file selected.");
                return;
            }

            // Access Main instance through the parent form, if this is within a child form.
            Main mainForm = (Main)this.ParentForm;

            // Center the loading form on MainPanel within Main
            CenterForm(loadingForm, mainForm.MainPanel);

            // Simulate some delay (e.g., applying tweaks)
            await Task.Delay(2000);

            // Proceed to optimize the game by running the registry commands
            await Task.Run(() => ApplyValorantTweaks(selectedFilePath));

            loadingForm.Close();

            // Show the success form after applying the tweaks
            GameBoostersSuccess successForm = new GameBoostersSuccess
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true // Ensure the form stays on top
            };

            CenterForm(successForm, mainForm.MainPanel);
            mainForm.TopMost = false;
        }

        // Method to apply registry tweaks specific to Valorant
        private void ApplyValorantTweaks(string valorantPath)
        {
            // Compatibility settings
            Microsoft.Win32.Registry.SetValue(
                @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers",
                valorantPath,
                "~ RUNASADMIN HIGHDPIAWARE"
            );

            // Cpu Priority to high
            Microsoft.Win32.Registry.SetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\VALORANT-Win64-Shipping.exe\PerfOptions",
                "CpuPriorityClass",
                3, // Set priority class
                Microsoft.Win32.RegistryValueKind.DWord
            );

            // Network optimizations
            string qosKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\valorant";
            Microsoft.Win32.Registry.SetValue(qosKey, "Application Name", "VALORANT-Win64-Shipping.exe");
            Microsoft.Win32.Registry.SetValue(qosKey, "DSCP value", "46");
            Microsoft.Win32.Registry.SetValue(qosKey, "Local IP", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Local IP Prefix Length", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Local Port", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Protocol", "UDP");
            Microsoft.Win32.Registry.SetValue(qosKey, "Remote IP", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Remote IP Prefix Length", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Remote Port", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "throttle Rate", "-1");
            Microsoft.Win32.Registry.SetValue(qosKey, "version", "1.0");
        }

        private async void OptimizeApex_Click(object sender, EventArgs e)
        {
            // Create and configure the loading form
            GameBoostersLoading loadingForm = new GameBoostersLoading
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true // Ensure the form stays on top
            };

            // Open a file dialog to let the user choose the Apex Legends executable
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Apex Legends Executable|r5apex.exe",
                Title = "Select the Apex Legends Executable",
                Multiselect = false
            };

            string selectedFilePath = string.Empty;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog.FileName;
            }
            else
            {
                MessageBox.Show("No file selected.");
                return;
            }

            // Access Main instance through the parent form, if this is within a child form.
            Main mainForm = (Main)this.ParentForm;

            // Center the loading form on MainPanel within Main
            CenterForm(loadingForm, mainForm.MainPanel);

            // Simulate some delay (e.g., applying tweaks)
            await Task.Delay(2000);

            // Proceed to optimize the game by running the registry commands
            await Task.Run(() => ApplyApexTweaks(selectedFilePath));

            loadingForm.Close();

            // Show the success form after applying the tweaks
            GameBoostersSuccess successForm = new GameBoostersSuccess
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true // Ensure the form stays on top
            };

            CenterForm(successForm, mainForm.MainPanel);
            mainForm.TopMost = false;
        }

        // Method to apply registry tweaks specific to Apex Legends
        private void ApplyApexTweaks(string apexPath)
        {
            // Compatibility settings
            Microsoft.Win32.Registry.SetValue(
                @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers",
                apexPath,
                "~ RUNASADMIN HIGHDPIAWARE"
            );

            // Cpu Priority to high
            Microsoft.Win32.Registry.SetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\r5apex.exe\PerfOptions",
                "CpuPriorityClass",
                3, // Set priority class
                Microsoft.Win32.RegistryValueKind.DWord
            );

            // Network optimizations
            string qosKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\apex";
            Microsoft.Win32.Registry.SetValue(qosKey, "Application Name", "r5apex.exe");
            Microsoft.Win32.Registry.SetValue(qosKey, "DSCP value", "46");
            Microsoft.Win32.Registry.SetValue(qosKey, "Local IP", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Local IP Prefix Length", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Local Port", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Protocol", "UDP");
            Microsoft.Win32.Registry.SetValue(qosKey, "Remote IP", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Remote IP Prefix Length", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Remote Port", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "throttle Rate", "-1");
            Microsoft.Win32.Registry.SetValue(qosKey, "version", "1.0");
        }

        private async void OptimizeMinecraft_Click(object sender, EventArgs e)
        {
            // Create and configure the loading form
            GameBoostersLoading loadingForm = new GameBoostersLoading
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true // Ensure the form stays on top
            };

            // Open a file dialog to let the user choose the Minecraft executable
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Minecraft Executable|Minecraft.exe",
                Title = "Select the Minecraft Executable",
                Multiselect = false
            };

            string selectedFilePath = string.Empty;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog.FileName;
            }
            else
            {
                MessageBox.Show("No file selected.");
                return;
            }

            // Access Main instance through the parent form, if this is within a child form.
            Main mainForm = (Main)this.ParentForm;

            // Center the loading form on MainPanel within Main
            CenterForm(loadingForm, mainForm.MainPanel);

            // Simulate some delay (e.g., applying tweaks)
            await Task.Delay(2000);

            // Proceed to optimize the game by running the registry commands
            await Task.Run(() => ApplyMinecraftTweaks(selectedFilePath));

            loadingForm.Close();

            // Show the success form after applying the tweaks
            GameBoostersSuccess successForm = new GameBoostersSuccess
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true // Ensure the form stays on top
            };

            CenterForm(successForm, mainForm.MainPanel);
            mainForm.TopMost = false;
        }

        // Method to apply registry tweaks specific to Minecraft
        private void ApplyMinecraftTweaks(string minecraftPath)
        {
            // Compatibility settings for Minecraft
            Microsoft.Win32.Registry.SetValue(
                @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers",
                minecraftPath,
                "~ RUNASADMIN HIGHDPIAWARE"
            );

            // CPU Priority settings for Minecraft
            Microsoft.Win32.Registry.SetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\Minecraft.exe\PerfOptions",
                "CpuPriorityClass",
                3, // Set priority class to high
                Microsoft.Win32.RegistryValueKind.DWord
            );

            // Network optimizations for Minecraft
            string qosKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\QoS\minecraft";
            Microsoft.Win32.Registry.SetValue(qosKey, "Application Name", "Minecraft.exe");
            Microsoft.Win32.Registry.SetValue(qosKey, "DSCP value", "46");
            Microsoft.Win32.Registry.SetValue(qosKey, "Local IP", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Local IP Prefix Length", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Local Port", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Protocol", "UDP");
            Microsoft.Win32.Registry.SetValue(qosKey, "Remote IP", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Remote IP Prefix Length", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "Remote Port", "*");
            Microsoft.Win32.Registry.SetValue(qosKey, "throttle Rate", "-1");
            Microsoft.Win32.Registry.SetValue(qosKey, "version", "1.0");
        }
    }
}