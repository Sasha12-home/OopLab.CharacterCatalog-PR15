using System.Text.Json;
using OopLab.Core.Abstractions;
using OopLab.Core.Models;

namespace OopLab.Data.Persistence;

public class JsonCharacterRepository : ICharacterRepository
{
    private readonly string _filePath;
    private readonly List<CharacterProfile> _items = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string StorageFilePath => _filePath;

    public JsonCharacterRepository(string filePath)
    {
        _filePath = filePath;
        LoadFromDisk();
        if (_items.Count == 0)
        {
            SeedDefaults();
            SaveChanges();
        }
    }

    public IReadOnlyList<CharacterProfile> GetAll() => _items.AsReadOnly();

    public CharacterProfile? GetByIdOrName(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return null;
        var q = query.Trim();
        return _items.FirstOrDefault(c =>
            c.Id.Equals(q, StringComparison.OrdinalIgnoreCase) ||
            c.Name.Equals(q, StringComparison.OrdinalIgnoreCase))
            ?? _items.FirstOrDefault(c => c.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(CharacterProfile character)
    {
        if (_items.Any(c => c.Name.Equals(character.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Персонаж з іменем '{character.Name}' вже існує у каталозі.");
        _items.Add(character);
    }

    public void UpsertMany(IEnumerable<CharacterProfile> characters)
    {
        foreach (var incoming in characters)
        {
            var existing = _items.FirstOrDefault(c =>
                c.Id.Equals(incoming.Id, StringComparison.OrdinalIgnoreCase) ||
                c.Name.Equals(incoming.Name, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                _items.Add(incoming);
            }
            else if (!existing.IsCustom)
            {
                existing.Id = incoming.Id;
                existing.Name = incoming.Name;
                existing.Status = incoming.Status;
                existing.Species = incoming.Species;
                existing.SubType = incoming.SubType;
                existing.Gender = incoming.Gender;
                existing.OriginWorld = incoming.OriginWorld;
                existing.CurrentLocation = incoming.CurrentLocation;
                existing.EpisodeCount = incoming.EpisodeCount;
                existing.Rarity = incoming.Rarity;
                existing.BaseHp = incoming.BaseHp;
                existing.BaseAtk = incoming.BaseAtk;
                existing.Description = incoming.Description;
                existing.AvatarUrl = incoming.AvatarUrl;
                if (!string.IsNullOrWhiteSpace(incoming.LocalAvatarPath))
                    existing.LocalAvatarPath = incoming.LocalAvatarPath;
            }
        }
    }

    public void SaveChanges()
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(_items, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(_filePath)) return;
        var json = File.ReadAllText(_filePath);
        var loaded = JsonSerializer.Deserialize<List<CharacterProfile>>(json, JsonOptions);
        if (loaded is not null)
            _items.AddRange(loaded);
    }

    private void SeedDefaults()
    {
        _items.AddRange(new[]
        {
            new CharacterProfile
            {
                Id = "1",
                Name = "Rick Sanchez",
                Status = "Alive",
                Species = "Human",
                Gender = "Male",
                OriginWorld = "Earth (C-137)",
                CurrentLocation = "Citadel of Ricks",
                EpisodeCount = 51,
                Rarity = 5,
                BaseHp = 14275,
                BaseAtk = 452,
                Description = "Геніальний вчений та винахідник портальної гармати зі всесвіту C-137.",
                AvatarUrl = "https://rickandmortyapi.com/api/character/avatar/1.jpeg",
                IsCustom = false
            },
            new CharacterProfile
            {
                Id = "2",
                Name = "Morty Smith",
                Status = "Alive",
                Species = "Human",
                Gender = "Male",
                OriginWorld = "unknown",
                CurrentLocation = "Earth (Replacement Dimension)",
                EpisodeCount = 51,
                Rarity = 5,
                BaseHp = 14275,
                BaseAtk = 452,
                Description = "Онук Ріка Санчеза та його постійний напарник у міжвимірних подорожах.",
                AvatarUrl = "https://rickandmortyapi.com/api/character/avatar/2.jpeg",
                IsCustom = false
            }
        });
    }
}
