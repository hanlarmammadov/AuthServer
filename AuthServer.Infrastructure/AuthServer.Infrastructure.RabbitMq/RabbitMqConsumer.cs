using AuthServer.Infrastructure.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.Infrastructure.RabbitMq
{
    public abstract class RabbitMqConsumer<TMessage>
    {
        protected readonly ILogger _logger;
        private readonly EventingBasicConsumer _consumer;
        private readonly IModel _model;
        private ISerializer _serializer;
        private readonly RabbitMqQueueConfig _queueConfig;

        public RabbitMqConsumer(IConnection rabbitMqConn, RabbitMqQueueConfig queueConfig, ILogger logger)
        {
            _serializer = new DefaultSerializer();
            _queueConfig = queueConfig ?? throw new ArgumentNullException("queueConfig");
            _logger = logger ?? throw new ArgumentNullException("logger");
            if (rabbitMqConn == null)
                throw new ArgumentNullException("rabbitMqConn");

            try
            {

                //Create rabbitmq model and consumer.
                _model = rabbitMqConn.CreateModel();
                _consumer = new EventingBasicConsumer(_model);
                // Attach the listener.
                _consumer.Received += MessageListener;
                // Begin receiving messages.
                _model.BasicConsume(queue: _queueConfig.Name,
                                    autoAck: _queueConfig.AutoAck,
                                    consumer: _consumer);
            }
            catch (Exception ex)
            {
                _logger.LogError("RabbitMqConsumer.BeginConsume", "Exception occurred when adding listener", new
                {
                    Exception = ex,
                    QueueConfig = _queueConfig
                });
                throw new RabbitMqConsumerException("Error when initializing consumer.", ex);
            }
        }

        internal RabbitMqQueueConfig QueueConfig => _queueConfig;
        internal EventingBasicConsumer Consumer => _consumer;
        internal IModel Model => _model;
        internal ISerializer Serializer => _serializer;
        internal ILogger Logger => _logger;

        public abstract Task ReceiveMessage(IModel model, TMessage message, BasicDeliverEventArgs e, ILogger logger);
        public void ResetSerializer(ISerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException("serializer");
        }
        private async void MessageListener(object sender, BasicDeliverEventArgs e)
        {
            var message = _serializer.Deserialize<TMessage>(e.Body);
            await ReceiveMessage((IModel)sender, message, e, _logger);
        }
    }
}
