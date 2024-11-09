using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToX_Free_Utility
{
    public partial class Purchase : Form
    {
        private Timer modalEffect_Timer; // Timer for the fade effect
        private bool isShow; // Determines if the modal is currently shown

        public Purchase()
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

        public static async Task ShowModal(Form parent, Purchase modalForm)
        {
            await Task.Delay(100); // Slight delay for visual consistency

            // Create the background overlay form
            Form modalBackground = new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0,  // Start transparent
                BackColor = Color.Black,
                Size = parent.ClientSize,
                Location = parent.PointToScreen(Point.Empty),
                ShowInTaskbar = false,
                TopMost = true  // Ensure it appears above the main form
            };

            // Gradually fade in the background to create a shadow effect
            modalBackground.Show();
            for (double opacity = 0; opacity <= 0.5; opacity += 0.05)
            {
                modalBackground.Opacity = opacity;
                await Task.Delay(20);
            }

            // Set the overlay form as the owner of the modal form
            modalForm.Owner = modalBackground;

            // Center the modalForm within the main form
            CenterForm(modalForm, parent);

            modalForm.isShow = true; // Set the flag to show
            modalForm.Show(); // Show the modal
            modalForm.modalEffect_Timer.Start(); // Start the opening animation

            // Wait until the modal form is closed
            await Task.Run(() =>
            {
                while (modalForm.Visible)
                {
                    Application.DoEvents(); // Keep the UI responsive
                }
            });

            // Gradually fade out the background overlay
            for (double opacity = modalBackground.Opacity; opacity >= 0; opacity -= 0.05)
            {
                modalBackground.Opacity = opacity;
                await Task.Delay(20);
            }
            modalBackground.Dispose(); // Dispose of the background after closing
        }


        // Center the modal form within the main form
        public static void CenterForm(Form newForm, Form parentForm)
        {
            int centerX = parentForm.Left + (parentForm.Width - newForm.Width) / 2;
            int centerY = parentForm.Top + (parentForm.Height - newForm.Height) / 2;

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

        // Method to start the closing animation
        public void CloseModal()
        {
            isShow = false; // Set the flag to indicate closing
            modalEffect_Timer.Start(); // Start the closing animation
        }

        private void guna2ImageButton2_Click(object sender, EventArgs e)
        {
            CloseModal(); // Trigger closing animation
        }

        private void PurchaseButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://toxtweaks.com/");
        }

        private void ShowcaseButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.youtube.com/watch?v=bJ1mIKzJaoU");
        }

        private void BenchmarksButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://cdn.discordapp.com/attachments/1283522030297288785/1283522609790713948/New_Project_1-1.png?ex=67063cd4&is=6704eb54&hm=34dafd1d0118fbc0c1ff1dee2484d0b78f8bb8c15dc9747eeaabb5cb10e40590&");
            Process.Start("https://cdn.discordapp.com/attachments/1283522030297288785/1286373206793523220/6c84d18a3ac4d9b2560382484a2a6749.png?ex=6706b868&is=670566e8&hm=9542821a3cf88af2d123dcb2820f12fbef726b5bca6161932f79c4d91b383cfc&");
        }

        private void Purchase_Shown(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.TopMost = false;
        }

        private void Purchase_Deactivate(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
