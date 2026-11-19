using OopLab.Cli.Graphics;
using OopLab.Core.Mapping;
using OopLab.Core.Models;
using OopLab.Core.Rendering;
using OopLab.Core.ViewModels;

namespace OopLab.Cli.Rendering;

public class ConsoleCharacterRenderer : IRenderer<CharacterProfile>, IRenderer<CharacterViewModel>
{
    private readonly CharacterViewModelMapper _mapper = new();

    public void Render(CharacterProfile item)
    {
        var vm = _mapper.ToViewModel(item);
        Render(vm);
    }

    public void RenderCollection(IEnumerable<CharacterProfile> items)
    {
        var vms = _mapper.ToViewModels(items);
        RenderCollection(vms);
    }

    public void Render(CharacterViewModel vm)
    {
        Console.WriteLine();
        CliGraphic.PrintBanner($"VIEWMODEL КАРТКА: {vm.DisplayName.ToUpperInvariant()} [{vm.TierRank}]");
        Console.WriteLine($"  ID:                 {vm.Id}");
        Console.WriteLine($"  Ім'я / Опис:        {vm.DisplayName} — {vm.Subtitle}");
        Console.WriteLine($"  Статус (UI):        {vm.StatusBadge} (Колір: {vm.StatusColorHex})");
        Console.WriteLine($"  Вид / Стать:        {vm.SpeciesLabel} | {vm.GenderLabel}");
        Console.WriteLine($"  Світ / Локація:     {vm.OriginAndLocation}");
        Console.WriteLine($"  Характеристики:     {vm.StatsSummary}");
        Console.WriteLine($"  Джерело:            {vm.SourceBadge}");
        Console.WriteLine($"  Біографія:          {vm.Biography}");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"  Remote Avatar URL:  {vm.AvatarImageUrl}");
        Console.ResetColor();

        if (vm.IsPortraitCached && !string.IsNullOrWhiteSpace(vm.LocalCachedImagePath))
        {
            var sizeKb = new FileInfo(vm.LocalCachedImagePath).Length / 1024.0;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Локальний буфер:    [ЗАКЕШОВАНО НА ДИСКУ — {sizeKb:F1} КБ]");
            Console.WriteLine($"  Локальний файл:     {vm.LocalCachedImagePath}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  Локальний буфер:    [НЕ ЗАКЕШОВАНО] (введіть: cache {vm.Id})");
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    public void RenderCollection(IEnumerable<CharacterViewModel> items)
    {
        var list = items.ToList();
        if (list.Count == 0)
        {
            CliGraphic.PrintInfo("За заданими критеріями персонажів не знайдено.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("┌────────────┬──────────────────────┬──────────┬──────────────┬───────┬──────────────┐");
        Console.WriteLine("│ ID         │ Ім'я                 │ Статус   │ Вид          │ Ранг  │ Буфер фото   │");
        Console.WriteLine("├────────────┼──────────────────────┼──────────┼──────────────┼───────┼──────────────┤");
        foreach (var vm in list)
        {
            var idShort = vm.Id.Length > 10 ? vm.Id[..10] : vm.Id;
            var nameShort = vm.DisplayName.Length > 20 ? vm.DisplayName[..20] : vm.DisplayName;
            var specShort = vm.SpeciesLabel.Length > 12 ? vm.SpeciesLabel[..12] : vm.SpeciesLabel;
            var cacheLabel = vm.IsPortraitCached ? "[OK] На диску" : "[--] Тільки URL";
            Console.WriteLine($"│ {idShort,-10} │ {nameShort,-20} │ {vm.StatusBadge,-8} │ {specShort,-12} │ {vm.TierRank,-5} │ {cacheLabel,-12} │");
        }
        Console.WriteLine("└────────────┴──────────────────────┴──────────┴──────────────┴───────┴──────────────┘");
        Console.WriteLine($"Всього записів: {list.Count}. Деталі ViewModel: info <ім'я або ID>");
        Console.WriteLine();
    }
}
