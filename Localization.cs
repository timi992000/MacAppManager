static class L
{
    public static string Current = "en";

    public static readonly Dictionary<string, (string Flag, string Name)> Languages = new()
    {
        ["en"] = ("🇬🇧", "English"),
        ["de"] = ("🇩🇪", "Deutsch"),
        ["fr"] = ("🇫🇷", "Français"),
        ["es"] = ("🇪🇸", "Español"),
        ["it"] = ("🇮🇹", "Italiano"),
    };

    public static readonly string[] Codes = [.. Languages.Keys];

    private static readonly Dictionary<string, Dictionary<string, string>> _t = new()
    {
        ["en"] = new()
        {
            ["title"]         = "App Manager",
            ["search"]        = "Search…",
            ["filterAll"]     = "All apps",
            ["filterRunning"] = "Running only",
            ["filterStopped"] = "Stopped only",
            ["apps"]          = "apps",
            ["start"]         = "Start",
            ["kill"]          = "Kill",
            ["uninstall"]     = "Uninstall",
            ["refresh"]       = "Refresh",
        },
        ["de"] = new()
        {
            ["title"]         = "App Manager",
            ["search"]        = "Suchen…",
            ["filterAll"]     = "Alle Apps",
            ["filterRunning"] = "Nur laufende",
            ["filterStopped"] = "Nur gestoppte",
            ["apps"]          = "Apps",
            ["start"]         = "Starten",
            ["kill"]          = "Beenden",
            ["uninstall"]     = "Deinstallieren",
            ["refresh"]       = "Aktualisieren",
        },
        ["fr"] = new()
        {
            ["title"]         = "Gestionnaire d'apps",
            ["search"]        = "Rechercher…",
            ["filterAll"]     = "Toutes les apps",
            ["filterRunning"] = "En cours seulement",
            ["filterStopped"] = "Arrêtées seulement",
            ["apps"]          = "apps",
            ["start"]         = "Démarrer",
            ["kill"]          = "Forcer la fermeture",
            ["uninstall"]     = "Désinstaller",
            ["refresh"]       = "Actualiser",
        },
        ["es"] = new()
        {
            ["title"]         = "Administrador de apps",
            ["search"]        = "Buscar…",
            ["filterAll"]     = "Todas las apps",
            ["filterRunning"] = "Solo en ejecución",
            ["filterStopped"] = "Solo detenidas",
            ["apps"]          = "apps",
            ["start"]         = "Iniciar",
            ["kill"]          = "Forzar cierre",
            ["uninstall"]     = "Desinstalar",
            ["refresh"]       = "Actualizar",
        },
        ["it"] = new()
        {
            ["title"]         = "Gestore app",
            ["search"]        = "Cerca…",
            ["filterAll"]     = "Tutte le app",
            ["filterRunning"] = "Solo in esecuzione",
            ["filterStopped"] = "Solo ferme",
            ["apps"]          = "app",
            ["start"]         = "Avvia",
            ["kill"]          = "Termina forzatamente",
            ["uninstall"]     = "Disinstalla",
            ["refresh"]       = "Aggiorna",
        },
    };

    public static string Get(string key) =>
        _t.TryGetValue(Current, out var d) && d.TryGetValue(key, out var v) ? v : key;
}
