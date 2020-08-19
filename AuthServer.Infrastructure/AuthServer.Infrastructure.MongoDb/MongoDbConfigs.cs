using Microsoft.Extensions.Configuration;
using System;

namespace AuthServer.Infrastructure.MongoDb
{
    public class MongoDbConfigs
    {
        private readonly string _connectionString;
        private readonly string _database;

        public MongoDbConfigs(string connectionString, string database)
        {
            _connectionString = connectionString;
            _database = database;
        }
        public MongoDbConfigs(IConfigurationSection mongoConfigs)
        {
            if (mongoConfigs == null)
                throw new ArgumentNullException("mongoConfigs");
            _connectionString = mongoConfigs["connectionString"];
            _database = mongoConfigs["database"];
        }

        public string ConnectionString => _connectionString;
        public string Database => _database;
    }
}
