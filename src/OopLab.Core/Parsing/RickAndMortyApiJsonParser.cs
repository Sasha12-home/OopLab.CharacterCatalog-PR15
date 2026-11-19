using System.Text.Json;
using System.Text.Json.Serialization;
using OopLab.Core.Models;

namespace OopLab.Core.Parsing;

public sealed class RickAndMortyApiJsonParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyList<CharacterProfile> ParseCharacterListPage(string jsonPage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPage);

        var pageDto = JsonSerializer.Deserialize<RmPageResponseDto>(jsonPage, SerializerOptions)
                      ?? throw new JsonException("Не вдалося розпарсити сторінку персонажів Rick and Morty API.");

        if (pageDto.Results is null)
            return Array.Empty<CharacterProfile>();

        return pageDto.Results.Select(MapDtoToProfile).ToList();
    }

    public CharacterProfile ParseSingleCharacter(string jsonObject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonObject);

        var dto = JsonSerializer.Deserialize<RmCharacterDto>(jsonObject, SerializerOptions)
                  ?? throw new JsonException("Не вдалося розпарсити об'єкт персонажа Rick and Morty API.");

        return MapDtoToProfile(dto);
    }

    private static CharacterProfile MapDtoToProfile(RmCharacterDto dto)
    {
        int episodes = dto.Episode?.Count ?? 1;
        int rarity = episodes switch
        {
            >= 40 => 5,
            >= 15 => 4,
            >= 5 => 3,
            >= 2 => 2,
            _ => 1
        };

        var (hp, atk) = EstimateStats(dto.Species ?? "Human", rarity, episodes);
        string originName = string.IsNullOrWhiteSpace(dto.Origin?.Name) ? "Unknown" : dto.Origin.Name.Trim();
        string locationName = string.IsNullOrWhiteSpace(dto.Location?.Name) ? "Unknown" : dto.Location.Name.Trim();

        return new CharacterProfile
        {
            Id = dto.Id.ToString(),
            Name = string.IsNullOrWhiteSpace(dto.Name) ? $"Character #{dto.Id}" : dto.Name.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "unknown" : dto.Status.Trim(),
            Species = string.IsNullOrWhiteSpace(dto.Species) ? "Human" : dto.Species.Trim(),
            SubType = dto.Type?.Trim() ?? string.Empty,
            Gender = string.IsNullOrWhiteSpace(dto.Gender) ? "unknown" : dto.Gender.Trim(),
            OriginWorld = originName,
            CurrentLocation = locationName,
            EpisodeCount = episodes,
            Rarity = rarity,
            BaseHp = hp,
            BaseAtk = atk,
            Description = $"Персонаж всесвіту Rick and Morty ({dto.Species}, статус: {dto.Status}). Вперше з'явився у світі '{originName}', остання локація — '{locationName}'. Кількість епізодів: {episodes}.",
            AvatarUrl = string.IsNullOrWhiteSpace(dto.Image)
                ? $"https://rickandmortyapi.com/api/character/avatar/{dto.Id}.jpeg"
                : dto.Image.Trim(),
            IsCustom = false
        };
    }

    private static (int Hp, int Atk) EstimateStats(string species, int rarity, int episodes)
    {
        int baseHp = 9500 + rarity * 700 + episodes * 25;
        int baseAtk = 200 + rarity * 30 + episodes * 2;

        if (species.Contains("Robot", StringComparison.OrdinalIgnoreCase) ||
            species.Contains("Alien", StringComparison.OrdinalIgnoreCase))
        {
            baseHp += 800;
            baseAtk += 25;
        }

        return (baseHp, baseAtk);
    }

    private sealed class RmPageResponseDto
    {
        [JsonPropertyName("results")]
        public List<RmCharacterDto>? Results { get; set; }
    }

    private sealed class RmCharacterDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("species")]
        public string? Species { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("origin")]
        public RmNamedRefDto? Origin { get; set; }

        [JsonPropertyName("location")]
        public RmNamedRefDto? Location { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("episode")]
        public List<string>? Episode { get; set; }
    }

    private sealed class RmNamedRefDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
