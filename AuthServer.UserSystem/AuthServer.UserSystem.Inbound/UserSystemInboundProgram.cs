using AuthServer.Infrastructure.Helpers;
using AuthServer.UserSystem.Data.Mappings;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Inbound.Consumers;
using AuthServer.UserSystem.Services.Commands;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Threading;
using AuthServer.Infrastructure.MongoDb;
using AuthServer.Infrastructure.RabbitMq;
using TacitusLogger;
using TacitusLogger.Builders;
using TacitusLogger.Destinations.MongoDb;
using Topshelf;

namespace AuthServer.UserSystem.Inbound
{
    class UserSystemInboundProgram
    {
        private static IConfigurationRoot _configs;

        private static MongoDbConfigs _mongoDbConfigs;
        private static MongoCollectionConfig _logsCollectionConfig;
        private static RabbitMqConfigs _rabbitmqConfigs;

        private static MongoDbConfigs _mongoDbUserSysDbConfigs;
        private static MongoCollectionConfig _accountsCollectionConfig;
        private static MongoCollectionConfig _usersCollectionConfig;
        private static MongoCollectionConfig _rolesCollectionConfig;

        private static ILogger _logger;
        private static Mutex _mutex;
        private static string _mutexName;

        private static void LoadConfigs()
        {
            _configs = new ConfigurationBuilder()
                    .AddJsonFile(".//Settings//app-settings.json")
                    .AddJsonFile(".//Settings//mongodb-settings.json")
                    .AddJsonFile(".//Settings//rabbitmq-settings.json")
                    .Build();

            _mongoDbConfigs = new MongoDbConfigs(_configs.GetSection("mongodb_logs_config"));
            _logsCollectionConfig = new MongoCollectionConfig(_configs.GetSection("mongodb_logs_config").GetSection("logsCollection"));

            _rabbitmqConfigs = new RabbitMqConfigs(_configs.GetSection("rabbitMq"));

            _mutexName = _configs["mutexName"];

            _mongoDbUserSysDbConfigs = new MongoDbConfigs(_configs.GetSection("mongodb_user_sys_db_config"));
            _accountsCollectionConfig = new MongoCollectionConfig(_configs.GetSection("mongodb_user_sys_db_config").GetSection("accountsCollection"));
            _usersCollectionConfig = new MongoCollectionConfig(_configs.GetSection("mongodb_user_sys_db_config").GetSection("usersCollection"));
            _rolesCollectionConfig = new MongoCollectionConfig(_configs.GetSection("mongodb_user_sys_db_config").GetSection("rolesCollection"));

        }

        private static void InitLogger()
        {
            MongoClient client = new MongoClient(_mongoDbConfigs.ConnectionString);
            IMongoDatabase mongoDb = client.GetDatabase(_mongoDbConfigs.Database);

            _logger = LoggerBuilder.Logger("User system inbound")
                                   .ForAllLogs()
                                   .Console().WithSimpleTemplateLogText("[$LogDate(HH:mm:ss) $Source] - $Description").Add()
                                   .MongoDb().WithCollection(mongoDb, _logsCollectionConfig.Name).Add()
                                   .BuildLogger();
        }

        private static IBusControl ConfigureBus()
        {
            MongoClient userSysClient = new MongoClient(_mongoDbUserSysDbConfigs.ConnectionString);
            MappingsInitializer.InitMappings();
            IMongoDatabase userSysDb = userSysClient.GetDatabase(_mongoDbUserSysDbConfigs.Database);

            Func<ValidateCredentialsConsumer> valCredConsumerFacoryMethod = () =>
            {
                IMongoCollection<Account> accountCollection = userSysDb.GetCollection<Account>(_accountsCollectionConfig.Name);
                IValidateCredentialsCommand validateCredentialsCommand = new ValidateCredentialsCommand(accountCollection,
                                                                                                        new SecretSha256Helper(),
                                                                                                        _logger);
                return new ValidateCredentialsConsumer(validateCredentialsCommand, _logger);
            };

            Func<UserClaimsConsumer> userClaimsConsumerFacoryMethod = () =>
            {
                IMongoCollection<Account> accountCollection = userSysDb.GetCollection<Account>(_accountsCollectionConfig.Name);
                IMongoCollection<User> userCollection = userSysDb.GetCollection<User>(_usersCollectionConfig.Name);
                IMongoCollection<Role> roleCollection = userSysDb.GetCollection<Role>(_rolesCollectionConfig.Name);
                IGetUserClaimsCommand getUserClaimsCommand = new GetUserClaimsCommand(userCollection, accountCollection, roleCollection, _logger);

                return new UserClaimsConsumer(getUserClaimsCommand, _logger);
            };

            var validateCredentialsMassTransitChannel = _configs.GetSection("rabbitMq").GetSection("validateCredentialsMassTransitChannel")["Name"];
            var getUserClaimsMassTransitChannel = _configs.GetSection("rabbitMq").GetSection("getUserClaimsMassTransitChannel")["Name"];

            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(_rabbitmqConfigs.Uri, h =>
                {
                    h.Username(_rabbitmqConfigs.User);
                    h.Password(_rabbitmqConfigs.Password);
                });

                cfg.ReceiveEndpoint(validateCredentialsMassTransitChannel, e =>
                {
                    e.Consumer<ValidateCredentialsConsumer>(valCredConsumerFacoryMethod);
                });

                cfg.ReceiveEndpoint(getUserClaimsMassTransitChannel, e =>
                {
                    e.Consumer<UserClaimsConsumer>(userClaimsConsumerFacoryMethod);
                });
            });
            return busControl;
        }

        static void Main(string[] args)
        {
            Console.Title = "User system inbound.";
            LoadConfigs();
            InitLogger();
            try
            {
                bool ownsMutext;
                _mutex = new Mutex(false, _mutexName, out ownsMutext);
                if (ownsMutext)
                {
                    IBusControl busControl = ConfigureBus();
                    busControl.Start();
                    _logger.LogEvent("UserSystemInboundProgram", $"Program started.");
                    PressToExitLoop(busControl);
                    _logger.LogEvent("UserSystemInboundProgram", $"Program stopped.");
                }
                else
                    _logger.LogError("UserSystemInboundProgram", $"Cannot run the program because another instance is running.");
            }
            catch (Exception ex)
            {
                _logger.LogError("UserSystemInboundProgram.MainCatch", $"Some error occurred.", ex);
                throw;
            }
        }
        private static void PressToExitLoop(IBusControl busControl)
        {
            while (true)
            {
                Console.WriteLine("Press q to exit.");
                if (Console.ReadLine() == "q")
                {
                    busControl.Stop();
                    break;
                }
            }
        }
    }
}

