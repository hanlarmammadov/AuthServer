using AuthServer.SecurityTokens.Services.Providers.Interfaces;
using TacitusLogger;
using System;
using System.Threading.Tasks;
using AuthServer.Infrastructure.RabbitMq;
using AuthServer.SecurityTokens.Models.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuthServer.SecurityTokens.RevokedTokensImporter.Consumers
{
    public class TokenInvalidatedEventRabbitMqConsumer : RabbitMqConsumer<RefreshTokenRevokedEvent>
    {
        private readonly ICachedRepo<string> _invalidatedTokenRepo;

        public TokenInvalidatedEventRabbitMqConsumer(ICachedRepo<string> invalidatedTokenRepo,
                                                     IConnection rabbitMqConn,
                                                     RabbitMqQueueConfig queueConfig,
                                                     ILogger logger)
            : base(rabbitMqConn, queueConfig, logger)
        {
            _invalidatedTokenRepo = invalidatedTokenRepo;
        }

        public override async Task ReceiveMessage(IModel model, RefreshTokenRevokedEvent message, BasicDeliverEventArgs e, ILogger logger)
        {
            try
            {
                if (!(await _invalidatedTokenRepo.Exists(message.TokenId)))
                {
                    await _invalidatedTokenRepo.Add(message.TokenId, message.TokenId, message.Expires);
                    _logger.LogInfo("TokenInvalidatedEventRabbitMqConsumer.Consume", $"Token with key: {message.TokenId} added to invalidated tokens.");
                }
                else
                {
                    _logger.LogError("TokenInvalidatedEventRabbitMqConsumer.Consume", $"Token with key: {message.TokenId} already exists.");
                }
            }
            catch (Exception ex)
            {
                // Log error.
                _logger.LogError("TokenInvalidatedEventRabbitMqConsumer.Consume", "Exception was thrown", new
                {
                    RefreshTokenRevokedEvent = message,
                    Exception = ex
                });
            }
        }
    }
}
