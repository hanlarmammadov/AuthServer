using System;

namespace AuthServer.Infrastructure.RabbitMq
{
    public class RabbitMqConsumerException : Exception
    {
        public RabbitMqConsumerException(string message, Exception innerException = null)
            : base(message, innerException)
        {

        }
    }
}
