using OopLab.Cli.Commands;
using OopLab.Cli.Parsing;

namespace OopLab.Cli.Strategies;

public abstract class CommandStrategyBase : ICommandStrategy
{
    private readonly Dictionary<string, Command> _commands = new(StringComparer.OrdinalIgnoreCase);

    public abstract string CategoryName { get; }

    public IEnumerable<string> GetCommandSelectors() => _commands.Keys;
    public IEnumerable<Command> GetCommands() => _commands.Values;

    public void RegisterCommand(Command command)
    {
        ArgumentNullException.ThrowIfNull(command);
        _commands[command.Selector] = command;
    }

    public async Task<bool> ExecCommandAsync(string selector, Args args)
    {
        if (!_commands.TryGetValue(selector, out var command))
            return false;

        await command.ExecuteAsync(args);
        return true;
    }
}
