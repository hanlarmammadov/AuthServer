using Microsoft.Extensions.Configuration;
using System;

namespace AuthServer.Infrastructure.Redis
{
    public class RedisCollectionConfig
    {
        private string _collName;

        public RedisCollectionConfig(string collectionName)
        {
            _collName = collectionName;
        }
        public RedisCollectionConfig(IConfigurationSection redisCollectionConfigs)
        {
            if (redisCollectionConfigs == null)
                throw new ArgumentNullException("redisConfigs");
            _collName = redisCollectionConfigs["name"];
        }

        public string CollectionName => _collName;
    }
}
