using Microsoft.Extensions.Configuration;
using System;

namespace AuthServer.Infrastructure.RabbitMq
{
    public class RabbitMqExchangeConfigs
    {
        private readonly string _name;
        private readonly bool _isDurable;
        private readonly string _type;

        public RabbitMqExchangeConfigs(string name, bool isDurable, string type)
        {
            _name = name ?? throw new ArgumentNullException("name");
            _isDurable = isDurable;
            _type = type ?? throw new ArgumentNullException("type");
        }
        public RabbitMqExchangeConfigs(IConfigurationSection exchangeConfigs)
        {
            if (exchangeConfigs == null)
                throw new ArgumentNullException("exchangeConfigs");

            _name = exchangeConfigs["name"];
            _isDurable = exchangeConfigs.GetValue<bool>("isDurable");
            _type = exchangeConfigs["type"];
        }

        public string Name => _name;
        public bool IsDurable => _isDurable;
        public string Type => _type;
    }
}
