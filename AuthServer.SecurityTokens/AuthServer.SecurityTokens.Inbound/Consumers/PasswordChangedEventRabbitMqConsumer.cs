using AuthServer.SecurityTokens.Services.Commands.Interfaces;
using TacitusLogger;
using AuthServer.UserSystem.Models.Events;
using System;
using System.Threading.Tasks;
using AuthServer.Infrastructure.RabbitMq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuthServer.SecurityTokens.Inbound.Consumers
{
    public class PasswordChangedEventRabbitMqConsumer : RabbitMqConsumer<PasswordChangedEvent>
    {
        private readonly IRevokeAllTokensForAccountCommand _revokeAllTokensForAccountCommand;

        public PasswordChangedEventRabbitMqConsumer(IRevokeAllTokensForAccountCommand revokeAllTokensForAccountCommand,
                                                    IConnection rabbitMqConn,
                                                    RabbitMqQueueConfig queueConfig,
                                                    ILogger logger)
            : base(rabbitMqConn, queueConfig, logger)
        {

            _revokeAllTokensForAccountCommand = revokeAllTokensForAccountCommand;
        }

        public override async Task ReceiveMessage(IModel model, PasswordChangedEvent message, BasicDeliverEventArgs e, ILogger logger)
        {
            try
            {
                await _revokeAllTokensForAccountCommand.Execute(message.AccountId);
                _logger.LogEvent("PasswordChangedEventConsumer.Consume", $"All tokens revoked for account: {message.AccountId}");
                model.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                //Log error
                await _logger.LogErrorAsync("PasswordChangedEventConsumer.Consume", "Exception was thrown", new
                {
                    Message = message,
                    Exception = ex
                });
            }
        }
    }
}
