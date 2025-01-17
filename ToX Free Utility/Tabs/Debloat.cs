using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility.LoadingForms;

namespace ToX_Free_Utility.Tabs
{
    public partial class Debloat : Form
    {
        #region Important#1
        public void AppChecker()
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            if (!appName.Equals("ToX Free Utility", StringComparison.OrdinalIgnoreCase))
            {
                Environment.Exit(0);
            }

            if (!this.Text.Contains("ToX Free Utility"))
            {
                Environment.Exit(0);
            }
        }
        #endregion

        private Timer checkButtonTimer;
        public Debloat()
        {
            InitializeComponent();
            SetupTimer();
        }

        private void SetupTimer()
        {
            checkButtonTimer = new Timer { Interval = 100 };
            checkButtonTimer.Tick += new EventHandler(OnTimerTick);
            checkButtonTimer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            UpdateLabelColors();
        }

        private void UpdateLabelColors()
        {
            var checkboxes = new[] { bingbutton, xboxbutton, wphonebutton, walarmbutton, m3dbutton, wmapsbutton, builderbutton, wsoundbutton, medgebutton, wcamerabutton,
                                     cleartemp, clearcache, cleardns, cleargamescache, clearbrowsercache, clearcookies, clearprefetch, clearhistory, clearbin, discordcache };

            var labels = new[] { binglabel, xboxlabel, wphonelabel, walarmlabel, m3dlabel, wmapslabel, builderlabel, wsoundlabel, medgelabel, wcameralabel,
                                 templabel, cachelabel, dnslabel, gamescachelabel, browsercachelabel, cookieslabel, prefetchlabel, historylabel, binlabel, discordlabel };

            for (int i = 0; i < checkboxes.Length; i++)
            {
                if (checkboxes[i] != null && labels[i] != null)
                {
                    labels[i].ForeColor = checkboxes[i].Checked ? Color.White : Color.Gainsboro;
                }
            }
        }
        private void binglabel_Click(object sender, EventArgs e) => ToggleCheckboxState(bingbutton);
        private void xboxlabel_Click(object sender, EventArgs e) => ToggleCheckboxState(xboxbutton);
        private void wphonelabel_Click(object sender, EventArgs e) => ToggleCheckboxState(wphonebutton);
        private void walarmlabel_Click(object sender, EventArgs e) => ToggleCheckboxState(walarmbutton);
        private void m3dlabel_Click(object sender, EventArgs e) => ToggleCheckboxState(m3dbutton);
        private void wmapslabel_Click(object sender, EventArgs e) => ToggleCheckboxState(wmapsbutton);
        private void builderlabel_Click(object sender, EventArgs e) => ToggleCheckboxState(builderbutton);
        private void wsoundlabel_Click(object sender, EventArgs e) => ToggleCheckboxState(wsoundbutton);
        private void wylabel_Click(object sender, EventArgs e) => ToggleCheckboxState(medgebutton);
        private void wcameralabel_Click(object sender, EventArgs e) => ToggleCheckboxState(wcamerabutton);
        private void templabel_Click(object sender, EventArgs e) => ToggleCheckboxState(cleartemp);
        private void cachelabel_Click(object sender, EventArgs e) => ToggleCheckboxState(clearcache);
        private void dnslabel_Click(object sender, EventArgs e) => ToggleCheckboxState(cleardns);
        private void gamescachelabel_Click(object sender, EventArgs e) => ToggleCheckboxState(cleargamescache);
        private void browsercachelabel_Click(object sender, EventArgs e) => ToggleCheckboxState(clearbrowsercache);
        private void cookieslabel_Click(object sender, EventArgs e) => ToggleCheckboxState(clearcookies);
        private void prefetchlabel_Click(object sender, EventArgs e) => ToggleCheckboxState(clearprefetch);
        private void historylabel_Click(object sender, EventArgs e) => ToggleCheckboxState(clearhistory);
        private void binlabel_Click(object sender, EventArgs e) => ToggleCheckboxState(clearbin);

        private void ToggleCheckboxState(Guna2CustomCheckBox checkbox) => checkbox.Checked = !checkbox.Checked;

        public void CenterForm(Form newForm, Panel targetComponent)
        {
            Point targetLocationOnScreen = targetComponent.PointToScreen(Point.Empty);
            int centerX = targetLocationOnScreen.X + (targetComponent.Width - newForm.Width) / 2;
            int centerY = targetLocationOnScreen.Y + (targetComponent.Height - newForm.Height) / 2;
            newForm.StartPosition = FormStartPosition.Manual;
            newForm.Location = new Point(centerX, centerY);
            newForm.Show();
        }

        private bool AnyCheckboxChecked() => bingbutton.Checked || xboxbutton.Checked || wphonebutton.Checked || walarmbutton.Checked ||
                                             m3dbutton.Checked || wmapsbutton.Checked || builderbutton.Checked || wsoundbutton.Checked ||
                                             medgebutton.Checked || wcamerabutton.Checked;

        private bool AnyCheckboxChecked2() => cleartemp.Checked || clearcache.Checked || cleardns.Checked || cleargamescache.Checked ||
                                              clearbrowsercache.Checked || clearcookies.Checked || clearprefetch.Checked || clearhistory.Checked ||
                                              clearbin.Checked;

        private async void Uninstall_Click(object sender, EventArgs e)
        {
            if (!AnyCheckboxChecked())
            {
                MessageBox.Show("Please select at least one app to Uninstall.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Create and configure the loading form to stay on top
            UninstallJunkLoading loadingForm = new UninstallJunkLoading
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true
            };

            // Access the Main instance through the parent form
            Main mainForm = (Main)this.ParentForm;

            // Show the loading form as a modal
            UninstallJunkLoading.ShowModal(mainForm, loadingForm);

            // Simulate some delay (e.g., applying tweaks)
            await Task.Delay(2000);

            // Run the uninstall process asynchronously
            await Task.Run(() => AppsDebloat());

            // Close the loading form with an animation
            loadingForm.CloseModal();

            // Show the success form after completion
            UninstallJunkSuccess successForm = new UninstallJunkSuccess
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true
            };
            await UninstallJunkSuccess.ShowModal(mainForm, successForm);

        }



        private async Task AppsDebloat()
        {
            if (bingbutton.Checked) ExecuteBatchCommand(@"powershell -Command ""Get-AppxPackage -AllUsers *Bing* | Remove-AppxPackage""");
            if (xboxbutton.Checked) ExecuteBatchCommand(@"powershell -Command ""Get-AppxPackage -AllUsers *XboxApp* | Remove-AppxPackage""");
            if (wphonebutton.Checked) ExecuteBatchCommand(@"powershell -Command ""Get-AppxPackage -AllUsers *YourPhone* | Remove-AppxPackage""");
            if (walarmbutton.Checked) ExecuteBatchCommand(@"powershell -Command ""Get-AppxPackage -AllUsers *Alarms* | Remove-AppxPackage""");
            if (m3dbutton.Checked) ExecuteBatchCommand(@"powershell -Command ""Get-AppxPackage -AllUsers *3DViewer* | Remove-AppxPackage""");
            if (wmapsbutton.Checked) ExecuteBatchCommand(@"powershell -Command ""Get-AppxPackage -AllUsers *Maps* | Remove-AppxPackage""");
            if (builderbutton.Checked) ExecuteBatchCommand(@"powershell -Command ""Get-AppxPackage -AllUsers *3DBuilder* | Remove-AppxPackage""");
            if (wsoundbutton.Checked) ExecuteBatchCommand(@"powershell -Command ""Get-AppxPackage -AllUsers *SoundRecorder* | Remove-AppxPackage""");
            if (medgebutton.Checked) ExecuteBatchCommand(@"powershell -Command ""Get-AppxPackage -AllUsers *MicrosoftEdge* | Remove-AppxPackage""");
            if (wcamerabutton.Checked) ExecuteBatchCommand(@"powershell -Command ""Get-AppxPackage -AllUsers *Camera* | Remove-AppxPackage""");
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

            Process process = new Process { StartInfo = procStartInfo };
            process.Start();
            process.WaitForExit();
        }

        private async void DeleteJunk_Click(object sender, EventArgs e)
        {
            if (!AnyCheckboxChecked2())
            {
                MessageBox.Show("Please select at least one option to clean.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Create and configure the loading form to stay on top
            UninstallJunkLoading loadingForm = new UninstallJunkLoading
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true
            };

            // Access the Main instance through the parent form
            Main mainForm = (Main)this.ParentForm;

            // Show the loading form as a modal
            UninstallJunkLoading.ShowModal(mainForm, loadingForm);

            // Simulate some delay (e.g., applying tweaks)
            await Task.Delay(2000);

            // Run the uninstall process asynchronously
            await Task.Run(() => JunkCleaner());

            // Close the loading form with an animation
            loadingForm.CloseModal();

            // Show the success form after completion
            UninstallJunkSuccess successForm = new UninstallJunkSuccess
            {
                StartPosition = FormStartPosition.CenterParent,
                TopMost = true
            };
            await UninstallJunkSuccess.ShowModal(mainForm, successForm);

        }

        private async Task JunkCleaner()
        {
            if (cleartemp.Checked) await ClearWindowsTempFiles();
            if (clearcache.Checked) await ClearWindowsCacheFiles();
            if (cleardns.Checked) await ClearDNSCache();
            if (cleargamescache.Checked) await ClearGamesCache();
            if (clearbrowsercache.Checked) await ClearBrowserCache();
            if (clearcookies.Checked) await ClearWindowsCookies();
            if (clearprefetch.Checked) await ClearWindowsPrefetch();
            if (clearhistory.Checked) await ClearWindowsHistory();
            if (clearbin.Checked) await EmptyRecycleBin();
        }

        private async Task ClearWindowsTempFiles()
        {
            await Task.Run(() =>
            {
                string tempPath = Path.GetTempPath();
                try
                {
                    if (Directory.Exists(tempPath))
                    {
                        ClearDirectory(tempPath);
                        Directory.CreateDirectory(tempPath); // Recreate the temp directory
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
            });
        }

        private async Task ClearWindowsCacheFiles()
        {
            await Task.Run(() =>
            {
                string cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages");
                try
                {
                    if (Directory.Exists(cachePath))
                    {
                        string[] dirs = Directory.GetDirectories(cachePath);
                        foreach (var dir in dirs)
                        {
                            try
                            {
                                ClearDirectory(dir);
                            }
                            catch (UnauthorizedAccessException)
                            {
                            }
                            catch (IOException)
                            {
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (IOException)
                {
                }
            });
        }

        private async Task ClearDNSCache()
        {
            ExecuteBatchCommand(@"ipconfig /flushdns");
        }
        private static readonly List<string> GameCachePaths = new List<string>
{
    @"%localappdata%\FortniteGame\Saved\Logs",
    @"%localappdata%\EpicGamesLauncher\Saved\Logs",
    @"%localappdata%\Steam\SteamApps\common\Counter-Strike Global Offensive\csgo\cache",
    @"%localappdata%\Ubisoft Game Launcher\cache",
    @"%localappdata%\Baldur's Gate 3\LevelCache",
    @"%localappdata%\NVIDIA\Corral",
    @"%localappdata%\Microsoft\Halo The Master Chief Collection\temp",
    @"%localappdata%\Riot Games\League of Legends\Logs",
    @"%localappdata%\Electronic Arts\BioWare\Mass Effect Legendary Edition\Cache",
    @"%localappdata%\Warframe\Downloaded\Cache",
    @"%localappdata%\GOG.com\Galaxy\cache",
    @"%localappdata%\AppData\Local\Brawlhalla\Cache",
    @"%localappdata%\Roblox\Versions\version-*/Cache",
    @"%localappdata%\Activision\Call of Duty Modern Warfare\logs",
    @"%localappdata%\Riot Games\VALORANT\logs",
    @"%localappdata%\Gears of War 4\Temp",
    @"%localappdata%\Microsoft Flight Simulator\Packages\Official\Steam\temp",
    @"%localappdata%\Gearbox Software\Borderlands 3\Cache",
    @"%localappdata%\Blizzard\Battle.net\Cache",
    @"%localappdata%\Sony\PlayStation Now\Cache",
    @"%localappdata%\BlueStacks\Engine\Cache",
    @"%localappdata%\CD Projekt Red\Cyberpunk 2077\Cache",
    @"%localappdata%\Ubisoft\For Honor\cache",
    @"%localappdata%\Electronic Arts\The Sims 4\Caches",
    @"%localappdata%\Rockstar Games\GTA V\cache",
    @"%localappdata%\Humble Bundle\Humble Games\cache",
    @"%localappdata%\NVIDIA\ShadowPlay\Cache",
    @"%localappdata%\Epic Games\Launcher\Saved\Temp",
    @"%localappdata%\Microsoft Store\Uninstallable\cache",
    @"%localappdata%\Microsoft\Age of Empires II DE\Temp",
    @"%localappdata%\Overwatch\cache",
    @"%localappdata%\Krafton\PUBG\Temp",
    @"%localappdata%\Vampyr\Cache",
    @"%localappdata%\CD Projekt Red\The Witcher 3\Cache",
    @"%localappdata%\Paradox Interactive\Stellaris\Temp",
    @"%localappdata%\Psyonix\Rocket League\Temp",
    @"%localappdata%\Treyarch\Call of Duty Black Ops Cold War\temp",
    @"%localappdata%\Gameforge\NosTale\Cache",
    @"%localappdata%\Activision\Call of Duty Warzone\temp",
    @"%localappdata%\World of Tanks\Cache",
    @"%localappdata%\Epic Games\Unreal Engine\Temp",
    @"%localappdata%\Paradox Interactive\Hearts of Iron IV\Cache",
    @"%localappdata%\NCSOFT\Blade & Soul\Temp",
    @"%localappdata%\ZeniMax Online Studios\The Elder Scrolls Online\cache",
    @"%localappdata%\Insomniac Games\Marvel's Spider-Man\cache",
    @"%localappdata%\Team17\Overcooked! 2\Cache",
    @"%localappdata%\Team Fortress 2\tf\cache",
    @"%localappdata%\Bungie\Destiny 2\Cache",
    @"%localappdata%\2K Games\Civilization VI\Temp",
    @"%localappdata%\Ubisoft\Tom Clancy's The Division\Temp",
    @"%localappdata%\Blizzard Entertainment\World of Warcraft\Cache",
    @"%localappdata%\Riot Games\Teamfight Tactics\Cache",
    @"%localappdata%\Discord\Cache",
    @"%localappdata%\Supercell\Clash Royale\Cache",
    @"%localappdata%\SCS Software\American Truck Simulator\Cache",
    @"%localappdata%\SCS Software\Euro Truck Simulator 2\Cache",
    @"%localappdata%\Dying Light\Temp",
    @"%localappdata%\Final Fantasy XIV\Temp",
    @"%localappdata%\Ubisoft\Ghost Recon Breakpoint\Cache",
    @"%localappdata%\Zynga\FarmVille 2\Cache",
    @"%localappdata%\Warner Bros. Games\Hogwarts Legacy\Temp",
    @"%localappdata%\Devolver Digital\Fall Guys\Cache",
    @"%localappdata%\SQUARE ENIX\FINAL FANTASY XV\Temp",
    @"%localappdata%\Monster Hunter World\Temp",
    @"%localappdata%\Activision\Call of Duty Black Ops 4\temp",
    @"%localappdata%\Sonic Team\Sonic Mania\Temp",
    @"%localappdata%\Gears 5\Cache",
    @"%localappdata%\Crytek\Hunt: Showdown\Temp",
    @"%localappdata%\SEGA\Total War: Warhammer II\Temp",
    @"%localappdata%\Techland\Dying Light 2\Temp",
    @"%localappdata%\Remedy Entertainment\Control\Temp",
    @"%localappdata%\Pearl Abyss\Black Desert Online\Temp",
    @"%localappdata%\Zynga\Zynga Poker\Cache",
    @"%localappdata%\Star Wars Battlefront II\Temp",
    @"%localappdata%\Tripwire Interactive\Killing Floor 2\Temp",
    @"%localappdata%\Bethesda Softworks\Fallout 76\Cache",
    @"%localappdata%\Bethesda Game Studios\Fallout 4\Cache",
    @"%localappdata%\Mojang Studios\Minecraft\Temp",
    @"%localappdata%\Riot Games\Legends of Runeterra\Cache",
    @"%localappdata%\Humble Bundle\Humble Trove\Cache",
    @"%localappdata%\Wargaming.net\World of Warships\Temp",
    @"%localappdata%\Warframe\Temp",
    @"%localappdata%\The Sims 3\Caches",
    @"%localappdata%\ZeniMax Online Studios\Fallout 76\Cache",
    @"%localappdata%\Yacht Club Games\Shovel Knight\Cache",
};
        private async Task ClearGamesCache()
        {
            await Task.Run(() =>
            {
                foreach (var path in GameCachePaths)
                {
                    try
                    {
                        string expandedPath = Environment.ExpandEnvironmentVariables(path);
                        if (Directory.Exists(expandedPath))
                        {
                            ClearDirectory(expandedPath);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            });
        }

        private static readonly List<string> BrowserCachePaths = new List<string>
{
    // Google Chrome
    @"%localappdata%\Google\Chrome\User Data\Default\Cache",
    @"%localappdata%\Google\Chrome\User Data\Default\Media Cache",

    // Microsoft Edge
    @"%localappdata%\Microsoft\Edge\User Data\Default\Cache",
    @"%localappdata%\Microsoft\Edge\User Data\Default\Media Cache",

    // Mozilla Firefox
    @"%localappdata%\Mozilla\Firefox\Profiles\*.default\cache2",
    @"%localappdata%\Mozilla\Firefox\Profiles\*.default\cache-train",

    // Internet Explorer / Edge Legacy
    @"%localappdata%\Microsoft\Windows\INetCache",
    @"%localappdata%\Microsoft\Windows\INetTemp",

    // Opera
    @"%localappdata%\Opera Software\Opera Stable\Cache",
    @"%localappdata%\Opera Software\Opera Stable\Media Cache",

    // Brave
    @"%localappdata%\BraveSoftware\Brave-Browser\User Data\Default\Cache",
    @"%localappdata%\BraveSoftware\Brave-Browser\User Data\Default\Media Cache",

    // Vivaldi
    @"%localappdata%\Vivaldi\User Data\Default\Cache",
    @"%localappdata%\Vivaldi\User Data\Default\Media Cache",

    // Yandex Browser
    @"%localappdata%\Yandex\YandexBrowser\User Data\Default\Cache",
    @"%localappdata%\Yandex\YandexBrowser\User Data\Default\Media Cache"
};

        public async Task ClearBrowserCache()
        {
            await Task.Run(() =>
            {
                foreach (var path in BrowserCachePaths)
                {
                    try
                    {
                        string expandedPath = Environment.ExpandEnvironmentVariables(path);
                        if (Directory.Exists(expandedPath))
                        {
                            ClearDirectory(expandedPath);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            });
        }

        private void ClearDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                try
                {
                    foreach (var file in Directory.GetFiles(directoryPath))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }
                        catch (IOException)
                        {
                        }
                    }

                    foreach (var subDir in Directory.GetDirectories(directoryPath))
                    {
                        ClearDirectory(subDir);
                    }

                    if (Directory.GetFileSystemEntries(directoryPath).Length == 0)
                    {
                        Directory.Delete(directoryPath);
                    }
                }
                catch (Exception)
                {
                }
            }
        }


        private async Task ClearWindowsCookies()
        {
            await Task.Run(() =>
            {
                string cookiesPath = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "cookies");
                if (Directory.Exists(cookiesPath))
                {
                    ClearDirectory(cookiesPath);
                }
            });
        }

        private async Task ClearWindowsPrefetch()
        {
            await Task.Run(() =>
            {
                string prefetchPath = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "Prefetch");
                if (Directory.Exists(prefetchPath))
                {
                    ClearDirectory(prefetchPath);
                }
            });
        }

        private async Task ClearWindowsHistory()
        {
            await Task.Run(() =>
            {
                string historyPath = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "history");
                if (Directory.Exists(historyPath))
                {
                    ClearDirectory(historyPath);
                }
            });
        }

        private async Task EmptyRecycleBin()
        {
            await Task.Run(() => ExecuteBatchCommand(@"rd /s /q C:\\$Recycle.Bin"));
        }

        private void ApplyAll1_Click(object sender, EventArgs e)
        {
            var checkboxes = new[] { bingbutton, xboxbutton, wphonebutton, walarmbutton, m3dbutton, wmapsbutton, builderbutton, wsoundbutton, medgebutton, wcamerabutton };

            bool allChecked = checkboxes.All(cb => cb.Checked); // Check if all are checked 

            foreach (var checkbox in checkboxes)
            {
                checkbox.Checked = !allChecked;
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
        private void ApplyAll2_Click(object sender, EventArgs e)
        {
            var checkboxes = new[] { cleartemp, clearcache, cleardns, cleargamescache, clearbrowsercache, clearcookies, clearprefetch, clearhistory, clearbin, discordcache };

            bool allChecked = checkboxes.All(cb => cb.Checked); // Check if all are checked 

            foreach (var checkbox in checkboxes)
            {
                checkbox.Checked = !allChecked;
            }
        }

        private void discordcache_Click(object sender, EventArgs e)
        {

        }

        private void discordlabel_Click(object sender, EventArgs e) => ToggleCheckboxState(discordcache);
        private async Task ClearDiscordCache()
        {
            await Task.Run(() => ExecuteBatchCommand(@"rd ""%AppData%\Discord\Cache"" /s /q
rd ""%AppData%\Discord\Code Cache"" /s /q
md ""%AppData%\Discord\Cache""
md ""%AppData%\Discord\Code Cache"""));
        }
    }
}