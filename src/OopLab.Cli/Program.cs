using OopLab.Cli;
using OopLab.Cli.Graphics;
using OopLab.Cli.Rendering;
using OopLab.Cli.Strategies;
using OopLab.Core.Services;
using OopLab.Data.Caching;
using OopLab.Data.Persistence;
using OopLab.Data.Remote;
using OopLab.Data.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

var catalogJsonPath = AppStoragePaths.GetCatalogJsonFilePath();
var portraitsCacheDir = AppStoragePaths.GetPortraitsCacheDirectory();

var repository = new JsonCharacterRepository(catalogJsonPath);
var imageCache = new DiskImageCacheStorage(portraitsCacheDir);
var remoteProvider = new RickAndMortyApiClient();
var catalogService = new CharacterCatalogService(repository, remoteProvider, imageCache);

var renderer = new ConsoleCharacterRenderer();
var cli = new CliFacade();

cli.UseStrategy(new InformationCommandStrategy(catalogService, renderer, () => cli.Strategies))
   .UseStrategy(new TransformativeCommandStrategy(catalogService, renderer))
   .UseStrategy(new InfrastructureCommandStrategy(catalogService, renderer));

if (args.Length > 0)
{
    var singleCommand = string.Join(' ', args.Select(a => a.Contains(' ') ? $"\"{a}\"" : a));
    await cli.ExecCommandAsync(singleCommand);
    return;
}

CliGraphic.PrintBanner("КАТАЛОГ ПЕРСОНАЖІВ — ПОСТІЙНІСТЬ ТА БУФЕРИЗАЦІЯ (ПР 15)");
Console.WriteLine($"Сховище стану:   {catalogJsonPath}");
Console.WriteLine($"Буфер портретів: {portraitsCacheDir}");
Console.WriteLine("Введіть 'help' для списку команд (напр. 'cache-info', 'sync --limit=10', 'cache 1', 'info 1').\n");

bool isRunning = true;
while (isRunning)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("catalog> ");
    Console.ResetColor();

    var input = Console.ReadLine();
    if (input is null) break;

    isRunning = await cli.ExecCommandAsync(input);
}
