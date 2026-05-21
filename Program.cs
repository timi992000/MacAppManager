using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using Spectre.Console;

AnsiConsole.Clear();

var mode = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[bold blue]App Manager[/] — Choose mode:")
        .AddChoices("Terminal UI", "GUI (Avalonia)", "Quit")
);

if (mode == "Quit") return;

if (mode == "Terminal UI")
{
    TerminalMode.Run();
}
else
{
    AnsiConsole.MarkupLine("[grey]Starting GUI…[/]");
    AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .StartWithClassicDesktopLifetime(args);
}

class App : Avalonia.Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();
        base.OnFrameworkInitializationCompleted();
    }
}
