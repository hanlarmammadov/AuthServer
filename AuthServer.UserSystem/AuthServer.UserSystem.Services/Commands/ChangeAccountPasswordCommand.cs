using AuthServer.Infrastructure.Eventing;
using AuthServer.Infrastructure.Helpers.Interfaces;
using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Models;
using AuthServer.UserSystem.Models.Events;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using AuthServer.Common.Validation;


namespace AuthServer.UserSystem.Services.Commands
{
    public class ChangeAccountPasswordCommand : EventPublisher<PasswordChangedEvent>, IChangeAccountPasswordCommand
    {
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly IValidationStrategy<PasswordChangeModel> _validationStrategy;
        private readonly ISecretHashHelper _secretHashHelper;
        private readonly ISecretGenerator _passwordSaltGenerator;
        private readonly ILogger _logger;

        public ChangeAccountPasswordCommand(IMongoCollection<Account> accountRepo,
                                            IValidationStrategy<PasswordChangeModel> validationStrategy,
                                            ISecretHashHelper secretHashHelper,
                                            ISecretGenerator passwordSaltGenerator,
                                            ILogger logger)
        {
            _accountRepo = accountRepo;
            _validationStrategy = validationStrategy;
            _secretHashHelper = secretHashHelper;
            _passwordSaltGenerator = passwordSaltGenerator;
            _logger = logger;
        }

        public async Task Execute(string accountId, PasswordChangeModel model, IValidator validator)
        {
            try
            {
                //Validate input
                _validationStrategy.Validate(model, validator);
                if (validator.HasErrors)
                    return;

                //Validate old password and find account
                Account account = await _accountRepo.Find(x => x.AccountId == accountId && x.AccountStatus != AccountStatus.Inactive).SingleOrDefaultAsync();
                if (account == null ||
                    account.PasswordStatus == PasswordStatus.Empty ||
                    !_secretHashHelper.ValidateSecret(account.Salt + model.OldPassword, account.Password))
                {
                    validator.AddError("Wrong old password", "OldPassword");
                    return;
                }

                //Generate new salt
                string newSalt = _passwordSaltGenerator.Generate();

                //Generate new hashed password 
                string newHashedPass = _secretHashHelper.GenerateHash(newSalt + model.NewPassword);

                //Save in db
                _accountRepo.UpdateOne(x => x.AccountId == accountId, CreateUpdateDef(newHashedPass, newSalt));

                //Raise a PasswordChanged event
                PasswordChangedEvent passwordChangedEvent = CreatePasswordChangedEvent(account);
                await Publish(passwordChangedEvent);

                return;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("ChangeAccountPasswordCommand.Execute", "Exception occurred", new
                {
                    Exception = ex
                });
                throw;
            }
        }
        protected UpdateDefinition<Account> CreateUpdateDef(string newHashedPass, string newSalt)
        {
            return Builders<Account>.Update.Set(x => x.Password, newHashedPass)
                                           .Set(x => x.Salt, newSalt)
                                           .Set(x => x.PasswordLastChanged, DateTime.Now)
                                           .Set(x => x.PasswordStatus, PasswordStatus.Set);
        }
        protected PasswordChangedEvent CreatePasswordChangedEvent(Account account)
        {
            return new PasswordChangedEvent()
            {
                CorrelationId = Guid.NewGuid().ToString("N"),
                Issuer = "AuthServer.UserSystem.Services.Commands.ChangeAccountPasswordCommand",
                IssuerSystem = "AuthServer.UserSystem",
                EventDate = DateTime.Now,

                AccountId = account.AccountId,
                Username = account.Username,
                EmailIsConfirmed = account.EmailStatus == EmailStatus.Confirmed,
                Email = account.Email
            };
        }
    }
}
