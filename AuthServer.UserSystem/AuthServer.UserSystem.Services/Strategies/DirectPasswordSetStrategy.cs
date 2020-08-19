using AuthServer.Infrastructure.Eventing;
using AuthServer.Infrastructure.Helpers.Interfaces; 
using AuthServer.Infrastructure.RabbitMq;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Models;
using AuthServer.UserSystem.Services.Strategies.Interfaces;
using MongoDB.Driver;
using System; 
using System.Linq;
using TacitusLogger;

namespace AuthServer.UserSystem.Services.Strategies
{
    public class DirectPasswordSetStrategy : EventPublisher<DirectPasswordSetEvent>, IPasswordSetStrategy
    {
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly ISecretHashHelper _passwordHashHelper;
        private readonly ISecretGenerator _passwordGenerator;
        private readonly ISecretGenerator _passwordSaltGenerator;
        private readonly ILogger _logger;

        public DirectPasswordSetStrategy(IMongoCollection<Account> accountRepo,
                                         ISecretHashHelper passwordHashHelper,
                                         ISecretGenerator passwordGenerator,
                                         ISecretGenerator passwordSaltGenerator,
                                         ILogger logger)
        {
            _accountRepo = accountRepo;
            _passwordHashHelper = passwordHashHelper;
            _passwordGenerator = passwordGenerator;
            _passwordSaltGenerator = passwordSaltGenerator;
            _logger = logger;
        }

        public void ImplementStrategy(string accountId, string newPasswordPlain)
        {
            Account account = null;
            try
            {
                // Get account from db.
                account = _accountRepo.Find(a => a.AccountId == accountId).SingleOrDefault();
                if (account.EmailStatus != EmailStatus.Confirmed)
                    return;

                // Prepare the password. 
                string passwordPlain = _passwordGenerator.Generate();
                string salt = _passwordSaltGenerator.Generate();
                string passHashed = _passwordHashHelper.GenerateHash(account.Salt + passwordPlain);

                // Update account data.
                UpdateDefinition<Account> updDef = Builders<Account>.Update
                                                             .Set(x => x.PasswordStatus, PasswordStatus.Set)
                                                             .Set(x => x.PasswordLastChanged, DateTime.Now)
                                                             .Set(x => x.Salt, salt)
                                                             .Set(x => x.Password, passHashed);
                _accountRepo.UpdateOne(x => x.AccountId == accountId, updDef);

                // Publish event that may be used, for example, to email the password to the user.
                Publish(new DirectPasswordSetEvent()
                {
                    Email = account.Email,
                    Username = account.Username,
                    PasswordPlain = passwordPlain
                });
            }
            catch (Exception ex)
            {
                // Log error.
                _logger.LogError("DirectPasswordSetStrategy.ImplementStrategy", "Exception was thrown", new
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
