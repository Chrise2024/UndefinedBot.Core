﻿namespace UndefinedBot.Core.Utils;

public sealed class CacheManager(string pluginName, ILogger logger) : IDisposable
{
    private readonly Dictionary<string, StorageCacheWrapper> _storageCache = [];
    private readonly Dictionary<string, FileCacheWrapper> _fileCache = [];
    private readonly string _cacheRootPath = Path.Join(Environment.CurrentDirectory, "Cache", pluginName);
    private readonly ILogger _logger = logger.Extend("Cache");

    public void UpdateCache()
    {
        DateTime curTime = DateTime.Now;
        foreach (KeyValuePair<string, StorageCacheWrapper> pair in _storageCache.Where(pair =>
                     pair.Value.ExpiredTime < curTime)) _storageCache.Remove(pair.Key);

        foreach (KeyValuePair<string, FileCacheWrapper> pair in _fileCache.Where(pair =>
                     pair.Value.ExpiredTime < curTime))
        {
            FileIO.SafeDeleteFile(pair.Value.FilePath);
            _fileCache.Remove(pair.Key);
        }
    }

    public string AddFile(string cacheName, string cachePath, long cacheDuration)
    {
        return AddFile(cacheName, cachePath, TimeSpan.FromSeconds(cacheDuration));
    }

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
            _logger.Error(ex, "Cache create failed");
            return "";
        }
    }

    public bool AddStorage(string cacheName, object cacheContent, long cacheDuration)
    {
        return AddStorage(cacheName, cacheContent, TimeSpan.FromSeconds(cacheDuration));
    }

    public bool AddStorage(string cacheName, object cacheContent, TimeSpan cacheDuration)
    {
        try
        {
            _storageCache[cacheName] = new StorageCacheWrapper(cacheContent, DateTime.Now + cacheDuration);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "cache create failed");
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
            _logger.Error(ex, "Cache modify failed");
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