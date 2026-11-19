using OopLab.Cli.Graphics;
using OopLab.Cli.Parsing;
using OopLab.Cli.Strategies;
using OopLab.Core.Rendering;

namespace OopLab.Cli;

public class CliFacade
{
    private readonly ArgParser _argParser = new();
    private readonly List<ICommandStrategy> _strategies = new();

    public IReadOnlyList<ICommandStrategy> Strategies => _strategies.AsReadOnly();

    public CliFacade UseStrategy(ICommandStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        _strategies.Add(strategy);
        return this;
    }

    public void Display<T>(IRenderer<T> renderer, T data)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.Render(data);
    }

    public void Display<T>(IRenderable<T> renderable)
    {
        ArgumentNullException.ThrowIfNull(renderable);
        renderable.Render();
    }

    public async Task<bool> ExecCommandAsync(string rawCommand)
    {
        var args = _argParser.ParseArgs(rawCommand);
        if (string.IsNullOrEmpty(args.CommandName))
            return true;

        if (args.CommandName is "exit" or "quit")
            return false;

        try
        {
            foreach (var strategy in _strategies)
            {
                if (await strategy.ExecCommandAsync(args.CommandName, args))
                    return true;
            }

            CliGraphic.PrintError($"Невідома команда '{args.CommandName}'. Введіть 'help' для перегляду доступних команд.");
        }
        catch (Exception ex)
        {
            CliGraphic.PrintError(ex.Message);
        }

        return true;
    }
}
