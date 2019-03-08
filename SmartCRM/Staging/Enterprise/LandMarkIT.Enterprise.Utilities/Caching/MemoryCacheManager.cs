using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;

namespace LandmarkIT.Enterprise.Utilities.Caching
{
    public class MemoryCacheManager
    {
        private static object _syncObject = new object();
        private static MemoryCache cache = MemoryCache.Default;
        //private static MemoryCacheManager _instance = default(MemoryCacheManager);

        //public static MemoryCacheManager Instance { get { if (_instance == null)_instance = new MemoryCacheManager(); return _instance; } }

        /// <summary>
        /// Adds object to Memory Cache
        /// </summary>
        /// <typeparam name="T">Cache Object type</typeparam>
        /// <param name="key">Cache Key</param>
        /// <param name="value">Cache Object</param>
        /// <param name="addDependency">set this as true if requires file dependency</param>
        /// 

        public void Add<T>(string key, T value, string dependencyFileName) where T : class
        {
            if (string.IsNullOrWhiteSpace(dependencyFileName))
            {
                Add<T>(key, value, DateTimeOffset.MaxValue);
            }
            else
            {
                if (!File.Exists(dependencyFileName))
                {
                    File.Create(dependencyFileName);
                }
                var policy = new CacheItemPolicy();
                policy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string> { dependencyFileName }));
                policy.UpdateCallback = CacheItemRemoved;
                policy.AbsoluteExpiration = DateTimeOffset.MaxValue;
                //policy.SlidingExpiration = TimeSpan.FromMinutes(30);
                CacheItem cacheItem = new CacheItem(key, value);
                cache.Set(cacheItem, policy);             //If the specified entry does not exist, it is created. If the specified entry exists, it is updated.
            }
        }

        public void Add<T>(string key, T value, DateTimeOffset absoluteExpiration) where T : class
        {
            cache.Set(key, value, absoluteExpiration);
        }
        public bool Add(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            return cache.Add(key, value, policy, regionName);
        }

        public T AddOrGetExisting<T>(string key, T value, string dependencyFileName) where T : class
        {
            var policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.MaxValue; ;
            policy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string> { dependencyFileName }));
            //policy.UpdateCallback = CacheItemRemoved;

            var result = cache.AddOrGetExisting(key, value, policy, null) as T;
            return result;
        }

        public T Get<T>(string key) where T : class
        {
            var result = default(T);
            if (cache.Contains(key)) result = cache.Get(key) as T;
            return result;
        }
        public void UpdateDependency(string cacheKey, string dependencyFileName)
        {
            using (var sw = new StreamWriter(dependencyFileName))
            {
                sw.Write(DateTime.Now.ToString());
                sw.Close();
            }
        }

        public object Get(string key)
        {
            var result = default(object);
            if (cache.Contains(key)) result = cache.Get(key);
            return result;
        }

        public bool IsExists(string key)
        {
            return cache.Contains(key);
        }

        public void Remove(string key)
        {
            cache.Remove(key);
        }

        public void Flush()
        {
            //TODO: implement this
        }

        public void CacheItemRemoved(CacheEntryUpdateArguments args)
        {
            if (args.RemovedReason == CacheEntryRemovedReason.ChangeMonitorChanged)
            {

                //var id = args.Key;
                //var updatedEntity = "MYMODIFIEDVALUE";
                //args.UpdatedCacheItem = new CacheItem(id, updatedEntity);

                //var policy = new CacheItemPolicy();
                //policy.AbsoluteExpiration = DateTimeOffset.MaxValue;
                //policy.ChangeMonitors.Add(new HostFileChangeMonitor(new List<string> { "D:\\Websites\\Myfile.txt" }));
                //policy.UpdateCallback = CacheItemRemoved;
                //args.UpdatedCacheItemPolicy = policy;
            }

        }
    }
}
