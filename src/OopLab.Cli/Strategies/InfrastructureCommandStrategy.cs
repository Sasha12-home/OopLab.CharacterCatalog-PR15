using OopLab.Cli.Commands;
using OopLab.Cli.Graphics;
using OopLab.Cli.Rendering;
using OopLab.Core.Services;

namespace OopLab.Cli.Strategies;

public sealed class InfrastructureCommandStrategy : CommandStrategyBase
{
    public override string CategoryName => "Інфраструктурні дії (Постійність даних та буферизація портретів)";

    public InfrastructureCommandStrategy(
        CharacterCatalogService catalogService,
        ConsoleCharacterRenderer renderer)
    {
        RegisterCommand(new Command(
            selector: "sync",
            description: "Завантажити персонажів з API та зберегти їхні портрети у локальний буфер (опція --no-images).",
            usage: "sync [--limit=20] [--no-images]",
            handler: async args =>
            {
                int? limit = int.TryParse(args.GetOption("limit"), out int l) ? l : 20;
                bool downloadImages = !args.HasOption("no-images");
                int synced = 0;
                int newlyCached = 0;

                await CliGraphic.RunWithSpinnerAsync($"Синхронізація JSON та буферизація портретів із {catalogService.RemoteSourceName}...", async () =>
                {
                    (synced, newlyCached) = await catalogService.SyncWithRemoteAsync(limit, downloadImages);
                });

                CliGraphic.PrintSuccess($"Синхронізовано {synced} персонажів. Завантажено нових портретів у буфер: {newlyCached}.");
            }));

        RegisterCommand(new Command(
            selector: "import",
            description: "Імпортувати персонажа з API за іменем або ID та одразу закешувати його портрет.",
            usage: "import <ім'я або ID>",
            handler: async args =>
            {
                var query = args.GetPositional(0);
                if (string.IsNullOrWhiteSpace(query))
                {
                    CliGraphic.PrintError("Вкажіть ім'я або ID персонажа. Приклад: import \"Birdperson\" або import 47");
                    return;
                }

                Core.Models.CharacterProfile? imported = null;
                await CliGraphic.RunWithSpinnerAsync($"Завантаження JSON та портрета для '{query}'...", async () =>
                {
                    imported = await catalogService.ImportFromRemoteAsync(query, downloadPortrait: true);
                });

                if (imported is null)
                {
                    CliGraphic.PrintError($"Персонажа '{query}' не знайдено у API.");
                    return;
                }

                CliGraphic.PrintSuccess($"Персонажа '{imported.Name}' (ID: {imported.Id}) імпортовано, портрет збережено локально!");
                imported.Render(renderer);
            }));

        RegisterCommand(new Command(
            selector: "cache",
            description: "Буферизувати портрет персонажа на диск (cache <ID|ім'я> або cache --all [--force]).",
            usage: "cache <ID|ім'я> | cache --all",
            handler: async args =>
            {
                bool force = args.HasOption("force");
                if (args.HasOption("all"))
                {
                    int total = 0, downloaded = 0, fromCache = 0;
                    await CliGraphic.RunWithSpinnerAsync("Перевірка та буферизація всіх портретів каталогу...", async () =>
                    {
                        (total, downloaded, fromCache) = await catalogService.CacheAllMissingPortraitsAsync(force);
                    });
                    CliGraphic.PrintSuccess($"Оброблено {total} записів: завантажено з мережі — {downloaded}, взято з локального буфера — {fromCache}.");
                    return;
                }

                var target = args.GetPositional(0);
                if (string.IsNullOrWhiteSpace(target))
                {
                    CliGraphic.PrintError("Вкажіть ID/ім'я персонажа або прапорець --all. Приклад: cache 1 або cache --all");
                    return;
                }

                var entry = await catalogService.CachePortraitForCharacterAsync(target, force);
                var origin = entry.LoadedFromCache
                    ? "Взято миттєво з локального буфера (HTTP-запит не виконувався)"
                    : "Завантажено з сервера та записано в локальний буфер";

                CliGraphic.PrintSuccess($"{origin}: {entry.LocalFilePath} ({entry.FileSizeBytes / 1024.0:F1} КБ)");
            }));

        RegisterCommand(new Command(
            selector: "cache-info",
            description: "Показати шляхи збереження JSON-стану та статистику локального кешу зображень.",
            usage: "cache-info",
            handler: _ =>
            {
                var stats = catalogService.GetImageCacheStats();
                Console.WriteLine();
                CliGraphic.PrintBanner("СТАН ПІДСИСТЕМИ ПОСТІЙНОСТІ ТА БУФЕРИЗАЦІЇ");
                Console.WriteLine($"  JSON-файл стану каталогу:  {catalogService.StorageFilePath}");
                Console.WriteLine($"  Папка буфера портретів:    {stats.DirectoryPath}");
                Console.WriteLine($"  Закешовано зображень:      {stats.CachedFilesCount} файлів");
                Console.WriteLine($"  Загальний обсяг буфера:    {stats.TotalSizeBytes / 1024.0:F2} КБ");
                Console.WriteLine();
                return Task.CompletedTask;
            }));

        RegisterCommand(new Command(
            selector: "cache-clear",
            description: "Очистити локальний файловий буфер портретів.",
            usage: "cache-clear",
            handler: _ =>
            {
                int deleted = catalogService.ClearImageCache();
                CliGraphic.PrintInfo($"Локальний буфер портретів очищено. Видалено файлів: {deleted}.");
                return Task.CompletedTask;
            }));
    }
}
