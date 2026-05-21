using Spectre.Console;

static class TerminalMode
{
    public static void Run()
    {
        while (true)
        {
            var appList = AppHelper.GetApps();

            var filter = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Which apps to show?")
                    .AddChoices("All apps", "Running only", "Stopped only", "Quit")
            );

            if (filter == "Quit") break;

            if (filter == "Running only")  appList = appList.Where(a =>  a.IsRunning).ToList();
            if (filter == "Stopped only")  appList = appList.Where(a => !a.IsRunning).ToList();

            if (appList.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No apps found for this filter.[/]");
                Thread.Sleep(1500);
                AnsiConsole.Clear();
                continue;
            }

            int pageSize = Math.Max(10, Console.WindowHeight - 8);

            var selected = AnsiConsole.Prompt(
                new MultiSelectionPrompt<AppItem>()
                    .Title("[bold]Select apps[/] [grey](type to search)[/]")
                    .PageSize(pageSize)
                    .UseConverter(a => $"{a.Name} {(a.IsRunning ? "[green]✓ running[/]" : "[grey]✗[/]")}")
                    .AddChoices(appList)
            );

            if (selected.Count == 0) break;

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Action for [bold cyan]{selected.Count}[/] app(s)?")
                    .AddChoices("Back", "Start", "Kill", "Uninstall (Trash)")
            );

            if (action == "Back") { AnsiConsole.Clear(); continue; }

            foreach (var app in selected)
            {
                if (action == "Start")
                {
                    AppActions.Start(app);
                    AnsiConsole.MarkupLine($"[green]Started:[/] {app.Name}");
                }
                else if (action == "Kill")
                {
                    AppActions.Kill(app);
                    AnsiConsole.MarkupLine($"[yellow]Killed:[/] {app.Name}");
                }
                else if (action == "Uninstall (Trash)")
                {
                    AppActions.Uninstall(app);
                    AnsiConsole.MarkupLine($"[red]Uninstalled:[/] {app.Name}");
                }
            }

            AnsiConsole.MarkupLine("\n[grey]Press any key...[/]");
            Console.ReadKey(true);
            AnsiConsole.Clear();
        }
    }
}
