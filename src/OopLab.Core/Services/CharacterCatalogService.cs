using OopLab.Core.Abstractions;
using OopLab.Core.Mapping;
using OopLab.Core.Models;
using OopLab.Core.Specifications;
using OopLab.Core.ViewModels;

namespace OopLab.Core.Services;

public class CharacterCatalogService
{
    private readonly ICharacterRepository _repository;
    private readonly IRemoteCharacterProvider _remoteProvider;
    private readonly IImageCacheStorage _imageCache;
    private readonly CharacterViewModelMapper _viewModelMapper;

    public CharacterCatalogService(
        ICharacterRepository repository,
        IRemoteCharacterProvider remoteProvider,
        IImageCacheStorage imageCache,
        CharacterViewModelMapper? viewModelMapper = null)
    {
        _repository = repository;
        _remoteProvider = remoteProvider;
        _imageCache = imageCache;
        _viewModelMapper = viewModelMapper ?? new CharacterViewModelMapper();
    }

    public string RemoteSourceName => _remoteProvider.SourceName;
    public string StorageFilePath => _repository.StorageFilePath;
    public string ImageCacheDirectory => _imageCache.CacheDirectoryPath;

    public IReadOnlyList<CharacterProfile> GetAllCharacters()
    {
        var all = _repository.GetAll();
        foreach (var c in all)
        {
            if (string.IsNullOrWhiteSpace(c.LocalAvatarPath) || !File.Exists(c.LocalAvatarPath))
            {
                c.LocalAvatarPath = _imageCache.GetCachedFilePath(c.Id);
            }
        }
        return all;
    }

    public IReadOnlyList<CharacterProfile> QueryCharacters(CharacterFilterCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IEnumerable<CharacterProfile> query = GetAllCharacters().Where(criteria.IsSatisfiedBy);

        query = criteria.SortBy?.ToLowerInvariant() switch
        {
            "rarity" => query.OrderByDescending(c => c.Rarity).ThenBy(c => c.Name),
            "episodes" => query.OrderByDescending(c => c.EpisodeCount).ThenBy(c => c.Name),
            "power" => query.OrderByDescending(c => _viewModelMapper.ToViewModel(c).PopularityScore),
            _ => query.OrderBy(c => int.TryParse(c.Id, out int numId) ? numId : int.MaxValue).ThenBy(c => c.Name)
        };

        return query.ToList();
    }

    public CharacterProfile? FindCharacter(string idOrName)
    {
        var profile = _repository.GetByIdOrName(idOrName);
        if (profile is not null && (string.IsNullOrWhiteSpace(profile.LocalAvatarPath) || !File.Exists(profile.LocalAvatarPath)))
        {
            profile.LocalAvatarPath = _imageCache.GetCachedFilePath(profile.Id);
        }
        return profile;
    }

    public CharacterViewModel? GetCharacterViewModel(string idOrName)
    {
        var profile = FindCharacter(idOrName);
        return profile is null ? null : _viewModelMapper.ToViewModel(profile);
    }

    public IReadOnlyList<CharacterViewModel> GetAllViewModels(CharacterFilterCriteria? criteria = null)
    {
        var items = criteria is null ? GetAllCharacters() : QueryCharacters(criteria);
        return _viewModelMapper.ToViewModels(items);
    }

    public async Task<(int SyncedCount, int NewlyCachedImages)> SyncWithRemoteAsync(
        int? limit = null,
        bool downloadPortraits = true,
        CancellationToken cancellationToken = default)
    {
        var remote = await _remoteProvider.FetchRemoteCharactersAsync(limit, cancellationToken);
        int newlyCached = 0;

        if (downloadPortraits)
        {
            foreach (var character in remote)
            {
                if (!string.IsNullOrWhiteSpace(character.AvatarUrl))
                {
                    var entry = await _imageCache.GetOrDownloadPortraitAsync(
                        character.Id,
                        character.AvatarUrl,
                        forceRefresh: false,
                        cancellationToken);

                    character.LocalAvatarPath = entry.LocalFilePath;
                    if (!entry.LoadedFromCache)
                        newlyCached++;
                }
            }
        }

        _repository.UpsertMany(remote);
        _repository.SaveChanges();
        return (remote.Count, newlyCached);
    }

    public async Task<CharacterProfile?> ImportFromRemoteAsync(
        string idOrName,
        bool downloadPortrait = true,
        CancellationToken cancellationToken = default)
    {
        var character = await _remoteProvider.FetchCharacterByIdOrNameAsync(idOrName, cancellationToken);
        if (character is null) return null;

        if (downloadPortrait && !string.IsNullOrWhiteSpace(character.AvatarUrl))
        {
            var entry = await _imageCache.GetOrDownloadPortraitAsync(
                character.Id,
                character.AvatarUrl,
                forceRefresh: false,
                cancellationToken);
            character.LocalAvatarPath = entry.LocalFilePath;
        }

        _repository.UpsertMany(new[] { character });
        _repository.SaveChanges();
        return character;
    }

    public async Task<ImageCacheEntry> CachePortraitForCharacterAsync(
        string idOrName,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        var character = FindCharacter(idOrName)
                        ?? throw new KeyNotFoundException($"Персонажа '{idOrName}' не знайдено у каталозі.");

        if (string.IsNullOrWhiteSpace(character.AvatarUrl))
            throw new InvalidOperationException($"У персонажа '{character.Name}' не вказано URL портрета.");

        var entry = await _imageCache.GetOrDownloadPortraitAsync(
            character.Id,
            character.AvatarUrl,
            forceRefresh,
            cancellationToken);

        character.LocalAvatarPath = entry.LocalFilePath;
        _repository.SaveChanges();
        return entry;
    }

    public async Task<(int TotalCharacters, int DownloadedNow, int AlreadyInCache)> CacheAllMissingPortraitsAsync(
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        var characters = GetAllCharacters();
        int downloadedNow = 0;
        int alreadyInCache = 0;

        foreach (var c in characters)
        {
            if (string.IsNullOrWhiteSpace(c.AvatarUrl)) continue;

            var entry = await _imageCache.GetOrDownloadPortraitAsync(
                c.Id,
                c.AvatarUrl,
                forceRefresh,
                cancellationToken);

            c.LocalAvatarPath = entry.LocalFilePath;
            if (entry.LoadedFromCache) alreadyInCache++;
            else downloadedNow++;
        }

        _repository.SaveChanges();
        return (characters.Count, downloadedNow, alreadyInCache);
    }

    public ImageCacheStats GetImageCacheStats() => _imageCache.GetCacheStats();

    public int ClearImageCache()
    {
        int deleted = _imageCache.ClearCache();
        foreach (var c in _repository.GetAll())
        {
            c.LocalAvatarPath = null;
        }
        _repository.SaveChanges();
        return deleted;
    }

    public async Task<CharacterProfile> CreateCustomCharacterAsync(
        string name,
        string status,
        string species,
        string gender,
        string originWorld,
        int rarity,
        int baseHp,
        int baseAtk,
        string description,
        string avatarUrl,
        CancellationToken cancellationToken = default)
    {
        var slug = name.Trim().ToLowerInvariant().Replace(' ', '-');
        var resolvedUrl = string.IsNullOrWhiteSpace(avatarUrl)
            ? "https://rickandmortyapi.com/api/character/avatar/19.jpeg"
            : avatarUrl.Trim();

        var character = new CharacterProfile
        {
            Id = $"custom-{slug}",
            Name = name.Trim(),
            Status = string.IsNullOrWhiteSpace(status) ? "Alive" : status.Trim(),
            Species = string.IsNullOrWhiteSpace(species) ? "Human" : species.Trim(),
            Gender = string.IsNullOrWhiteSpace(gender) ? "Unknown" : gender.Trim(),
            OriginWorld = string.IsNullOrWhiteSpace(originWorld) ? "Earth (C-137)" : originWorld.Trim(),
            CurrentLocation = string.IsNullOrWhiteSpace(originWorld) ? "Earth (C-137)" : originWorld.Trim(),
            EpisodeCount = rarity * 8,
            Rarity = Math.Clamp(rarity, 1, 5),
            BaseHp = Math.Max(100, baseHp),
            BaseAtk = Math.Max(10, baseAtk),
            Description = description.Trim(),
            AvatarUrl = resolvedUrl,
            IsCustom = true
        };

        try
        {
            var entry = await _imageCache.GetOrDownloadPortraitAsync(character.Id, resolvedUrl, false, cancellationToken);
            character.LocalAvatarPath = entry.LocalFilePath;
        }
        catch
        {
        }

        _repository.Add(character);
        _repository.SaveChanges();
        return character;
    }
}
