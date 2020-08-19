using AuthServer.Infrastructure.Serialization;
using RabbitMQ.Client;
using System;
using System.Text;
using TacitusLogger;

namespace AuthServer.Infrastructure.RabbitMq
{
    public class RabbitMqProducer<TMessage> : IRabbitMqProducer<TMessage>
    {
        protected readonly ILogger _logger;
        private readonly IConnection _rabbitmqConn;
        private readonly RabbitMqExchangeConfigs _exchangeConfigs;
        private IModel _channel;
        private ISerializer _serializer;

        public RabbitMqProducer(IConnection rabbitmqConn,
                                RabbitMqExchangeConfigs exchangeConfigs,
                                ILogger logger)
        {
            _rabbitmqConn = rabbitmqConn ?? throw new ArgumentNullException("rabbitmqConn");
            _exchangeConfigs = exchangeConfigs ?? throw new ArgumentNullException("exchangeConfigs");
            _logger = logger ?? throw new ArgumentNullException("logger");
            _serializer = new DefaultSerializer();

            //Configure rabbitmq.
            _channel = _rabbitmqConn.CreateModel();
            _channel.ExchangeDeclare(_exchangeConfigs.Name, _exchangeConfigs.Type, _exchangeConfigs.IsDurable);
        }

        public ISerializer Serializer => _serializer;
        public IConnection RabbitMqConnection => _rabbitmqConn;
        internal RabbitMqExchangeConfigs RabbitMqExchangeConfigs => _exchangeConfigs;
        internal ILogger Logger => _logger;
        internal IModel Channel => _channel;

        public void SendMessage(TMessage message, string routingKey = "", IBasicProperties basicProperties = null)
        {
            string json = _serializer.Serialize(message);
            byte[] bytes = Encoding.ASCII.GetBytes(json);
            _channel.BasicPublish(_exchangeConfigs.Name, routingKey, basicProperties, bytes);
        }
        public void ResetSerializer(ISerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException("serializer");
        }
    }
}
