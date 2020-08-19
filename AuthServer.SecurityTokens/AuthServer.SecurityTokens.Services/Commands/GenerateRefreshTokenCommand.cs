using AuthServer.SecurityTokens.Services.Commands.Interfaces;
using AuthServer.SecurityTokens.Services.StartupConfigs;
using AuthServer.Infrastructure.Helpers.Interfaces;
using AuthServer.SecurityTokens.Entities;
using AuthServer.SecurityTokens.Models;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using TacitusLogger;
using AuthServer.Infrastructure.Jwt;

namespace AuthServer.SecurityTokens.Services.Commands
{
    public class GenerateRefreshTokenCommand : IGenerateRefreshTokenCommand
    {
        private readonly ILogger _logger;
        private readonly JwtConfig _rTokenConfig;
        private readonly ISymmetricKeyProvider _symmetricKeyProvider;
        private readonly ISecretGenerator _tokenIdGenerator;
        private readonly IMongoCollection<AccountRTokenInfo> _accountRTokenRepo;

        public GenerateRefreshTokenCommand(JwtConfig rTokenConfig,
                                           ISymmetricKeyProvider symmetricKeyProvider,
                                           ISecretGenerator tokenIdGenerator,
                                           IMongoCollection<AccountRTokenInfo> accountRTokenRepo,
                                           ILogger logger)
        {
            _logger = logger;
            _rTokenConfig = rTokenConfig;
            _symmetricKeyProvider = symmetricKeyProvider;
            _tokenIdGenerator = tokenIdGenerator;
            _accountRTokenRepo = accountRTokenRepo;
        }


        public async Task<TokenResult> Execute(string accountId, List<Claim> claims, TokenAdditionalData additionalData, DateTime? customExpireDate = null)
        {
            try
            {
                SymmetricSecurityKey key = _symmetricKeyProvider.GetKey();
                SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                //Generate and add token Jti claim. Will be used in short tokens to identify the creator refresh token
                string tokenJti = _tokenIdGenerator.Generate();
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, tokenJti));

                //Set expire date
                DateTime expireDate;
                if (customExpireDate != null)
                    expireDate = customExpireDate.Value;
                else
                    expireDate = DateTime.Now.AddMinutes(_rTokenConfig.ExpiresInMin);

                JwtSecurityToken jwtTokenOptions = new JwtSecurityToken(
                    issuer: _rTokenConfig.ValidIssuer,
                    audience: _rTokenConfig.ValidAudience,
                    claims: claims,
                    expires: expireDate,
                    signingCredentials: creds
                );

                //Generate token string
                string token = new JwtSecurityTokenHandler().WriteToken(jwtTokenOptions);

                //Create token db record 
                AccountRTokenInfo accountRTokenInfo = new AccountRTokenInfo()
                {
                    TokenId = tokenJti,
                    AccountId = accountId,
                    ExpireDate = expireDate,
                    Status = AccountRTokenStatus.Active,
                    CreateDate = DateTime.Now,
                    DeviceInfo = additionalData.DeviceInfo,
                    RequesterIPv4 = additionalData.RequesterIPv4,
                    RequesterIPv6 = additionalData.RequesterIPv6
                };

                //Save token db record
                await _accountRTokenRepo.InsertOneAsync(accountRTokenInfo);

                return new TokenResult(tokenJti, token, jwtTokenOptions.ValidTo);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("GenerateRefreshTokenCommand.Execute", "Exception was thrown", new
                {
                    Exception = ex
                });

                throw;
            }
        }
    }
}
