using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Services.Strategies.Interfaces;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Strategies
{
    public class InstantEmailConfirmationStrategy : IEmailConfirmationStrategy
    {
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly ILogger _logger;

        public InstantEmailConfirmationStrategy(IMongoCollection<Account> accountRepo, ILogger logger)
        { 
            _accountRepo = accountRepo;
            _logger = logger;
        }

        public async Task ImplementConfirmation(string accountId)
        {
            Account account = null;
            try
            {
                //Get account from db
                account = await _accountRepo.Find(a => a.AccountId == accountId).SingleOrDefaultAsync();
               
                //Update Account's EmailStatus field
                var updDef = Builders<Account>.Update.Set(x => x.EmailStatus, EmailStatus.Confirmed);
                await _accountRepo.UpdateOneAsync(a => a.AccountId == accountId, updDef); 
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("InstantEmailConfirmationStrategy.ImplementConfirmation", "Exception was thrown", new
                {
                    AccountId = accountId,
                    Account = account,
                    Exception = ex
                });
                throw;
            }
        } 
    }
}
