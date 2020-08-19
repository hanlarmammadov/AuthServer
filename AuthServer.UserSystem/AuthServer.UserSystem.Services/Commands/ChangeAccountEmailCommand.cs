using AuthServer.Infrastructure.Eventing;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Models;
using AuthServer.UserSystem.Models.Events;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using AuthServer.UserSystem.Services.Strategies.Interfaces;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using AuthServer.Common.Validation;
using TacitusLogger;

namespace AuthServer.UserSystem.Services.Commands
{
    public class ChangeAccountEmailCommand : EventPublisher<AccountEmailChangedEvent>, IChangeAccountEmailCommand
    {
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly IMongoCollection<EmailChangeRecord> _emailChangeRecordRepo;
        private readonly IValidationStrategy<ChangeEmailModel> _validationStrategy;
        private readonly IEmailConfirmationStrategy _emailConfirmationStrategy;
        private readonly ILogger _logger;

        public ChangeAccountEmailCommand(IMongoCollection<Account> accountRepo,
                                         IMongoCollection<EmailChangeRecord> emailChangeRecordRepo,
                                         IValidationStrategy<ChangeEmailModel> validationStrategy,
                                         IEmailConfirmationStrategy emailConfirmationStrategy,
                                         ILogger logger)
        {
            _accountRepo = accountRepo;
            _emailChangeRecordRepo = emailChangeRecordRepo;
            _validationStrategy = validationStrategy;
            _emailConfirmationStrategy = emailConfirmationStrategy;
            _logger = logger;
        }
         
        public async Task Execute(string accountId, ChangeEmailModel model, IValidator validator)
        {
            try
            {
                //Validation
                _validationStrategy.Validate(model, validator);
                if (validator.HasErrors)
                    return;

                //Check email existence
                if (await _accountRepo.CountDocumentsAsync(x => x.Email == model.NewEmail) != 0)
                {
                    validator.AddError("Email already exists");
                    return;
                }

                //Get account from db
                var accountFilter = AccountFilterDefinition(accountId);
                Account account = await _accountRepo.Find(accountFilter).SingleOrDefaultAsync();
                if (account == null)
                {
                    validator.AddError("Account not found");
                    return;
                }

                //string oldEmail = account.Email;

                //Update account in db
                _accountRepo.UpdateOne(accountFilter, AccountUpdateDefinition(model.NewEmail));

                //Create and save EmailChangeRecord
                EmailChangeRecord emailChangeRecord = new EmailChangeRecord()
                {
                    RecordId = Guid.NewGuid().ToString("n"),
                    AccountId = account.AccountId,
                    OldEmail = account.Email,
                    NewEmail = model.NewEmail,
                    Status = EmailChangeRecordSatus.Changed,
                    CreateDate = DateTime.Now
                };
                _emailChangeRecordRepo.InsertOne(emailChangeRecord);

                //Publish AccountEmailChangedEvent
                await Publish(CreateEvent(model.NewEmail, account, emailChangeRecord.RecordId));

                //Execute confirmation strategy for new email
                await _emailConfirmationStrategy.ImplementConfirmation(account.AccountId);

                return;
            }
            catch (Exception ex)
            {
                //Log exception 
                await _logger.LogErrorAsync("ChangeAccountEmailCommand.Execute", "Exception occurred", new
                {
                    ChangeEmailModel = model,
                    Exception = ex
                });

                //rethrow
                throw;
            }
        }
        protected UpdateDefinition<Account> AccountUpdateDefinition(string newEmail)
        {
            return Builders<Account>.Update.Set(x => x.Email, newEmail)
                                     .Set(x => x.EmailStatus, EmailStatus.NotConfirmed);
        }
        protected FilterDefinition<Account> AccountFilterDefinition(string accountId)
        {
            return Builders<Account>.Filter.And(Builders<Account>.Filter.Eq(x => x.AccountId, accountId),
                                                Builders<Account>.Filter.Ne(x => x.AccountStatus, AccountStatus.Inactive));
        }
        protected AccountEmailChangedEvent CreateEvent(string newEmail, Account account, string emailChangeRecordId)
        {
            return new AccountEmailChangedEvent()
            {
                CorrelationId = Guid.NewGuid().ToString("N"),
                Issuer = "AuthServer.UserSystem.Services.Commands",
                IssuerSystem = "AuthServer.UserSystem",
                EventDate = DateTime.Now,
                EmailChangeRecordId = emailChangeRecordId,
                AccountId = account.AccountId,
                Username = account.Username,
                OldEmailIsConfirmed = account.EmailStatus == EmailStatus.Confirmed,
                OldEmail = account.Email,
                NewEmail = newEmail,
            };
        }
    }
}
