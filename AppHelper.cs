using System.Diagnostics;
using Microsoft.Win32;

static class AppHelper
{
    public static List<AppItem> GetApps()
    {
        var apps = new List<AppItem>();

        if (OperatingSystem.IsMacOS())
        {
            var dirs = new[] { "/Applications", "/System/Applications", "/Users/Shared/Applications" };
            foreach (var dir in dirs)
            {
                if (!Directory.Exists(dir)) continue;
                foreach (var path in Directory.GetDirectories(dir, "*.app"))
                {
                    var name = Path.GetFileNameWithoutExtension(path);
                    var running = Process.GetProcessesByName(name).Length > 0;
                    apps.Add(new AppItem(name, path, running));
                }
            }
        }
        else if (OperatingSystem.IsWindows())
        {
            apps.AddRange(GetWindowsApps());
        }
        else if (OperatingSystem.IsLinux())
        {
            var desktopDirs = new[]
            {
                "/usr/share/applications",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/applications"),
            };
            foreach (var dir in desktopDirs)
            {
                if (!Directory.Exists(dir)) continue;
                foreach (var file in Directory.GetFiles(dir, "*.desktop"))
                {
                    var lines = File.ReadAllLines(file);
                    var name = lines.FirstOrDefault(l => l.StartsWith("Name="))?.Split('=', 2)[1];
                    var exec = lines.FirstOrDefault(l => l.StartsWith("Exec="))?.Split('=', 2)[1]?.Split(' ')[0];
                    var noDisplay = lines.Any(l => l == "NoDisplay=true");
                    if (name == null || exec == null || noDisplay) continue;
                    var procName = Path.GetFileNameWithoutExtension(exec);
                    var running = Process.GetProcessesByName(procName).Length > 0;
                    apps.Add(new AppItem(name, file, running));
                }
            }
        }

        return apps.OrderBy(a => a.Name).ToList();
    }

#pragma warning disable CA1416
    private static List<AppItem> GetWindowsApps()
    {
        // Collect all running exe paths once for fast lookup
        var runningPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var proc in Process.GetProcesses())
        {
            try { var path = proc.MainModule?.FileName; if (path != null) runningPaths.Add(path); }
            catch { }
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var apps = new List<AppItem>();

        // Registry uninstall keys — most reliable source for installed apps
        var registryKeys = new[]
        {
            (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", Registry.LocalMachine),
            (@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", Registry.LocalMachine),
            (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", Registry.CurrentUser),
        };

        foreach (var (keyPath, hive) in registryKeys)
        {
            using var key = hive.OpenSubKey(keyPath);
            if (key == null) continue;

            foreach (var subName in key.GetSubKeyNames())
            {
                using var sub = key.OpenSubKey(subName);
                var displayName = sub?.GetValue("DisplayName") as string;
                if (string.IsNullOrWhiteSpace(displayName)) continue;

                // Skip update/component entries
                var systemComponent = sub?.GetValue("SystemComponent") as int?;
                if (systemComponent == 1) continue;
                if (displayName.Contains("Update for", StringComparison.OrdinalIgnoreCase)) continue;
                if (displayName.Contains("Redistributable", StringComparison.OrdinalIgnoreCase)) continue;

                if (!seen.Add(displayName)) continue;

                // Try to resolve exe path
                string? exePath = null;

                var displayIcon = (sub?.GetValue("DisplayIcon") as string)?.Split(',')[0].Trim('"', ' ');
                if (displayIcon?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true && File.Exists(displayIcon))
                    exePath = displayIcon;

                if (exePath == null)
                {
                    var location = sub?.GetValue("InstallLocation") as string;
                    if (!string.IsNullOrEmpty(location) && Directory.Exists(location))
                    {
                        try
                        {
                            exePath = Directory.GetFiles(location, "*.exe", SearchOption.TopDirectoryOnly)
                                .FirstOrDefault(f => !Path.GetFileName(f).Contains("uninstall", StringComparison.OrdinalIgnoreCase));
                        }
                        catch (UnauthorizedAccessException) { }
                    }
                }

                var running = exePath != null && runningPaths.Contains(exePath);
                apps.Add(new AppItem(displayName, exePath ?? displayName, running));
            }
        }

        return apps;
    }
#pragma warning restore CA1416
}
