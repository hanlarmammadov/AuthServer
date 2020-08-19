using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using MongoDB.Driver;
using System;
using System.Linq;
using AuthServer.Infrastructure.Helpers.Interfaces;
using System.Threading.Tasks;
using AuthServer.Infrastructure.Eventing;
using AuthServer.UserSystem.Models.Events;

namespace AuthServer.UserSystem.Services.Commands
{
    public class ConfirmEmailCommand : IConfirmEmailCommand
    {
        private readonly IMongoCollection<ConfirmEmailRequest> _emailConfirmCollection;
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly ISecretHashHelper _codeHashHelper;
        private readonly int _daysToExpire;
        private readonly ILogger _logger;

        public ConfirmEmailCommand(IMongoCollection<ConfirmEmailRequest> emailConfirmCollection,
                                   IMongoCollection<Account> accountRepo,
                                   ISecretHashHelper codeHashHelper,
                                   int daysToExpire,
                                   ILogger logger)
        {
            _emailConfirmCollection = emailConfirmCollection;
            _accountRepo = accountRepo;
            _codeHashHelper = codeHashHelper;
            _daysToExpire = daysToExpire;
            _logger = logger;
        }

        public async Task<bool> Execute(string code)
        {
            ConfirmEmailRequest request = null;
            string codeHashed = null;
            try
            {
                codeHashed = _codeHashHelper.GenerateHash(code);
                DateTime maxDate = DateTime.Now.AddDays(-_daysToExpire);
                request = (await _emailConfirmCollection.FindAsync(x => x.Code == codeHashed && x.Status == ConfirmEmailRequestStatus.NotResolved && x.CreateDate > maxDate)).SingleOrDefault();

                if (request == null)
                    return false;

                var account = (await _accountRepo.FindAsync(x => x.AccountId == request.AccountId)).SingleOrDefault();

                if (request.Email != account.Email)
                {
                    await _logger.LogInfoAsync("ConfirmEmailCommand.ConfirmEmail", "Request email and account email are different", new
                    {
                        RequestEmail = request.Email,
                        AccountEmail = account.Email
                    });
                    return false;
                }
                if (account.EmailStatus == EmailStatus.Confirmed)
                    return false;

                //update account and save 
                _accountRepo.UpdateOne(x => x.AccountId == account.AccountId, Builders<Account>.Update.Set(x => x.EmailStatus, EmailStatus.Confirmed));

                //update request and save
                var updDef = Builders<ConfirmEmailRequest>.Update.Set(x => x.Status, ConfirmEmailRequestStatus.Resolved)
                                                                 .Set(x => x.ResolveDate, DateTime.Now);
                await _emailConfirmCollection.UpdateOneAsync(x => x.Id == request.Id, updDef);

                //log and return 
                await _logger.LogEventAsync("ConfirmEmailCommand.ConfirmEmail", "Successfully confirmed", new
                {
                    CodeHashed = codeHashed,
                    ConfirmEmailRequest = request
                });
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("ConfirmEmailCommand.ConfirmEmail", "Exception occurred", new
                {
                    Exception = ex,
                    CodeHashed = codeHashed,
                    ConfirmEmailRequest = request
                });
                throw;
            }
        }
    }
}
