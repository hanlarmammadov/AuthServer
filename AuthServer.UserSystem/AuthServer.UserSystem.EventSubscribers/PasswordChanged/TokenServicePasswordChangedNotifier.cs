using AuthServer.Common.Patterns; 
using AuthServer.UserSystem.Models.Events;
using MassTransit; 
using System; 
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.UserSystem.EventSubscribers.PasswordChanged
{
    public class TokenServicePasswordChangedNotifier : IEventSubscriber<PasswordChangedEvent>
    {
        private readonly IPublishEndpoint _massTransitPublishEndpoint;
        private readonly ILogger _logger;

        public TokenServicePasswordChangedNotifier(IPublishEndpoint massTransitPublishEndpoint, ILogger logger)
        {
            _massTransitPublishEndpoint = massTransitPublishEndpoint;
            _logger = logger;
        }

        public async Task HandleEvent(PasswordChangedEvent evnt)
        {
            try
            {
                await _massTransitPublishEndpoint.Publish<PasswordChangedEvent>(evnt); 
            }
            catch(Exception ex)
            {
                //Log error
                _logger.LogError("TokenServicePasswordChangedNotifier.HandleEvent", "Exception was thrown", new
                {
                    PasswordChangedEvent = evnt,
                    Exception = ex
                });

                throw;
            }
        }
    }
}
