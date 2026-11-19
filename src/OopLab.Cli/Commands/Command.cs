using OopLab.Cli.Parsing;

namespace OopLab.Cli.Commands;

public sealed class Command
{
    public string Selector { get; }
    public string Description { get; }
    public string Usage { get; }
    private readonly Func<Args, Task> _handler;

    public Command(string selector, string description, string usage, Func<Args, Task> handler)
    {
        Selector = selector.ToLowerInvariant();
        Description = description;
        Usage = usage;
        _handler = handler;
    }

    public Task ExecuteAsync(Args args) => _handler(args);
}
