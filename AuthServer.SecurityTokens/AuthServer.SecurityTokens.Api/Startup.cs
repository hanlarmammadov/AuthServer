using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AuthServer.SecurityTokens.Api.StartupConfigs;
using AuthServer.SecurityTokens.Services.StartupConfigs;
using AuthServer.SecurityTokens.Services.Providers;
using RabbitMQ.Client;
using AuthServer.Infrastructure.Serialization;
using AuthServer.Infrastructure.RabbitMq;
using MongoDB.Driver;
using AuthServer.Infrastructure.MongoDb;
using MassTransit;
using AuthServer.UserSystem.Models;
using AuthServer.SecurityTokens.Services.Commands.Interfaces;
using AuthServer.SecurityTokens.Services.Commands;
using AuthServer.Infrastructure.Helpers;
using AuthServer.SecurityTokens.Entities;
using AuthServer.SecurityTokens.Services.EventSubscribers.RevokedTokenEvent;
using AuthServer.SecurityTokens.Data;
using StackExchange.Redis;
using AuthServer.SecurityTokens.Services.Queries;
using AuthServer.SecurityTokens.Services.Queries.Interfaces;
using AuthServer.SecurityTokens.Services.Providers.Interfaces;
using AuthServer.Infrastructure.Redis;
using TacitusLogger;
using TacitusLogger.Builders;
using TacitusLogger.Destinations.MongoDb;
using AuthServer.SecurityTokens.Models.Events;
using AuthServer.UserSystem.Models.MQ;
using AuthServer.Infrastructure.Jwt;

namespace AuthServer.SecurityTokens.Api
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly JwtConfig _rTokenConfig;
        private readonly JwtConfig _sTokenConfig;
        private readonly SymmetricKeyConfig _rTokenKeyConfig;
        private readonly SymmetricKeyConfig _sTokenKeyConfig;
        private readonly RedisConfigs _redisConfigs;
        private readonly RabbitMqConfigs _rabbitmqConfigs;
        private readonly RabbitMqExchangeConfigs _rabbitmqRevokedTokenExchangeConfigs;
        private readonly MongoDbConfigs _mongoDbLogConfigs;
        private readonly MongoCollectionConfig _infoLogCollectionConfig;
        private readonly MongoCollectionConfig _errorLogCollectionConfig;

        private readonly MongoDbConfigs _mongoDbConfigs;
        private readonly MongoCollectionConfig _accountRTokensCollectionConfig;

        private IBusControl _busControl;

        public Startup(IConfiguration config)
        {
            // For unit tests.
            if (config == null)
                return;

            _config = config;
            _rTokenConfig = new JwtConfig(config.GetSection("jwtRefreshToken"));
            _sTokenConfig = new JwtConfig(config.GetSection("jwtShortToken"));
            _rTokenKeyConfig = new SymmetricKeyConfig(config.GetSection("jwtRefreshTokenSigningKey"));
            _sTokenKeyConfig = new SymmetricKeyConfig(config.GetSection("jwtShortTokenSigningKey"));
            _redisConfigs = new RedisConfigs(config.GetSection("redis"));
            _rabbitmqConfigs = new RabbitMqConfigs(config.GetSection("rabbitMq"));
            _rabbitmqRevokedTokenExchangeConfigs = new RabbitMqExchangeConfigs(config.GetSection("rabbitMq").GetSection("revokedTokenExchange"));

            _mongoDbLogConfigs = new MongoDbConfigs(config.GetSection("mongoLogDb"));
            _infoLogCollectionConfig = new MongoCollectionConfig(config.GetSection("mongoLogDb").GetSection("infoLogCollection"));
            _errorLogCollectionConfig = new MongoCollectionConfig(config.GetSection("mongoLogDb").GetSection("errorLogCollection"));

            _mongoDbConfigs = new MongoDbConfigs(config.GetSection("mongoDb"));
            _accountRTokensCollectionConfig = new MongoCollectionConfig(config.GetSection("mongoDb").GetSection("accountRTokenCollection"));
        }

        private ILogger InitLogger(IServiceCollection services)
        {
            MongoClient client = new MongoClient(_mongoDbLogConfigs.ConnectionString);
            IMongoDatabase logdb = client.GetDatabase(_mongoDbLogConfigs.Database);

            var logger = LoggerBuilder.Logger()
                                      .ForAllLogs()
                                      .Console().WithSimpleTemplateLogText("[$LogDate(HH:mm:ss) $Source] - $Description").Add()
                                      .MongoDb().WithCollection(logdb, _infoLogCollectionConfig.Name).Add()
                                      .BuildLogger();

            services.AddSingleton(typeof(ILogger), (serviceProvider) =>
            {
                return logger;
            });

            return logger;
        }

        private IMongoDatabase InitMongoDb()
        {
            MongoMappingsInitializer.Init();
            MongoClient client = new MongoClient(_mongoDbConfigs.ConnectionString);
            IMongoDatabase mongdb = client.GetDatabase(_mongoDbConfigs.Database);
            return mongdb;
        }

        private ConnectionMultiplexer InitRedis()
        {
            ConnectionMultiplexer reddisDbMultiplexer = StackExchange.Redis.ConnectionMultiplexer.Connect(_redisConfigs.ConfigurationOptions);
            return reddisDbMultiplexer;
        }

        private IConnection InitRabbitMqConn()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = _rabbitmqConfigs.Host,
                Port = _rabbitmqConfigs.Port,
                UserName = _rabbitmqConfigs.User,
                Password = _rabbitmqConfigs.Password,
            };
            IConnection rabbitmqConn = factory.CreateConnection();
            return rabbitmqConn;
        }

        private IBusControl InitMassTransit()
        {
            _busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri(_rabbitmqConfigs.Uri), h =>
                {
                    h.Username(_rabbitmqConfigs.User);
                    h.Password(_rabbitmqConfigs.Password);
                });
            });
            _busControl.Start();
            return _busControl;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ILogger logger = InitLogger(services);

            IMongoDatabase mongodb = InitMongoDb();

            ConnectionMultiplexer redisMultiplexer = InitRedis();

            IConnection rabbitMqConn = InitRabbitMqConn();

            IBusControl massTransitBus = InitMassTransit();
            IClientFactory massTransitClientFactory = massTransitBus.CreateClientFactory();


            #region Various

            //ITokenExtractor
            services.AddTransient(typeof(ITokenExtractor), (serviceProvider) =>
            {
                return new TokenExtractor(_rTokenConfig, new SymmetricKeyProvider(_rTokenKeyConfig));
            });

            #endregion

            #region Commands

            //IGenerateRefreshTokenCommand
            services.AddTransient(typeof(IGenerateRefreshTokenCommand), (serviceProvider) =>
            {
                IMongoCollection<AccountRTokenInfo> rtokensRepo = mongodb.GetCollection<AccountRTokenInfo>(_accountRTokensCollectionConfig.Name);

                return new GenerateRefreshTokenCommand(_rTokenConfig,
                                                        new SymmetricKeyProvider(_rTokenKeyConfig),
                                                        new GuidBasedSecretGenerator(),
                                                        rtokensRepo,
                                                        logger);
            });

            //IGenerateShortTokenCommand
            services.AddTransient(typeof(IGenerateShortTokenCommand), (serviceProvider) =>
            {
                var revokedTokensRedisCollectionConfigs = new RedisCollectionConfig(_config.GetSection("redis").GetSection("revokedTokensCollection"));
                RedisCachedRepo<string> revokedTokensRedisRepo = new RedisCachedRepo<string>(redisMultiplexer.GetDatabase(), revokedTokensRedisCollectionConfigs.CollectionName, new DefaultSerializer(), logger);

                return new GenerateShortTokenCommand(new TokenExtractor(_rTokenConfig, new SymmetricKeyProvider(_rTokenKeyConfig)),
                                                     revokedTokensRedisRepo,
                                                     new SymmetricKeyProvider(_sTokenKeyConfig),
                                                     new GuidBasedSecretGenerator(),
                                                     _sTokenConfig,
                                                     logger);
            });

            //IRevokeAllTokensForAccountCommand
            services.AddTransient(typeof(IRevokeAllTokensForAccountCommand), (serviceProvider) =>
            {
                IMongoCollection<AccountRTokenInfo> rtokensRepo = mongodb.GetCollection<AccountRTokenInfo>(_accountRTokensCollectionConfig.Name);

                var revokedTokensRedisCollectionConfigs = new RedisCollectionConfig(_config.GetSection("redis").GetSection("revokedTokensCollection"));
                RedisCachedRepo<string> revokedTokensRedisRepo = new RedisCachedRepo<string>(redisMultiplexer.GetDatabase(), revokedTokensRedisCollectionConfigs.CollectionName, new DefaultSerializer(), logger);

                var command = new RevokeAllTokensForAccountCommand(rtokensRepo, logger);


                var exchangeConfigs = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("refreshTokenRevokedEventExchange"));
                command.AddSubsciber(new RabbitMqEventPublisher<RefreshTokenRevokedEvent>(
                                                                 new RabbitMqProducer<RefreshTokenRevokedEvent>(rabbitMqConn, exchangeConfigs, logger),
                                                                 logger));

                command.AddSubsciber(new RevokedTokenRedisCacher(revokedTokensRedisRepo, logger));
                return command;
            });

            //IRevokeTokenCommand
            services.AddTransient(typeof(IRevokeTokenCommand), (serviceProvider) =>
            {
                IMongoCollection<AccountRTokenInfo> rtokensRepo = mongodb.GetCollection<AccountRTokenInfo>(_accountRTokensCollectionConfig.Name);
                var revokedTokensRedisCollectionConfigs = new RedisCollectionConfig(_config.GetSection("redis").GetSection("revokedTokensCollection"));

                RedisCachedRepo<string> revokedTokensRedisRepo = new RedisCachedRepo<string>(redisMultiplexer.GetDatabase(), revokedTokensRedisCollectionConfigs.CollectionName, new DefaultSerializer(), logger);

                var command = new RevokeTokenCommand(rtokensRepo, logger);

                var exchangeConfigs = new RabbitMqExchangeConfigs(_config.GetSection("rabbitMq").GetSection("refreshTokenRevokedEventExchange"));
                command.AddSubsciber(new RabbitMqEventPublisher<RefreshTokenRevokedEvent>(
                                                                 new RabbitMqProducer<RefreshTokenRevokedEvent>(rabbitMqConn, exchangeConfigs, logger),
                                                                 logger));

                command.AddSubsciber(new RevokedTokenRedisCacher(revokedTokensRedisRepo, logger));

                return command;
            });

            #endregion

            #region Queries

            //IGetAllTokensForAccountQuery
            services.AddTransient(typeof(IGetAllTokensForAccountQuery), (serviceProvider) =>
            {
                IMongoCollection<AccountRTokenInfo> rtokensRepo = mongodb.GetCollection<AccountRTokenInfo>(_accountRTokensCollectionConfig.Name);
                return new GetAllTokensForAccountQuery(rtokensRepo, logger);
            });

            #endregion

            #region MassTransit

            services.AddTransient(typeof(IRequestClient<UserClaimsRequest>), (serviceProvider) =>
            {
                var massTransitChannelName = _config.GetSection("rabbitMq").GetSection("getUserClaimsMassTransitChannel")["Name"];
                var requestClient = massTransitClientFactory.CreateRequestClient<UserClaimsMQRequest>(new Uri($"rabbitmq://{_rabbitmqConfigs.Host}/{massTransitChannelName}"));
                return requestClient;
            });

            services.AddTransient(typeof(IRequestClient<AuthValidationRequest>), (serviceProvider) =>
            {
                var massTransitChannelName = _config.GetSection("rabbitMq").GetSection("validateCredentialsMassTransitChannel")["Name"];
                var requestClient = massTransitClientFactory.CreateRequestClient<AuthValidationMQRequest>(new Uri($"rabbitmq://{_rabbitmqConfigs.Host}/{massTransitChannelName}"));
                return requestClient;
            });

            #endregion

            #region Mvc Framework

            var mvcBuilder = services.AddMvc();
            MvcConfigProvider mvcConfigProvider = new MvcConfigProvider();
            mvcBuilder.AddMvcOptions(mvcConfigProvider.GetMvcOptionsConfigurer())
                      .AddJsonOptions(mvcConfigProvider.GetJsonOptionsConfigurer());
            services.AddSession();
            #endregion
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
