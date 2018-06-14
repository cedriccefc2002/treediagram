using Microsoft.Extensions.Configuration;

namespace Server.lib.Config
{
    public class IceAdapterConfig
    {
        private IceAdapterConfig() { }
        public string name { get; set; }
        public string endpoints { get; set; }

        public static IceAdapterConfig Config
        {
            get
            {
                IConfigurationSection section = ConfigProvider.configuration.GetSection("Ice.Adapter");
                return new IceAdapterConfig()
                {
                    name = section.GetSection("name").Value,
                    endpoints = section.GetSection("endpoints").Value
                };
            }
        }
    }
}