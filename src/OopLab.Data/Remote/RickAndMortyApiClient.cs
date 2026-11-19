using OopLab.Core.Abstractions;
using OopLab.Core.Models;
using OopLab.Core.Parsing;

namespace OopLab.Data.Remote;

public class RickAndMortyApiClient : IRemoteCharacterProvider
{
    private readonly HttpClient _httpClient;
    private readonly RickAndMortyApiJsonParser _parser;

    public string SourceName => "https://rickandmortyapi.com/api/character";

    public RickAndMortyApiClient(HttpClient? httpClient = null, RickAndMortyApiJsonParser? parser = null)
    {
        _httpClient = httpClient ?? new HttpClient
        {
            BaseAddress = new Uri("https://rickandmortyapi.com/api/"),
            Timeout = TimeSpan.FromSeconds(10)
        };
        _parser = parser ?? new RickAndMortyApiJsonParser();
    }

    public async Task<IReadOnlyList<CharacterProfile>> FetchRemoteCharactersAsync(
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pageJson = await _httpClient.GetStringAsync("character?page=1", cancellationToken);
            var parsed = _parser.ParseCharacterListPage(pageJson);

            return limit.HasValue && limit.Value > 0
                ? parsed.Take(limit.Value).ToList()
                : parsed;
        }
        catch (Exception)
        {
            var fallback = ParseOfflineFallbackPage();
            return limit.HasValue && limit.Value > 0
                ? fallback.Take(limit.Value).ToList()
                : fallback;
        }
    }

    public async Task<CharacterProfile?> FetchCharacterByIdOrNameAsync(string idOrName, CancellationToken cancellationToken = default)
    {
        var query = idOrName.Trim();
        try
        {
            if (int.TryParse(query, out int numericId))
            {
                var json = await _httpClient.GetStringAsync($"character/{numericId}", cancellationToken);
                return _parser.ParseSingleCharacter(json);
            }
            else
            {
                var searchJson = await _httpClient.GetStringAsync($"character/?name={Uri.EscapeDataString(query)}", cancellationToken);
                var matches = _parser.ParseCharacterListPage(searchJson);
                return matches.FirstOrDefault();
            }
        }
        catch (Exception)
        {
            var fallback = ParseOfflineFallbackPage();
            return fallback.FirstOrDefault(c =>
                c.Id.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
        }
    }

    private IReadOnlyList<CharacterProfile> ParseOfflineFallbackPage()
    {
        const string samplePageJson = """
        {
          "results": [
            {
              "id": 1,
              "name": "Rick Sanchez",
              "status": "Alive",
              "species": "Human",
              "type": "",
              "gender": "Male",
              "origin": { "name": "Earth (C-137)" },
              "location": { "name": "Citadel of Ricks" },
              "image": "https://rickandmortyapi.com/api/character/avatar/1.jpeg",
              "episode": ["1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","17","18","19","20","21","22","23","24","25","26","27","28","29","30","31","32","33","34","35","36","37","38","39","40","41","42","43","44","45","46","47","48","49","50","51"]
            },
            {
              "id": 2,
              "name": "Morty Smith",
              "status": "Alive",
              "species": "Human",
              "type": "",
              "gender": "Male",
              "origin": { "name": "unknown" },
              "location": { "name": "Earth (Replacement Dimension)" },
              "image": "https://rickandmortyapi.com/api/character/avatar/2.jpeg",
              "episode": ["1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","17","18","19","20","21","22","23","24","25","26","27","28","29","30","31","32","33","34","35","36","37","38","39","40","41","42","43","44","45","46","47","48","49","50","51"]
            },
            {
              "id": 3,
              "name": "Summer Smith",
              "status": "Alive",
              "species": "Human",
              "type": "",
              "gender": "Female",
              "origin": { "name": "Earth (Replacement Dimension)" },
              "location": { "name": "Earth (Replacement Dimension)" },
              "image": "https://rickandmortyapi.com/api/character/avatar/3.jpeg",
              "episode": ["6","7","8","9","10","11","12","14","15","16","17","18","19","20","21","22","23","24","25","26","27","29","30","31","32","33","34","35","36","38","39","40","41","42","43","44","45","46","47","48","49","50","51"]
            }
          ]
        }
        """;

        return _parser.ParseCharacterListPage(samplePageJson);
    }
}
