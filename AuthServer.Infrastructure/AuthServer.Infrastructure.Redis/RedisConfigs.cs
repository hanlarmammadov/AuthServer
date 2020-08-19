using Microsoft.Extensions.Configuration;

namespace AuthServer.Infrastructure.Redis
{
    public class RedisConfigs
    {
        private string _configurationOptions;

        public RedisConfigs(string configurationOptions)
        {
            _configurationOptions = configurationOptions;
        }
        public RedisConfigs(IConfigurationSection redisConfigs)
        {
            _configurationOptions = redisConfigs["configurationOptions"];
        }

        public string ConfigurationOptions => _configurationOptions;
    }
}
