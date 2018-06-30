using Adou.ConfigCenter.Client;
using Adou.ConfigCenter.ConsoleApp1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var consulHost = "localhost";
            var consulPort = 8500;
            var token = "";
            var config = new ConsulRegistryConfiguration(consulHost, consulPort, "Adou.ConfigCenter.ClientTest", token);

            ConfigContainer.Init(config)
                .Monitor<TestConfig>();

            Task.Run(async () =>
            {
                do
                {
                    var testConfig = ConfigContainer.Current.Get<TestConfig>().Result;

                    Console.WriteLine(testConfig.Address + "              " + testConfig.Port);

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
