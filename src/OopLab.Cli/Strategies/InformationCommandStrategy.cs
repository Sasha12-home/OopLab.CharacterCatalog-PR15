using OopLab.Cli.Commands;
using OopLab.Cli.Graphics;
using OopLab.Cli.Rendering;
using OopLab.Core.Services;
using OopLab.Core.Specifications;

namespace OopLab.Cli.Strategies;

public sealed class InformationCommandStrategy : CommandStrategyBase
{
    public override string CategoryName => "Інформаційні команди (Перегляд каталогу та статусу буфера)";

    public InformationCommandStrategy(
        CharacterCatalogService catalogService,
        ConsoleCharacterRenderer renderer,
        Func<IEnumerable<ICommandStrategy>> strategiesAccessor)
    {
        RegisterCommand(new Command(
            selector: "list",
            description: "Список персонажів (опції: --status=Alive --species=Alien --cached --sort=power).",
            usage: "list [--cached] [--sort=power]",
            handler: args =>
            {
                var criteria = new CharacterFilterCriteria
                {
                    Status = args.GetOption("status"),
                    Species = args.GetOption("species"),
                    Gender = args.GetOption("gender"),
                    SearchText = args.GetOption("search"),
                    SortBy = args.GetOption("sort"),
                    OnlyCachedPortraits = args.HasOption("cached") ? true : null
                };

                if (int.TryParse(args.GetOption("rarity"), out int r))
                    criteria.MinRarity = r;

                var viewModels = catalogService.GetAllViewModels(criteria);
                renderer.RenderCollection(viewModels);
                return Task.CompletedTask;
            }));

        RegisterCommand(new Command(
            selector: "info",
            description: "Показати ViewModel-картку персонажа з шляхом до локально закешованого портрета.",
            usage: "info <ім'я або ID>",
            handler: args =>
            {
                var query = args.GetPositional(0);
                if (string.IsNullOrWhiteSpace(query))
                {
                    CliGraphic.PrintError("Вкажіть ім'я або ID персонажа. Приклад: info 1 або info Rick");
                    return Task.CompletedTask;
                }

                var vm = catalogService.GetCharacterViewModel(query);
                if (vm is null)
                {
                    CliGraphic.PrintError($"Персонажа '{query}' не знайдено локально. Спробуйте 'import {query}' або 'sync'.");
                    return Task.CompletedTask;
                }

                renderer.Render(vm);
                return Task.CompletedTask;
            }));

        RegisterCommand(new Command(
            selector: "help",
            description: "Показати довідку за всіма зареєстрованими стратегіями команд.",
            usage: "help",
            handler: _ =>
            {
                Console.WriteLine();
                CliGraphic.PrintBanner("ДОВІДКА КОМАНДНОГО ІНТЕРФЕЙСУ (ПР 15 — ПОСТІЙНІСТЬ ТА БУФЕРИЗАЦІЯ)");
                foreach (var strategy in strategiesAccessor())
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n[{strategy.CategoryName}]");
                    Console.ResetColor();
                    foreach (var cmd in strategy.GetCommands())
                    {
                        Console.WriteLine($"  {cmd.Usage,-38} — {cmd.Description}");
                    }
                }
                Console.WriteLine($"  {"exit",-38} — Завершити роботу програми.\n");
                return Task.CompletedTask;
            }));
    }
}
