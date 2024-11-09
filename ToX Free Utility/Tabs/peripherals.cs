using Guna.UI2.WinForms;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Drawing;
using ToX_Free_Utility.LoadingForms;

namespace ToX_Free_Utility.Tabs
{
    public partial class peripherals : Form
    {

        public peripherals()
        {
            InitializeComponent();
        }

        private void peripherals_Load(object sender, EventArgs e)
        {
            KBOpti.Checked = Properties.Settings.Default.KBOpti;
            KBDelay.Checked = Properties.Settings.Default.KBDelay;
            KBRate.Checked = Properties.Settings.Default.KBRate;
            KBDQS.Checked = Properties.Settings.Default.KBDQS;
            MOpti.Checked = Properties.Settings.Default.MOpti;
            M1_1.Checked = Properties.Settings.Default.M1_1;
            MAcc.Checked = Properties.Settings.Default.MAcc;
            MDQS.Checked = Properties.Settings.Default.MDQS;
        }

        private void SaveState(Guna2ToggleSwitch Tswitch, string Tbool)
        {
            Properties.Settings.Default[Tbool] = Tswitch.Checked;
            Properties.Settings.Default.Save();
        }

        private void KBOpti_CheckedChanged(object sender, EventArgs e)
        {
            SaveState(KBOpti, "KBOpti");
        }

        private void KBDelay_CheckedChanged(object sender, EventArgs e)
        {
            SaveState(KBDelay, "KBDelay");
        }

        private void KBRate_CheckedChanged(object sender, EventArgs e)
        {
            SaveState(KBRate, "KBRate");
        }

        private void KBDQS_CheckedChanged(object sender, EventArgs e)
        {
            SaveState(KBDQS, "KBDQS");
        }

        private void MOpti_CheckedChanged(object sender, EventArgs e)
        {
            SaveState(MOpti, "MOpti");
        }

        private void M1_1_CheckedChanged(object sender, EventArgs e)
        {
            SaveState(M1_1, "M1_1");
        }

        private void MAcc_CheckedChanged(object sender, EventArgs e)
        {
            SaveState(MAcc, "MAcc");
        }

        private void MDQS_CheckedChanged(object sender, EventArgs e)
        {
            SaveState(MDQS, "MDQS");
        }

        private async void KBOpti_Click(object sender, EventArgs e)
        {
            if (KBOpti.Checked)
            {
                // Create and configure the loading form to stay on top
                TweaksLoading loadingForm = new TweaksLoading
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };

                // Access the Main instance through the parent form
                Main mainForm = (Main)this.ParentForm;

                // Show the loading form as a modal (awaited)
                TweaksLoading.ShowModal(mainForm, loadingForm);

                // Simulate some delay (e.g., applying tweaks)
                await Task.Delay(2000);

                // Run the keyboard optimization process
                await Task.Run(() => KeyboardOptimization());

                // Close the loading form with an animation
                loadingForm.CloseModal();

                // Show the success form after completion
                TweaksSuccess successForm = new TweaksSuccess
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };
                await TweaksSuccess.ShowModal(mainForm, successForm);
            }
        }

        private async void KBDelay_Click(object sender, EventArgs e)
        {
            if (KBDelay.Checked)
            {
                TweaksLoading loadingForm = new TweaksLoading
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };

                Main mainForm = (Main)this.ParentForm;
                TweaksLoading.ShowModal(mainForm, loadingForm);

                await Task.Delay(2000);
                await Task.Run(() => KeyboardDelay());

                loadingForm.CloseModal();

                TweaksSuccess successForm = new TweaksSuccess
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };
                await TweaksSuccess.ShowModal(mainForm, successForm);
            }
        }

        private async void KBRate_Click(object sender, EventArgs e)
        {
            if (KBRate.Checked)
            {
                TweaksLoading loadingForm = new TweaksLoading
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };

                Main mainForm = (Main)this.ParentForm;
                TweaksLoading.ShowModal(mainForm, loadingForm);

                await Task.Delay(2000);
                await Task.Run(() => KeyboardRate());

                loadingForm.CloseModal();

                TweaksSuccess successForm = new TweaksSuccess
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };
                await TweaksSuccess.ShowModal(mainForm, successForm);
            }
        }

        private async void KBDQS_Click(object sender, EventArgs e)
        {
            if (KBDQS.Checked)
            {
                TweaksLoading loadingForm = new TweaksLoading
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };

                Main mainForm = (Main)this.ParentForm;
                TweaksLoading.ShowModal(mainForm, loadingForm);

                await Task.Delay(2000);
                await Task.Run(() => KeyboardDQS());

                loadingForm.CloseModal();

                TweaksSuccess successForm = new TweaksSuccess
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };
                await TweaksSuccess.ShowModal(mainForm, successForm);
            }
        }

        private async void MOpti_Click(object sender, EventArgs e)
        {
            if (MOpti.Checked)
            {
                TweaksLoading loadingForm = new TweaksLoading
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };

                Main mainForm = (Main)this.ParentForm;
                TweaksLoading.ShowModal(mainForm, loadingForm);

                await Task.Delay(2000);
                await Task.Run(() => MouseOptimization());

                loadingForm.CloseModal();

                TweaksSuccess successForm = new TweaksSuccess
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };
                await TweaksSuccess.ShowModal(mainForm, successForm);
            }
        }

        private async void M1_1_Click(object sender, EventArgs e)
        {
            if (M1_1.Checked)
            {
                TweaksLoading loadingForm = new TweaksLoading
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };

                Main mainForm = (Main)this.ParentForm;
                TweaksLoading.ShowModal(mainForm, loadingForm);

                await Task.Delay(2000);
                await Task.Run(() => Mouse1_1Optimization());

                loadingForm.CloseModal();

                TweaksSuccess successForm = new TweaksSuccess
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };
                await TweaksSuccess.ShowModal(mainForm, successForm);
            }
        }

        private async void MAcc_Click(object sender, EventArgs e)
        {
            if (MAcc.Checked)
            {
                TweaksLoading loadingForm = new TweaksLoading
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };

                Main mainForm = (Main)this.ParentForm;
                TweaksLoading.ShowModal(mainForm, loadingForm);

                await Task.Delay(2000);
                await Task.Run(() => MouseAcceleration());

                loadingForm.CloseModal();

                TweaksSuccess successForm = new TweaksSuccess
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };
                await TweaksSuccess.ShowModal(mainForm, successForm);
            }
        }

        private async void MDQS_Click(object sender, EventArgs e)
        {
            if (MDQS.Checked)
            {
                TweaksLoading loadingForm = new TweaksLoading
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };

                Main mainForm = (Main)this.ParentForm;
                TweaksLoading.ShowModal(mainForm, loadingForm);

                await Task.Delay(2000);
                await Task.Run(() => MouseDQS());

                loadingForm.CloseModal();

                TweaksSuccess successForm = new TweaksSuccess
                {
                    StartPosition = FormStartPosition.CenterParent,
                    TopMost = true
                };
                await TweaksSuccess.ShowModal(mainForm, successForm);
            }
        }

        private void MouseOptimization()
        {
            ExecuteBatchCommand("Reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Input\\Settings\\ControllerProcessor\\CursorSpeed\" /v \"CursorSensitivity\" /t REG_DWORD /d \"64\" /f\r\n");
            ExecuteBatchCommand("Reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Input\\Settings\\ControllerProcessor\\CursorSpeed\" /v \"CursorUpdateInterval\" /t REG_DWORD /d \"1\" /f\r\n");
            ExecuteBatchCommand("Reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Input\\Settings\\ControllerProcessor\\CursorSpeed\" /v \"IRecho !S_MAGENTA!oteNavigationDelta\" /t REG_DWORD /d \"10\" /f\r\n");

            ExecuteBatchCommand("Reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Input\\Settings\\ControllerProcessor\\CursorMagnetism\" /v \"AttractionRectInsetInDIPS\" /t REG_DWORD /d \"5\" /f\r\n");
            ExecuteBatchCommand("Reg.exe add \"HKLM\\SOFTWARE\\Microsoft\\Input\\Settings\\ControllerProcessor\\CursorMagnetism\" /v \"DistanceThresholdInDIPS\" /t REG_DWORD /d \"40\" /f\r\n");

            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Mouse\" /v \"MouseSensitivity\" /t REG_SZ /d \"10\" /f\r\n");
            ExecuteBatchCommand("Reg.exe add \"HKU\\.DEFAULT\\Control Panel\\Mouse\" /v \"MouseSpeed\" /t REG_SZ /d \"0\" /f\r\n");
            ExecuteBatchCommand("Reg.exe add \"HKU\\.DEFAULT\\Control Panel\\Mouse\" /v \"MouseThreshold1\" /t REG_SZ /d \"0\" /f\r\n");
            ExecuteBatchCommand("Reg.exe add \"HKU\\.DEFAULT\\Control Panel\\Mouse\" /v \"MouseThreshold2\" /t REG_SZ /d \"0\" /f\r\n");

            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Mouse\" /v \"SmoothMouseYCurve\" /t REG_BINARY /d \"0000000000000000000038000000000000007000000000000000A800000000000000E00000000000\" /f\r\n");
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Mouse\" /v \"SmoothMouseXCurve\" /t REG_BINARY /d \"0000000000000000C0CC0C0000000000809919000000000040662600000000000033330000000000\" /f\r\n");
        }

        private void KeyboardOptimization()
        {
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Keyboard\" /v \"KeyboardSpeed\" /t REG_DWORD /d \"31\" /f\r\n");
        }

        private void KeyboardDelay()
        {
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Keyboard\" /v \"KeyboardDelay\" /t REG_DWORD /d \"1\" /f\r\n");
        }

        private void KeyboardRate()
        {
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Keyboard\" /v \"KeyboardRepeatRate\" /t REG_DWORD /d \"0\" /f\r\n");
        }

        private void KeyboardDQS()
        {
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Keyboard\" /v \"KeyboardRepeatRate\" /t REG_DWORD /d \"32\" /f\r\n");
        }

        private void Mouse1_1Optimization()
        {
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Mouse\" /v \"MouseSpeed\" /t REG_DWORD /d \"1\" /f\r\n");
        }

        private void MouseAcceleration()
        {
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Mouse\" /v \"MouseAcceleration\" /t REG_DWORD /d \"0\" /f\r\n");
        }

        private void MouseDQS()
        {
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Mouse\" /v \"MouseDQS\" /t REG_DWORD /d \"1\" /f\r\n");
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
    }
}

