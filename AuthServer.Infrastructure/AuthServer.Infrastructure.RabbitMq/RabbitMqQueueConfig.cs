using Microsoft.Extensions.Configuration;
using System;

namespace AuthServer.Infrastructure.RabbitMq
{
    public class RabbitMqQueueConfig
    {
        private readonly string _name;
        private readonly bool _autoAck;

        public RabbitMqQueueConfig(string name, bool autoAck)
        {
            _name = name ?? throw new ArgumentNullException("name");
            _autoAck = autoAck;
        }
        public RabbitMqQueueConfig(IConfigurationSection queueConfigs)
        {
            if (queueConfigs == null)
                throw new ArgumentNullException("queueConfigs");
            _name = queueConfigs["name"];
            _autoAck = queueConfigs.GetValue<bool>("autoAck");
        }

        public string Name => _name;
        public bool AutoAck => _autoAck;
    }
}
