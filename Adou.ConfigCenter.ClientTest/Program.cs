using Adou.ConfigCenter.Client;
using Consul;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Adou.ConfigCenter.ClientTest
{
    class Program
    {
        static string _previousAsJson = "";

        static void Main(string[] args)
        {
            var consulHost = "localhost";
            var consulPort = 8500;
            var token = "";
            var config = new ConsulRegistryConfiguration(consulHost, consulPort, "Adou.ConfigCenter.ClientTest", token);

            ConfigContainer.Init(config)
                .Monitor<TestConfig>();


            Task.Run( async () =>
            {
                do
                {
                    var testConfig = ConfigContainer.Current.Get<TestConfig>().Result;

                    Console.WriteLine(JsonConvert.SerializeObject(testConfig));

                    Thread.Sleep(5000);
                } while (true);
            });


            do
            {
                Console.ReadKey();
            } while (true);

        }
    }
}
