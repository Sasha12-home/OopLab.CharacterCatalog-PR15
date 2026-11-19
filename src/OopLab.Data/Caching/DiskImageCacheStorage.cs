using OopLab.Core.Abstractions;

namespace OopLab.Data.Caching;

public sealed class DiskImageCacheStorage : IImageCacheStorage
{
    private readonly HttpClient _httpClient;

    public string CacheDirectoryPath { get; }

    public DiskImageCacheStorage(string cacheDirectoryPath, HttpClient? httpClient = null)
    {
        CacheDirectoryPath = cacheDirectoryPath;
        Directory.CreateDirectory(CacheDirectoryPath);

        _httpClient = httpClient ?? new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    public bool IsCached(string characterId) => GetCachedFilePath(characterId) is not null;

    public string? GetCachedFilePath(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId)) return null;
        var safeName = SanitizeFileName(characterId);

        foreach (var ext in new[] { ".jpeg", ".jpg", ".png", ".webp" })
        {
            var candidate = Path.Combine(CacheDirectoryPath, safeName + ext);
            if (File.Exists(candidate) && new FileInfo(candidate).Length > 0)
                return candidate;
        }

        return null;
    }

    public async Task<ImageCacheEntry> GetOrDownloadPortraitAsync(
        string characterId,
        string remoteUrl,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(characterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteUrl);

        if (!forceRefresh)
        {
            var existingPath = GetCachedFilePath(characterId);
            if (existingPath is not null)
            {
                var info = new FileInfo(existingPath);
                return new ImageCacheEntry(
                    CharacterId: characterId,
                    LocalFilePath: existingPath,
                    FileSizeBytes: info.Length,
                    LoadedFromCache: true);
            }
        }

        var ext = ResolveExtensionFromUrl(remoteUrl);
        var targetFilePath = Path.Combine(CacheDirectoryPath, SanitizeFileName(characterId) + ext);

        byte[] imageBytes;
        try
        {
            imageBytes = await _httpClient.GetByteArrayAsync(remoteUrl, cancellationToken);
        }
        catch (Exception)
        {
            targetFilePath = Path.Combine(CacheDirectoryPath, SanitizeFileName(characterId) + ".png");
            imageBytes = CreateMinimalFallbackPngBytes();
        }

        await File.WriteAllBytesAsync(targetFilePath, imageBytes, cancellationToken);

        return new ImageCacheEntry(
            CharacterId: characterId,
            LocalFilePath: targetFilePath,
            FileSizeBytes: imageBytes.LongLength,
            LoadedFromCache: false);
    }

    public ImageCacheStats GetCacheStats()
    {
        if (!Directory.Exists(CacheDirectoryPath))
            return new ImageCacheStats(CacheDirectoryPath, 0, 0);

        var files = Directory.GetFiles(CacheDirectoryPath);
        long totalBytes = files.Sum(f => new FileInfo(f).Length);
        return new ImageCacheStats(CacheDirectoryPath, files.Length, totalBytes);
    }

    public int ClearCache()
    {
        if (!Directory.Exists(CacheDirectoryPath)) return 0;
        var files = Directory.GetFiles(CacheDirectoryPath);
        foreach (var f in files)
        {
            File.Delete(f);
        }
        return files.Length;
    }

    private static string ResolveExtensionFromUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var ext = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();
            if (ext is ".jpeg" or ".jpg" or ".png" or ".webp")
                return ext;
        }
        return ".jpeg";
    }

    private static string SanitizeFileName(string raw)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(raw.Trim().ToLowerInvariant().Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "unknown" : cleaned;
    }

    private static byte[] CreateMinimalFallbackPngBytes()
    {
        return Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");
    }
}
