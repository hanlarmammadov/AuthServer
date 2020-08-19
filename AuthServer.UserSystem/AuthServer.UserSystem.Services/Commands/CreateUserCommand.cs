using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using AuthServer.UserSystem.Services.Models;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Common.Validation;
using AuthServer.Infrastructure.Eventing;
using AuthServer.UserSystem.Models.Events;

namespace AuthServer.UserSystem.Services.Commands
{
    public class CreateUserCommand : EventPublisher<UserCreatedEvent>, ICreateUserCommand
    {
        private readonly IValidationStrategy<CreateUserModel> _newUserValidationStrategy;
        private readonly ILogger _logger;
        private readonly IMongoCollection<User> _userRepo;
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly IMongoCollection<Role> _roleRepo;

        public CreateUserCommand(IMongoCollection<User> userRepo,
                                 IMongoCollection<Account> accountRepo,
                                 IMongoCollection<Role> roleRepo,
                                 IValidationStrategy<CreateUserModel> newUserValidationStrategy,
                                 ILogger logger)
        {
            _newUserValidationStrategy = newUserValidationStrategy;
            _logger = logger;
            _userRepo = userRepo;
            _accountRepo = accountRepo;
            _roleRepo = roleRepo;
        }

        protected User CreateUserFromModel(CreateUserModel model, string accountId)
        {
            User user = new User()
            {
                AccountId = accountId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                CreateDate = DateTime.Now,
                RoleIds = model.RoleIds,
                Contacts = model.Contacts.Select(c => new Contact()
                {
                    Type = c.Type,
                    Value = c.Value,
                }).ToList()
            };
            return user;
        }

        public async Task<string> Execute(string accountId, CreateUserModel model, IValidator validator)
        {
            try
            {
                //Validate incoming model
                _newUserValidationStrategy.Validate(model, validator);
                if (validator.HasErrors)
                    return null;

                //Check if user with such account id already exists
                long count = await _userRepo.CountDocumentsAsync(u => u.AccountId == accountId);
                if (count != 0)
                {
                    validator.AddError("User already exists");
                    return null;
                }

                //Check account
                long accountCount = await _accountRepo.CountDocumentsAsync(x => x.AccountId == accountId && x.AccountDataStatus == AccountDataStatus.Completed);
                if (accountCount != 1)
                {
                    validator.AddError("Account is not completed");
                    return null;
                }

                //Check roles
                if (model.RoleIds != null && model.RoleIds.Count != 0)
                {
                    long rolesCount = await _roleRepo.CountDocumentsAsync(Builders<Role>.Filter.In(x => x.Id, model.RoleIds));

                    if (rolesCount != model.RoleIds.Count)
                    {
                        validator.AddError("Invalid roles");
                        return null;
                    }
                }

                //Create user enity
                User user = CreateUserFromModel(model, accountId);

                //Save in db
                await _userRepo.InsertOneAsync(user);

                //Log user creation
                await _logger.LogEventAsync("UserManager.CreateUser", "User created", new
                {
                    AccountId = user.AccountId,
                    FullName = $"{user.FirstName} {user.LastName}"
                });

                // Issue an event.
                await Publish(new UserCreatedEvent()
                {
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    IssuerSystem = "AuthServer.UserSystem",
                    Issuer = "AuthServer.UserSystem.Services.Commands.CreateUserCommand",
                    EventDate = DateTime.Now,

                    AccountId = user.AccountId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RoleIds = user.RoleIds
                });

                return accountId;

            }
            catch (Exception ex)
            {
                //Log exception 
                await _logger.LogErrorAsync("CreateUserFromModel.Execute", "Exception occurred", new
                {
                    AccountId = accountId,
                    UserModel = model,
                    Exception = ex
                });

                //rethrow
                throw;
            }
        }
    }
}
