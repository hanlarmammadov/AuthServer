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
    public class UserRolesChangedEventRabbitMqConsumer : RabbitMqConsumer<UserRolesChangedEvent>
    {
        private readonly IInvalidateAllTokensForAccountCommand _invalidateAllTokensForAccountCommand;

        public UserRolesChangedEventRabbitMqConsumer(IInvalidateAllTokensForAccountCommand invalidateAllTokensForAccountCommand,
                                                      IConnection rabbitMqConn,
                                                      RabbitMqQueueConfig queueConfig,
                                                      ILogger logger)
            : base(rabbitMqConn, queueConfig, logger)
        {

            _invalidateAllTokensForAccountCommand = invalidateAllTokensForAccountCommand;
        }

        public override async Task ReceiveMessage(IModel model, UserRolesChangedEvent message, BasicDeliverEventArgs e, ILogger logger)
        {
            try
            {
                await _invalidateAllTokensForAccountCommand.Execute(message.AccountId);
                _logger.LogEvent("UserClaimsChangedEventConsumer.Consume", $"All active tokens invalidated for account: {message.AccountId}");
                model.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                //Log error
                await _logger.LogErrorAsync("UserClaimsChangedEventConsumer.Consume", "Exception was thrown", new
                {
                    Message = message,
                    Exception = ex
                });
            }
        }
    }
}
