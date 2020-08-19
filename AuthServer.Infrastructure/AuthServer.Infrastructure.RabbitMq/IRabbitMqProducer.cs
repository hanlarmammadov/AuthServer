
using RabbitMQ.Client;

namespace AuthServer.Infrastructure.RabbitMq
{
    public interface IRabbitMqProducer<TMessage>
    {
        void SendMessage(TMessage message, string routingKey = "", IBasicProperties basicProperties = null);
    }
}
