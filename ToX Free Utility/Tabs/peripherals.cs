using Guna.UI2.WinForms;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility.LoadingForms;

namespace ToX_Free_Utility.Tabs
{
    public partial class peripherals : Form
    {
        public peripherals() => InitializeComponent();

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

        private void KBOpti_CheckedChanged(object sender, EventArgs e) => SaveState(KBOpti, "KBOpti");
        private void KBDelay_CheckedChanged(object sender, EventArgs e) => SaveState(KBDelay, "KBDelay");
        private void KBRate_CheckedChanged(object sender, EventArgs e) => SaveState(KBRate, "KBRate");
        private void KBDQS_CheckedChanged(object sender, EventArgs e) => SaveState(KBDQS, "KBDQS");
        private void MOpti_CheckedChanged(object sender, EventArgs e) => SaveState(MOpti, "MOpti");
        private void M1_1_CheckedChanged(object sender, EventArgs e) => SaveState(M1_1, "M1_1");
        private void MAcc_CheckedChanged(object sender, EventArgs e) => SaveState(MAcc, "MAcc");
        private void MDQS_CheckedChanged(object sender, EventArgs e) => SaveState(MDQS, "MDQS");

        private async void KBOpti_Click(object sender, EventArgs e)
        {
            if (KBOpti.Checked) await RunWithLoadingScreenAsync(() => KeyboardOptimization());
            else RevertKeyboardOptimization();
        }

        private async void KBDelay_Click(object sender, EventArgs e)
        {
            if (KBDelay.Checked) await RunWithLoadingScreenAsync(() => KeyboardDelay());
            else RevertKeyboardDelay();
        }

        private async void KBRate_Click(object sender, EventArgs e)
        {
            if (KBRate.Checked) await RunWithLoadingScreenAsync(() => KeyboardRate());
            else RevertKeyboardRate();
        }

        private async void KBDQS_Click(object sender, EventArgs e)
        {
            if (KBDQS.Checked) await RunWithLoadingScreenAsync(() => KeyboardDQS());
            else RevertKeyboardDQS();
        }

        private async void MOpti_Click(object sender, EventArgs e)
        {
            if (MOpti.Checked) await RunWithLoadingScreenAsync(() => MouseOptimization());
            else RevertMouseOptimization();
        }

        private async void M1_1_Click(object sender, EventArgs e)
        {
            if (M1_1.Checked) await RunWithLoadingScreenAsync(() => Mouse1_1Optimization());
            else RevertMouse1_1Optimization();
        }

        private async void MAcc_Click(object sender, EventArgs e)
        {
            if (MAcc.Checked) await RunWithLoadingScreenAsync(() => MouseAcceleration());
            else RevertMouseAcceleration();
        }

        private async void MDQS_Click(object sender, EventArgs e)
        {
            if (MDQS.Checked) await RunWithLoadingScreenAsync(() => MouseDQS());
            else RevertMouseDQS();
        }

        private async Task RunWithLoadingScreenAsync(Action tweakAction)
        {
            TweaksLoading loadingForm = new TweaksLoading
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true
            };

            Main mainForm = (Main)this.ParentForm;
            TweaksLoading.ShowModal(mainForm, loadingForm);

            await Task.Delay(2000);
            await Task.Run(tweakAction);

            loadingForm.CloseModal();

            TweaksSuccess successForm = new TweaksSuccess
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true
            };
            await TweaksSuccess.ShowModal(mainForm, successForm);
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

        private void RevertMouseOptimization()
        {
            ExecuteBatchCommand("Reg.exe delete \"HKLM\\SOFTWARE\\Microsoft\\Input\\Settings\\ControllerProcessor\\CursorSpeed\" /v \"CursorSensitivity\" /f\r\n");
            ExecuteBatchCommand("Reg.exe delete \"HKLM\\SOFTWARE\\Microsoft\\Input\\Settings\\ControllerProcessor\\CursorSpeed\" /v \"CursorUpdateInterval\" /f\r\n");
            ExecuteBatchCommand("Reg.exe delete \"HKLM\\SOFTWARE\\Microsoft\\Input\\Settings\\ControllerProcessor\\CursorSpeed\" /v \"IRecho !S_MAGENTA!oteNavigationDelta\" /f\r\n");
            ExecuteBatchCommand("Reg.exe delete \"HKLM\\SOFTWARE\\Microsoft\\Input\\Settings\\ControllerProcessor\\CursorMagnetism\" /v \"AttractionRectInsetInDIPS\" /f\r\n");
            ExecuteBatchCommand("Reg.exe delete \"HKLM\\SOFTWARE\\Microsoft\\Input\\Settings\\ControllerProcessor\\CursorMagnetism\" /v \"DistanceThresholdInDIPS\" /f\r\n");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Mouse\" /v \"MouseSensitivity\" /f\r\n");
            ExecuteBatchCommand("Reg.exe delete \"HKU\\.DEFAULT\\Control Panel\\Mouse\" /v \"MouseSpeed\" /f\r\n");
            ExecuteBatchCommand("Reg.exe delete \"HKU\\.DEFAULT\\Control Panel\\Mouse\" /v \"MouseThreshold1\" /f\r\n");
            ExecuteBatchCommand("Reg.exe delete \"HKU\\.DEFAULT\\Control Panel\\Mouse\" /v \"MouseThreshold2\" /f\r\n");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Mouse\" /v \"SmoothMouseYCurve\" /f\r\n");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Mouse\" /v \"SmoothMouseXCurve\" /f\r\n");
        }

        private void KeyboardOptimization()
        {
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"AutoRepeatDelay\" /t REG_SZ /d \"0\" /f");
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"AutoRepeatRate\" /t REG_SZ /d \"0\" /f");
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"BounceTime\" /t REG_SZ /d \"0\" /f");
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"DelayBeforeAcceptance\" /t REG_SZ /d \"0\" /f");
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"Flags\" /t REG_SZ /d \"0\" /f");
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"Last BounceKey Setting\" /t REG_DWORD /d \"0\" /f");
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"Last Valid Delay\" /t REG_DWORD /d \"0\" /f");
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"Last Valid Repeat\" /t REG_DWORD /d \"0\" /f");
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"Last Valid Wait\" /t REG_DWORD /d \"1000\" /f");
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Accessibility\\StickyKeys\" /v \"Flags\" /t REG_SZ /d \"0\" /f");
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Accessibility\\ToggleKeys\" /v \"Flags\" /t REG_SZ /d \"0\" /f");
        }

        private void RevertKeyboardOptimization()
        {
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"AutoRepeatDelay\" /f");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"AutoRepeatRate\" /f");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"BounceTime\" /f");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"DelayBeforeAcceptance\" /f");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"Flags\" /f");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"Last BounceKey Setting\" /f");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"Last Valid Delay\" /f");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"Last Valid Repeat\" /f");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Accessibility\\Keyboard Response\" /v \"Last Valid Wait\" /f");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Accessibility\\StickyKeys\" /v \"Flags\" /f");
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Accessibility\\ToggleKeys\" /v \"Flags\" /f");
        }

        private void KeyboardDelay()
        {
            ExecuteBatchCommand("reg add \"HKCU\\Control Panel\\Keyboard\" /v \"KeyboardDelay\" /t REG_SZ /d \"0\" /f\r\n");
        }

        private void RevertKeyboardDelay()
        {
            ExecuteBatchCommand("reg delete \"HKCU\\Control Panel\\Keyboard\" /v \"KeyboardDelay\" /f\r\n");
        }

        private void KeyboardRate()
        {
            ExecuteBatchCommand("reg add \"HKCU\\Control Panel\\Keyboard\" /v \"KeyboardSpeed\" /t REG_SZ /d \"31\" /f\r\n");
        }

        private void RevertKeyboardRate()
        {
            ExecuteBatchCommand("reg delete \"HKCU\\Control Panel\\Keyboard\" /v \"KeyboardSpeed\" /f\r\n");
        }

        private void KeyboardDQS()
        {
            ExecuteBatchCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\kbdclass\\Parameters\" /v \"KeyboardDataQueueSize\" /t REG_DWORD /d \"16\" /f\r\n");
        }

        private void RevertKeyboardDQS()
        {
            ExecuteBatchCommand("reg delete \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\kbdclass\\Parameters\" /v \"KeyboardDataQueueSize\" /f\r\n");
        }

        private void Mouse1_1Optimization()
        {
            ExecuteBatchCommand($"reg add \"HKCU\\Control Panel\\Mouse\" /v \"MouseSensitivity\" /t REG_SZ /d \"10\" /f\r\n");
        }

        private void RevertMouse1_1Optimization()
        {
            ExecuteBatchCommand("reg delete \"HKCU\\Control Panel\\Mouse\" /v \"MouseSensitivity\" /f\r\n");
        }

        private void MouseAcceleration()
        {
            ExecuteBatchCommand("Reg.exe add \"HKCU\\Control Panel\\Mouse\" /v \"MouseAcceleration\" /t REG_DWORD /d \"0\" /f\r\n");
        }

        private void RevertMouseAcceleration()
        {
            ExecuteBatchCommand("Reg.exe delete \"HKCU\\Control Panel\\Mouse\" /v \"MouseAcceleration\" /f\r\n");
        }

        private void MouseDQS()
        {
            ExecuteBatchCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\mouclass\\Parameters\" /v \"MouseDataQueueSize\" /t REG_DWORD /d \"16\" /f\r\n");
        }

        private void RevertMouseDQS()
        {
            ExecuteBatchCommand("reg delete \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\mouclass\\Parameters\" /v \"MouseDataQueueSize\" /f\r\n");
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