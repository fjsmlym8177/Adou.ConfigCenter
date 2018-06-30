using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Adou.ConfigCenter.Client
{
    internal class FileConfigRepository : IConfigRepository
    {
        public Dictionary<string, string> pathMaps = new Dictionary<string, string>();
        public static Dictionary<string, object> locks = new Dictionary<string, object>();

        private readonly IMemoryCache _cache;

        #region Utitilies

        private T GetCacheValue<T>(string key)
        {
            object val = null;
            if (key != null && _cache.TryGetValue(key, out val))
            {
                return (T)val;
            }
            else
            {
                return default(T);
            }
        }
        /// <summary>
        /// 添加缓存内容
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetChacheValue(string key, object value)
        {
            if (key != null)
            {
                _cache.Set(key, value, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMilliseconds(5000)
                });
            }
        }

        #endregion


        public FileConfigRepository()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public Task<T> Get<T>()
        {
            var memoryConfig = GetCacheValue<T>(typeof(T).FullName);

            if (memoryConfig != null)
            {
                return Task.FromResult<T>(memoryConfig);
            }

            var jsonConfig = "";

            lock (locks[typeof(T).FullName])
            {
                if (System.IO.File.Exists(pathMaps[typeof(T).FullName]))
                {
                    jsonConfig = System.IO.File.ReadAllText(pathMaps[typeof(T).FullName]);
                }
            }

            var result = JsonConvert.DeserializeObject<T>(jsonConfig);

            if (result != null)
            {
                SetChacheValue(typeof(T).FullName, result);
            }

            return Task.FromResult<T>(result);
        }

        public Task<bool> Set<T>(T value)
        {
            string jsonConfig = JsonConvert.SerializeObject(value, Formatting.Indented);

            lock (locks[typeof(T).FullName])
            {
                if (System.IO.File.Exists(pathMaps[typeof(T).FullName]))
                {
                    System.IO.File.Delete(pathMaps[typeof(T).FullName]);
                }

                System.IO.File.WriteAllText(pathMaps[typeof(T).FullName], jsonConfig);
            }

            SetChacheValue(typeof(T).FullName, value);
            return Task.FromResult(true);
        }
    }
}
