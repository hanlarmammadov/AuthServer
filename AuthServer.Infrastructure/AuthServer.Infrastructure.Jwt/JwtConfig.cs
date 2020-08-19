using Microsoft.Extensions.Configuration;
using System;

namespace AuthServer.Infrastructure.Jwt
{
    public class JwtConfig
    {
        private readonly bool _validateAudience;
        private readonly bool _validateIssuer;
        private readonly bool _validateLifetime;
        private readonly string _validIssuer;
        private readonly string _validAudience;
        private readonly int _expiresInMin;

        public JwtConfig(bool validateAudience, bool validateIssuer, bool validateLifetime, string validIssuer, string validAudience, int expiresInMin)
        {
            _validateAudience = validateAudience;
            _validateIssuer = validateIssuer;
            _validateLifetime = validateLifetime;
            _validIssuer = validIssuer;
            _validAudience = validAudience;
            _expiresInMin = expiresInMin;
        }
        public JwtConfig(IConfigurationSection config)
        {
            if (config == null)
                throw new ArgumentNullException("configs");

            _validateAudience = config.GetValue<bool>("validateAudience");
            _validateIssuer = config.GetValue<bool>("validateIssuer");
            _validateLifetime = config.GetValue<bool>("validateLifetime");
            _validIssuer = config["issuer"];
            _validAudience = config["audience"];
            _expiresInMin = config.GetValue<int>("expiresInMin");
        }
        public bool ValidateAudience => _validateAudience;
        public bool ValidateIssuer => _validateIssuer;
        public bool ValidateLifetime => _validateLifetime;
        public string ValidIssuer => _validIssuer;
        public string ValidAudience => _validAudience;
        public int ExpiresInMin => _expiresInMin;
    }
}
