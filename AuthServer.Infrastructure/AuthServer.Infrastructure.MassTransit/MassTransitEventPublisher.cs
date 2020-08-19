using AuthServer.Common.Patterns;
using MassTransit;
using System;
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.Infrastructure.MassTransit
{
    public class MassTransitEventPublisher<TEvent> : IEventSubscriber<TEvent> where TEvent : class
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger _logger;

        public MassTransitEventPublisher(IPublishEndpoint publishEndpoint, ILogger logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task HandleEvent(TEvent evnt)
        {
            try
            {
                await _publishEndpoint.Publish(evnt);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("MassTransitEventPublisher.HandleEvent", "Exception was thrown", new
                {
                    Exception = ex
                });

                throw;
            }
        }
    }
}
