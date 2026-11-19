namespace OopLab.Data.Storage;

public static class AppStoragePaths
{
    public const string AppFolderName = "OopLab.CharacterCatalog";

    public static string GetBaseAppDataDirectory()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
        {
            localAppData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local",
                "share");
        }

        var fullDir = Path.Combine(localAppData, AppFolderName);
        Directory.CreateDirectory(fullDir);
        return fullDir;
    }

    public static string GetCatalogJsonFilePath()
        => Path.Combine(GetBaseAppDataDirectory(), "catalog_state.json");

    public static string GetPortraitsCacheDirectory()
    {
        var cacheDir = Path.Combine(GetBaseAppDataDirectory(), "portraits_cache");
        Directory.CreateDirectory(cacheDir);
        return cacheDir;
    }
}
