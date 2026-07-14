using System.Collections.Generic;
using System.Linq;

namespace ToX_Free_Utility_NEW_ERA.Tweaks
{
    public sealed class AppDebloatTweak
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string UninstallBatch { get; set; }
        public string InstallBatch { get; set; }
    }

    public static class AppDebloatTweaks
    {
        public static List<AppDebloatTweak> All => new List<AppDebloatTweak>
        {
            new AppDebloatTweak
            {
                Key = "BingBloatware",
                Name = "Bing Bloatware",
                UninstallBatch = BuildRemoveScript(
                    "*Microsoft.BingNews*",
                    "*Microsoft.BingSearch*",
                    "*Microsoft.BingWeather*",
                    "*Microsoft.BingFinance*",
                    "*Microsoft.BingFoodAndDrink*",
                    "*Microsoft.BingSports*",
                    "*Microsoft.BingTravel*",
                    "*Microsoft.BingHealthAndFitness*",
                    "*MicrosoftWindows.Client.WebExperience*"),
                InstallBatch = BuildStoreInstallScript("ms-windows-store://")
            },

            new AppDebloatTweak
            {
                Key = "XboxApp",
                Name = "Xbox App",
                UninstallBatch = BuildRemoveScript(
                    "*Microsoft.XboxApp*",
                    "*Microsoft.XboxGamingOverlay*",
                    "*Microsoft.XboxGameOverlay*",
                    "*Microsoft.Xbox.TCUI*",
                    "*Microsoft.XboxSpeechToTextOverlay*",
                    "*Microsoft.GamingApp*"),
                InstallBatch = BuildStoreInstallScript("ms-windows-store://pdp/?ProductId=9MV0B5HZVK9Z")
            },

            new AppDebloatTweak
            {
                Key = "WindowsPhone",
                Name = "Windows Phone",
                UninstallBatch = BuildRemoveScript(
                    "*Microsoft.YourPhone*",
                    "*MicrosoftWindows.CrossDevice*",
                    "*Microsoft.YourPhone.YourPhone*"),
                InstallBatch = BuildStoreInstallScript("ms-windows-store://pdp/?ProductId=9NMPJ99VJBWV")
            },

            new AppDebloatTweak
            {
                Key = "WindowsAlarms",
                Name = "Windows Alarms",
                UninstallBatch = BuildRemoveScript("*Microsoft.WindowsAlarms*"),
                InstallBatch = BuildStoreInstallScript("ms-windows-store://pdp/?ProductId=9WZDNCRFJ3PR")
            },

            new AppDebloatTweak
            {
                Key = "Microsoft3DViewer",
                Name = "Microsoft 3D Viewer",
                UninstallBatch = BuildRemoveScript(
                    "*Microsoft.Microsoft3DViewer*",
                    "*Microsoft.3DViewer*"),
                InstallBatch = BuildStoreInstallScript("ms-windows-store://pdp/?ProductId=9NBLGGH42THS")
            },

            new AppDebloatTweak
            {
                Key = "WindowsMaps",
                Name = "Windows Maps",
                UninstallBatch = BuildRemoveScript("*Microsoft.WindowsMaps*"),
                InstallBatch = BuildStoreInstallScript("ms-windows-store://pdp/?ProductId=9WZDNCRDTBVB")
            },

            new AppDebloatTweak
            {
                Key = "M3D_Builder",
                Name = "3D Builder",
                UninstallBatch = BuildRemoveScript("*Microsoft.3DBuilder*"),
                InstallBatch = BuildStoreInstallScript("ms-windows-store://pdp/?ProductId=9WZDNCRFJ3T6")
            },

            new AppDebloatTweak
            {
                Key = "WindowsSoundRecorder",
                Name = "Windows Sound Recorder",
                UninstallBatch = BuildRemoveScript("*Microsoft.WindowsSoundRecorder*"),
                InstallBatch = BuildStoreInstallScript("ms-windows-store://pdp/?ProductId=9WZDNCRFHWKN")
            },

            new AppDebloatTweak
            {
                Key = "MicrosoftEdge",
                Name = "Microsoft Edge",
                UninstallBatch = @"taskkill /f /im msedge.exe >nul 2>&1
taskkill /f /im MicrosoftEdgeUpdate.exe >nul 2>&1
sc stop edgeupdate >nul 2>&1
sc stop edgeupdatem >nul 2>&1
powershell -NoProfile -ExecutionPolicy Bypass -Command ""Get-AppxPackage -AllUsers *Microsoft.MicrosoftEdge.Stable* -ErrorAction SilentlyContinue | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue; Get-AppxPackage *Microsoft.MicrosoftEdge.Stable* -ErrorAction SilentlyContinue | Remove-AppxPackage -ErrorAction SilentlyContinue; Get-AppxProvisionedPackage -Online | Where-Object { $_.PackageName -like '*Microsoft.MicrosoftEdge.Stable*' } | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue""
reg add ""HKLM\SOFTWARE\Policies\Microsoft\EdgeUpdate"" /v InstallDefault /t REG_DWORD /d 0 /f >nul 2>&1
reg add ""HKLM\SOFTWARE\Policies\Microsoft\EdgeUpdate"" /v UpdateDefault /t REG_DWORD /d 0 /f >nul 2>&1",
                InstallBatch = BuildStoreInstallScript("https://www.microsoft.com/edge/download")
            },

            new AppDebloatTweak
            {
                Key = "WindowsCamera",
                Name = "Windows Camera",
                UninstallBatch = BuildRemoveScript("*Microsoft.WindowsCamera*"),
                InstallBatch = BuildStoreInstallScript("ms-windows-store://pdp/?ProductId=9WZDNCRFJBBG")
            }
        };

        public static AppDebloatTweak GetByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            return All.FirstOrDefault(
                x => x.Key.Equals(key, System.StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildRemoveScript(params string[] packagePatterns)
        {
            string patternList = string.Join(",", packagePatterns.Select(p => $"'{p}'"));

            return $@"powershell -NoProfile -ExecutionPolicy Bypass -Command ""
$patterns = @({patternList})
foreach ($pattern in $patterns) {{
    Get-AppxPackage -AllUsers $pattern -ErrorAction SilentlyContinue | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue
    Get-AppxPackage $pattern -ErrorAction SilentlyContinue | Remove-AppxPackage -ErrorAction SilentlyContinue
    Get-AppxProvisionedPackage -Online | Where-Object {{ $_.PackageName -like $pattern -or $_.DisplayName -like $pattern }} | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue
}}""";
        }

        private static string BuildStoreInstallScript(string url)
        {
            return $@"powershell -NoProfile -ExecutionPolicy Bypass -Command ""Start-Process '{url}'""";
        }
    }
}
