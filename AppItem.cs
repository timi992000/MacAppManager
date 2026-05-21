record AppItem(string Name, string Path, bool IsRunning)
{
    public override string ToString() => $"{Name} {(IsRunning ? "✓" : "")}";
}
