using AuthServer.Infrastructure.Eventing;
using AuthServer.Infrastructure.Helpers.Interfaces;
using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Services.Strategies.Interfaces;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.UserSystem.Models.Event;

namespace AuthServer.UserSystem.Services.Strategies
{
    public class ConfirmLinkEmailConfirmationStrategy : EventPublisher<EmailConfirmationByLinkRequestEvent>, IEmailConfirmationStrategy
    {
        private readonly string _confirmLinkUrl;
        private readonly ILogger _logger;
        private readonly IMongoCollection<ConfirmEmailRequest> _requestRepo;
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly ISecretHashHelper _hashHelper;
        private readonly ISecretGenerator _emailConfirmCodeGenerator;

        public ConfirmLinkEmailConfirmationStrategy(IMongoCollection<ConfirmEmailRequest> requestRepo,
                                                    IMongoCollection<Account> accountRepo,
                                                    ISecretHashHelper hashHelper,
                                                    ISecretGenerator emailConfirmCodeGenerator,
                                                    string confirmLinkUrl,
                                                    ILogger logger)
        {
            _requestRepo = requestRepo;
            _accountRepo = accountRepo;
            _hashHelper = hashHelper;
            _emailConfirmCodeGenerator = emailConfirmCodeGenerator;
            _confirmLinkUrl = confirmLinkUrl;
            _logger = logger;
        }

        public async Task ImplementConfirmation(string accountId)
        {
            Account account = null;
            try
            {
                // Get account from db.
                account = _accountRepo.Find(a => a.AccountId == accountId).SingleOrDefault();

                // Generate confirmation code and it's hash
                string emailConfirmCodePlain = _emailConfirmCodeGenerator.Generate();
                string emailConirmCodeHashed = _hashHelper.GenerateHash(emailConfirmCodePlain);

                // Will be saved in db.
                ConfirmEmailRequest confirmEmailRequest = new ConfirmEmailRequest()
                {
                    Id = Guid.NewGuid().ToString("n"),
                    AccountId = account.AccountId,
                    Code = emailConirmCodeHashed,
                    Email = account.Email,
                    CreateDate = DateTime.Now,
                    Status = ConfirmEmailRequestStatus.NotResolved
                };
                _requestRepo.InsertOne(confirmEmailRequest);

                // Will be sent to user's email.
                EmailConfirmationByLinkRequestEvent confMail = new EmailConfirmationByLinkRequestEvent()
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    Issuer = "AuthServer.UserSystem.Services.Strategies.ConfirmLinkEmailConfirmationStrategy",
                    IssuerSystem = "AuthServer.UserSystem",
                    EventDate = DateTime.Now,

                    Username = account.Username,
                    Email = account.Email,
                    ConfirmationUrl = string.Format(_confirmLinkUrl, emailConfirmCodePlain)
                };
                await Publish(confMail);
            }
            catch (Exception ex)
            {
                // Log error.
                _logger.LogError("ConfirmLinkEmailConfirmationStrategy.ImplementConfirmation", "Exception was thrown.", new
                {
                    Account = account,
                    Exception = ex
                });
                throw;
            }
        }
    }
}
