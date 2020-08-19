using AuthServer.Infrastructure.Helpers.Interfaces;
using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Models;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Commands
{
    public class ValidateCredentialsCommand : IValidateCredentialsCommand
    {
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly ILogger _logger;
        private readonly ISecretHashHelper _hashHelper;

        public ValidateCredentialsCommand(IMongoCollection<Account> accountRepo,
                                          ISecretHashHelper hashHelper,
                                          ILogger logger)
        {
            _accountRepo = accountRepo;
            _hashHelper = hashHelper;
            _logger = logger;
        }

        public async Task<(bool Result, string AccountId)> Execute(AuthValidationRequest request)
        {
            try
            {
                if ((request == null) || (request.UsernameOrEmail == null) || (request.Password == null))
                    throw new ArgumentNullException("Credentials were not provided");

                //Retrieve account and check existence
                Account account = _accountRepo.Find(a => a.Username == request.UsernameOrEmail || a.Email == request.UsernameOrEmail)
                                              .FirstOrDefault();

                if (account == null)
                    return (false, null);

                //Validate password
                if (!_hashHelper.ValidateSecret(account.Salt + request.Password, account.Password))
                    return (false, null);

                return (true, account.AccountId);
            }
            catch (Exception ex)
            {
                //Log error
                await _logger.LogErrorAsync("ValidateCredentialsCommand.Execute", "Exception was thrown", new
                {
                    AuthValidationRequest = request,
                    Exception = ex
                });

                throw;
            }
        }
    }
}
