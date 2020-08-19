using AuthServer.Infrastructure.Jwt;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace AuthServer.SecurityTokens.Services.StartupConfigs
{
    public class SymmetricKeyProvider : ISymmetricKeyProvider
    {
        private readonly SymmetricKeyConfig _symmetricKeyConfig;

        public SymmetricKeyProvider(SymmetricKeyConfig symmetricKeyConfig)
        {
            _symmetricKeyConfig = symmetricKeyConfig ?? throw new ArgumentNullException("symmetricKeyConfig");
        }

        internal SymmetricKeyConfig SymmetricKeyConfig => _symmetricKeyConfig;

        public SymmetricSecurityKey GetKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_symmetricKeyConfig.KeyValue));
        }
    }
}
