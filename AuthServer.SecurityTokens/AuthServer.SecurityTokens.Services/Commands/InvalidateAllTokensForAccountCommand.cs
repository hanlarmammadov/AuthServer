using AuthServer.SecurityTokens.Services.Commands.Interfaces;
using AuthServer.Infrastructure.Eventing;
using AuthServer.SecurityTokens.Entities;
using AuthServer.SecurityTokens.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TacitusLogger;
using AuthServer.SecurityTokens.Models.Events;

namespace AuthServer.SecurityTokens.Services.Commands
{
    public class InvalidateAllTokensForAccountCommand : EventPublisher<RefreshTokenRevokedEvent>, IInvalidateAllTokensForAccountCommand
    {
        private readonly IMongoCollection<AccountRTokenInfo> _accountTokenRepo;
        private readonly ILogger _logger;

        public InvalidateAllTokensForAccountCommand(IMongoCollection<AccountRTokenInfo> accountTokenRepo,
                                                    ILogger logger)
        {
            _accountTokenRepo = accountTokenRepo;
            _logger = logger;
        }

        protected UpdateDefinition<AccountRTokenInfo> GetUpdateDefinitions()
        {
            return Builders<AccountRTokenInfo>.Update.Set(x => x.Status, AccountRTokenStatus.ClaimsChanged)
                                                     .Set(x => x.ModifiedDate, DateTime.Now);
        }

        protected FilterDefinition<AccountRTokenInfo> GetFilterDefinition(string accountId)
        {
            return Builders<AccountRTokenInfo>.Filter.And(Builders<AccountRTokenInfo>.Filter.Eq(x => x.AccountId, accountId),
                                                          Builders<AccountRTokenInfo>.Filter.Eq(x => x.Status, AccountRTokenStatus.Active));
        }

        public async Task Execute(string accountId)
        {
            List<AccountRTokenInfo> tokens = null;
            try
            {
                //Retrieve all tokens of provided account from db
                var filterDef = GetFilterDefinition(accountId);
                tokens = await _accountTokenRepo.Find(filterDef).ToListAsync();
                if (tokens == null || tokens.Count == 0)
                    return;

                //Eventing for each revoked token
                var tasksList = new List<Task>();
                foreach (var token in tokens)
                    tasksList.Add(Publish(new RefreshTokenRevokedEvent()
                    {
                        CorrelationId = Guid.NewGuid().ToString("N"),
                        IssuerSystem = "AuthServer.SecurityTokens",
                        Issuer = "AuthServer.SecurityTokens.Services.Commands.InvalidateAllTokensForAccountCommand",
                        EventDate = DateTime.Now,
                        TokenId = token.TokenId,
                        Expires = token.ExpireDate
                    }));
                await Task.WhenAll(tasksList);

                //Update tokens in db
                _accountTokenRepo.UpdateMany(filterDef, GetUpdateDefinitions());

                //Log operation
                _logger.LogEvent("InvalidateAllTokensForAccountCommand.Execute", $"Active refresh tokens invalidated for account: {accountId}", new { Tokens = tokens });

            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("InvalidateAllTokensForAccountCommand.Execute", "Exception was thrown", new
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
