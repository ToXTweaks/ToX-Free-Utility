﻿using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToX_Free_Utility.LoadingForms
{
    public partial class TweaksLoading : Form
    {
        public Timer modalEffect_Timer; // Timer for the fade effect
        private bool isShow; // Determines if the modal is currently shown

        public TweaksLoading()
        {
            InitializeComponent();
            this.Opacity = 0; // Start invisible

            // Initialize and configure the timer
            modalEffect_Timer = new Timer
            {
                Interval = 30
            };
            modalEffect_Timer.Tick += ModalEffect_Timer_Tick;

            ValidateApp();
        }

        #region Application Validation
        private void ValidateApp()
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            string exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

            if (!appName.Equals("ToX Free Utility", StringComparison.OrdinalIgnoreCase) ||
                !exeName.Equals("ToX Free Utility.exe", StringComparison.OrdinalIgnoreCase) ||
                !this.Text.Contains("ToX Free Utility"))
            {
                Environment.Exit(0);
            }
        }
        #endregion

        public static async Task ShowModal(Form parent, TweaksLoading modalForm)
        {
            await Task.Delay(100); // Slight delay for visual consistency

            // Retrieve the MainPanel control
            var mainPanel = parent.Controls["MainPanel"] as Panel;
            if (mainPanel == null)
            {
                MessageBox.Show("MainPanel not found on the parent form.");
                return;
            }

            // Create the background overlay form
            Form modalBackground = new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.50,
                BackColor = Color.Black,
                Size = mainPanel.Size,
                Location = mainPanel.PointToScreen(Point.Empty),
                ShowInTaskbar = false
            };

            modalBackground.Show();
            modalForm.Owner = modalBackground;

            // Center the modalForm within MainPanel
            CenterForm(modalForm, mainPanel);

            modalForm.isShow = true; // Set the flag to show
            modalForm.Show(); // Show the modal
            modalForm.modalEffect_Timer.Start(); // Start the opening animation

            // Wait until the form is closed
            await Task.Run(() =>
            {
                while (modalForm.Visible)
                {
                    Application.DoEvents(); // Keep the UI responsive
                }
            });

            modalBackground.Dispose(); // Dispose of the background after closing
        }

        // Center the modal form within the target component
        public static void CenterForm(Form newForm, Panel targetComponent)
        {
            Point targetLocationOnScreen = targetComponent.PointToScreen(Point.Empty);
            int centerX = targetLocationOnScreen.X + (targetComponent.Width - newForm.Width) / 2;
            int centerY = targetLocationOnScreen.Y + (targetComponent.Height - newForm.Height) / 2;

            newForm.StartPosition = FormStartPosition.Manual;
            newForm.Location = new Point(centerX, centerY);
        }

        // Timer tick event for handling the opening and closing animations
        private void ModalEffect_Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (isShow) // Opening animation
                {
                    if (Opacity < 1)
                    {
                        Opacity += 0.1; // Increase opacity faster
                    }
                    else
                    {
                        modalEffect_Timer.Stop(); // Stop the timer when fully visible
                    }
                }
                else // Closing animation
                {
                    if (Opacity > 0)
                    {
                        Opacity -= 0.1; // Decrease opacity faster
                    }
                    else
                    {
                        modalEffect_Timer.Stop();
                        this.Close(); // Close the form when fully invisible
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions if necessary
            }
        }

        private void guna2ProgressBar3_ValueChanged(object sender, EventArgs e)
        {
            // Handle progress bar value change if needed
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            CloseModal(); // Trigger closing animation
        }

        // Method to start the closing animation
        public void CloseModal()
        {
            isShow = false; // Set the flag to indicate closing
            modalEffect_Timer.Start(); // Start the closing animation
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (modalEffect_Timer != null)
            {
                modalEffect_Timer.Stop(); // Ensure timer is stopped
            }
        }
    }
}
