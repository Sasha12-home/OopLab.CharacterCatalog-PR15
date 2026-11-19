namespace OopLab.Cli.Parsing;

public sealed class Args
{
    public string CommandName { get; }
    public IReadOnlyList<string> Positional { get; }
    public IReadOnlyDictionary<string, string?> Options { get; }

    public Args(string commandName, IReadOnlyList<string> positional, IReadOnlyDictionary<string, string?> options)
    {
        CommandName = commandName;
        Positional = positional;
        Options = options;
    }

    public bool HasOption(string key) => Options.ContainsKey(key);
    public string? GetOption(string key) => Options.TryGetValue(key, out var val) ? val : null;
    public string? GetPositional(int index) => index >= 0 && index < Positional.Count ? Positional[index] : null;
}
