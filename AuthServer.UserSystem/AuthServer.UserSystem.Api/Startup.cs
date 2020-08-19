using AuthServer.Infrastructure.Helpers;
using AuthServer.Infrastructure.MongoDb;
using AuthServer.Infrastructure.RabbitMq;
using AuthServer.UserSystem.Api.StartupConfigs;
using AuthServer.UserSystem.Data.Mappings;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Models.Events;
using AuthServer.UserSystem.Services.Commands;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using AuthServer.UserSystem.Services.Strategies;
using AuthServer.UserSystem.Services.Queries;
using AuthServer.UserSystem.Services.Queries.Interfaces;
using AuthServer.UserSystem.Services.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using RabbitMQ.Client;
using TacitusLogger;
using TacitusLogger.Builders;
using AuthServer.Common.Localization;
using AuthServer.Common.Validation;
using AuthServer.UserSystem.Models.Event;

namespace AuthServer.UserSystem.Api
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly RabbitMqConfigs _rabbitmqConfigs;
        private MongoDbConfigs _mongoDbLogsConfigs;
        private MongoCollectionConfig _infoLogCollectionConfig;
        private MongoCollectionConfig _errorLogCollectionConfig;

        private MongoDbConfigs _mongoDbUserSysDbConfigs;
        private MongoCollectionConfig _accountsCollectionConfig;
        private MongoCollectionConfig _emailConfirmRequestsCollectionConfig;
        private MongoCollectionConfig _roleCollectionConfig;
        private MongoCollectionConfig _userCollectionConfig;


        private string _emailConfirmUrlBase;

        public Startup(IConfiguration config)
        {
            _config = config;
            _rabbitmqConfigs = new RabbitMqConfigs(config.GetSection("rabbitMq"));
            _mongoDbLogsConfigs = new MongoDbConfigs(config.GetSection("mongodb_logs_config"));
            _infoLogCollectionConfig = new MongoCollectionConfig(config.GetSection("mongodb_logs_config").GetSection("infoLogCollection"));
            _errorLogCollectionConfig = new MongoCollectionConfig(config.GetSection("mongodb_logs_config").GetSection("errorLogCollection"));
            _emailConfirmUrlBase = config["emailConfirmUrlBase"];

            _mongoDbUserSysDbConfigs = new MongoDbConfigs(config.GetSection("mongodb_user_sys_db_config"));
            _accountsCollectionConfig = new MongoCollectionConfig(config.GetSection("mongodb_user_sys_db_config").GetSection("accountsCollection"));
            _emailConfirmRequestsCollectionConfig = new MongoCollectionConfig(config.GetSection("mongodb_user_sys_db_config").GetSection("emailConfirmRequestsCollection"));
            _roleCollectionConfig = new MongoCollectionConfig(config.GetSection("mongodb_user_sys_db_config").GetSection("rolesCollection"));
            _userCollectionConfig = new MongoCollectionConfig(config.GetSection("mongodb_user_sys_db_config").GetSection("usersCollection"));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            #region Init logger   
            var logger = LoggerBuilder.Logger()
                                      .ForAllLogs()
                                      .Console().WithSimpleTemplateLogText("[$LogDate(HH:mm:ss) $Source] - $Description").Add()
                                      .BuildLogger();
            #endregion

            //ILogger
            services.AddTransient(typeof(ILogger), (serviceProvider) => logger);

            //IValidatorFactory
            services.AddTransient(typeof(IValidatorFactory), (serviceProvider) =>
            {
                return new ValidatorFactory(new StubCultureProvider());
            });


            //RabbitMq
            RabbitMQ.Client.IConnectionFactory factory = new ConnectionFactory
            {
                HostName = _rabbitmqConfigs.Host,
                Port = _rabbitmqConfigs.Port,
                UserName = _rabbitmqConfigs.User,
                Password = _rabbitmqConfigs.Password,
            };
            RabbitMQ.Client.IConnection rabbitmqConn = factory.CreateConnection();

            MongoClient userSysClient = new MongoClient(_mongoDbUserSysDbConfigs.ConnectionString);
            MappingsInitializer.InitMappings();
            IMongoDatabase userSysDb = userSysClient.GetDatabase(_mongoDbUserSysDbConfigs.Database);

            #region Commands

            services.AddTransient(typeof(ICreateAccountCommand), (serviceProvider) =>
            {
                IMongoCollection<Account> accountCollection = userSysDb.GetCollection<Account>(_accountsCollectionConfig.Name);

                // Email confirmation strategy.
                IMongoCollection<ConfirmEmailRequest> emailConfirmCollection = userSysDb.GetCollection<ConfirmEmailRequest>(_emailConfirmRequestsCollectionConfig.Name);
                var emailConfirmationStrategy = new ConfirmLinkEmailConfirmationStrategy(emailConfirmCollection,
                                                                                   accountCollection,
                                                                                   new SecretSha256Helper(),
                                                                                   new GuidBasedSecretGenerator(32),
                                                                                   _emailConfirmUrlBase,
                                                                                   logger);
                //Add event subscribers to the email confirmation strategy.
                var emailConfirmationByLinkRequestEventExchange = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("emailConfirmationByLinkRequestEventExchange"));
                emailConfirmationStrategy.AddSubsciber(new RabbitMqEventPublisher<EmailConfirmationByLinkRequestEvent>(
                                                                 new RabbitMqProducer<EmailConfirmationByLinkRequestEvent>(rabbitmqConn, emailConfirmationByLinkRequestEventExchange, logger),
                                                                 logger));

                // Create password set strategy.
                var passwordStrategy = new UserAssignedPasswordSetStrategy(accountCollection,
                                                                                      new SecretSha256Helper(),
                                                                                      new GuidBasedSecretGenerator(32),
                                                                                      logger);
                // Create the command.
                var command = new CreateAccountCommand(accountCollection,
                                                       emailConfirmationStrategy,
                                                       passwordStrategy,
                                                       new NewAccountValidationStrategy(),
                                                       logger);
                // Add event subscribers to the command.
                var accountCreatedEventExchangeConfigs = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("accountCreatedEventExchange"));
                command.AddSubsciber(new RabbitMqEventPublisher<NewAccountCreatedEvent>(
                                                               new RabbitMqProducer<NewAccountCreatedEvent>(rabbitmqConn, accountCreatedEventExchangeConfigs, logger),
                                                               logger));

                return command;
            });

            services.AddTransient(typeof(IConfirmEmailCommand), (serviceProvider) =>
            {
                IMongoCollection<Account> accountCollection = userSysDb.GetCollection<Account>(_accountsCollectionConfig.Name);
                IMongoCollection<ConfirmEmailRequest> emailConfirmCollection = userSysDb.GetCollection<ConfirmEmailRequest>(_emailConfirmRequestsCollectionConfig.Name);

                return new ConfirmEmailCommand(emailConfirmCollection, accountCollection, new SecretSha256Helper(), 30, logger);
            });

            services.AddTransient(typeof(ICreateRoleCommand), (serviceProvider) =>
            {
                IMongoCollection<Role> roleCollection = userSysDb.GetCollection<Role>(_roleCollectionConfig.Name);

                var command = new CreateRoleCommand(roleCollection, new NewRoleValidationStrategy(), logger);

                // Add event subscribers to the command.
                var eventExchageConfigs = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("roleCreatedEventExchange"));
                command.AddSubsciber(new RabbitMqEventPublisher<RoleCreatedOrEditedEvent>(
                                                               new RabbitMqProducer<RoleCreatedOrEditedEvent>(rabbitmqConn, eventExchageConfigs, logger),
                                                               logger));

                return command;
            });

            services.AddTransient(typeof(IEditRoleCommand), (serviceProvider) =>
            {
                IMongoCollection<Role> roleCollection = userSysDb.GetCollection<Role>(_roleCollectionConfig.Name);

                var command = new EditRoleCommand(roleCollection, new NewRoleValidationStrategy(), logger);

                // Add event subscribers to the command.
                var eventExchageConfigs = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("roleEditedEventExchange"));
                command.AddSubsciber(new RabbitMqEventPublisher<RoleCreatedOrEditedEvent>(
                                                               new RabbitMqProducer<RoleCreatedOrEditedEvent>(rabbitmqConn, eventExchageConfigs, logger),
                                                               logger));

                return command;
            });

            services.AddTransient(typeof(ICreateUserCommand), (serviceProvider) =>
            {
                IMongoCollection<User> userCollection = userSysDb.GetCollection<User>(_userCollectionConfig.Name);
                IMongoCollection<Account> accountCollection = userSysDb.GetCollection<Account>(_accountsCollectionConfig.Name);
                IMongoCollection<Role> roleCollection = userSysDb.GetCollection<Role>(_roleCollectionConfig.Name);

                var command = new CreateUserCommand(userCollection,
                                                    accountCollection,
                                                    roleCollection,
                                                    new NewUserValidationStrategy(new NewContactValidationStrategy()),
                                                    logger);
                // Add event subscribers to the command.
                var eventExchageConfigs = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("userCreatedEventExchange"));
                command.AddSubsciber(new RabbitMqEventPublisher<UserCreatedEvent>(
                                                               new RabbitMqProducer<UserCreatedEvent>(rabbitmqConn, eventExchageConfigs, logger),
                                                               logger));
                return command;
            });

            services.AddTransient(typeof(IEditUserRolesCommand), (serviceProvider) =>
            {
                IMongoCollection<User> userCollection = userSysDb.GetCollection<User>(_userCollectionConfig.Name);
                IMongoCollection<Account> accountCollection = userSysDb.GetCollection<Account>(_accountsCollectionConfig.Name);
                IMongoCollection<Role> roleCollection = userSysDb.GetCollection<Role>(_roleCollectionConfig.Name);

                var command = new EditUserRolesCommand(userCollection, accountCollection, roleCollection, logger);

                //Add event subscribers to this command.
                var exchangeConfigs = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("userRolesChangedEventExchange"));
                command.AddSubsciber(new RabbitMqEventPublisher<UserRolesChangedEvent>(
                                                                 new RabbitMqProducer<UserRolesChangedEvent>(rabbitmqConn, exchangeConfigs, logger),
                                                                 logger));
                return command;
            });

            services.AddTransient(typeof(IEditUserDataAndContactsCommand), (serviceProvider) =>
            {
                IMongoCollection<User> userCollection = userSysDb.GetCollection<User>(_userCollectionConfig.Name);
                IMongoCollection<Account> accountCollection = userSysDb.GetCollection<Account>(_accountsCollectionConfig.Name);
                IMongoCollection<Role> roleCollection = userSysDb.GetCollection<Role>(_roleCollectionConfig.Name);

                var command = new EditUserDataAndContactsCommand(userCollection,
                                                                 accountCollection,
                                                                 new UserDataAndContactsEditValidationStrategy(new NewContactValidationStrategy()),
                                                                 logger);
                //Add event subscribers to this command.
                var exchangeConfigs = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("userDataChangedEventExchange"));
                command.AddSubsciber(new RabbitMqEventPublisher<UserDataChangedEvent>(
                                                                 new RabbitMqProducer<UserDataChangedEvent>(rabbitmqConn, exchangeConfigs, logger),
                                                                 logger));
                return command;
            });

            services.AddTransient(typeof(IChangeAccountPasswordCommand), (serviceProvider) =>
            {
                IMongoCollection<Account> accountCollection = userSysDb.GetCollection<Account>(_accountsCollectionConfig.Name);

                var command = new ChangeAccountPasswordCommand(accountCollection,
                                                               new PasswordChangeValidationStrategy(),
                                                               new SecretSha256Helper(),
                                                               new GuidBasedSecretGenerator(),
                                                               logger);
                //Add event subscribers to this command.
                var exchangeConfigs = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("passwordChangedEventExchange"));
                command.AddSubsciber(new RabbitMqEventPublisher<PasswordChangedEvent>(
                                                                 new RabbitMqProducer<PasswordChangedEvent>(rabbitmqConn, exchangeConfigs, logger),
                                                                 logger));
                return command;
            });

            //IChangeAccountEmailCommand
            services.AddTransient(typeof(IChangeAccountEmailCommand), (serviceProvider) =>
            {
                IMongoCollection<Account> accountCollection = userSysDb.GetCollection<Account>(_accountsCollectionConfig.Name);
                IMongoCollection<EmailChangeRecord> emailChangedRecordCollection = userSysDb.GetCollection<EmailChangeRecord>("emailChangeRecords");
                IMongoCollection<ConfirmEmailRequest> emailConfirmCollection = userSysDb.GetCollection<ConfirmEmailRequest>(_emailConfirmRequestsCollectionConfig.Name);

                var emailConfirmationStrategy = new ConfirmLinkEmailConfirmationStrategy(emailConfirmCollection,
                                                                                         accountCollection,
                                                                                         new SecretSha256Helper(),
                                                                                         new GuidBasedSecretGenerator(32),
                                                                                         _emailConfirmUrlBase,
                                                                                         logger);
                //Add event subscribers to this strategy.
                var newEmailAddedEventExchageConfigs = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("emailConfirmationByLinkRequestEventExchange"));
                emailConfirmationStrategy.AddSubsciber(new RabbitMqEventPublisher<EmailConfirmationByLinkRequestEvent>(
                                                                 new RabbitMqProducer<EmailConfirmationByLinkRequestEvent>(rabbitmqConn, newEmailAddedEventExchageConfigs, logger),
                                                                 logger));

                var command = new ChangeAccountEmailCommand(accountCollection,
                                                            emailChangedRecordCollection,
                                                            new EmailValidationStrategy(),
                                                            emailConfirmationStrategy,
                                                            logger);
                //Add event subscribers to this command.
                var exchangeConfigs = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("accountEmailChangedEventExchange"));
                command.AddSubsciber(new RabbitMqEventPublisher<AccountEmailChangedEvent>(
                                                                 new RabbitMqProducer<AccountEmailChangedEvent>(rabbitmqConn, exchangeConfigs, logger),
                                                                 logger));
                return command;
            });

            //IUndoChangeAccountEmailCommand
            services.AddTransient(typeof(IUndoChangeAccountEmailCommand), (serviceProvider) =>
            {
                IMongoCollection<Account> accountCollection = userSysDb.GetCollection<Account>(_accountsCollectionConfig.Name);
                IMongoCollection<EmailChangeRecord> emailChangedRecordCollection = userSysDb.GetCollection<EmailChangeRecord>("emailChangeRecords");

                var command = new UndoChangeAccountEmailCommand(accountCollection,
                                                            emailChangedRecordCollection,
                                                            logger);
                //Add event subscribers to this command.
                var exchangeConfigs = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("accountEmailChangeUndoEvent"));
                command.AddSubsciber(new RabbitMqEventPublisher<AccountEmailChangeUndoEvent>(
                                                                 new RabbitMqProducer<AccountEmailChangeUndoEvent>(rabbitmqConn, exchangeConfigs, logger),
                                                                 logger));
                return command;
            });

            #endregion

            #region Queries

            services.AddTransient(typeof(IGetUsersQuery), (serviceProvider) =>
            {
                IMongoCollection<User> userCollection = userSysDb.GetCollection<User>(_userCollectionConfig.Name);

                return new GetUsersQuery(userCollection, logger);
            });

            services.AddTransient(typeof(IGetUserDetailsQuery), (serviceProvider) =>
            {
                IMongoCollection<User> userCollection = userSysDb.GetCollection<User>(_userCollectionConfig.Name);
                IMongoCollection<Role> roleCollection = userSysDb.GetCollection<Role>(_roleCollectionConfig.Name);

                return new GetUserDetailsQuery(userCollection, roleCollection, logger);
            });

            services.AddTransient(typeof(IGetRolesQuery), (serviceProvider) =>
            {
                IMongoCollection<Role> roleCollection = userSysDb.GetCollection<Role>(_roleCollectionConfig.Name);

                return new GetRolesQuery(roleCollection, logger);
            });

            #endregion

            #region MVC Framework

           var mvcBuilder = services.AddMvc(opt =>
            {
                opt.ModelBinderProviders.Insert(0, new IntToBoolModelBinderProvider());
            })
            .AddSessionStateTempDataProvider(); 
           
            services.AddSession();
            MvcConfigProvider mvcConfigProvider = new MvcConfigProvider();

            mvcBuilder.AddMvcOptions(mvcConfigProvider.GetMvcOptionsConfigurer())
                      .AddJsonOptions(mvcConfigProvider.GetJsonOptionsConfigurer());


            #endregion

            services.AddLogging();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
            }
            else
            {
                app.UseExceptionHandler();
            }

            app.UseAuthentication();
            app.UseStaticFiles(new StaticFileOptions()
            {
                ServeUnknownFileTypes = false,
                OnPrepareResponse = (StaticFileResponseContext ctx) =>
                {

                }
            });
            app.UseSession();
            app.UseMvc();
        }
    }
}
