using AuthServer.Infrastructure.Helpers.Interfaces;
using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities; 
using AuthServer.UserSystem.Services.Strategies.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Strategies
{
    public class UserAssignedPasswordSetStrategy : IPasswordSetStrategy
    {
        private readonly IMongoCollection<Account> _accountRepo; 
        private readonly ISecretHashHelper _passwordHashHelper; 
        private readonly ISecretGenerator _passwordSaltGenerator;
        private readonly ILogger _logger;

        public UserAssignedPasswordSetStrategy(IMongoCollection<Account> accountRepo,
                                               ISecretHashHelper passwordHashHelper,
                                               ISecretGenerator passwordSaltGenerator,
                                               ILogger logger)
        {
            _accountRepo = accountRepo; 
            _passwordHashHelper = passwordHashHelper; 
            _passwordSaltGenerator = passwordSaltGenerator;
            _logger = logger;
        }

        public void ImplementStrategy(string accountId, string newPasswordPlain)
        {
            Account account = null;
            try
            {
                //Get account from db
                account = _accountRepo.Find(a => a.AccountId == accountId).SingleOrDefault();
              
                //Password  
                string salt = _passwordSaltGenerator.Generate();
                string passHashed = _passwordHashHelper.GenerateHash(salt + newPasswordPlain);

                //Update account data
                UpdateDefinition<Account> updDef = Builders<Account>.Update
                                                             .Set(x => x.PasswordStatus, PasswordStatus.Set)
                                                             .Set(x => x.PasswordLastChanged, DateTime.Now)
                                                             .Set(x => x.Salt, salt)
                                                             .Set(x => x.Password, passHashed);

                _accountRepo.UpdateOne(x => x.AccountId == accountId, updDef);  
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("UserAssignedPasswordSetStrategy.ImplementStrategy", "Exception was thrown", new
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
