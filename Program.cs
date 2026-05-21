using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Spectre.Console;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Themes.Fluent;
using Avalonia.Controls.ApplicationLifetimes;

AnsiConsole.Clear();

// 1. Auswahlmenü beim Start
var mode = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[bold blue]macOS App Manager[/] - Wie möchtest du starten?")
        .AddChoices("Terminal UI", "Grafische Oberfläche (Avalonia GUI)", "Beenden")
);

if (mode == "Beenden") return;

if (mode == "Terminal UI")
{
    RunTerminalMode();
}
else if (mode == "Grafische Oberfläche (Avalonia GUI)")
{
    AnsiConsole.MarkupLine("[grey]Starte GUI... (Dies kann beim ersten Mal einen Moment dauern)[/]");
    AppBuilder.Configure<AppManagerGui>()
        .UsePlatformDetect()
        .StartWithClassicDesktopLifetime(args);
}

// ==========================================
// TERMINAL MODUS 
// ==========================================
void RunTerminalMode()
{
    while (true)
    {
        var appList = AppHelper.GetApps();

        // Neu: Filter-Abfrage im Terminal
        var filter = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Welche Apps sollen angezeigt werden?")
                .AddChoices("Alle Apps", "Nur laufende Apps", "Nur gestoppte Apps", "Beenden")
        );

        if (filter == "Beenden") break;

        if (filter == "Nur laufende Apps") appList = appList.Where(a => a.IsRunning).ToList();
        else if (filter == "Nur gestoppte Apps") appList = appList.Where(a => !a.IsRunning).ToList();

        if (appList.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Keine Apps für diesen Filter gefunden.[/]");
            Thread.Sleep(1500);
            AnsiConsole.Clear();
            continue;
        }

        int pageSize = Math.Max(10, Console.WindowHeight - 8);

        var selectedApps = AnsiConsole.Prompt(
            new MultiSelectionPrompt<AppItem>()
                .Title("[bold]Hauptmenü[/]: Wähle Anwendungen aus [grey](Tippen zum Suchen)[/]")
                .PageSize(pageSize)
                .UseConverter(a => $"{a.Name} {(a.IsRunning ? "[green]✓ (Läuft)[/]" : "[grey]✗[/]")}")
                .AddChoices(appList)
        );

        if (selectedApps.Count == 0) break;

        // Neu: "Starten" als Option hinzugefügt
        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Was soll mit den [bold cyan]{selectedApps.Count}[/] Apps passieren?")
                .AddChoices("Zurück", "Starten", "Beenden (Kill)", "Deinstallieren (Papierkorb)")
        );

        if (action == "Zurück")
        {
            AnsiConsole.Clear();
            continue;
        }

        foreach (var app in selectedApps)
        {
            if (action == "Starten")
            {
                // Nutzt das macOS "open" Kommando
                Process.Start("open", new[] { "-a", app.Path })?.WaitForExit();
                AnsiConsole.MarkupLine($"[green]Gestartet:[/] {app.Name}");
            }
            else if (action == "Beenden (Kill)")
            {
                Process.Start("pkill", new[] { "-9", "-f", app.Path })?.WaitForExit();
                AnsiConsole.MarkupLine($"[green]Beendet:[/] {app.Name}");
            }
            else if (action == "Deinstallieren (Papierkorb)")
            {
                Process.Start("pkill", new[] { "-9", "-f", app.Path })?.WaitForExit();
                Thread.Sleep(500);
                var script = $"tell application \"Finder\" to move POSIX file \"{app.Path}\" to trash";
                Process.Start("osascript", new[] { "-e", script })?.WaitForExit();
                AnsiConsole.MarkupLine($"[red]Deinstalliert:[/] {app.Name}");
            }
        }

        AnsiConsole.MarkupLine("\n[grey]Drücke eine Taste...[/]");
        Console.ReadKey(true);
        AnsiConsole.Clear();
    }
}

// ==========================================
// AVALONIA GUI MODUS
// ==========================================
class AppManagerGui : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        base.OnFrameworkInitializationCompleted();
    }
}

class MainWindow : Window
{
    private ListBox _listBox;
    private List<AppItem> _allApps;
    private TextBox _searchBox;
    private ComboBox _filterBox;
    private TextBlock _countLabel;

    public MainWindow()
    {
        Title = "macOS App Manager";
        Width = 650;
        Height = 700;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _allApps = AppHelper.GetApps();

        // Suchleiste
        _searchBox = new TextBox { PlaceholderText = "Suchen...", Margin = new Thickness(10, 10, 5, 10) };

        // Neu: Filter Dropdown
        _filterBox = new ComboBox
        {
            ItemsSource = new[] { "Alle Apps", "Nur laufende", "Nur gestoppte" },
            SelectedIndex = 0,
            Margin = new Thickness(5, 10, 10, 10),
            Width = 150
        };

        // Neu: Anzeige der Anzahl gefilterter Elemente
        _countLabel = new TextBlock
        {
            Text = $"Anzahl: {_allApps.Count}",
            Margin = new Thickness(5, 10, 10, 10),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        // Layout für Suchleiste und Filter
        var topPanel = new DockPanel();
        DockPanel.SetDock(_filterBox, Dock.Right);
        DockPanel.SetDock(_countLabel, Dock.Right);
        topPanel.Children.Add(_filterBox);
        topPanel.Children.Add(_countLabel);
        topPanel.Children.Add(_searchBox);

        // App-Liste
        _listBox = new ListBox
        {
            SelectionMode = Avalonia.Controls.SelectionMode.Multiple,
            Margin = new Thickness(10, 0, 10, 10)
        };
        UpdateList(_allApps);

        // Buttons
        var startBtn = new Button { Content = "Starten", Margin = new Thickness(5), Background = Avalonia.Media.Brushes.DarkGreen, Foreground = Avalonia.Media.Brushes.White };
        var killBtn = new Button { Content = "Beenden (Kill)", Margin = new Thickness(5), Background = Avalonia.Media.Brushes.Orange };
        var uninstallBtn = new Button { Content = "Deinstallieren", Margin = new Thickness(5), Background = Avalonia.Media.Brushes.DarkRed, Foreground = Avalonia.Media.Brushes.White };
        var refreshBtn = new Button { Content = "Aktualisieren", Margin = new Thickness(5) };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(10)
        };
        buttonPanel.Children.Add(startBtn);
        buttonPanel.Children.Add(killBtn);
        buttonPanel.Children.Add(uninstallBtn);
        buttonPanel.Children.Add(refreshBtn);

        // Haupt-Layout
        var dockPanel = new DockPanel();
        DockPanel.SetDock(topPanel, Dock.Top);
        DockPanel.SetDock(buttonPanel, Dock.Bottom);

        dockPanel.Children.Add(topPanel);
        dockPanel.Children.Add(buttonPanel);
        dockPanel.Children.Add(_listBox);

        Content = dockPanel;

        // Events
        _searchBox.TextChanged += (s, e) => ApplyFilters();
        _filterBox.SelectionChanged += (s, e) => ApplyFilters();

        refreshBtn.Click += (s, e) => RefreshData();
        startBtn.Click += (s, e) => ExecuteAction("start");
        killBtn.Click += (s, e) => ExecuteAction("kill");
        uninstallBtn.Click += (s, e) => ExecuteAction("uninstall");
    }

    // Kombiniert Suche und Status-Filter
    private void ApplyFilters()
    {
        var term = _searchBox.Text?.ToLower() ?? "";
        var query = _allApps.Where(a => a.Name.ToLower().Contains(term));

        if (_filterBox.SelectedIndex == 1) // Nur laufende
            query = query.Where(a => a.IsRunning);
        else if (_filterBox.SelectedIndex == 2) // Nur gestoppte
            query = query.Where(a => !a.IsRunning);

        var filteredApps = query.ToList();
        UpdateList(filteredApps);
        UpdateItemCount(filteredApps.Count);
    }

    private void UpdateList(List<AppItem> apps)
    {
        _listBox.ItemsSource = apps;
    }

    private void UpdateItemCount(int count)
    {
        _countLabel.Text = $"Anzahl: {count}";
    }

    private void RefreshData()
    {
        _allApps = AppHelper.GetApps();
        ApplyFilters(); // Filter beim Aktualisieren beibehalten
    }

    private void ExecuteAction(string action)
    {
        if (_listBox.SelectedItems == null) return;

        var selected = _listBox.SelectedItems.Cast<AppItem>().ToList();
        foreach (var app in selected)
        {
            if (action == "start")
            {
                Process.Start("open", new[] { "-a", app.Path })?.WaitForExit();
            }
            else if (action == "kill")
            {
                Process.Start("pkill", new[] { "-9", "-f", app.Path })?.WaitForExit();
            }
            else if (action == "uninstall")
            {
                Process.Start("pkill", new[] { "-9", "-f", app.Path })?.WaitForExit();
                Thread.Sleep(500);
                var script = $"tell application \"Finder\" to move POSIX file \"{app.Path}\" to trash";
                Process.Start("osascript", new[] { "-e", script })?.WaitForExit();
            }
        }

        // Kurz warten, damit das System Zeit hat, den Status zu ändern, bevor neu geladen wird
        Thread.Sleep(500);
        RefreshData();
    }

}

// ==========================================
// HILFSKLASSEN
// ==========================================
record AppItem(string Name, string Path, bool IsRunning)
{
    public override string ToString() => $"{Name} {(IsRunning ? "✓ (Läuft)" : "")}";
}

static class AppHelper
{
    // List<AppItem> GetApps()
    // {
    //     var appDirectories = new List<string>();
    //     if (Directory.Exists("/Applications"))
    //         appDirectories.AddRange(Directory.GetDirectories("/Applications", "*.app"));

    //     var userAppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Applications");
    //     if (Directory.Exists(userAppDir))
    //         appDirectories.AddRange(Directory.GetDirectories(userAppDir, "*.app"));

    //     var processes = Process.GetProcesses();

    //     return appDirectories.Select(path =>
    //     {
    //         var name = Path.GetFileNameWithoutExtension(path);
    //         var proc = processes.FirstOrDefault(p => p.ProcessName.Contains(name, StringComparison.OrdinalIgnoreCase));
    //         return new AppItem(name, path, proc != null);
    //     }).OrderBy(a => a.Name).ToList();
    // }

    // ==========================================
    // ZENTRALE DATENBESCHAFFUNG
    // ==========================================
    public static List<AppItem> GetApps()
    {
        var apps = new List<AppItem>();
        var appDirs = new[] { "/Applications", "/System/Applications", "/Users/Shared/Applications" };

        foreach (var dir in appDirs)
        {
            if (!Directory.Exists(dir)) continue;

            foreach (var app in Directory.GetDirectories(dir, "*.app"))
            {
                var name = Path.GetFileNameWithoutExtension(app);
                var isRunning = Process.GetProcessesByName(name).Length > 0;
                apps.Add(new AppItem(name, app, isRunning));
            }
        }

        return apps.OrderBy(a => a.Name).ToList();
    }
}