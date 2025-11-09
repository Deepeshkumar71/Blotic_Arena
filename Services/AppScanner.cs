using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using BloticArena.Models;

namespace BloticArena.Services
{
    public class AppScanner
    {
        private static readonly Dictionary<string, string> AppIcons = new()
        {
            { "chrome", "🌐" },
            { "firefox", "🦊" },
            { "edge", "🌊" },
            { "brave", "🦁" },
            { "opera", "🎭" },
            { "visual studio", "💻" },
            { "vscode", "📝" },
            { "code", "📝" },
            { "notepad", "📄" },
            { "word", "📘" },
            { "excel", "📊" },
            { "powerpoint", "📽️" },
            { "outlook", "📧" },
            { "teams", "👥" },
            { "discord", "💬" },
            { "slack", "💼" },
            { "spotify", "🎵" },
            { "steam", "🎮" },
            { "epic", "🎮" },
            { "valorant", "🎯" },
            { "league of legends", "⚔️" },
            { "riot", "🎮" },
            { "photoshop", "🎨" },
            { "illustrator", "✏️" },
            { "premiere", "🎬" },
            { "vlc", "🎬" },
            { "winrar", "📦" },
            { "7zip", "📦" },
            { "git", "🔧" },
            { "python", "🐍" },
            { "java", "☕" },
            { "node", "🟢" },
            { "docker", "🐳" },
            { "virtualbox", "📦" },
            { "vmware", "💿" },
            { "zoom", "📹" },
            { "skype", "📞" },
            { "telegram", "✈️" },
            { "whatsapp", "💚" },
            { "obs", "🎥" },
            { "gimp", "🖼️" },
            { "blender", "🎲" },
            { "unity", "🎮" },
            { "unreal", "🎮" },
            { "android studio", "🤖" },
            { "intellij", "💡" },
            { "pycharm", "🐍" },
            { "webstorm", "🌐" },
            { "rider", "🏃" },
            { "postman", "📮" },
            { "insomnia", "😴" },
            { "figma", "🎨" },
            { "adobe", "🅰️" },
            { "calculator", "🔢" },
            { "paint", "🎨" },
            { "terminal", "⌨️" },
            { "powershell", "⚡" },
            { "cmd", "⌨️" },
            { "explorer", "📁" }
        };

        public List<AppInfo> ScanInstalledApps()
        {
            var apps = new HashSet<AppInfo>(new AppInfoComparer());

            // ONLY scan Windows Registry for main installed applications
            ScanRegistry(apps, Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            ScanRegistry(apps, Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            ScanRegistry(apps, Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");

            // Scan Start Menu shortcuts (main apps only)
            ScanStartMenu(apps, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));
            ScanStartMenu(apps, Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));

            return apps.OrderBy(a => a.Name).ToList();
        }

        private void ScanRegistry(HashSet<AppInfo> apps, RegistryKey rootKey, string subKeyPath)
        {
            try
            {
                using var key = rootKey.OpenSubKey(subKeyPath);
                if (key == null) return;

                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    try
                    {
                        using var subKey = key.OpenSubKey(subKeyName);
                        if (subKey == null) continue;

                        var displayName = subKey.GetValue("DisplayName")?.ToString();
                        var installLocation = subKey.GetValue("InstallLocation")?.ToString();
                        var displayIcon = subKey.GetValue("DisplayIcon")?.ToString();
                        var uninstallString = subKey.GetValue("UninstallString")?.ToString();
                        var displayVersion = subKey.GetValue("DisplayVersion")?.ToString();

                        if (string.IsNullOrWhiteSpace(displayName)) continue;

                        // Skip system components, tools, and non-user apps
                        var lowerName = displayName.ToLower();
                        if (lowerName.Contains("update") || 
                            lowerName.Contains("redistributable") ||
                            lowerName.Contains("security update") ||
                            lowerName.Contains("runtime") ||
                            lowerName.Contains("sdk") ||
                            lowerName.Contains("tools for") ||
                            lowerName.Contains("headers and") ||
                            lowerName.Contains("(mne)") ||
                            lowerName.Contains("x86") && lowerName.Contains("x64") ||
                            lowerName.Contains("troubleshooting") ||
                            lowerName.Contains("licensing") ||
                            lowerName.Contains("telemetry") ||
                            lowerName.Contains("universal crt") ||
                            lowerName.Contains("prerequisites") ||
                            lowerName.Contains("vcredist") ||
                            lowerName.Contains("microsoft visual c++") ||
                            lowerName.Contains(".net") && !lowerName.Contains("framework") ||
                            displayName.StartsWith("KB"))
                            continue;
                        
                        // Must have a valid display icon or install location
                        if (string.IsNullOrWhiteSpace(displayIcon) && string.IsNullOrWhiteSpace(installLocation))
                            continue;

                        // Find the actual executable to launch (not uninstaller)
                        string? launchCommand = FindLaunchCommand(displayName, installLocation, displayIcon, uninstallString);
                        
                        var path = installLocation ?? displayIcon ?? string.Empty;
                        
                        // Extract icon path from DisplayIcon
                        string? iconPath = null;
                        if (!string.IsNullOrWhiteSpace(displayIcon))
                        {
                            // DisplayIcon might be "path.exe,0" format
                            var iconParts = displayIcon.Split(',');
                            iconPath = iconParts[0].Trim('"');
                        }
                        
                        apps.Add(new AppInfo
                        {
                            Name = displayName,
                            Path = path,
                            LaunchCommand = launchCommand ?? path,
                            Icon = GetIconForApp(displayName),
                            Type = "Installed App",
                            Category = GetCategoryForApp(displayName),
                            IconImage = !string.IsNullOrWhiteSpace(iconPath) && File.Exists(iconPath) 
                                ? IconExtractor.ExtractIconFromFile(iconPath) 
                                : null
                        });
                    }
                    catch { /* Skip problematic entries */ }
                }
            }
            catch { /* Skip if registry access fails */ }
        }

        private void ScanDirectory(HashSet<AppInfo> apps, string directory)
        {
            if (!Directory.Exists(directory)) return;

            try
            {
                var exeFiles = Directory.GetFiles(directory, "*.exe", SearchOption.TopDirectoryOnly);
                foreach (var exePath in exeFiles)
                {
                    try
                    {
                        var fileName = Path.GetFileNameWithoutExtension(exePath);
                        
                        // Skip common system files
                        if (IsSystemFile(fileName)) continue;

                        apps.Add(new AppInfo
                        {
                            Name = fileName,
                            Path = exePath,
                            Icon = GetIconForApp(fileName),
                            Type = "Executable",
                            IconImage = IconExtractor.ExtractIconFromFile(exePath)
                        });
                    }
                    catch { /* Skip problematic files */ }
                }

                // Recursively scan subdirectories (limited depth)
                var subDirs = Directory.GetDirectories(directory);
                foreach (var subDir in subDirs.Take(50)) // Limit to prevent excessive scanning
                {
                    try
                    {
                        var dirName = Path.GetFileName(subDir);
                        var exeInSubDir = Directory.GetFiles(subDir, "*.exe", SearchOption.TopDirectoryOnly);
                        
                        foreach (var exePath in exeInSubDir.Take(5)) // Limit per directory
                        {
                            var fileName = Path.GetFileNameWithoutExtension(exePath);
                            if (IsSystemFile(fileName)) continue;

                            apps.Add(new AppInfo
                            {
                                Name = fileName,
                                Path = exePath,
                                Icon = GetIconForApp(fileName),
                                Type = "Executable",
                                IconImage = IconExtractor.ExtractIconFromFile(exePath)
                            });
                        }
                    }
                    catch { /* Skip problematic directories */ }
                }
            }
            catch { /* Skip if directory access fails */ }
        }

        private void ScanStartMenu(HashSet<AppInfo> apps, string startMenuPath)
        {
            if (!Directory.Exists(startMenuPath)) return;

            try
            {
                var lnkFiles = Directory.GetFiles(startMenuPath, "*.lnk", SearchOption.AllDirectories);
                foreach (var lnkPath in lnkFiles)
                {
                    try
                    {
                        var fileName = Path.GetFileNameWithoutExtension(lnkPath);
                        var lowerFileName = fileName.ToLower();
                        
                        // Skip uninstall, help, readme, and other non-app shortcuts
                        if (lowerFileName.Contains("uninstall") ||
                            lowerFileName.Contains("readme") ||
                            lowerFileName.Contains("help") ||
                            lowerFileName.Contains("documentation") ||
                            lowerFileName.Contains("license") ||
                            lowerFileName.Contains("website") ||
                            lowerFileName.Contains("visit") ||
                            lowerFileName.Contains("support") ||
                            lowerFileName.Contains("tutorial"))
                            continue;

                        // Resolve shortcut to get actual executable path
                        var resolvedPath = ResolveShortcut(lnkPath) ?? lnkPath;

                        apps.Add(new AppInfo
                        {
                            Name = fileName,
                            Path = resolvedPath,
                            LaunchCommand = resolvedPath,
                            Icon = GetIconForApp(fileName),
                            Type = "Shortcut",
                            Category = GetCategoryForApp(fileName),
                            IconImage = IconExtractor.ExtractIconFromFile(resolvedPath)
                        });
                    }
                    catch { /* Skip problematic shortcuts */ }
                }
            }
            catch { /* Skip if start menu access fails */ }
        }

        private string GetIconForApp(string appName)
        {
            var lowerName = appName.ToLower();
            
            foreach (var kvp in AppIcons)
            {
                if (lowerName.Contains(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            // Default icons based on app type
            if (lowerName.Contains("game")) return "🎮";
            if (lowerName.Contains("media") || lowerName.Contains("player")) return "🎬";
            if (lowerName.Contains("music")) return "🎵";
            if (lowerName.Contains("photo") || lowerName.Contains("image")) return "🖼️";
            if (lowerName.Contains("video")) return "📹";
            if (lowerName.Contains("browser")) return "🌐";
            if (lowerName.Contains("editor") || lowerName.Contains("studio")) return "💻";
            if (lowerName.Contains("office")) return "📊";
            if (lowerName.Contains("mail")) return "📧";
            if (lowerName.Contains("chat") || lowerName.Contains("message")) return "💬";

            return "📦";
        }

        private bool IsSystemFile(string fileName)
        {
            var lowerName = fileName.ToLower();
            var systemFiles = new[] { 
                "unins", "uninst", "setup", "install", "update", 
                "helper", "crash", "report", "feedback", "telemetry",
                "service", "daemon", "agent", "launcher"
            };

            return systemFiles.Any(sf => lowerName.Contains(sf));
        }

        private string GetCategoryForApp(string appName)
        {
            var lowerName = appName.ToLower();

            // Browsers
            if (lowerName.Contains("chrome") || lowerName.Contains("firefox") || 
                lowerName.Contains("edge") || lowerName.Contains("brave") || 
                lowerName.Contains("opera") || lowerName.Contains("browser"))
                return "Browsers";

            // Development
            if (lowerName.Contains("visual studio") || lowerName.Contains("vscode") || 
                lowerName.Contains("code") || lowerName.Contains("git") || 
                lowerName.Contains("python") || lowerName.Contains("java") || 
                lowerName.Contains("node") || lowerName.Contains("docker") || 
                lowerName.Contains("android studio") || lowerName.Contains("intellij") ||
                lowerName.Contains("pycharm") || lowerName.Contains("webstorm") ||
                lowerName.Contains("rider") || lowerName.Contains("postman"))
                return "Development";

            // Games
            if (lowerName.Contains("steam") || lowerName.Contains("epic") || 
                lowerName.Contains("valorant") || lowerName.Contains("league") || 
                lowerName.Contains("riot") || lowerName.Contains("game") ||
                lowerName.Contains("minecraft") || lowerName.Contains("roblox"))
                return "Games";

            // Communication
            if (lowerName.Contains("discord") || lowerName.Contains("slack") || 
                lowerName.Contains("teams") || lowerName.Contains("zoom") || 
                lowerName.Contains("skype") || lowerName.Contains("telegram") ||
                lowerName.Contains("whatsapp") || lowerName.Contains("outlook") ||
                lowerName.Contains("mail") || lowerName.Contains("chat"))
                return "Communication";

            // Media & Entertainment
            if (lowerName.Contains("spotify") || lowerName.Contains("vlc") || 
                lowerName.Contains("media") || lowerName.Contains("player") ||
                lowerName.Contains("music") || lowerName.Contains("video") ||
                lowerName.Contains("obs") || lowerName.Contains("audacity"))
                return "Media";

            // Productivity
            if (lowerName.Contains("office") || lowerName.Contains("word") || 
                lowerName.Contains("excel") || lowerName.Contains("powerpoint") ||
                lowerName.Contains("onenote") || lowerName.Contains("notion") ||
                lowerName.Contains("evernote") || lowerName.Contains("notepad"))
                return "Productivity";

            // Design & Creative
            if (lowerName.Contains("photoshop") || lowerName.Contains("illustrator") || 
                lowerName.Contains("premiere") || lowerName.Contains("after effects") ||
                lowerName.Contains("gimp") || lowerName.Contains("blender") ||
                lowerName.Contains("figma") || lowerName.Contains("canva") ||
                lowerName.Contains("davinci"))
                return "Design";

            // Utilities
            if (lowerName.Contains("winrar") || lowerName.Contains("7zip") || 
                lowerName.Contains("zip") || lowerName.Contains("calculator") ||
                lowerName.Contains("paint") || lowerName.Contains("terminal") ||
                lowerName.Contains("powershell") || lowerName.Contains("cmd"))
                return "Utilities";

            return "Other";
        }

        private string? FindLaunchCommand(string appName, string? installLocation, string? displayIcon, string? uninstallString)
        {
            var lowerName = appName.ToLower();

            // Check for protocol-based launchers (like Steam, Epic, Riot Client)
            if (lowerName.Contains("steam"))
                return "steam://open/main";
            
            if (lowerName.Contains("valorant") || lowerName.Contains("riot client"))
                return "valorant://launch";
            
            if (lowerName.Contains("league of legends"))
                return "league://launch";
            
            if (lowerName.Contains("epic games"))
                return "com.epicgames.launcher://";

            // Try to find executable from DisplayIcon first (most reliable)
            if (!string.IsNullOrWhiteSpace(displayIcon))
            {
                var iconParts = displayIcon.Split(',');
                var iconPath = iconParts[0].Trim('"');
                
                // Make sure it's not an uninstaller
                if (File.Exists(iconPath) && !iconPath.ToLower().Contains("unins"))
                {
                    return iconPath;
                }
            }

            // Try to find main executable in install location
            if (!string.IsNullOrWhiteSpace(installLocation) && Directory.Exists(installLocation))
            {
                try
                {
                    // Look for executable with app name
                    var exeFiles = Directory.GetFiles(installLocation, "*.exe", SearchOption.TopDirectoryOnly);
                    
                    // Filter out uninstallers
                    var validExes = exeFiles.Where(f =>
                    {
                        var name = Path.GetFileNameWithoutExtension(f).ToLower();
                        return !name.Contains("unins") &&
                               !name.Contains("uninst") &&
                               !name.Contains("setup") &&
                               !name.Contains("install") &&
                               !name.Contains("update");
                    }).ToList();

                    // Try to find exe matching app name
                    var dirName = Path.GetFileName(installLocation.TrimEnd(Path.DirectorySeparatorChar));
                    var matchingExe = validExes.FirstOrDefault(f =>
                        Path.GetFileNameWithoutExtension(f).Equals(dirName, StringComparison.OrdinalIgnoreCase));
                    
                    if (matchingExe != null) return matchingExe;

                    // Return first valid exe
                    if (validExes.Any()) return validExes.First();
                }
                catch { }
            }

            return null;
        }

        private string? ResolveShortcut(string shortcutPath)
        {
            try
            {
                if (!shortcutPath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                    return shortcutPath;

                // Use IWshRuntimeLibrary to resolve shortcuts
                Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return shortcutPath;

                dynamic? shell = Activator.CreateInstance(shellType);
                if (shell == null) return shortcutPath;

                try
                {
                    dynamic shortcut = shell.CreateShortcut(shortcutPath);
                    string targetPath = shortcut.TargetPath;
                    
                    if (!string.IsNullOrWhiteSpace(targetPath) && File.Exists(targetPath))
                        return targetPath;
                }
                finally
                {
                    Marshal.ReleaseComObject(shell);
                }
            }
            catch
            {
                // If resolution fails, return original path
            }

            return shortcutPath;
        }

        private class AppInfoComparer : IEqualityComparer<AppInfo>
        {
            public bool Equals(AppInfo? x, AppInfo? y)
            {
                if (x == null || y == null) return false;
                return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(AppInfo obj)
            {
                return obj.Name.ToLower().GetHashCode();
            }
        }
    }
}
