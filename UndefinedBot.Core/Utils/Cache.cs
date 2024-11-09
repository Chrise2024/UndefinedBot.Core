using System.Data;
using Newtonsoft.Json.Linq;

namespace UndefinedBot.Core.Utils
{
    public class CacheManager
    {
        private readonly JObject _storageCache = [];
        private readonly Dictionary<string,FileCacheProperty> _fileCache = [];
        private readonly Logger _cacheLogger;
        private readonly string _cacheRootPath;
        public CacheManager(string pluginName,string cacheRootPath,CommandFinishEvent finishEvent)
        {
            _cacheLogger = new(pluginName);
            _cacheRootPath = cacheRootPath;
            finishEvent.OnCommandFinish += UpdateCache;
        }
        public void UpdateCache()
        {
            long curTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            List<string> dels = [];
            foreach (var pair in _storageCache)
            {
                if ((pair.Value?.ToObject<StorageCacheProperty<object>>()?.ExpiredTime ?? 0) < curTime)
                {
                    dels.Add(pair.Key);
                }
            }
            foreach (string del in dels)
            {
                _storageCache.Remove(del);
            }
            dels.Clear();
            foreach (var pair in _fileCache)
            {
                if (pair.Value.ExpiredTime < curTime)
                {
                    FileIO.SafeDeleteFile(pair.Value.FilePath);
                    dels.Add(pair.Key);
                }
            }
            foreach (string del in dels)
            {
                _fileCache.Remove(del);
            }
        }
        public string AddFile(string cacheName,string cachePath, long cacheDuration)
        {
            try
            {
                string fullPath = Path.Join(_cacheRootPath, cachePath);
                _fileCache[cacheName] = new FileCacheProperty(fullPath,DateTimeOffset.UtcNow.ToUnixTimeSeconds() + cacheDuration);
                return fullPath;
            }
            catch(Exception ex)
            {
                _cacheLogger.Error("Cache","Cache Create Failed");
                _cacheLogger.Error("Cache",ex.ToString());
                _cacheLogger.Error("Cache",ex.StackTrace ?? "");
                return "";
            }
        }
        public bool AddStorage<T>(string cacheName, T cacheContent, long cacheDuration)
        {
            try
            {
                _storageCache[cacheName] = JToken.FromObject(new StorageCacheProperty<T>(cacheContent ?? throw new NoNullAllowedException(),cacheDuration));
                return true;
            }
            catch(Exception ex)
            {
                _cacheLogger.Error("Cache","Cache Create Failed");
                _cacheLogger.Error("Cache",ex.ToString());
                _cacheLogger.Error("Cache",ex.StackTrace ?? "");
                return false;
            }
        }

        public T? ModifyStorage<T>(string cacheName, T newContent)
        {
            try
            {
                StorageCacheProperty<T> sp = _storageCache.Value<StorageCacheProperty<T>>(cacheName) ?? throw new NullReferenceException();
                sp.Content = newContent;
                //_storageCache[cacheName] = JToken.FromObject(new StorageCacheProperty<T>(cacheContent ?? throw new NoNullAllowedException(),cacheDuration));
                return newContent;
            }
            catch(Exception ex)
            {
                _cacheLogger.Error("Cache","Cache Modify Failed");
                _cacheLogger.Error("Cache",ex.ToString());
                _cacheLogger.Error("Cache",ex.StackTrace ?? "");
                return default;
            }
        }
        public string GetFile(string cacheName)
        {
            return _fileCache.TryGetValue(cacheName,out FileCacheProperty? fp) ? fp.FilePath : "";
        }
        public T? GetStorage<T>(string cacheName)
        {
            StorageCacheProperty<T>? sp = _storageCache.Value<StorageCacheProperty<T>>(cacheName);
            return sp != null ? sp.Content : default;
        }
    }
    public class StorageCacheProperty<T>(T content,long expiredTime)
    {
        public T Content = content;
        public readonly long ExpiredTime = expiredTime;
    }
    public class FileCacheProperty(string path,long expiredTime)
    {
        public readonly string FilePath = path;
        public readonly long ExpiredTime = expiredTime;
    }
    public enum CacheType
    {
        File = 0,
        Storage = 1,
    }
}