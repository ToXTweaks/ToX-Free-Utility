using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToX_Free_Utility_NEW_ERA.Settings;
using ToX_Free_Utility_NEW_ERA.Tweaks;
using Vortex.Ui;

namespace ToX_Free_Utility_NEW_ERA.Pages
{
    public partial class gamebooster : UserControl
    {
        public gamebooster()
        {
            InitializeComponent();
            AnimationSpeed = 2;
        }

        private void gamebooster_Load(object sender, EventArgs e)
        {
            InitializeGameBooster();
        }
        #region Game Booster Models

        public sealed class GameBoosterRoot
        {
            [JsonPropertyName("games")]
            public List<GameBoosterDefinition> Games { get; set; } = new List<GameBoosterDefinition>();
        }

        public sealed class GameBoosterDefinition
        {
            [JsonPropertyName("key")]
            public string Key { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("requiresAdmin")]
            public bool RequiresAdmin { get; set; }

            [JsonPropertyName("applyBatch")]
            public List<string> ApplyBatch { get; set; } = new List<string>();

            [JsonPropertyName("revertBatch")]
            public List<string> RevertBatch { get; set; } = new List<string>();
        }

        public sealed class SavedGamePaths
        {
            [JsonPropertyName("paths")]
            public Dictionary<string, string> Paths { get; set; } =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public sealed class CustomGameEntry
        {
            [JsonPropertyName("key")]
            public string Key { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("path")]
            public string Path { get; set; }
        }

        public sealed class CustomGamesRoot
        {
            [JsonPropertyName("games")]
            public List<CustomGameEntry> Games { get; set; } = new List<CustomGameEntry>();
        }

        public sealed class GameCarouselItem
        {
            public string Key { get; set; }
            public string Name { get; set; }
            public Image ResourceImage { get; set; }
            public string ExecutablePath { get; set; }
            public bool IsCustom { get; set; }
        }

        public sealed class GameBoosterExecutionResult
        {
            public bool Success { get; private set; }
            public string Message { get; private set; }

            public static GameBoosterExecutionResult Ok(string message)
            {
                return new GameBoosterExecutionResult
                {
                    Success = true,
                    Message = message ?? string.Empty
                };
            }

            public static GameBoosterExecutionResult Fail(string message)
            {
                return new GameBoosterExecutionResult
                {
                    Success = false,
                    Message = message ?? string.Empty
                };
            }
        }

        #endregion

        #region Game Booster Fields

        private Image[] gameImages;
        private Image[] thumbImages;

        private List<GameCarouselItem> gameItems = new List<GameCarouselItem>();
        private Dictionary<string, int> nameToIndex;

        private int currentIndex = 0;
        public int AnimationSpeed { get; set; } = 7;

        private Timer animationTimer;
        private int animationDirection;
        private int animationStep;
        private int animationTotalSteps;
        private int animationStepSize;
        private TaskCompletionSource<bool> slideTcs;
        private bool isAnimating;

        #endregion

        #region Game Booster Paths

        private string GameBoosterConfigFolder => TweakStateStore.AppDataFolder;

        private string LegacyGameBoosterConfigFolder =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");

        private string GamePathsJsonPath =>
            Path.Combine(GameBoosterConfigFolder, "SavedGamePaths.json");

        private string CustomGamesJsonPath =>
            Path.Combine(GameBoosterConfigFolder, "CustomGames.json");

        private string GameBoosterTempBatchFolder =>
            Path.Combine(Path.GetTempPath(), "ToXFreeUtility", "GameBoosters");

        private void MigrateLegacyGameConfigsIfNeeded()
        {
            try
            {
                Directory.CreateDirectory(GameBoosterConfigFolder);

                MigrateLegacyConfigFile(
                    Path.Combine(LegacyGameBoosterConfigFolder, "SavedGamePaths.json"),
                    GamePathsJsonPath);

                MigrateLegacyConfigFile(
                    Path.Combine(LegacyGameBoosterConfigFolder, "CustomGames.json"),
                    CustomGamesJsonPath);
            }
            catch
            {
            }
        }

        private static void MigrateLegacyConfigFile(string legacyPath, string newPath)
        {
            if (File.Exists(newPath) || !File.Exists(legacyPath))
                return;

            File.Copy(legacyPath, newPath);
        }

        #endregion

        #region Game Booster Helpers

        private static string NormalizeSearchText(string s)
        {
            return (s ?? string.Empty).Trim().ToLowerInvariant();
        }

        private Task WaitForProcessExitAsync(Process process)
        {
            if (process.HasExited)
                return Task.CompletedTask;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            void Handler(object sender, EventArgs e)
            {
                process.Exited -= Handler;
                tcs.TrySetResult(true);
            }

            process.EnableRaisingEvents = true;
            process.Exited += Handler;

            if (process.HasExited)
            {
                process.Exited -= Handler;
                return Task.CompletedTask;
            }

            return tcs.Task;
        }

        private string SafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";

            foreach (char c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');

            return value;
        }

        private Color DarkenColor(Color color, int amount)
        {
            int r = Math.Max(0, color.R - amount);
            int g = Math.Max(0, color.G - amount);
            int b = Math.Max(0, color.B - amount);
            return Color.FromArgb(color.A, r, g, b);
        }

        private void SetCarouselOpacity(VortexPictureBox pictureBox, float opacity, float dim)
        {
            pictureBox.Opacity = opacity;
            pictureBox.DimOpacity = dim;
        }

        private static string GetInstallFolderHint(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            try
            {
                if (File.Exists(path))
                    return Path.GetDirectoryName(path) ?? string.Empty;

                if (Directory.Exists(path))
                    return path;
            }
            catch
            {
            }

            return string.Empty;
        }

        private int WrapGameIndex(int index)
        {
            int count = gameItems.Count;
            return (index % count + count) % count;
        }

        #endregion

        #region Initialize Game Booster

        private void BuildStockGames()
        {
            gameItems = new List<GameCarouselItem>
    {
        new GameCarouselItem { Key = "Fortnite", Name = "Fortnite", ResourceImage = ToX_Free_Utility_NEW_ERA.Properties.Resources.fortnite, IsCustom = false },
        new GameCarouselItem { Key = "Valorant", Name = "Valorant", ResourceImage = ToX_Free_Utility_NEW_ERA.Properties.Resources.valorant, IsCustom = false },
        new GameCarouselItem { Key = "Minecraft", Name = "Minecraft", ResourceImage = ToX_Free_Utility_NEW_ERA.Properties.Resources.minecraft, IsCustom = false },
        new GameCarouselItem { Key = "Apex", Name = "Apex", ResourceImage = ToX_Free_Utility_NEW_ERA.Properties.Resources.apex, IsCustom = false },
        new GameCarouselItem { Key = "GTA", Name = "GTA", ResourceImage = ToX_Free_Utility_NEW_ERA.Properties.Resources.gta, IsCustom = false },
        new GameCarouselItem { Key = "Roblox", Name = "Roblox", ResourceImage = ToX_Free_Utility_NEW_ERA.Properties.Resources.roblox, IsCustom = false },
        new GameCarouselItem { Key = "CallOfDuty", Name = "Call of Duty", ResourceImage = ToX_Free_Utility_NEW_ERA.Properties.Resources.cod, IsCustom = false },
        new GameCarouselItem { Key = "Rivals", Name = "Rivals", ResourceImage = ToX_Free_Utility_NEW_ERA.Properties.Resources.rivals, IsCustom = false },
        new GameCarouselItem { Key = "FIFA", Name = "FIFA", ResourceImage = ToX_Free_Utility_NEW_ERA.Properties.Resources.fifa, IsCustom = false },
        new GameCarouselItem { Key = "Warzone", Name = "Call of Duty : Warzone", ResourceImage = ToX_Free_Utility_NEW_ERA.Properties.Resources.codw, IsCustom = false },
        new GameCarouselItem { Key = "Rust", Name = "Rust", ResourceImage = ToX_Free_Utility_NEW_ERA.Properties.Resources.rust, IsCustom = false },
        new GameCarouselItem { Key = "CS2", Name = "CS2", ResourceImage = ToX_Free_Utility_NEW_ERA.Properties.Resources.csgo, IsCustom = false }
    };
        }

        public async void InitializeGameBooster()
        {
            try
            {
                MigrateLegacyGameConfigsIfNeeded();
                BuildStockGames();
                ApplySavedExecutablesToGames();
                LoadCustomGames();
                BuildNameIndex();

                if (currentIndex >= gameItems.Count)
                    currentIndex = 0;

                gameImages = new Image[gameItems.Count];
                thumbImages = new Image[gameItems.Count];

                for (int i = 0; i < gameItems.Count; i++)
                {
                    Image resource = GetResourceByIndex(i);
                    if (resource != null)
                    {
                        thumbImages[i] = FastResize(
                            resource,
                            Math.Max(1, GameCurrent.Width / 3),
                            Math.Max(1, GameCurrent.Height / 3));
                    }
                }

                gameImages[currentIndex] = ResizeImage(
                    GetResourceByIndex(currentIndex),
                    GameCurrent.Width,
                    GameCurrent.Height);

                int prevIndex = WrapGameIndex(currentIndex - 1);
                int nextIndex = WrapGameIndex(currentIndex + 1);

                gameImages[prevIndex] = FastResize(GetResourceByIndex(prevIndex), GamePrev.Width, GamePrev.Height);
                gameImages[nextIndex] = FastResize(GetResourceByIndex(nextIndex), GameNext.Width, GameNext.Height);

                UpdateImages();
                LoadSavedPathForCurrentGame();

                int preloadWidth = GameCurrent.Width;
                int preloadHeight = GameCurrent.Height;
                await Task.Run(() => PreloadAllImages(preloadWidth, preloadHeight));
            }
            catch (Exception ex)
            {
                TweakExecutor.ShowMessage($"Failed to initialize Game Booster.\n\n{ex}", "Game Booster Error");
            }
        }

        private void PreloadAllImages(int targetWidth, int targetHeight)
        {
            // NOTE: must stay single-threaded. System.Drawing/GDI+ is not thread-safe, and the
            // source resource images are shared instances — resizing them on several threads at
            // once (the old Parallel.For) raced inside GDI+ and crashed the process natively.
            for (int i = 0; i < gameItems.Count; i++)
            {
                if (gameImages[i] != null)
                    continue;

                Image img = ResizeImage(GetResourceByIndex(i), targetWidth, targetHeight);
                gameImages[i] = img;

                if (IsHandleCreated)
                {
                    BeginInvoke((Action)(() =>
                    {
                        if (!IsDisposed)
                            UpdateImages();
                    }));
                }
            }
        }

        private Image GetResourceByIndex(int index)
        {
            if (gameItems == null || gameItems.Count == 0)
                return null;

            index = WrapGameIndex(index);
            return gameItems[index].ResourceImage;
        }

        private Image ResizeImage(Image img, int width, int height)
        {
            if (img == null || width <= 0 || height <= 0)
                return null;

            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.DrawImage(img, 0, 0, width, height);
            }
            return bmp;
        }

        private Image FastResize(Image img, int width, int height)
        {
            if (img == null || width <= 0 || height <= 0)
                return null;

            return img.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
        }

        private void UpdateImages()
        {
            if (gameImages == null || thumbImages == null || gameItems.Count == 0)
                return;

            int prevIndex = WrapGameIndex(currentIndex - 1);
            int nextIndex = WrapGameIndex(currentIndex + 1);

            GameCurrent.Image = gameImages[currentIndex] ?? thumbImages[currentIndex];
            GamePrev.Image = gameImages[prevIndex] ?? thumbImages[prevIndex];
            GameNext.Image = gameImages[nextIndex] ?? thumbImages[nextIndex];

            SetCarouselOpacity(GameCurrent, 1f, 0f);
            SetCarouselOpacity(GamePrev, 0.65f, 0.28f);
            SetCarouselOpacity(GameNext, 0.65f, 0.28f);

            string currentGame = gameItems[currentIndex].Name;
            GameName.Text = currentGame;
            optimizeGame.Text = $"Optimize {currentGame}";
            optimizeGame.LoadingText = $"Optimizing {currentGame}...";

            string game = currentGame.ToLowerInvariant();
            Color baseColor;

            if (game.Contains("valorant"))
                baseColor = Color.FromArgb(255, 67, 91);
            else if (game.Contains("fortnite"))
                baseColor = Color.FromArgb(80, 165, 245);
            else if (game.Contains("minecraft"))
                baseColor = Color.FromArgb(102, 153, 0);
            else if (game.Contains("apex"))
                baseColor = Color.FromArgb(200, 16, 46);
            else if (game.Contains("gta"))
                baseColor = Color.FromArgb(0, 0, 0);
            else if (game.Contains("roblox"))
                baseColor = Color.FromArgb(255, 0, 0);
            else if (game.Contains("call of duty : warzone"))
                baseColor = Color.FromArgb(55, 120, 180);
            else if (game.Contains("call of duty"))
                baseColor = Color.FromArgb(70, 70, 70);
            else if (game.Contains("rivals"))
                baseColor = Color.FromArgb(123, 31, 162);
            else if (game.Contains("fifa"))
                baseColor = Color.FromArgb(0, 70, 130);
            else if (game.Contains("rust"))
                baseColor = Color.FromArgb(183, 65, 14);
            else if (game.Contains("cs"))
                baseColor = Color.FromArgb(255, 153, 51);
            else
                baseColor = Color.FromArgb(4, 120, 251);

            optimizeGame.BackgroundColor = baseColor;
            optimizeGame.HoverBackColor = DarkenColor(baseColor, 25);
            optimizeGame.PressedBackColor = DarkenColor(baseColor, 50);

            int centerX = this.Width / 2;
            int y = GameCurrent.Location.Y;
            int spacing = 20;

            GameCurrent.Location = new Point(centerX - GameCurrent.Width / 2, y);
            GamePrev.Location = new Point(GameCurrent.Left - GamePrev.Width - spacing, y);
            GameNext.Location = new Point(GameCurrent.Right + spacing, y);

            LoadSavedPathForCurrentGame();

            if (loadGame != null)
                loadGame.Text = "Select EXE";

            if (gamePath != null)
            {
                string exe = gameItems[currentIndex].ExecutablePath;
                gamePath.PlaceholderText = string.IsNullOrWhiteSpace(exe)
                    ? $"Browse for {currentGame} executable..."
                    : $"{currentGame} executable ready";
            }
        }

        #endregion

        #region Game Index

        private void BuildNameIndex()
        {
            nameToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < gameItems.Count; i++)
            {
                string normalizedName = NormalizeSearchText(gameItems[i].Name);
                string normalizedKey = NormalizeSearchText(gameItems[i].Key);

                if (!nameToIndex.ContainsKey(normalizedName))
                    nameToIndex[normalizedName] = i;

                if (!nameToIndex.ContainsKey(normalizedKey))
                    nameToIndex[normalizedKey] = i;
            }

            int idx;

            idx = gameItems.FindIndex(x => x.Key == "Fortnite");
            if (idx >= 0) nameToIndex["fn"] = idx;

            idx = gameItems.FindIndex(x => x.Key == "Valorant");
            if (idx >= 0) nameToIndex["val"] = idx;

            idx = gameItems.FindIndex(x => x.Key == "Minecraft");
            if (idx >= 0) nameToIndex["mc"] = idx;

            idx = gameItems.FindIndex(x => x.Key == "Apex");
            if (idx >= 0) nameToIndex["apex legends"] = idx;

            idx = gameItems.FindIndex(x => x.Key == "GTA");
            if (idx >= 0)
            {
                nameToIndex["grand theft auto"] = idx;
                nameToIndex["gta v"] = idx;
                nameToIndex["gta 5"] = idx;
            }

            idx = gameItems.FindIndex(x => x.Key == "Roblox");
            if (idx >= 0) nameToIndex["rbx"] = idx;

            idx = gameItems.FindIndex(x => x.Key == "CallOfDuty");
            if (idx >= 0) nameToIndex["cod"] = idx;

            idx = gameItems.FindIndex(x => x.Key == "Rivals");
            if (idx >= 0) nameToIndex["marvel rivals"] = idx;

            idx = gameItems.FindIndex(x => x.Key == "FIFA");
            if (idx >= 0)
            {
                nameToIndex["fc"] = idx;
                nameToIndex["ea fc"] = idx;
            }

            idx = gameItems.FindIndex(x => x.Key == "Warzone");
            if (idx >= 0)
            {
                nameToIndex["warzone"] = idx;
                nameToIndex["cod warzone"] = idx;
                nameToIndex["codw"] = idx;
            }

            idx = gameItems.FindIndex(x => x.Key == "CS2");
            if (idx >= 0)
            {
                nameToIndex["csgo"] = idx;
                nameToIndex["cs"] = idx;
                nameToIndex["cs go"] = idx;
                nameToIndex["counter strike"] = idx;
                nameToIndex["counter strike 2"] = idx;
            }
        }

        private int FindGameIndex(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return -1;

            string q = NormalizeSearchText(query);

            if (nameToIndex.TryGetValue(q, out int exactMatch))
                return exactMatch;

            for (int i = 0; i < gameItems.Count; i++)
            {
                if (NormalizeSearchText(gameItems[i].Name).Contains(q))
                    return i;
            }

            return -1;
        }

        public void SlideToGame(string query)
        {
            int target = FindGameIndex(query);
            if (target == -1 || target == currentIndex)
                return;

            currentIndex = target;
            UpdateImages();
        }

        #endregion

        #region Animation

        private void StartSlide(int direction)
        {
            if (isAnimating || gameItems == null || gameItems.Count == 0)
                return;

            isAnimating = true;
            animationDirection = direction < 0 ? -1 : 1;
            animationStep = 0;
            animationTotalSteps = Math.Max(1, AnimationSpeed * 2);
            animationStepSize = Math.Max(1, GameCurrent.Width / animationTotalSteps);

            if (animationTimer != null)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
                animationTimer = null;
            }

            slideTcs = new TaskCompletionSource<bool>();

            animationTimer = new Timer();
            animationTimer.Interval = 15;
            animationTimer.Tick += AnimationTick;
            animationTimer.Start();
        }

        private void AnimationTick(object sender, EventArgs e)
        {
            if (animationStep >= animationTotalSteps)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
                animationTimer = null;

                currentIndex = WrapGameIndex(currentIndex + animationDirection);
                UpdateImages();

                isAnimating = false;
                slideTcs?.TrySetResult(true);
                slideTcs = null;
                return;
            }

            int offset = animationDirection * animationStepSize;
            GameCurrent.Left -= offset;
            GamePrev.Left -= offset;
            GameNext.Left -= offset;

            animationStep++;
        }

        #endregion

        #region Game Booster Config / Execute

        private string GetCurrentGameKey()
        {
            return gameItems[currentIndex].Key;
        }

        private bool TryResolveGameExecutable(string installFolder, out string executablePath)
        {
            executablePath = gameItems[currentIndex].ExecutablePath;

            if (!string.IsNullOrWhiteSpace(executablePath) && File.Exists(executablePath))
                return true;

            string typedPath = gamePath?.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(typedPath) && File.Exists(typedPath) &&
                typedPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                executablePath = typedPath;
                gameItems[currentIndex].ExecutablePath = executablePath;
                return true;
            }

            if (TryGetSavedExecutable(GetCurrentGameKey(), out string savedExe))
            {
                executablePath = savedExe;
                gameItems[currentIndex].ExecutablePath = executablePath;
                return true;
            }

            if (GameBoosterDefaults.TryDetectExecutable(GetCurrentGameKey(), out string detectedExe))
            {
                executablePath = detectedExe;
                gameItems[currentIndex].ExecutablePath = executablePath;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(installFolder) && Directory.Exists(installFolder))
            {
                string preferred = GameBoosterDefaults.GetSuggestedExeFileName(GetCurrentGameKey());
                if (!string.IsNullOrWhiteSpace(preferred))
                {
                    string preferredPath = Path.Combine(installFolder, preferred);
                    if (File.Exists(preferredPath))
                    {
                        executablePath = preferredPath;
                        gameItems[currentIndex].ExecutablePath = executablePath;
                        return true;
                    }
                }

                string[] exeFiles = Directory.GetFiles(installFolder, "*.exe", SearchOption.TopDirectoryOnly);
                if (exeFiles.Length == 0)
                    exeFiles = Directory.GetFiles(installFolder, "*.exe", SearchOption.AllDirectories);

                if (exeFiles.Length > 0)
                {
                    executablePath = exeFiles
                        .OrderByDescending(File.GetLastWriteTimeUtc)
                        .First();

                    gameItems[currentIndex].ExecutablePath = executablePath;
                    return true;
                }
            }

            executablePath = null;
            return false;
        }

        private async Task<bool> ExecuteGameBoosterAsync(string gameKey, string gameInstallPath)
        {
            if (!TryResolveGameExecutable(gameInstallPath, out string executablePath))
            {
                TweakExecutor.ShowMessage(
                    $"Select the {gameItems[currentIndex].Name} executable with Select EXE, or install the game in a default location.",
                    "Missing Game Executable");
                return false;
            }

            SaveCurrentGameExecutable(executablePath);

            string installFolder = GetInstallFolderHint(executablePath);

            if (GameBoosterTweaks.RequiresAdminForGameOptimize && !TweakExecutor.IsRunningAsAdministrator())
            {
                TweakExecutor.ShowMessage(
                    "Game optimization requires administrator privileges.",
                    "Administrator Required");
                return false;
            }

            List<string> applyBatch = GameBoosterTweaks.GetGameApplyBatch(
                executablePath,
                gameItems[currentIndex].Name);

            GameBoosterExecutionResult result = await ExecuteGameBoosterBatchAsync(
                gameKey,
                applyBatch,
                installFolder,
                executablePath);

            if (!result.Success)
            {
                TweakExecutor.ShowMessage(
                    $"Failed to optimize '{gameItems[currentIndex].Name}'.\n\n{result.Message}",
                    "Optimization Failed");
                return false;
            }

            return true;
        }

        private async Task<GameBoosterExecutionResult> ExecuteGameBoosterBatchAsync(
            string gameKey,
            List<string> scriptLines,
            string gameInstallPath,
            string gameExecutablePath)
        {
            try
            {
                Directory.CreateDirectory(GameBoosterTempBatchFolder);

                string batchPath = Path.Combine(
                    GameBoosterTempBatchFolder,
                    $"{SafeFileName(gameKey)}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bat");

                StringBuilder builder = new StringBuilder();
                foreach (string line in scriptLines)
                    builder.AppendLine(line ?? string.Empty);

                string scriptContent = builder.ToString();
                scriptContent = scriptContent.Replace("%GAME_PATH%", gameInstallPath ?? string.Empty);
                scriptContent = scriptContent.Replace("%GAME_NAME%", gameItems[currentIndex].Name);
                scriptContent = scriptContent.Replace("%GAME_KEY%", gameKey);
                scriptContent = scriptContent.Replace("%GAME_EXE%", gameExecutablePath ?? string.Empty);

                File.WriteAllText(batchPath, scriptContent, Encoding.ASCII);

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"\"{batchPath}\"\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        WorkingDirectory = string.IsNullOrWhiteSpace(gameInstallPath) || !Directory.Exists(gameInstallPath)
                            ? AppDomain.CurrentDomain.BaseDirectory
                            : gameInstallPath
                    };

                    using (Process process = new Process())
                    {
                        process.StartInfo = psi;

                        StringBuilder outputBuilder = new StringBuilder();
                        StringBuilder errorBuilder = new StringBuilder();

                        process.OutputDataReceived += (_, e) =>
                        {
                            if (e.Data != null)
                                outputBuilder.AppendLine(e.Data);
                        };

                        process.ErrorDataReceived += (_, e) =>
                        {
                            if (e.Data != null)
                                errorBuilder.AppendLine(e.Data);
                        };

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        await WaitForProcessExitAsync(process);

                        string output = outputBuilder.ToString().Trim();
                        string error = errorBuilder.ToString().Trim();

                        if (process.ExitCode != 0)
                        {
                            return GameBoosterExecutionResult.Fail(
                                $"Batch exited with code {process.ExitCode}.\n\nOutput:\n{output}\n\nError:\n{error}");
                        }

                        return GameBoosterExecutionResult.Ok(output);
                    }
                }
                finally
                {
                    try
                    {
                        if (File.Exists(batchPath))
                            File.Delete(batchPath);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                return GameBoosterExecutionResult.Fail(ex.ToString());
            }
        }

        #endregion

        #region Custom Games

        private void LoadCustomGames()
        {
            try
            {
                if (!File.Exists(CustomGamesJsonPath))
                    return;

                string json = File.ReadAllText(CustomGamesJsonPath, Encoding.UTF8);

                CustomGamesRoot root = JsonSerializer.Deserialize<CustomGamesRoot>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (root?.Games == null)
                    return;

                foreach (CustomGameEntry game in root.Games)
                {
                    if (string.IsNullOrWhiteSpace(game.Name))
                        continue;

                    Bitmap bmp = null;

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(game.Path) && File.Exists(game.Path))
                        {
                            using (Icon icon = Icon.ExtractAssociatedIcon(game.Path))
                            {
                                if (icon != null)
                                    bmp = icon.ToBitmap();
                            }
                        }
                    }
                    catch
                    {
                    }

                    gameItems.Add(new GameCarouselItem
                    {
                        Key = string.IsNullOrWhiteSpace(game.Key)
                            ? SafeFileName(game.Name).Replace(" ", "")
                            : game.Key,
                        Name = game.Name,
                        ExecutablePath = game.Path,
                        ResourceImage = bmp ?? ToX_Free_Utility_NEW_ERA.Properties.Resources.fortnite,
                        IsCustom = true
                    });
                }
            }
            catch (Exception ex)
            {
                TweakExecutor.ShowMessage($"Failed to load custom games.\n\n{ex.Message}", "Custom Games Error");
            }
        }

        private void SaveCustomGames()
        {
            try
            {
                Directory.CreateDirectory(GameBoosterConfigFolder);

                CustomGamesRoot root = new CustomGamesRoot();

                foreach (GameCarouselItem item in gameItems)
                {
                    if (!item.IsCustom)
                        continue;

                    root.Games.Add(new CustomGameEntry
                    {
                        Key = item.Key,
                        Name = item.Name,
                        Path = item.ExecutablePath
                    });
                }

                string json = JsonSerializer.Serialize(root, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(CustomGamesJsonPath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                TweakExecutor.ShowMessage($"Failed to save custom games.\n\n{ex.Message}", "Save Error");
            }
        }

        #endregion

        #region Save / Load Game Path

        private void ApplySavedExecutablesToGames()
        {
            SavedGamePaths data = LoadSavedGamePaths();

            foreach (GameCarouselItem item in gameItems)
            {
                if (TryGetSavedExecutable(item.Key, data, out string exePath))
                    item.ExecutablePath = exePath;
            }
        }

        private bool TryGetSavedExecutable(string gameKey, out string executablePath)
        {
            return TryGetSavedExecutable(gameKey, LoadSavedGamePaths(), out executablePath);
        }

        private bool TryGetSavedExecutable(string gameKey, SavedGamePaths data, out string executablePath)
        {
            executablePath = null;

            if (data?.Paths == null || string.IsNullOrWhiteSpace(gameKey))
                return false;

            if (!data.Paths.TryGetValue(gameKey, out string savedValue) ||
                string.IsNullOrWhiteSpace(savedValue))
            {
                return false;
            }

            string normalized = NormalizeSavedPath(savedValue, gameKey);
            if (string.IsNullOrWhiteSpace(normalized) || !File.Exists(normalized))
                return false;

            executablePath = normalized;
            return true;
        }

        private string NormalizeSavedPath(string savedValue, string gameKey)
        {
            if (string.IsNullOrWhiteSpace(savedValue))
                return null;

            if (File.Exists(savedValue) && savedValue.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                return savedValue;

            if (Directory.Exists(savedValue))
            {
                string preferred = GameBoosterDefaults.GetSuggestedExeFileName(gameKey);
                if (!string.IsNullOrWhiteSpace(preferred))
                {
                    string preferredPath = Path.Combine(savedValue, preferred);
                    if (File.Exists(preferredPath))
                        return preferredPath;
                }

                try
                {
                    string firstExe = Directory
                        .EnumerateFiles(savedValue, "*.exe", SearchOption.AllDirectories)
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(firstExe))
                        return firstExe;
                }
                catch
                {
                }
            }

            return savedValue;
        }

        private SavedGamePaths LoadSavedGamePaths()
        {
            try
            {
                if (!File.Exists(GamePathsJsonPath))
                    return new SavedGamePaths();

                string json = File.ReadAllText(GamePathsJsonPath, Encoding.UTF8);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                SavedGamePaths data = JsonSerializer.Deserialize<SavedGamePaths>(json, options);
                return data ?? new SavedGamePaths();
            }
            catch
            {
                return new SavedGamePaths();
            }
        }

        private void SaveSavedGamePaths(SavedGamePaths data)
        {
            Directory.CreateDirectory(GameBoosterConfigFolder);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(GamePathsJsonPath, json, Encoding.UTF8);
        }

        private void LoadSavedPathForCurrentGame()
        {
            if (gamePath == null || gameItems.Count == 0)
                return;

            string key = GetCurrentGameKey();
            GameCarouselItem current = gameItems[currentIndex];

            if (TryGetSavedExecutable(key, out string savedExe))
            {
                current.ExecutablePath = savedExe;
                gamePath.Text = savedExe;
                return;
            }

            if (!string.IsNullOrWhiteSpace(current.ExecutablePath) && File.Exists(current.ExecutablePath))
            {
                gamePath.Text = current.ExecutablePath;
                return;
            }

            if (GameBoosterDefaults.TryDetectExecutable(key, out string detectedExe))
            {
                current.ExecutablePath = detectedExe;
                gamePath.Text = detectedExe;
                return;
            }

            gamePath.Text = string.Empty;
        }

        private void SaveCurrentGameExecutable(string executablePath)
        {
            SavedGamePaths data = LoadSavedGamePaths();
            string key = GetCurrentGameKey();

            data.Paths[key] = executablePath ?? string.Empty;
            SaveSavedGamePaths(data);

            gameItems[currentIndex].ExecutablePath = executablePath;

            if (gameItems[currentIndex].IsCustom)
                SaveCustomGames();

            if (gamePath != null)
                gamePath.Text = executablePath ?? string.Empty;
        }

        private void AssignExecutableToCurrentGame(string executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                return;

            gameItems[currentIndex].ExecutablePath = executablePath;
            SaveCurrentGameExecutable(executablePath);
        }

        #endregion

        #region Events

        private void GameNext_Click(object sender, EventArgs e)
        {
            StartSlide(1);
        }

        private void GamePrev_Click(object sender, EventArgs e)
        {
            StartSlide(-1);
        }

        private void searchGame_TextChanged(object sender, EventArgs e)
        {
            SlideToGame(searchGame.Text);
        }

        private async void optimizeGame_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Delay(100);

                if (gameItems.Count == 0)
                    return;

                string gameKey = GetCurrentGameKey();
                string installHint = GetInstallFolderHint(gamePath?.Text?.Trim());

                this.Enabled = false;

                bool ok = await ExecuteGameBoosterAsync(gameKey, installHint);

                if (ok)
                {
                    TweakExecutor.ShowMessage($"{gameItems[currentIndex].Name} optimized successfully.", "Success");
                }
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private void optimizeGame_LoadingComplete(object sender, EventArgs e)
        {
            this.Enabled = true;
        }

        private void saveGame_Click(object sender, EventArgs e)
        {
            if (gameItems.Count == 0)
                return;

            string installHint = GetInstallFolderHint(gamePath?.Text?.Trim());

            if (!TryResolveGameExecutable(installHint, out string exePath))
            {
                TweakExecutor.ShowMessage(
                    $"Select the {gameItems[currentIndex].Name} executable with Select EXE first.",
                    "Missing Executable");
                return;
            }

            SaveCurrentGameExecutable(exePath);

            string installFolder = GetInstallFolderHint(exePath);

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = string.IsNullOrWhiteSpace(installFolder)
                        ? AppDomain.CurrentDomain.BaseDirectory
                        : installFolder,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                TweakExecutor.ShowMessage($"Failed to launch the game.\n\n{ex.Message}", "Launch Error");
            }
        }

        private void loadGame_Click(object sender, EventArgs e)
        {
            if (gameItems.Count == 0)
                return;

            string gameName = gameItems[currentIndex].Name;
            string gameKey = GetCurrentGameKey();
            string currentPath = gamePath?.Text?.Trim();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = $"Select {gameName} executable";
                dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
                dialog.Multiselect = false;
                dialog.CheckFileExists = true;
                dialog.FileName = GameBoosterDefaults.GetSuggestedExeFileName(gameKey);
                dialog.InitialDirectory = GameBoosterDefaults.GetBrowseFolder(gameKey, currentPath);

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                AssignExecutableToCurrentGame(dialog.FileName);

                TweakExecutor.ShowMessage(
                    $"{gameName} executable saved. You can optimize or launch it now.",
                    "Executable Selected");
            }
        }

        #endregion

    }
}
