using AuthServer.Common.Exceptions;
using AuthServer.SecurityTokens.Services.Commands.Interfaces;
using AuthServer.SecurityTokens.Services.Providers.Interfaces;
using AuthServer.SecurityTokens.Services.StartupConfigs;
using AuthServer.Infrastructure.Helpers.Interfaces;
using TacitusLogger;
using AuthServer.SecurityTokens.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthServer.Infrastructure.Jwt;
using AuthServer.SecurityTokens.Services.Providers;

namespace AuthServer.SecurityTokens.Services.Commands
{
    public class GenerateShortTokenCommand : IGenerateShortTokenCommand
    {
        private readonly ITokenExtractor _refreshTokenExtracter;
        private readonly ICachedRepo<string> _revokedTokenRepo;
        private readonly ISymmetricKeyProvider _symmetricKeyProvider;
        private readonly ISecretGenerator _tokenIdGenerator;
        private readonly JwtConfig _shortTokenConfig;
        private readonly ILogger _logger;

        public GenerateShortTokenCommand(ITokenExtractor refreshTokenExtracter,
                                         ICachedRepo<string> revokedTokenRepo,
                                         ISymmetricKeyProvider symmetricKeyProvider,
                                         ISecretGenerator tokenIdGenerator,
                                         JwtConfig shortTokenConfig,
                                         ILogger logger)
        {
            _refreshTokenExtracter = refreshTokenExtracter;
            _revokedTokenRepo = revokedTokenRepo;
            _symmetricKeyProvider = symmetricKeyProvider;
            _tokenIdGenerator = tokenIdGenerator;
            _shortTokenConfig = shortTokenConfig;
            _logger = logger;
        }

        public async Task<TokenResult> Execute(string refreshToken)
        {
            try
            {
                //1.Validate and extract refresh token
                if (!_refreshTokenExtracter.TryExractToken(refreshToken, out List<Claim> refreshTokenClaims))
                    throw new InvalidTokenException("Jwt token is corrupted or expired");

                string rTokenJti = refreshTokenClaims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

                //2.Check if refresh token is revoked. If it is throw a security exception
                if (await _revokedTokenRepo.Exists(rTokenJti))
                    throw new TokenRevokedException($"Token {rTokenJti} already revoked");

                //3.Generate short token 
                var key = _symmetricKeyProvider.GetKey();
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                //Create stoken claims using some claims from parent rtoken 
                var shortTokenClaims = refreshTokenClaims.Where(c => c.Type == ClaimTypes.Name || c.Type == ClaimTypes.Role).Select(c => new Claim(c.Type, c.Value)).ToList();
                //Add token Jti claim 
                string jti = _tokenIdGenerator.Generate();
                shortTokenClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, jti));
                //Add RefreshTokenId of the rtoken to the stoken claims as RefreshTokenId claim
                shortTokenClaims.Add(new Claim("rtokenjti", rTokenJti));

                JwtSecurityToken jwtTokenOptions = new JwtSecurityToken(
                       issuer: _shortTokenConfig.ValidIssuer,
                       audience: _shortTokenConfig.ValidAudience,
                       claims: shortTokenClaims,
                       expires: DateTime.Now.AddMinutes(_shortTokenConfig.ExpiresInMin),
                       signingCredentials: creds
                   );

                string shortToken = new JwtSecurityTokenHandler().WriteToken(jwtTokenOptions);

                return new TokenResult(null, shortToken, jwtTokenOptions.ValidTo);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("GenerateShortTokenCommand.Execute", "Exception was thrown", new
                {
                    Exception = ex
                });

                throw;
            }
        }
    }
}
