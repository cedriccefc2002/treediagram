using Microsoft.Extensions.Configuration;

namespace Server.lib.Config
{
    public class Neo4jConfig
    {
        private Neo4jConfig() { }
        public string uri { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public static Neo4jConfig Config
        {
            get
            {
                IConfigurationSection section = ConfigProvider.configuration.GetSection("Neo4j");
                return new Neo4jConfig()
                {
                    uri = section.GetSection("uri").Value,
                    username = section.GetSection("username").Value,
                    password = section.GetSection("password").Value
                };
            }
        }
    }
}