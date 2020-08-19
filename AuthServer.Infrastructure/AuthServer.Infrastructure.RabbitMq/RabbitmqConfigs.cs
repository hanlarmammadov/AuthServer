using Microsoft.Extensions.Configuration;
using System;

namespace AuthServer.Infrastructure.RabbitMq
{
    public class RabbitMqConfigs
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _password;

        public RabbitMqConfigs(string host, int port, string user, string password)
        {
            _host = host;
            _port = port;
            _user = user;
            _password = user;
        }
        public RabbitMqConfigs(IConfigurationSection configSection)
        {
            if (configSection == null)
                throw new ArgumentNullException("configSection");

            _host = configSection["host"];
            _port = configSection.GetValue<int>("port");
            _user = configSection["username"];
            _password = configSection["password"];
        }

        public string Host => _host;
        public string Uri => $"rabbitmq://{_host}:{_port}";
        public int Port => _port;
        public string User => _user;
        public string Password => _password;
    }
}
