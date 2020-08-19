using Microsoft.Extensions.Configuration;
using System;

namespace AuthServer.Infrastructure.EntityFramework
{
    public class EFContextConfigs
    {
        private readonly string _connectionString;

        public EFContextConfigs(IConfigurationSection configs)
        {
            if (configs == null)
                throw new ArgumentNullException("configs");

            _connectionString = configs["connString"];
        }

        public string ConnectionString => _connectionString;

    }
}
