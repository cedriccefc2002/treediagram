using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Server.lib
{
    public static class ConfigProvider
    {
        private static readonly IConfigurationBuilder builder;
        public static readonly IConfigurationRoot configuration;

        static ConfigProvider()
        {
            builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            configuration = builder.Build();
            Console.WriteLine($"ConfigVersion = \"{configuration["Version"]}\"");
        }
    }
}