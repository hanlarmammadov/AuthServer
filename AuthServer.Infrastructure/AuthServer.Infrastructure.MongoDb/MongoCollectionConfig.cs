using Microsoft.Extensions.Configuration;
using System;

namespace AuthServer.Infrastructure.MongoDb
{
    public class MongoCollectionConfig
    { 
        private readonly string _name;

        public MongoCollectionConfig(string name)
        {
            _name = name;
        }
        public MongoCollectionConfig(IConfigurationSection collectionConfigs)
        {
            if (collectionConfigs == null)
                throw new ArgumentNullException("collectionConfigs");
            _name = collectionConfigs["name"];
        }

        public string Name => _name;
    }
}
