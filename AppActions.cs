using System.Diagnostics;

static class AppActions
{
    public static void Start(AppItem app)
    {
        if (OperatingSystem.IsMacOS())
            Process.Start("open", new[] { "-a", app.Path })?.WaitForExit();
        else if (OperatingSystem.IsWindows())
            Process.Start(new ProcessStartInfo(app.Path) { UseShellExecute = true });
        else if (OperatingSystem.IsLinux())
            Process.Start(new ProcessStartInfo("xdg-open", app.Path) { UseShellExecute = true });
    }

    public static void Kill(AppItem app)
    {
        var name = OperatingSystem.IsWindows()
            ? Path.GetFileNameWithoutExtension(app.Path)
            : Path.GetFileNameWithoutExtension(app.Path);

        foreach (var p in Process.GetProcessesByName(name))
        {
            try { p.Kill(true); } catch { }
        }
    }

    public static void Uninstall(AppItem app)
    {
        Kill(app);
        Thread.Sleep(500);

        if (OperatingSystem.IsMacOS())
        {
            var script = $"tell application \"Finder\" to move POSIX file \"{app.Path}\" to trash";
            Process.Start("osascript", new[] { "-e", script })?.WaitForExit();
        }
        else if (OperatingSystem.IsLinux())
        {
            var trashCmd = File.Exists("/usr/bin/gio") ? "gio" : "trash";
            var trashArg = File.Exists("/usr/bin/gio") ? $"trash \"{app.Path}\"" : $"\"{app.Path}\"";
            Process.Start("bash", new[] { "-c", $"{trashCmd} {trashArg}" })?.WaitForExit();
        }
        else if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start ms-settings:appsfeatures") { UseShellExecute = true });
        }
    }
}
