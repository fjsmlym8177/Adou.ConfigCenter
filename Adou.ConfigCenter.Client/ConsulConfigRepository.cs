using Consul;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Adou.ConfigCenter.Client
{
    internal class ConsulConfigRepository : IConfigRepository
    {
        private string prefix = "";
        private ConsulClient consulClient;

        public ConsulConfigRepository(ConsulRegistryConfiguration config)
        {
            prefix = config.PreInConsul;
            consulClient = new ConsulClient(c =>
            {
                c.Address = new Uri($"http://{config.Host}:{config.Port}");

                if (!string.IsNullOrEmpty(config?.Token))
                {
                    c.Token = config.Token;
                }
            });
        }


        private string GetKeyName<T>()
        {
            return prefix + ":" + typeof(T).Name;
        }

        public async Task<T> Get<T>()
        {
            var queryResult = await consulClient.KV.Get(GetKeyName<T>());

            if (queryResult.Response==null)
            {
                return JsonConvert.DeserializeObject<T>("");
            }

            var bytes = queryResult.Response.Value;

            var json = Encoding.UTF8.GetString(bytes);

            try
            {
                var consulConfig = JsonConvert.DeserializeObject<T>(json);

                return consulConfig;
            }
            catch (Exception ex)
            {
                //错误预警告知

                return default(T);
            }

        }

        public async Task<bool> Set<T>(T value)
        {
            var json = JsonConvert.SerializeObject(value, Formatting.Indented);

            var bytes = Encoding.UTF8.GetBytes(json);

            var kvPair = new KVPair(GetKeyName<T>())
            {
                Value = bytes
            };

            var result = await consulClient.KV.Put(kvPair);

            if (result.Response)
                return true;
            else
                return false;
        }
    }
}
