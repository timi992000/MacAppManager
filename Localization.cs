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
            ["start"]              = "Start",
            ["kill"]               = "Kill",
            ["killModeSoft"]       = "Soft",
            ["killModeForce"]      = "Force",
            ["killModeSoftForce"]  = "Soft → Force",
            ["killModeSoftTip"]       = "Asks the app to close gracefully",
            ["killModeForceTip"]      = "Immediately kills the process (data loss possible)",
            ["killModeSoftForceTip"]  = "Asks first, force kills after 5s if unresponsive",
            ["uninstall"]          = "Uninstall",
            ["refresh"]            = "Refresh",
        },
        ["de"] = new()
        {
            ["title"]         = "App Manager",
            ["search"]        = "Suchen…",
            ["filterAll"]     = "Alle Apps",
            ["filterRunning"] = "Nur laufende",
            ["filterStopped"] = "Nur gestoppte",
            ["apps"]          = "Apps",
            ["start"]              = "Starten",
            ["kill"]               = "Beenden",
            ["killModeSoft"]       = "Sanft",
            ["killModeForce"]      = "Erzwingen",
            ["killModeSoftForce"]  = "Sanft → Erzwingen",
            ["killModeSoftTip"]       = "Fordert die App auf, sich sauber zu beenden",
            ["killModeForceTip"]      = "Beendet den Prozess sofort (Datenverlust möglich)",
            ["killModeSoftForceTip"]  = "Versucht sanft, killt nach 5s wenn keine Reaktion",
            ["uninstall"]          = "Deinstallieren",
            ["refresh"]            = "Aktualisieren",
        },
        ["fr"] = new()
        {
            ["title"]         = "Gestionnaire d'apps",
            ["search"]        = "Rechercher…",
            ["filterAll"]     = "Toutes les apps",
            ["filterRunning"] = "En cours seulement",
            ["filterStopped"] = "Arrêtées seulement",
            ["apps"]          = "apps",
            ["start"]              = "Démarrer",
            ["kill"]               = "Fermer",
            ["killModeSoft"]       = "Doux",
            ["killModeForce"]      = "Forcer",
            ["killModeSoftForce"]  = "Doux → Forcer",
            ["killModeSoftTip"]       = "Demande à l'app de se fermer proprement",
            ["killModeForceTip"]      = "Tue le processus immédiatement (perte de données possible)",
            ["killModeSoftForceTip"]  = "Essaie d'abord, force après 5s sans réponse",
            ["uninstall"]          = "Désinstaller",
            ["refresh"]            = "Actualiser",
        },
        ["es"] = new()
        {
            ["title"]         = "Administrador de apps",
            ["search"]        = "Buscar…",
            ["filterAll"]     = "Todas las apps",
            ["filterRunning"] = "Solo en ejecución",
            ["filterStopped"] = "Solo detenidas",
            ["apps"]          = "apps",
            ["start"]              = "Iniciar",
            ["kill"]               = "Cerrar",
            ["killModeSoft"]       = "Suave",
            ["killModeForce"]      = "Forzar",
            ["killModeSoftForce"]  = "Suave → Forzar",
            ["killModeSoftTip"]       = "Pide a la app que se cierre correctamente",
            ["killModeForceTip"]      = "Termina el proceso de inmediato (posible pérdida de datos)",
            ["killModeSoftForceTip"]  = "Intenta primero, fuerza tras 5s sin respuesta",
            ["uninstall"]          = "Desinstalar",
            ["refresh"]            = "Actualizar",
        },
        ["it"] = new()
        {
            ["title"]         = "Gestore app",
            ["search"]        = "Cerca…",
            ["filterAll"]     = "Tutte le app",
            ["filterRunning"] = "Solo in esecuzione",
            ["filterStopped"] = "Solo ferme",
            ["apps"]          = "app",
            ["start"]              = "Avvia",
            ["kill"]               = "Chiudi",
            ["killModeSoft"]       = "Normale",
            ["killModeForce"]      = "Forza",
            ["killModeSoftForce"]  = "Normale → Forza",
            ["killModeSoftTip"]       = "Chiede all'app di chiudersi normalmente",
            ["killModeForceTip"]      = "Termina il processo immediatamente (possibile perdita di dati)",
            ["killModeSoftForceTip"]  = "Prima prova, forza dopo 5s senza risposta",
            ["uninstall"]          = "Disinstalla",
            ["refresh"]            = "Aggiorna",
        },
    };

    public static string Get(string key) =>
        _t.TryGetValue(Current, out var d) && d.TryGetValue(key, out var v) ? v : key;
}
