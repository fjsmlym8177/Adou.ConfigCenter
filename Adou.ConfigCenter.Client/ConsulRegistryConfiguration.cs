using System;

namespace Adou.ConfigCenter.Client
{
    public class ConsulRegistryConfiguration
    {
        public ConsulRegistryConfiguration(string host, int port, string preInConsul, string token)
        {
            Host = host;
            Port = port;
            PreInConsul = preInConsul;
            Token = token;
        }

        public string PreInConsul { get; }
        public string Host { get; }
        public int Port { get; }
        public string Token { get; }
    }
}
