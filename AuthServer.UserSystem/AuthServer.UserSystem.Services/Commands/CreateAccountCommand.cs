using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using AuthServer.UserSystem.Services.Models;
using AuthServer.UserSystem.Services.Strategies.Interfaces;
using MongoDB.Driver;
using System;
using AuthServer.Common.Validation;
using AuthServer.Infrastructure.Eventing;
using AuthServer.UserSystem.Models.Events;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Commands
{
    public class CreateAccountCommand : EventPublisher<NewAccountCreatedEvent>, ICreateAccountCommand
    {
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly IEmailConfirmationStrategy _emailConfirmationStrategy;
        private readonly IPasswordSetStrategy _passwordSetStrategy;
        private readonly IValidationStrategy<AccountModel> _newAccountValidationStrategy;
        private readonly ILogger _logger;

        public CreateAccountCommand(IMongoCollection<Account> accountRepo,
                                    IEmailConfirmationStrategy emailConfirmationStrategy,
                                    IPasswordSetStrategy passwordSetStrategy,
                                    IValidationStrategy<AccountModel> newAccountValidationStrategy,
                                    ILogger logger)
        {
            _accountRepo = accountRepo;
            _emailConfirmationStrategy = emailConfirmationStrategy;
            _passwordSetStrategy = passwordSetStrategy;
            _newAccountValidationStrategy = newAccountValidationStrategy;
            _logger = logger;
        }


        public async Task<string> Execute(AccountModel accountModel, IValidator validator)
        {
            try
            {
                // Data validation.
                _newAccountValidationStrategy.Validate(accountModel, validator);
                if (validator.HasErrors)
                    return null;

                // Create an Account entity.
                Account accountStub = new Account()
                {
                    AccountId = Guid.NewGuid().ToString("n"),
                    Username = accountModel.Username.ToLower(),
                    Email = accountModel.Email.ToLower(),
                    AccountDataStatus = AccountDataStatus.Completed,
                    AccountStatus = AccountStatus.Active,
                    PasswordStatus = PasswordStatus.Empty,
                    EmailStatus = EmailStatus.NotConfirmed,
                    AccountCreated = DateTime.Now
                };

                // Save the account in the database.
                await _accountRepo.InsertOneAsync(accountStub);
                accountModel.Id = accountStub.AccountId;

                // Execute email confirmation logic.
                await _emailConfirmationStrategy.ImplementConfirmation(accountStub.AccountId);

                // Password setting strategy. model.Password will or not be use depending on strategy.
                _passwordSetStrategy.ImplementStrategy(accountStub.AccountId, accountModel.Password);

                // Issue a NewAccountCreatedEvent event.
                NewAccountCreatedEvent newAccountCreatedEvent = new NewAccountCreatedEvent()
                {
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    Issuer = "AuthServer.UserSystem.Services.Commands.CreateAccountCommand",
                    IssuerSystem = "AuthServer.UserSystem",
                    EventDate = DateTime.Now,
                    AccountId = accountStub.AccountId,
                    Username = accountStub.Username,
                    Email = accountStub.Email
                };

                await Publish(newAccountCreatedEvent);

                return accountStub.AccountId;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("CreateAccountCommand.Execute", "Exception occurred", new
                {
                    Exception = ex,
                    AccountModel = accountModel,
                    Validator = validator
                });
                throw;
            }
        }
    }
}
