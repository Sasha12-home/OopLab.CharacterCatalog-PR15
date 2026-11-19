namespace OopLab.Core.Abstractions;

public interface IImageCacheStorage
{
    string CacheDirectoryPath { get; }
    bool IsCached(string characterId);
    string? GetCachedFilePath(string characterId);
    Task<ImageCacheEntry> GetOrDownloadPortraitAsync(
        string characterId,
        string remoteUrl,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default);
    ImageCacheStats GetCacheStats();
    int ClearCache();
}

public sealed record ImageCacheEntry(
    string CharacterId,
    string LocalFilePath,
    long FileSizeBytes,
    bool LoadedFromCache);

public sealed record ImageCacheStats(
    string DirectoryPath,
    int CachedFilesCount,
    long TotalSizeBytes);
