using AuthServer.SecurityTokens.Services.Providers;
using AuthServer.Infrastructure.MongoDb;
using AuthServer.Infrastructure.RabbitMq;
using AuthServer.Infrastructure.Serialization;
using AuthServer.SecurityTokens.RevokedTokensImporter.Consumers;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using StackExchange.Redis;
using System;
using System.Threading;
using TacitusLogger;
using AuthServer.Infrastructure.Redis;
using TacitusLogger.Builders;
using TacitusLogger.Destinations.MongoDb;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace AuthServer.SecurityTokens.RevokedTokensImporter
{
    class RevokedTokensImporterProgram
    {
        private static IConfigurationRoot _configs;
        private static ILogger _logger;
        private static Mutex _mutex;
        private static string _appName;
        private static string _mutexName;
        private static List<object> _rabbitMqConsumers;

        private static void LoadConfigs()
        {
            _configs = new ConfigurationBuilder()
                    .AddJsonFile(".//Settings//app-settings.json")
                    .AddJsonFile(".//Settings//rabbitmq-settings.json")
                    .AddJsonFile(".//Settings//redis-settings.json")
                    .AddJsonFile(".//Settings//mongodb-settings.json")
                    .Build();
        }

        private static void InitLogger()
        {
            var mongoDbConfigs = new MongoDbConfigs(_configs.GetSection("mongodb"));
            var infoLogCollectionConfig = new MongoCollectionConfig(_configs.GetSection("mongodb").GetSection("infoLogCollection"));
            MongoClient client = new MongoClient(mongoDbConfigs.ConnectionString);
            IMongoDatabase db = client.GetDatabase(mongoDbConfigs.Database);

            _logger = LoggerBuilder.Logger(_appName)
                                   .ForAllLogs()
                                   .Console().WithSimpleTemplateLogText("[$LogDate(HH:mm:ss) $Source] - $Description").Add()
                                   .MongoDb().WithCollection(db, infoLogCollectionConfig.Name).Add()
                                   .BuildLogger();
        }

        private static ConnectionMultiplexer InitRedis()
        {
            var redisConfigs = new RedisConfigs(_configs.GetSection("redisDb"));

            ConnectionMultiplexer reddisDbMultiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(redisConfigs.ConfigurationOptions);
            return reddisDbMultiplexer;
        }
         
        private static IConnection ConfigureRabbitMqConn()
        {
            var rabbitMqConfig = new RabbitMqConfigs(_configs.GetSection("rabbitMq"));
             
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = rabbitMqConfig.Host,
                Port = rabbitMqConfig.Port,
                UserName = rabbitMqConfig.User,
                Password = rabbitMqConfig.Password,
            };
            IConnection rabbitMqConn = factory.CreateConnection();
            return rabbitMqConn;
        }

        static List<object> ConfigureRabbitMqConsumers()
        {
            //Redis
            ConnectionMultiplexer redisMultiplexer = InitRedis();
            var rabbitMqConn = ConfigureRabbitMqConn();

            // Factory method for token revoked event consumer.
            Func<TokenRevokedEventRabbitMqConsumer> tokenRevokedEventRabbitMqConsumerFactoryMethod = () =>
            {
                var revokedTokensRedisCollectionConfigs = new RedisCollectionConfig(_configs.GetSection("redis").GetSection("revokedTokensCollection")); 
                var revokedTokenRepo = new RedisCachedRepo<string>(redisMultiplexer.GetDatabase(), revokedTokensRedisCollectionConfigs.CollectionName, new DefaultSerializer(), _logger);
                var queueConfig = new RabbitMqQueueConfig(_configs.GetSection("rabbitMq").GetSection("refreshTokenRevokedEventQueue"));
                TokenRevokedEventRabbitMqConsumer consumer = new TokenRevokedEventRabbitMqConsumer(revokedTokenRepo, rabbitMqConn, queueConfig, _logger);
                return consumer;
            };

            // Factory method for token invalidated event consumer.
            Func<TokenInvalidatedEventRabbitMqConsumer> tokenInvalidatedEventRabbitMqConsumerFactoryMethod = () =>
            {
                var revokedTokensRedisCollectionConfigs = new RedisCollectionConfig(_configs.GetSection("redis").GetSection("invalidatedTokensCollection"));
                var invalidatedTokenRepo = new RedisCachedRepo<string>(redisMultiplexer.GetDatabase(), revokedTokensRedisCollectionConfigs.CollectionName, new DefaultSerializer(), _logger);
                var queueConfig = new RabbitMqQueueConfig(_configs.GetSection("rabbitMq").GetSection("refreshTokenInvalidatedEventQueue"));
                TokenInvalidatedEventRabbitMqConsumer consumer = new TokenInvalidatedEventRabbitMqConsumer(invalidatedTokenRepo, rabbitMqConn, queueConfig, _logger);
                return consumer;
            };

            return new List<object>()
            {
                 tokenRevokedEventRabbitMqConsumerFactoryMethod(),
                 tokenInvalidatedEventRabbitMqConsumerFactoryMethod()
            }; 
        }

        private static void InitAppConfigs()
        {
            _appName = _configs["appName"];
            _mutexName = _configs["mutexName"];
        }

        static void Main(string[] args)
        {
            Console.Title = $"{_appName} tokens importer";
            LoadConfigs();
            InitAppConfigs();
            InitLogger();
            try
            {
                bool ownsMutext;
                _mutex = new Mutex(false, _mutexName, out ownsMutext);

                if (ownsMutext)
                {
                    _rabbitMqConsumers = ConfigureRabbitMqConsumers();
                    _logger.LogEvent("AuthServer.SecurityTokens.RevokedTokensImporter", $"Tokens importer started.");
                    PressToExitLoop();
                    _logger.LogEvent("AuthServer.SecurityTokens.RevokedTokensImporter", $"Tokens importer finished.");
                }
                else
                {
                    _logger.LogError("RevokedTokensImporterProgram", $"Cannot run the program because another instance is running.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("RevokedTokensImporterProgram.MainCatch", $"Some error occurred.", ex);
                throw;
            }
        }
        private static void PressToExitLoop()
        {
            while (true)
            {
                Console.WriteLine("Press q to exit.");
                if (Console.ReadLine() == "q")
                {
                    _rabbitMqConsumers = null;
                    break;
                }
            }
        }
    }
}
