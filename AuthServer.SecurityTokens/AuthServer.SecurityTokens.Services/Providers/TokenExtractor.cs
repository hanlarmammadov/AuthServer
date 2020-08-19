using AuthServer.Infrastructure.Jwt;
using AuthServer.SecurityTokens.Services.Providers.Interfaces;
using AuthServer.SecurityTokens.Services.StartupConfigs;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace AuthServer.SecurityTokens.Services.Providers
{
    public class TokenExtractor : ITokenExtractor
    {
        private readonly JwtConfig _refreshTokenConfig;
        private readonly ISymmetricKeyProvider _symmetricKeyProvider;

        public TokenExtractor(JwtConfig refreshTokenConfig, ISymmetricKeyProvider symmetricKeyProvider)
        {
            _refreshTokenConfig = refreshTokenConfig ?? throw new ArgumentNullException("refreshTokenConfig");
            _symmetricKeyProvider = symmetricKeyProvider ?? throw new ArgumentNullException("symmetricKeyProvider");
        }

        internal JwtConfig RefreshTokenConfig => _refreshTokenConfig;
        internal ISymmetricKeyProvider SymmetricKeyProvider => _symmetricKeyProvider;

        public bool TryExractToken(string token, out List<Claim> claims)
        {
            claims = null;
            try
            {
                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    ValidIssuer = _refreshTokenConfig.ValidIssuer,
                    ValidAudience = _refreshTokenConfig.ValidAudience,
                    IssuerSigningKey = _symmetricKeyProvider.GetKey()
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;
                ClaimsPrincipal valResult = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
                claims = valResult.Claims.ToList();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
