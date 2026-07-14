using System.Collections.Generic;

namespace ToX_Free_Utility_NEW_ERA.Tweaks
{
    public sealed class JunkCleanTweak
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string CleanBatch { get; set; }
        public bool UsesEmptyDir { get; set; }
    }

    public static class JunkCleanTweaks
    {
        public static List<JunkCleanTweak> All => new List<JunkCleanTweak>
        {
            new JunkCleanTweak
            {
                Key = "WindowsTempFiles",
                Name = "Clear Windows Temp Files",
                UsesEmptyDir = true,
                CleanBatch = @"echo Clearing Windows temp folders...
call :EmptyDir ""C:\Windows\Temp""
call :EmptyDir ""%TEMP%""
call :EmptyDir ""%LOCALAPPDATA%\Temp"""
            },

            new JunkCleanTweak
            {
                Key = "WindowsCacheFiles",
                Name = "Clear Windows Cache Files",
                UsesEmptyDir = true,
                CleanBatch = @"echo Clearing Windows cache files...
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Windows\INetCache""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Windows\Explorer""
call :EmptyDir ""%LOCALAPPDATA%\D3DSCache""
del /q /f /s ""%LOCALAPPDATA%\IconCache.db"" >nul 2>&1
del /q /f /s ""%LOCALAPPDATA%\Microsoft\Windows\Explorer\thumbcache_*.db"" >nul 2>&1"
            },

            new JunkCleanTweak
            {
                Key = "DnsCache",
                Name = "Clear DNS Cache",
                CleanBatch = @"echo Flushing DNS cache...
ipconfig /flushdns >nul 2>&1"
            },

            new JunkCleanTweak
            {
                Key = "GamesCache",
                Name = "Clear Games Cache",
                UsesEmptyDir = true,
                CleanBatch = @"echo Clearing game launcher caches...
taskkill /F /IM steam.exe /T >nul 2>&1
taskkill /F /IM steamwebhelper.exe /T >nul 2>&1
taskkill /F /IM EpicGamesLauncher.exe /T >nul 2>&1
taskkill /F /IM EADesktop.exe /T >nul 2>&1
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\Cache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\Code Cache""
call :EmptyDir ""%LOCALAPPDATA%\Steam\htmlcache\Default\GPUCache""
call :EmptyDir ""%LOCALAPPDATA%\EpicGamesLauncher\Saved\Logs""
for /d %%p in (""%LOCALAPPDATA%\EpicGamesLauncher\Saved\webcache_*"") do (
    call :EmptyDir ""%%p\Cache""
    call :EmptyDir ""%%p\Code Cache""
)
call :EmptyDir ""%LOCALAPPDATA%\EADesktop\cache""
call :EmptyDir ""%LOCALAPPDATA%\Electronic Arts\EA Desktop\Logs"""
            },

            new JunkCleanTweak
            {
                Key = "BrowsersCache",
                Name = "Clear Browsers Cache",
                UsesEmptyDir = true,
                CleanBatch = @"echo Clearing browser caches...
taskkill /F /IM chrome.exe /T >nul 2>&1
taskkill /F /IM msedge.exe /T >nul 2>&1
taskkill /F /IM brave.exe /T >nul 2>&1
taskkill /F /IM firefox.exe /T >nul 2>&1
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\Default\Cache""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\Default\Code Cache""
call :EmptyDir ""%LOCALAPPDATA%\Google\Chrome\User Data\Default\GPUCache""
for /d %%p in (""%LOCALAPPDATA%\Google\Chrome\User Data\Profile*"") do (
    call :EmptyDir ""%%p\Cache""
    call :EmptyDir ""%%p\Code Cache""
    call :EmptyDir ""%%p\GPUCache""
)
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Cache""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\Code Cache""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Edge\User Data\Default\GPUCache""
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\Cache""
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\Code Cache""
call :EmptyDir ""%LOCALAPPDATA%\BraveSoftware\Brave-Browser\User Data\Default\GPUCache""
for /d %%p in (""%LOCALAPPDATA%\Mozilla\Firefox\Profiles\*"") do (
    call :EmptyDir ""%%p\cache2""
    call :EmptyDir ""%%p\startupCache""
    call :EmptyDir ""%%p\thumbnails""
)"
            },

            new JunkCleanTweak
            {
                Key = "WindowsCookies",
                Name = "Clear Windows Cookies",
                UsesEmptyDir = true,
                CleanBatch = @"echo Clearing Windows cookies...
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Windows\INetCookies""
call :EmptyDir ""%LOCALAPPDATA%\Microsoft\Windows\INetCache""
call :EmptyDir ""%APPDATA%\Microsoft\Windows\Cookies"""
            },

            new JunkCleanTweak
            {
                Key = "WindowsPrefetch",
                Name = "Clear Windows Prefetch",
                CleanBatch = @"echo Clearing Windows prefetch...
del /q /f /s ""C:\Windows\Prefetch\*.*"" >nul 2>&1"
            },

            new JunkCleanTweak
            {
                Key = "WindowsHistory",
                Name = "Clear Windows History",
                CleanBatch = @"echo Clearing Windows recent history...
del /q /f ""%LOCALAPPDATA%\Microsoft\Windows\Recent\*.*"" >nul 2>&1
del /q /f ""%APPDATA%\Microsoft\Windows\Recent\*.*"" >nul 2>&1
del /q /f ""%USERPROFILE%\Recent\*.*"" >nul 2>&1"
            },

            new JunkCleanTweak
            {
                Key = "DiscordCache",
                Name = "Clear Discord Cache",
                UsesEmptyDir = true,
                CleanBatch = @"echo Clearing Discord cache...
taskkill /F /IM Discord.exe /T >nul 2>&1
taskkill /F /IM DiscordSystemHelper.exe /T >nul 2>&1
for /d %%p in (""%LOCALAPPDATA%\Discord\packages\Discord-*"") do call :EmptyDir ""%%p""
call :EmptyDir ""%APPDATA%\discord\Cache\Cache_Data""
call :EmptyDir ""%APPDATA%\discord\component_crx_cache""
call :EmptyDir ""%APPDATA%\discord\blob_storage""
call :EmptyDir ""%APPDATA%\discord\Code Cache""
call :EmptyDir ""%APPDATA%\discord\GPUCache""
call :EmptyDir ""%APPDATA%\discord\logs"""
            },

            new JunkCleanTweak
            {
                Key = "RecycleBin",
                Name = "Empty Recycle Bin",
                CleanBatch = @"echo Emptying Recycle Bin...
powershell -NoProfile -Command ""Clear-RecycleBin -Force -ErrorAction SilentlyContinue"" >nul 2>&1"
            }
        };
    }
}
