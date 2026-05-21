using System.Diagnostics;

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
            var dirs = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            };
            foreach (var dir in dirs)
            {
                if (!Directory.Exists(dir)) continue;
                foreach (var sub in Directory.GetDirectories(dir))
                {
                    var exe = Directory.GetFiles(sub, "*.exe", SearchOption.TopDirectoryOnly)
                        .FirstOrDefault(f => !Path.GetFileName(f).Contains("uninstall", StringComparison.OrdinalIgnoreCase));
                    if (exe == null) continue;
                    var name = Path.GetFileNameWithoutExtension(exe);
                    var running = Process.GetProcessesByName(name).Length > 0;
                    apps.Add(new AppItem(name, exe, running));
                }
            }
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
}
