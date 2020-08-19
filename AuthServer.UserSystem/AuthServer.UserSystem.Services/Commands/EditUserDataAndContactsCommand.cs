using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Models.Events;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using AuthServer.UserSystem.Services.Models;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Common.Validation;
using AuthServer.Common.Exceptions;
using AuthServer.Infrastructure.Eventing;

namespace AuthServer.UserSystem.Services.Commands
{
    public class EditUserDataAndContactsCommand : EventPublisher<UserDataChangedEvent>, IEditUserDataAndContactsCommand
    {
        private readonly IMongoCollection<User> _userRepo;
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly IValidationStrategy<CreateUserModel> _validationStrategy;
        private readonly ILogger _logger;

        public EditUserDataAndContactsCommand(IMongoCollection<User> userRepo,
                                              IMongoCollection<Account> accountRepo,
                                              IValidationStrategy<CreateUserModel> validationStrategy,
                                              ILogger logger)
        {
            _userRepo = userRepo;
            _accountRepo = accountRepo;
            _validationStrategy = validationStrategy;
            _logger = logger;
        }

        public async Task Execute(CreateUserModel userModel, IValidator validator)
        {
            try
            {
                // Validate.
                _validationStrategy.Validate(userModel, validator);
                if (validator.HasErrors)
                    return;

                ////Retrieve account
                //Account account = await _accountRepo.Find(x => x.AccountId == userModel.AccountId && x.AccountStatus != AccountStatus.Inactive)
                //                                 .SingleOrDefaultAsync();
                //if (account == null)
                //    throw new ObjectNotFoundException("Account not found");

                // Retrieve user.
                User user = await _userRepo.Find(x => x.AccountId == userModel.AccountId).SingleOrDefaultAsync();
                if (user == null)
                    throw new ObjectNotFoundException("User not found");

                // Set update definition.
                UpdateDefinition<User> updateDef = CreateUpdateDefinition(userModel);

                // Update.
                await _userRepo.UpdateOneAsync(x => x.AccountId == userModel.AccountId, updateDef);

                // Publish an event .
                await Publish(CreateEventObj(user));

                return;
            }
            catch (Exception ex)
            {
                // Log.
                await _logger.LogErrorAsync("EditUserDataAndContactsCommand.Execute", "Exception occurred", new
                {
                    UserModel = userModel,
                    Exception = ex
                });

                throw;
            }
        }
        protected UserDataChangedEvent CreateEventObj(User user )
        {
            return new UserDataChangedEvent()
            {
                CorrelationId = Guid.NewGuid().ToString("N"),
                Issuer = "AuthServer.UserSystem.Services.Commands.EditUserDataAndContactsCommand",
                IssuerSystem = "AuthServer.UserSystem",
                EventDate = DateTime.Now,

                AccountId = user.AccountId,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };
        }
        protected UpdateDefinition<User> CreateUpdateDefinition(CreateUserModel userModel)
        {
            var contacts = userModel.Contacts.Select(cm => new Contact() { Type = cm.Type, Value = cm.Value }).ToList();

            var updateDef = Builders<User>.Update.Set(x => x.FirstName, userModel.FirstName)
                                   .Set(x => x.LastName, userModel.LastName)
                                   .Set(x => x.Gender, userModel.Gender)
                                   .Set(x => x.ModifiedDate, DateTime.Now)
                                   .Set(x => x.Contacts, contacts);
            return updateDef;
        }
    }
}
