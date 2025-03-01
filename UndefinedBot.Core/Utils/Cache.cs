
using UndefinedBot.Core.Utils.Logging;

namespace UndefinedBot.Core.Utils;

public sealed class CacheManager(string pluginName,ILogger logger) : IDisposable
{
    private readonly Dictionary<string, StorageCacheWrapper> _storageCache = [];
    private readonly Dictionary<string, FileCacheWrapper> _fileCache = [];
    private readonly string _cacheRootPath = Path.Join(Environment.CurrentDirectory, "Cache", pluginName);
    private readonly ILogger _logger = logger.Extend("Cache");

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
                new FileCacheWrapper(fullPath, DateTime.Now + cacheDuration);
            return fullPath;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Cache Create Failed");
            return "";
        }
    }
    public bool AddStorage(string cacheName, object cacheContent, long cacheDuration)=>
        AddStorage(cacheName, cacheContent, TimeSpan.FromSeconds(cacheDuration));
    public bool AddStorage(string cacheName, object cacheContent, TimeSpan cacheDuration)
    {
        try
        {
            _storageCache[cacheName] = new StorageCacheWrapper(cacheContent, DateTime.Now + cacheDuration);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Cache Create Failed");
            return false;
        }
    }

    public T? ModifyStorage<T>(string cacheName, T newContent) where T : notnull
    {
        try
        {
            StorageCacheWrapper sp = _storageCache[cacheName];
            sp.Content = newContent;
            return newContent;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Cache Modify Failed");
            return default;
        }
    }

    public string? GetFile(string cacheName)
    {
        return _fileCache.TryGetValue(cacheName, out FileCacheWrapper? fp) ? fp.FilePath : null;
    }

    public T? GetStorage<T>(string cacheName) where T : notnull
    {
        if (!_storageCache.TryGetValue(cacheName, out StorageCacheWrapper? cp)) return default;
        return cp.Content is T content ? content : throw new Exception("Incorrect Cache Type");
    }

    public void Dispose()
    {
        _fileCache.Clear();
        _storageCache.Clear();
    }
}

public sealed class StorageCacheWrapper(object content, DateTime expiredTime)
{
    public object Content = content;
    public readonly DateTime ExpiredTime = expiredTime;
}

public sealed class FileCacheWrapper(string path, DateTime expiredTime)
{
    public readonly string FilePath = path;
    public readonly DateTime ExpiredTime = expiredTime;
}