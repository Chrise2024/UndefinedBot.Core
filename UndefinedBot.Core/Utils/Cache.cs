
namespace UndefinedBot.Core.Utils;

public sealed class CacheManager : IDisposable
{
    private readonly Dictionary<string, StorageCacheProperty> _storageCache = [];
    private readonly Dictionary<string, FileCacheProperty> _fileCache = [];
    private readonly FixedLogger _cacheLogger;
    private readonly string _cacheRootPath;

    public CacheManager(string pluginName)
    {
        _cacheLogger = new (["Plugin", pluginName, "Cache"]);
        _cacheRootPath = Path.Join(Environment.CurrentDirectory, "Cache", pluginName);
    }

    public void UpdateCache()
    {
        DateTime curTime = DateTime.Now;
        foreach (var pair in _storageCache.Where(pair => pair.Value.ExpiredTime < curTime))
        {
            _storageCache.Remove(pair.Key);
        }

        foreach (var pair in _fileCache.Where(pair => pair.Value.ExpiredTime < curTime))
        {
            FileIO.SafeDeleteFile(pair.Value.FilePath);
            _fileCache.Remove(pair.Key);
        }
    }

    public string AddFile(string cacheName, string cachePath, long cacheDuration) =>
        AddFile(cacheName, cachePath, TimeSpan.FromSeconds(cacheDuration));
    public string AddFile(string cacheName, string cachePath, TimeSpan cacheDuration)
    {
        try
        {
            string fullPath = Path.Join(_cacheRootPath, cachePath);
            _fileCache[cacheName] =
                new FileCacheProperty(fullPath, DateTime.Now + cacheDuration);
            return fullPath;
        }
        catch (Exception ex)
        {
            _cacheLogger.Error(ex, "Cache Create Failed");
            return "";
        }
    }
    public bool AddStorage(string cacheName, object cacheContent, long cacheDuration)=>
        AddStorage(cacheName, cacheContent, TimeSpan.FromSeconds(cacheDuration));
    public bool AddStorage(string cacheName, object cacheContent, TimeSpan cacheDuration)
    {
        try
        {
            _storageCache[cacheName] = new StorageCacheProperty(cacheContent, DateTime.Now + cacheDuration);
            return true;
        }
        catch (Exception ex)
        {
            _cacheLogger.Error(ex, "Cache Create Failed");
            return false;
        }
    }

    public T? ModifyStorage<T>(string cacheName, T newContent) where T : notnull
    {
        try
        {
            StorageCacheProperty sp = _storageCache[cacheName];
            sp.Content = newContent;
            return newContent;
        }
        catch (Exception ex)
        {
            _cacheLogger.Error(ex, "Cache Modify Failed");
            return default;
        }
    }

    public string? GetFile(string cacheName)
    {
        return _fileCache.TryGetValue(cacheName, out FileCacheProperty? fp) ? fp.FilePath : null;
    }

    public T? GetStorage<T>(string cacheName) where T : notnull
    {
        if (!_storageCache.TryGetValue(cacheName, out StorageCacheProperty? cp)) return default;
        return cp.Content is T content ? content : throw new Exception("Incorrect Cache Type");
    }

    public void Dispose()
    {
        _fileCache.Clear();
        _storageCache.Clear();
        _cacheLogger.Dispose();
    }
}

public sealed class StorageCacheProperty(object content, DateTime expiredTime)
{
    public object Content = content;
    public readonly DateTime ExpiredTime = expiredTime;
}

public sealed class FileCacheProperty(string path, DateTime expiredTime)
{
    public readonly string FilePath = path;
    public readonly DateTime ExpiredTime = expiredTime;
}

public enum CacheType
{
    File = 0,
    Storage = 1,
}