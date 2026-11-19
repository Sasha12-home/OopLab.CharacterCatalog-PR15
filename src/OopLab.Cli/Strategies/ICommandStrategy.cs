using OopLab.Cli.Commands;
using OopLab.Cli.Parsing;

namespace OopLab.Cli.Strategies;

public interface ICommandStrategy
{
    string CategoryName { get; }
    IEnumerable<string> GetCommandSelectors();
    IEnumerable<Command> GetCommands();
    void RegisterCommand(Command command);
    Task<bool> ExecCommandAsync(string selector, Args args);
}
