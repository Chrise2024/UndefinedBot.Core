﻿using System.Data;
using System.Text.Json;

namespace UndefinedBot.Core.Utils;

public class CacheManager
{
    private readonly Dictionary<string,object> _storageCache = [];
    private readonly Dictionary<string, FileCacheProperty> _fileCache = [];
    private readonly GeneralLogger _cacheLogger;
    private readonly string _cacheRootPath;

    public CacheManager(string pluginName, string cacheRootPath, CommandFinishEvent finishEvent)
    {
        _cacheLogger = new(pluginName);
        _cacheRootPath = cacheRootPath;
        finishEvent.OnCommandFinish += UpdateCache;
    }

    public void UpdateCache()
    {
        long curTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();


        foreach (var pair in _storageCache.Where(pair => ((pair.Value as StorageCacheProperty<object>)?.ExpiredTime ?? 0) < curTime))
        {
            _storageCache.Remove(pair.Key);
        }

        foreach (var pair in _fileCache.Where(pair => pair.Value.ExpiredTime < curTime))
        {
            FileIO.SafeDeleteFile(pair.Value.FilePath);
            _fileCache.Remove(pair.Key);
        }
    }

    public string AddFile(string cacheName, string cachePath, long cacheDuration)
    {
        try
        {
            string fullPath = Path.Join(_cacheRootPath, cachePath);
            _fileCache[cacheName] =
                new FileCacheProperty(fullPath, DateTimeOffset.UtcNow.ToUnixTimeSeconds() + cacheDuration);
            return fullPath;
        }
        catch (Exception ex)
        {
            _cacheLogger.Error("Cache Create Failed");
            _cacheLogger.Error(ex.ToString());
            _cacheLogger.Error(ex.StackTrace ?? "");
            return "";
        }
    }

    public bool AddStorage<T>(string cacheName, T cacheContent, long cacheDuration)
    {
        try
        {
            _storageCache[cacheName] =
                JsonSerializer.SerializeToNode(new StorageCacheProperty<T>(cacheContent ?? throw new NoNullAllowedException(),
                    cacheDuration))!;
            return true;
        }
        catch (Exception ex)
        {
            _cacheLogger.Error("Cache Create Failed");
            _cacheLogger.Error(ex.ToString());
            _cacheLogger.Error(ex.StackTrace ?? "");
            return false;
        }
    }

    public T? ModifyStorage<T>(string cacheName, T newContent)
    {
        try
        {
            StorageCacheProperty<T> sp = _storageCache[cacheName] as StorageCacheProperty<T> ??
                                         throw new NullReferenceException();
            sp.Content = newContent;
            //_storageCache[cacheName] = JToken.FromObject(new StorageCacheProperty<T>(cacheContent ?? throw new NoNullAllowedException(),cacheDuration));
            return newContent;
        }
        catch (Exception ex)
        {
            _cacheLogger.Error("Cache Modify Failed");
            _cacheLogger.Error(ex.ToString());
            _cacheLogger.Error(ex.StackTrace ?? "");
            return default;
        }
    }

    public string GetFile(string cacheName)
    {
        return _fileCache.TryGetValue(cacheName, out FileCacheProperty? fp) ? fp.FilePath : "";
    }

    public T? GetStorage<T>(string cacheName)
    {
        return _storageCache[cacheName] is StorageCacheProperty<T> sp ? sp.Content : default;
    }
}

public class StorageCacheProperty<T>(T content, long expiredTime)
{
    public T Content = content;
    public readonly long ExpiredTime = expiredTime;
}

public class FileCacheProperty(string path, long expiredTime)
{
    public readonly string FilePath = path;
    public readonly long ExpiredTime = expiredTime;
}

public enum CacheType
{
    File = 0,
    Storage = 1,
}
