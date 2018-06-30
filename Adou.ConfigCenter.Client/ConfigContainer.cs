using Consul;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Adou.ConfigCenter.Client
{
    public class ConfigContainer
    {
        private FileConfigRepository _fileConfigRepository;
        private ConsulConfigRepository _consulConfigRepository;
        private static Dictionary<string, Timer> _timers = new Dictionary<string, Timer>();


        private ConfigContainer(FileConfigRepository fileConfigRepository, ConsulConfigRepository consulConfigRepository)
        {
            _fileConfigRepository = fileConfigRepository;
            _consulConfigRepository = consulConfigRepository;
        }


        public static ConfigContainer Current { get; private set; }

        public static ConfigContainer Init(ConsulRegistryConfiguration config)
        {
            Current = new ConfigContainer(
                new FileConfigRepository(),
                new ConsulConfigRepository(config));


            return Current;
        }

        public ConfigContainer Monitor<T>(string path = "")
        {
            if (string.IsNullOrEmpty(path))
                Current._fileConfigRepository.pathMaps.Add(typeof(T).FullName, $"{typeof(T).FullName}.json");
            else
                Current._fileConfigRepository.pathMaps.Add(typeof(T).FullName, path);

            FileConfigRepository.locks.Add(typeof(T).FullName, new object());

            bool _polling = false;
            int delay = 5000;
            var timer = new Timer(async x =>
            {
                if (_polling)
                {
                    return;
                }

                _polling = true;
                await Poll<T>();
                _polling = false;
            }, null, 0, delay);
            GC.KeepAlive(timer);

            _timers.Add(typeof(T).FullName, timer);

            return Current;
        }


        private static async Task Poll<T>()
        {
            //_logger.LogInformation("Started polling consul");

            //try
            //{
            var fileConfig = await Current._fileConfigRepository.Get<T>();

            var consulConfig = await Current._consulConfigRepository.Get<T>();

            if (fileConfig == null)
            {
                if (consulConfig == null)
                {
                    var config = (T)typeof(T).Assembly.CreateInstance(typeof(T).FullName);
                    await Current._consulConfigRepository.Set(config);
                    await Current._fileConfigRepository.Set(config);
                    return;
                }

                await Current._fileConfigRepository.Set(consulConfig);
                //_logger.LogWarning($"error geting file config, errors are {string.Join(",", fileConfig.Errors.Select(x => x.Message))}");
                return;
            }

            var fileJson = JsonConvert.SerializeObject(fileConfig);
            var consulJson = JsonConvert.SerializeObject(consulConfig);


            if (consulJson != fileJson)
            {
                if (consulConfig == null)
                {
                    await Current._consulConfigRepository.Set(fileConfig);
                    return;
                }

                await Current._fileConfigRepository.Set(consulConfig);
            }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}


            //_logger.LogInformation("Finished polling consul");
        }

        private static ConfigContainer _configContainer;





        public async Task<T> Get<T>()
        {
            var result = await _fileConfigRepository.Get<T>();
            return result;
        }

    }
}
