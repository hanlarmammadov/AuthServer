using AuthServer.SecurityTokens.Services.Commands;
using AuthServer.SecurityTokens.Services.Commands.Interfaces;
using AuthServer.SecurityTokens.Services.EventSubscribers.RevokedTokenEvent;
using AuthServer.SecurityTokens.Services.Providers;
using AuthServer.Infrastructure.MongoDb;
using AuthServer.Infrastructure.RabbitMq;
using AuthServer.Infrastructure.Serialization;
using AuthServer.SecurityTokens.Data;
using AuthServer.SecurityTokens.Entities;
using AuthServer.SecurityTokens.Inbound.Consumers;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using RabbitMQ.Client;
using StackExchange.Redis;
using System;
using System.Threading;
using TacitusLogger;
using AuthServer.Infrastructure.Redis;
using TacitusLogger.Builders;
using TacitusLogger.Destinations.MongoDb;
using AuthServer.SecurityTokens.Models.Events;
using System.Collections.Generic;

namespace AuthServer.SecurityTokens.Inbound
{
    class SecurityTokensInboundProgram
    {
        private static ILogger _logger;
        private static IConfigurationRoot _configs;
        private static string _accountTokenMongoCollectionName;
        private static List<Object> _rabbitMqConsumers;

        private static void LoadConfigFiles()
        {
            _configs = new ConfigurationBuilder()
                .AddJsonFile(".//Settings//app-settings.json")
                .AddJsonFile(".//Settings//rabbitmq-settings.json")
                .AddJsonFile(".//Settings//mongodb-settings.json")
                .AddJsonFile(".//Settings//redis-settings.json")
                .Build();
        }

        private static void InitLogger()
        {
            var mongoDbSection = new MongoDbConfigs(_configs.GetSection("mongodb_log_sys"));
            var infoLogCollectionConfig = new MongoCollectionConfig(_configs.GetSection("mongodb_log_sys").GetSection("infoLogCollection"));
            MongoClient client = new MongoClient(mongoDbSection.ConnectionString);
            IMongoDatabase db = client.GetDatabase(mongoDbSection.Database);

            _logger = LoggerBuilder.Logger()
                                   .ForAllLogs()
                                   .Console().WithSimpleTemplateLogText("[$LogDate(HH:mm:ss) $Source] - $Description").Add()
                                   .MongoDb().WithCollection(db, infoLogCollectionConfig.Name).Add()
                                   .BuildLogger();
        }

        private static ConnectionMultiplexer InitRedis()
        {
            var redisConfigs = new RedisConfigs(_configs.GetSection("redis"));
            ConnectionMultiplexer reddisDbMultiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(redisConfigs.ConfigurationOptions);
            return reddisDbMultiplexer;
        }

        private static RabbitMQ.Client.IConnection InitRabbitMqConn()
        {
            RabbitMqConfigs rabbitmqConfigs = new RabbitMqConfigs(_configs.GetSection("rabbitMq"));

            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = rabbitmqConfigs.Host,
                Port = rabbitmqConfigs.Port,
                UserName = rabbitmqConfigs.User,
                Password = rabbitmqConfigs.Password,
            };
            RabbitMQ.Client.IConnection rabbitmqConn = factory.CreateConnection();
            return rabbitmqConn;
        }

        private static IMongoDatabase InitMongoDb()
        {
            var mongoDbSection = new MongoDbConfigs(_configs.GetSection("mongodb_token_sys"));
            var accountTokenCollectionConfig = new MongoCollectionConfig(_configs.GetSection("mongodb_token_sys").GetSection("accountRTokenCollection"));
            _accountTokenMongoCollectionName = accountTokenCollectionConfig.Name;

            MongoMappingsInitializer.Init();
            MongoClient client = new MongoClient(mongoDbSection.ConnectionString);
            IMongoDatabase mongdb = client.GetDatabase(mongoDbSection.Database);
            return mongdb;
        }

        private static List<object> ConfigureRabbitMqConsumers()
        {
            var mongoDb = InitMongoDb();
            var redisMultiplexer = InitRedis();
            var rabbitMqConn = InitRabbitMqConn();

            //Factory method for creating PasswordChangedEventRabbitMqConsumer.
            Func<PasswordChangedEventRabbitMqConsumer> passwordChangedEventRabbitMqConsumerFactoryMethod = () =>
             {
                 // Create command for the consumer.
                 IMongoCollection<AccountRTokenInfo> rtokensRepo = mongoDb.GetCollection<AccountRTokenInfo>(_accountTokenMongoCollectionName);
                 var revokedTokensRedisDbPrefix = new RedisCollectionConfig(_configs.GetSection("redis").GetSection("revokedTokensCollection")).CollectionName;
                 RedisCachedRepo<string> revokedTokensRedisRepo = new RedisCachedRepo<string>(redisMultiplexer.GetDatabase(), revokedTokensRedisDbPrefix, new DefaultSerializer(), _logger);
                 var command = new RevokeAllTokensForAccountCommand(rtokensRepo, _logger);

                 // Add event subscribers to the command.
                 var refreshTokenRevokedEventExchangeConfig = new RabbitMqExchangeConfigs(_configs.GetSection("rabbitMq").GetSection("refreshTokenRevokedEventExchange"));
                 command.AddSubsciber(new RabbitMqEventPublisher<RefreshTokenRevokedEvent>(
                                                                  new RabbitMqProducer<RefreshTokenRevokedEvent>(rabbitMqConn, refreshTokenRevokedEventExchangeConfig, _logger),
                                                                  _logger));
                 command.AddSubsciber(new RevokedTokenRedisCacher(revokedTokensRedisRepo, _logger));

                 // Create consumer and return it.
                 var userClaimsChangedEventQueueConfig = new RabbitMqQueueConfig(_configs.GetSection("rabbitMq").GetSection("passwordChangedEventQueue"));
                 PasswordChangedEventRabbitMqConsumer consumer = new PasswordChangedEventRabbitMqConsumer(command, rabbitMqConn, userClaimsChangedEventQueueConfig, _logger);
                 return consumer;
             };

            //Factory method for creating UserRolesChangedEventRabbitMqConsumer.
            Func<UserRolesChangedEventRabbitMqConsumer> userRolesChangedEventRabbitMqConsumerFactoryMethod = () =>
            {
                // Create command for the consumer. 
                IMongoCollection<AccountRTokenInfo> rtokensRepo = mongoDb.GetCollection<AccountRTokenInfo>(_accountTokenMongoCollectionName);
                var invalidatedTokensRedisDbPrefix = new RedisCollectionConfig(_configs.GetSection("redis").GetSection("invalidatedTokensCollection")).CollectionName;
                RedisCachedRepo<string> invalidatedTokensRedisRepo = new RedisCachedRepo<string>(redisMultiplexer.GetDatabase(), invalidatedTokensRedisDbPrefix, new DefaultSerializer(), _logger);
                var command = new InvalidateAllTokensForAccountCommand(rtokensRepo, _logger);

                // Add event subscribers to the command.
                var refreshTokenInvalidatedEventExchangeConfigs = new RabbitMqExchangeConfigs(_configs.GetSection("rabbitMq").GetSection("refreshTokenInvalidatedEventExchange"));
                command.AddSubsciber(new RabbitMqEventPublisher<RefreshTokenRevokedEvent>(
                                                                 new RabbitMqProducer<RefreshTokenRevokedEvent>(rabbitMqConn, refreshTokenInvalidatedEventExchangeConfigs, _logger),
                                                                 _logger));
                command.AddSubsciber(new InvalidatedTokenRedisCacher(invalidatedTokensRedisRepo, _logger));

                // Create consumer and return it.
                var userClaimsChangedEventQueueConfig = new RabbitMqQueueConfig(_configs.GetSection("rabbitMq").GetSection("userRolesChangedEventQueue"));
                UserRolesChangedEventRabbitMqConsumer consumer = new UserRolesChangedEventRabbitMqConsumer(command, rabbitMqConn, userClaimsChangedEventQueueConfig, _logger);
                return consumer;
            };

            // Create and return RabbitMq consumers list.
            var rabbitMqConsumers = new List<object>()
            {
                passwordChangedEventRabbitMqConsumerFactoryMethod(),
                userRolesChangedEventRabbitMqConsumerFactoryMethod()
            };
            return rabbitMqConsumers;
        }

        static void Main(string[] args)
        {
            Console.Title = "Security tokens inbound.";
            LoadConfigFiles();
            InitLogger();
            try
            {
                bool ownsMutext;
                Mutex mutex = new Mutex(false, "Global\\SecurityTokensInbound", out ownsMutext);
                if (ownsMutext)
                {
                    _rabbitMqConsumers = ConfigureRabbitMqConsumers();
                    _logger.LogInfo("AuthServer.SecurityTokens.Inbound", $"The consumers configured and the programm started.");
                    PressToExitLoop();
                    _logger.LogInfo("AuthServer.SecurityTokens.Inbound", $"The programm finished by the user.");
                }
                else
                {
                    _logger.LogError("AuthServer.SecurityTokens.Inbound", $"Cannot run the program because another instance is running.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("AuthServer.SecurityTokens.Inbound", $"Some error occurred. Program will be terminated.", ex);
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
