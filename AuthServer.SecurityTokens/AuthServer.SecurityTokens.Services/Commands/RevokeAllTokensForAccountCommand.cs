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
    public class RevokeAllTokensForAccountCommand : EventPublisher<RefreshTokenRevokedEvent>, IRevokeAllTokensForAccountCommand
    {
        private readonly IMongoCollection<AccountRTokenInfo> _accountRTokenRepo;
        private readonly ILogger _logger;

        public RevokeAllTokensForAccountCommand(IMongoCollection<AccountRTokenInfo> accountRTokenRepo,
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

        protected FilterDefinition<AccountRTokenInfo> GetFilterDefinition(string accountId)
        {
            return Builders<AccountRTokenInfo>.Filter.And(Builders<AccountRTokenInfo>.Filter.Eq(x => x.AccountId, accountId),
                                                          Builders<AccountRTokenInfo>.Filter.Or(
                                                               Builders<AccountRTokenInfo>.Filter.Eq(x => x.Status, AccountRTokenStatus.Active),
                                                               Builders<AccountRTokenInfo>.Filter.Eq(x => x.Status, AccountRTokenStatus.ClaimsChanged)));
        }

        public async Task Execute(string accountId)
        {
            List<AccountRTokenInfo> tokens = null;
            try
            {
                // Retrieve all tokens of provided account from db.
                tokens = await _accountRTokenRepo.Find(GetFilterDefinition(accountId)).ToListAsync();
                if (tokens == null || tokens.Count == 0)
                    return;

                // Eventing for each revoked token.
                var publishTasks = new List<Task>();
                foreach (var token in tokens)
                    publishTasks.Add(
                    Publish(new RefreshTokenRevokedEvent()
                    {
                        CorrelationId = Guid.NewGuid().ToString("N"),
                        IssuerSystem = "AuthServer.SecurityTokens",
                        Issuer = "AuthServer.SecurityTokens.Services.Commands.RevokeAllTokensForAccountCommand",
                        EventDate = DateTime.Now,
                        TokenId = token.TokenId,
                        Expires = token.ExpireDate
                    }));
                await Task.WhenAll(publishTasks);

                // Update tokens in db.
                await _accountRTokenRepo.UpdateManyAsync(x => x.AccountId == accountId, GetUpdateDefinitions());

                // Log operation.
                await _logger.LogEventAsync("RevokeAllAccountTokensCommand.Execute", $"Refresh tokens revoked for account: {accountId}", new { Tokens = tokens });
            }
            catch (Exception ex)
            {
                // Log error.
                await _logger.LogErrorAsync("RevokeAllAccountTokensCommand.Execute", "Exception was thrown", new
                {
                    AccountId = accountId,
                    Tokens = tokens,
                    Exception = ex
                });

                throw;
            }
        }
    }
}
