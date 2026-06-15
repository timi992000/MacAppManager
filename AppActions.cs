using System.Diagnostics;

enum KillMode { Soft, Force, SoftThenForce }

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

    public static void Kill(AppItem app, KillMode mode = KillMode.Soft)
    {
        var name = Path.GetFileNameWithoutExtension(app.Path);

        foreach (var p in Process.GetProcessesByName(name))
        {
            try
            {
                switch (mode)
                {
                    case KillMode.Soft:
                        SendSoftClose(p, name);
                        break;
                    case KillMode.Force:
                        // SIGKILL — no cleanup, immediate termination
                        p.Kill(true);
                        break;
                    case KillMode.SoftThenForce:
                        SendSoftClose(p, name);
                        // Fall back to force if the app doesn't exit within 5 seconds
                        if (!p.WaitForExit(5000))
                            p.Kill(true);
                        break;
                }
            }
            catch { }
        }
    }

    // Requests a graceful quit using the platform's native mechanism.
    // macOS: Apple Event (kAEQuitApplication) — triggers save dialogs, same as ⌘Q.
    // Windows: WM_CLOSE message via CloseMainWindow.
    // Linux: SIGTERM.
    // .NET has no cross-platform API for this; Process.CloseMainWindow() is Windows-only under the hood.
    private static void SendSoftClose(Process p, string appName)
    {
        if (OperatingSystem.IsMacOS())
            Process.Start("osascript", new[] { "-e", $"tell application \"{appName}\" to quit" })?.WaitForExit();
        else if (OperatingSystem.IsWindows())
            p.CloseMainWindow();
        else
            Process.Start("kill", new[] { "-TERM", p.Id.ToString() })?.WaitForExit();
    }

    public static void Uninstall(AppItem app)
    {
        // Always force-kill before uninstalling to ensure the app is fully stopped
        Kill(app, KillMode.Force);
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
