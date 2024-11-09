using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToX_Free_Utility.LoadingForms
{
    public partial class UninstallJunkSuccess : Form
    {
        private Timer modalEffect_Timer; // Timer for the fade effect
        private bool isShow; // Determines if the modal is currently shown

        public UninstallJunkSuccess()
        {
            InitializeComponent();
            this.Opacity = 0; // Start invisible

            // Initialize and configure the timer
            modalEffect_Timer = new Timer
            {
                Interval = 30
            };
            modalEffect_Timer.Tick += ModalEffect_Timer_Tick;
        }

        public static async Task ShowModal(Form parent, UninstallJunkSuccess modalForm)
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
                        Opacity += 0.05; // Increase opacity slowly
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
                        Opacity -= 0.05; // Decrease opacity slowly
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
            }
        }

        // Close button click event
        private void guna2ImageButton1_Click_1(object sender, EventArgs e)
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

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            CloseModal();
        }
    }
}
