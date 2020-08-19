using AuthServer.Common.Patterns;
using AuthServer.SecurityTokens.Services.Commands.Interfaces;
using AuthServer.Infrastructure.Eventing;
using TacitusLogger;
using AuthServer.SecurityTokens.Entities;
using AuthServer.SecurityTokens.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthServer.SecurityTokens.Models.Events;

namespace AuthServer.SecurityTokens.Services.Commands
{
    public class RevokeTokenCommand : EventPublisher<RefreshTokenRevokedEvent>, IRevokeTokenCommand
    {
        private readonly IMongoCollection<AccountRTokenInfo> _accountRTokenRepo;
        private readonly ILogger _logger;

        public RevokeTokenCommand(IMongoCollection<AccountRTokenInfo> accountRTokenRepo,
                                  ILogger logger)
        {
            _accountRTokenRepo = accountRTokenRepo;
            _logger = logger;
        }

        protected UpdateDefinition<AccountRTokenInfo> GetUpdateDefinitions()
        {
            return Builders<AccountRTokenInfo>.Update.Set(x => x.Status, AccountRTokenStatus.Revoked)
                                                     .Set(x => x.ModifiedDate, DateTime.Now);
        }

        public async Task Execute(string accountId, string tokenId)
        {
            AccountRTokenInfo token = null;
            try
            {
                // Retrieve all tokens of provided account from db.
                token = await _accountRTokenRepo.Find(x => x.AccountId == accountId && x.TokenId == tokenId).SingleOrDefaultAsync();
                if (token == null)
                    return;

                // Eventing for revoked token.
                await Publish(new RefreshTokenRevokedEvent()
                {
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    IssuerSystem = "AuthServer.SecurityTokens",
                    Issuer = "AuthServer.SecurityTokens.Services.Commands.RevokeTokenCommand",
                    EventDate = DateTime.Now,
                    TokenId = token.TokenId,
                    Expires = token.ExpireDate
                });

                // Update tokens in db.
                _accountRTokenRepo.UpdateOne(x => x.TokenId == tokenId, GetUpdateDefinitions());

                // Log operation.
                _logger.LogEvent("RevokeTokenCommand.Execute", $"Refresh token revoked : {tokenId}", new { Token = token });
            }
            catch (Exception ex)
            {
                // Log error.
                _logger.LogError("RevokeAllAccountTokensCommand.Execute", "Exception was thrown", new
                {
                    TokenId = tokenId,
                    Token = token,
                    Exception = ex
                });

                throw;
            }
        }
    }
}
