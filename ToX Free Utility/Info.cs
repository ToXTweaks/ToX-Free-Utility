﻿using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToX_Free_Utility
{
    public partial class Info : Form
    {

        public Info()
        {
            InitializeComponent();
            InitializeComponents();
            FetchMemberCount();
            CenterLabel();
        }

        private void OpenDiscordInvite(string inviteCode)
        {
            string discordInviteUrlApp = $"discord:/invite/{inviteCode}";
            string discordInviteUrl = $"https://discord.com/invite/{inviteCode}";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = discordInviteUrlApp,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = discordInviteUrl,
                    UseShellExecute = true
                });
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
        private void OpenLink(string url)
        {
            string link = $"{url}";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        private void ToXTweaksDisc_Click(object sender, EventArgs e)
        {
            OpenDiscordInvite("toxtweaks");
        }

        private void YouTube_Click(object sender, EventArgs e)
        {
            OpenLink("https://www.youtube.com/@toxtweaks");
        }

        private void TikTok_Click(object sender, EventArgs e)
        {
            OpenLink("https://www.tiktok.com/@tox.tweaks");
        }

        private void Twitter_Click(object sender, EventArgs e)
        {
            OpenLink("https://x.com/ToXTweaks");
        }

        private void Instagram_Click(object sender, EventArgs e)
        {
            OpenLink("https://www.instagram.com/toxtweaks/");
        }

        private void Website_Click(object sender, EventArgs e)
        {
            OpenLink("https://toxtweaks.com/");
        }

        private void GitHub_Click(object sender, EventArgs e)
        {
            OpenLink("https://github.com/ToXTweaks");
        }

        private void SlatCC_Click(object sender, EventArgs e)
        {
            OpenLink("https://slat.cc/toxtweaks");
        }

        private void TrustPilot_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Soon");
        }

        private void PayPal_Click(object sender, EventArgs e)
        {
            OpenLink("https://www.paypal.com/paypalme/toxtweaks");
        }
        private async void FetchMemberCount()
        {
            string inviteCode = "toxtweaks";
            string apiUrl = $"https://discord.com/api/v9/invites/{inviteCode}?with_counts=true";

            try
            {
                using (var client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(responseBody);

                    downloads.Text = json["approximate_member_count"].Value<string>();
                }
            }
            catch (Exception ex)
            {
                downloads.Text = "Error: " + ex.Message;
            }
        }
        private void CenterLabel()
        {
            downloads.Left = ((guna2GradientPanel2.Width - downloads.Width) / 2) + 2;
            downloads.Top = ((guna2GradientPanel2.Height - downloads.Height) / 2) + 24;
        }
    }
}