using Microsoft.Extensions.Configuration;
using System;

namespace AuthServer.Infrastructure.Jwt
{
    public class SymmetricKeyConfig
    {
        private string _keyValue;

        public SymmetricKeyConfig(string keyValue)
        {
            _keyValue = keyValue;
        }
        public SymmetricKeyConfig(IConfigurationSection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            _keyValue = config["keyValue"];
        }

        public string KeyValue => _keyValue;
    }
}
