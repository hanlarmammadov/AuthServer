using AuthServer.Infrastructure.Eventing;
using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Models.Events;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Commands
{
    public class UndoChangeAccountEmailCommand : EventPublisher<AccountEmailChangeUndoEvent>, IUndoChangeAccountEmailCommand
    {
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly IMongoCollection<EmailChangeRecord> _emailChangeRecordRepo;
        private readonly ILogger _logger;

        public UndoChangeAccountEmailCommand(IMongoCollection<Account> accountRepo,
                                             IMongoCollection<EmailChangeRecord> emailChangeRecordRepo,
                                             ILogger logger)
        {
            _accountRepo = accountRepo;
            _emailChangeRecordRepo = emailChangeRecordRepo;
            _logger = logger;
        }

        public async Task<bool> Execute(string emailChangeRecordId)
        {
            EmailChangeRecord record = null;
            Account account = null;
            try
            {
                //Get record
                record = await _emailChangeRecordRepo.Find(x => x.RecordId == emailChangeRecordId && x.Status == EmailChangeRecordSatus.Changed).SingleOrDefaultAsync();
                if (record == null)
                    return false;

                //Get account
                account = await _accountRepo.Find(x => x.AccountId == record.AccountId).SingleOrDefaultAsync();
                if (account == null)
                    throw new NullReferenceException("Account for given given EmailChangeRecord was not found");

                //Update account's email and email status
                await _accountRepo.UpdateOneAsync(x => x.AccountId == record.AccountId, AccountUpdateDef(record.OldEmail));

                //Update email change record
                await _emailChangeRecordRepo.UpdateOneAsync(x => x.RecordId == emailChangeRecordId, RecordUpdateDef());

                //Publish an event
                await Publish(new AccountEmailChangeUndoEvent()
                {
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    Issuer = "AuthServer.UserSystem.Services.Commands",
                    IssuerSystem = "AuthServer.UserSystem",
                    EventDate = DateTime.Now,

                    AccountId = account.AccountId,
                    Username = account.Username,
                    Email = record.OldEmail
                });

                return true;
            }
            catch (Exception ex)
            {
                //Log exception 
                await _logger.LogErrorAsync("UndoChangeAccountEmailCommand.Execute", "Exception occurred", new
                {
                    EmailChangeRecordId = emailChangeRecordId,
                    EmailChangeRecord = record,
                    Exception = ex
                });

                //rethrow
                throw;
            }
        }
        protected UpdateDefinition<Account> AccountUpdateDef(string newEmail)
        {
            return Builders<Account>.Update.Set(x => x.Email, newEmail)
                                           .Set(x => x.EmailStatus, EmailStatus.Confirmed);
        }
        protected UpdateDefinition<EmailChangeRecord> RecordUpdateDef()
        {
            return Builders<EmailChangeRecord>.Update.Set(x => x.Status, EmailChangeRecordSatus.RolledBack)
                                                     .Set(x => x.UpdateDate, DateTime.Now);
        }
    }
}
