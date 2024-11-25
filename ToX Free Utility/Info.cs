using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows.Forms;

namespace ToX_Free_Utility
{
    public partial class Info : Form
    {

        public Info()
        {
            InitializeComponent();
            InitializeComponents();
            FetchDownloadCount();
            CenterLabel();
        }

        private void OpenDiscordInvite(string inviteCode)
        {
            string discordInviteUrlApp = $"discord:/invite/{inviteCode}";
            string discordInviteUrl = $"https://discord.com/invite/{inviteCode}";
            Process.Start(new ProcessStartInfo
            {
                FileName = discordInviteUrl,
                UseShellExecute = true
            });
            //try
            //{
            //    Process.Start(new ProcessStartInfo
            //    {
            //        FileName = discordInviteUrlApp,
            //        UseShellExecute = true
            //    });
            //}
            //catch (Exception ex)
            //{
            //    Process.Start(new ProcessStartInfo
            //    {
            //        FileName = discordInviteUrl,
            //        UseShellExecute = true
            //    });
            //}
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
            MessageBox.Show("Soon");
        }
        private async void FetchDownloadCount()
        {
            string apiUrl = "https://api.github.com/repos/ToXTweaks/ToX-Free-Utility/releases";

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("ToX-Free-Utility");

                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    JArray releases = JArray.Parse(responseBody);

                    int totalDownloads = 0;

                    foreach (var release in releases)
                    {
                        JArray assets = release["assets"] as JArray;
                        if (assets != null)
                        {
                            foreach (var asset in assets)
                            {
                                totalDownloads += asset["download_count"].Value<int>();
                            }
                        }
                    }

                    downloads.Text = totalDownloads.ToString();
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

        private static readonly HttpClient client = new HttpClient();

        private async void UpdateChecker_Click(object sender, EventArgs e)
        {
            string currentVersion = "2.1";
            string apiUrl = "https://api.github.com/repos/ToXTweaks/ToX-Free-Utility/releases/latest";

            try
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseBody);

                string latestVersionTag = json["tag_name"].Value<string>();
                string latestVersion = latestVersionTag.StartsWith("v") ? latestVersionTag.Substring(1) : latestVersionTag;

                if (new Version(latestVersion) > new Version(currentVersion))
                {
                    DialogResult dialogResult = MessageBox.Show(
                        $"A new version ({latestVersion}) is available! Do you want to download it?",
                        "Update Available",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (dialogResult == DialogResult.Yes)
                    {
                        OpenLink(json["html_url"].Value<string>());
                    }
                }
                else
                {
                    MessageBox.Show("You are using the latest version.", "Up to Date", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking for updates: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ToXCoding_Click(object sender, EventArgs e)
        {
            OpenDiscordInvite("hrghbbuqsV");
        }
    }
}