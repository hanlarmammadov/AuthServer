using AuthServer.Common.Patterns;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.Infrastructure.RabbitMq
{
    public class RabbitMqEventPublisher<TEvent> : IEventSubscriber<TEvent> where TEvent : class
    {
        protected readonly ILogger _logger;
        private readonly RabbitMqProducer<TEvent> _rabbitMqProducer;
        private readonly Func<TEvent, string> _routingKeyGenerator;
        private readonly IBasicProperties _basicProperties;

        public RabbitMqEventPublisher(RabbitMqProducer<TEvent> rabbitMqProducer, ILogger logger, Func<TEvent, string> routingKeyGenerator, IBasicProperties basicProperties = null)
        {
            _rabbitMqProducer = rabbitMqProducer ?? throw new ArgumentNullException("rabbitMqProducer");
            _logger = logger ?? throw new ArgumentNullException("logger");
            _routingKeyGenerator = routingKeyGenerator ?? throw new ArgumentNullException("routingKeyGenerator");
            _basicProperties = basicProperties;
        }
        public RabbitMqEventPublisher(RabbitMqProducer<TEvent> rabbitMqProducer, ILogger logger)
            : this(rabbitMqProducer, logger, e => string.Empty)
        {

        }

        internal RabbitMqProducer<TEvent> RabbitMqProducer => _rabbitMqProducer;
        internal ILogger Logger => _logger;
        internal Func<TEvent, string> RoutingKeyGenerator => _routingKeyGenerator;
        internal IBasicProperties BasicProperties => _basicProperties;

        public async Task HandleEvent(TEvent evnt)
        {
            try
            {
                _rabbitMqProducer.SendMessage(evnt, _routingKeyGenerator(evnt), _basicProperties);
            }
            catch (Exception ex)
            {
                //Log error
                await _logger.LogErrorAsync("RabbitMqEventPublisher.HandleEvent", "Exception was thrown.", new
                {
                    Exception = ex,
                    RabbitMqExchangeConfigs = RabbitMqProducer.RabbitMqExchangeConfigs
                });

                throw;
            }
        }
    }
}
