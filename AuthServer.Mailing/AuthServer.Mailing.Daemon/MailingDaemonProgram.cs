using AuthServer.Infrastructure.MailKit;
using AuthServer.Infrastructure.MongoDb;
using AuthServer.Infrastructure.RabbitMq;
using AuthServer.Mailing.Daemon.MailModelFactories;
using AuthServer.Mailing.Infrastructure;
using AuthServer.Mailing.Sender.Managers;
using AuthServer.UserSystem.Models;
using AuthServer.UserSystem.Models.Events;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using TacitusLogger;
using TacitusLogger.Builders;
using TacitusLogger.Destinations.MongoDb;

namespace AuthServer.Mailing.Daemon
{
    class MailingDaemonProgram
    {
        private static IConfigurationRoot _configs;
        private static ILogger _logger;
        private static Mutex _mutex;
        private static List<Object> _rabbitMqConsumers;

        private static void LoadConfigs()
        {
            _configs = new ConfigurationBuilder()
                    .AddJsonFile(".//Settings//app-settings.json")
                    .AddJsonFile(".//Settings//rabbitmq-settings.json")
                    .AddJsonFile(".//Settings//mongodb-settings.json")
                    .AddJsonFile(".//Settings//smtp-settings.json")
                    .Build();
        }
        private static void InitLogger()
        {
            var mongoDbConfigs = new MongoDbConfigs(_configs.GetSection("mongodb"));
            var infoLogCollectionConfig = new MongoCollectionConfig(_configs.GetSection("mongodb").GetSection("logsCollection"));
            MongoClient client = new MongoClient(mongoDbConfigs.ConnectionString);
            IMongoDatabase db = client.GetDatabase(mongoDbConfigs.Database);

            _logger = LoggerBuilder.Logger("Mailing daemon")
                                   .ForAllLogs()
                                   .Console().WithSimpleTemplateLogText("$LogDate(HH:mm:ss) - $LogType - $Description").Add()
                                   .MongoDb().WithCollection(db, infoLogCollectionConfig.Name).Add()
                                   .File().WithPath(".\\logs.txt").Add()
                                   .BuildLogger();
        }
        private static SmtpClient ConfigureSmtpClient(SmtpConfig smtpConfig)
        {
            SmtpClient smtpClient = new SmtpClient()
            {
                Host = smtpConfig.Host,
                Port = smtpConfig.Port,
                EnableSsl = smtpConfig.Ssl,
                Credentials = new System.Net.NetworkCredential(smtpConfig.Username, smtpConfig.Password)
            };

            return smtpClient;
        }
        private static IConnection ConfigureRabbitMqConn()
        {
            var rabbitMqConfigs = new RabbitMqConfigs(_configs.GetSection("rabbitMq"));

            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = rabbitMqConfigs.Host,
                Port = rabbitMqConfigs.Port,
                UserName = rabbitMqConfigs.User,
                Password = rabbitMqConfigs.Password,
            };
            IConnection rabbitmqConn = factory.CreateConnection();
            return rabbitmqConn;
        }
        private static List<Object> ConfigureRabbitMqConsumers()
        {
            var smtpConfig = new SmtpConfig(_configs.GetSection("smtp"));


            var templateFileForConfirmMail = _configs["templateFileForConfirmMail"];
            var templateFileForDirectPassRecovery = _configs["templateFileForDirectPassRecovery"];
            var templateFileForResetLinkPassRecovery = _configs["templateFileForResetLinkPassRecovery"];
            var templateFileForUserClaimsChangedEvent = _configs["templateFileForUserClaimsChangedEvent"];
            var templateFileForPasswordChangedEvent = _configs["templateFileForPasswordChangedEvent"];
            var templateFileForAccountEmailChangedEvent = _configs["templateFileForAccountEmailChangedEvent"];
            var templateFileForAccountEmailChangeUndoEvent = _configs["templateFileForAccountEmailChangeUndoEvent"];

            var rabbitMqConnection = ConfigureRabbitMqConn();


            Func<MailSenderRabbitMqConsumer<ConfirmMailModel>> confirmMailModelConsumerFactory = () =>
            {
                SmtpClient smtpClient = ConfigureSmtpClient(smtpConfig);
                var queueConfig = new RabbitMqQueueConfig(_configs.GetSection("rabbitMq").GetSection("confirmMailQueue"));
                MailSenderRabbitMqConsumer<ConfirmMailModel> consumer = new MailSenderRabbitMqConsumer<ConfirmMailModel>(
                                                                                               new ConfirmMailModelMailModelFactory(smtpConfig, templateFileForConfirmMail),
                                                                                               new SmtpMailSender(smtpClient, _logger),
                                                                                               rabbitMqConnection,
                                                                                               queueConfig,
                                                                                               _logger);
                return consumer;
            };

            // DirectPassRecoveryMailModel  consumer factory.
            Func<MailSenderRabbitMqConsumer<DirectPassRecoveryMailModel>> directPassRecoveryModelConsumerFactory = () =>
            {
                SmtpClient smtpClient = ConfigureSmtpClient(smtpConfig);
                var queueConfig = new RabbitMqQueueConfig(_configs.GetSection("rabbitMq").GetSection("directPassRecoveryMailQueue"));
                return new MailSenderRabbitMqConsumer<DirectPassRecoveryMailModel>(
                                        new DirectPassRecoveryMailModelFactory(smtpConfig, templateFileForDirectPassRecovery),
                                        new SmtpMailSender(smtpClient, _logger),
                                        rabbitMqConnection,
                                        queueConfig,
                                        _logger);
            };

            // ResetLinkPasswordRecoveryMailModel consumer factory.
            Func<MailSenderRabbitMqConsumer<ResetLinkPasswordRecoveryMailModel>> resetLinkPasswordRecoveryModelConsumerFactory = () =>
            {
                SmtpClient smtpClient = ConfigureSmtpClient(smtpConfig);
                var queueConfig = new RabbitMqQueueConfig(_configs.GetSection("rabbitMq").GetSection("resetLinkPasswordRecoveryMailQueue"));
                return new MailSenderRabbitMqConsumer<ResetLinkPasswordRecoveryMailModel>(
                                        new ResetLinkPasswordRecoveryMailModelFactory(smtpConfig, templateFileForResetLinkPassRecovery),
                                        new SmtpMailSender(smtpClient, _logger),
                                        rabbitMqConnection,
                                        queueConfig,
                                        _logger);
            };

            // UserClaimsChangedEvent consumer factory.
            Func<MailSenderRabbitMqConsumer<UserClaimsChangedEvent>> userClaimsChangedEventConsumerFactory = () =>
            {
                SmtpClient smtpClient = ConfigureSmtpClient(smtpConfig);
                var queueConfig = new RabbitMqQueueConfig(_configs.GetSection("rabbitMq").GetSection("userClaimsChangedEventQueue"));
                return new MailSenderRabbitMqConsumer<UserClaimsChangedEvent>(
                                        new UserClaimsChangedEventModelFactory(smtpConfig, templateFileForUserClaimsChangedEvent),
                                        new SmtpMailSender(smtpClient, _logger),
                                        rabbitMqConnection,
                                        queueConfig,
                                        _logger);
            };

            // PasswordChangedEvent consumer factory.
            Func<MailSenderRabbitMqConsumer<PasswordChangedEvent>> passwordChangedEventConsumerFactory = () =>
            {
                SmtpClient smtpClient = ConfigureSmtpClient(smtpConfig);
                var queueConfig = new RabbitMqQueueConfig(_configs.GetSection("rabbitMq").GetSection("passwordChangedEventQueue"));
                return new MailSenderRabbitMqConsumer<PasswordChangedEvent>(
                                        new PasswordChangedEventModelFactory(smtpConfig, templateFileForPasswordChangedEvent),
                                        new SmtpMailSender(smtpClient, _logger),
                                        rabbitMqConnection,
                                        queueConfig,
                                        _logger);
            };

            // AccountEmailChangedEvent consumer factory.
            Func<MailSenderRabbitMqConsumer<AccountEmailChangedEvent>> accountEmailChangedEventConsumerFactory = () =>
            {
                SmtpClient smtpClient = ConfigureSmtpClient(smtpConfig);
                var queueConfig = new RabbitMqQueueConfig(_configs.GetSection("rabbitMq").GetSection("accountEmailChangedEventQueue"));
                return new MailSenderRabbitMqConsumer<AccountEmailChangedEvent>(
                                        new AccountEmailChangedEventMailModelFactory(smtpConfig, templateFileForAccountEmailChangedEvent, "https://example.com/api/v1/account/emailChange"),
                                        new SmtpMailSender(smtpClient, _logger),
                                        rabbitMqConnection,
                                        queueConfig,
                                        _logger);
            };

            // AccountEmailChangedEvent consumer factory.
            Func<MailSenderRabbitMqConsumer<AccountEmailChangeUndoEvent>> accountEmailChangeUndoEventConsumerFactory = () =>
            {
                SmtpClient smtpClient = ConfigureSmtpClient(smtpConfig);
                var queueConfig = new RabbitMqQueueConfig(_configs.GetSection("rabbitMq").GetSection("accountEmailChangeUndoEventQueue"));
                return new MailSenderRabbitMqConsumer<AccountEmailChangeUndoEvent>(
                                        new AccountEmailChangeUndoEventMailModelFactory(smtpConfig, templateFileForAccountEmailChangeUndoEvent),
                                        new SmtpMailSender(smtpClient, _logger),
                                        rabbitMqConnection,
                                        queueConfig,
                                        _logger);
            };

            return new List<object>()
            {
                confirmMailModelConsumerFactory(),
                directPassRecoveryModelConsumerFactory(),
                resetLinkPasswordRecoveryModelConsumerFactory(),
                userClaimsChangedEventConsumerFactory(),
                passwordChangedEventConsumerFactory(),
                accountEmailChangedEventConsumerFactory(),
                accountEmailChangeUndoEventConsumerFactory()
            };
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
        static void Main(string[] args)
        {
            Console.Title = "Mailer daemon";

            LoadConfigs();
            InitLogger();
            try
            {
                var mutexName = _configs["mutexName"];
                bool ownsMutext;
                _mutex = new Mutex(false, mutexName, out ownsMutext);
                if (ownsMutext)
                {
                    _rabbitMqConsumers = ConfigureRabbitMqConsumers();
                    PressToExitLoop();
                }
                else
                    _logger.LogError("MailingDaemonProgram", $"Cannot run the program because another instance is running.");
            }
            catch (Exception ex)
            {
                _logger.LogError("MailingDaemonProgram.MainCatch", $"Some error occurred.", ex);
                throw;
            }

        }
    }
}
