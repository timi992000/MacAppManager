record AppItem(string Name, string Path, bool IsRunning, string? ExecName = null)
{
    // ExecName is the actual process name (e.g. "MSTeams" for "Microsoft Teams.app")
    public string ProcName => ExecName ?? System.IO.Path.GetFileNameWithoutExtension(Path);

    public override string ToString() => $"{Name} {(IsRunning ? "✓" : "")}";
}
