using AuthServer.SecurityTokens.Services.Providers.Interfaces;
using TacitusLogger;
using System;
using System.Threading.Tasks;
using AuthServer.SecurityTokens.Models.Events;
using AuthServer.Infrastructure.RabbitMq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuthServer.SecurityTokens.RevokedTokensImporter.Consumers
{
    public class TokenRevokedEventRabbitMqConsumer : RabbitMqConsumer<RefreshTokenRevokedEvent>
    {
        private readonly ICachedRepo<string> _revokedTokenRepo;

        public TokenRevokedEventRabbitMqConsumer(ICachedRepo<string> revokedTokenRepo,
                                                 IConnection rabbitMqConn,
                                                 RabbitMqQueueConfig queueConfig,
                                                 ILogger logger)
            : base(rabbitMqConn, queueConfig, logger)
        {
            _revokedTokenRepo = revokedTokenRepo;
        }

        public override async Task ReceiveMessage(IModel model, RefreshTokenRevokedEvent message, BasicDeliverEventArgs e, ILogger logger)
        {
            try
            {
                // Check if revoked token already exists in db.
                if (!(await _revokedTokenRepo.Exists(message.TokenId)))
                {
                    await _revokedTokenRepo.Add(message.TokenId, message.TokenId, message.Expires);
                    _logger.LogInfo("TokenRevokedEventRabbitMqConsumer.Consume", $"Token with key: {message.TokenId} added to revoked tokens.");
                }
                else
                {
                    _logger.LogError("TokenRevokedEventRabbitMqConsumer.Consume", $"Token with key: {message.TokenId} already exists.");
                }
            }
            catch (Exception ex)
            {
                // Log error.
                _logger.LogError("TokenRevokedEventRabbitMqConsumer.Consume", "Exception was thrown", new
                {
                    RefreshTokenRevokedEvent = message,
                    Exception = ex
                });
            }
        }
    }
}
