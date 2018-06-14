using System.IO;
using Microsoft.Extensions.Configuration;

namespace Server.lib
{
    public static class ConfigProvider
    {
        public static readonly IConfigurationRoot configuration;

        static ConfigProvider()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var configuration = builder.Build();
        }

        public static Config.IceAdapterConfig IceAdapter
        {
            get
            {
                var IceAdapterConfig = configuration.GetSection("Ice.Adapter");
                return new Config.IceAdapterConfig()
                {
                    name = IceAdapterConfig.GetSection("name").Value,
                    endpoints = IceAdapterConfig.GetSection("endpoints").Value
                };
            }
        }
    }
}