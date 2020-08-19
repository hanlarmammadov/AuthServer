using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Models.Events;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using AuthServer.UserSystem.Services.Models;
using MongoDB.Driver;
using System.Threading.Tasks;
using AuthServer.Common.Validation;
using System;
using AuthServer.Infrastructure.Eventing;
using AuthServer.Common.Exceptions;

namespace AuthServer.UserSystem.Services.Commands
{
    public class EditUserRolesCommand : EventPublisher<UserRolesChangedEvent>, IEditUserRolesCommand
    {
        private readonly IMongoCollection<User> _userRepo;
        private readonly IMongoCollection<Account> _accountRepo;
        private readonly IMongoCollection<Role> _roleRepo;
        private readonly ILogger _logger;

        public EditUserRolesCommand(IMongoCollection<User> userRepo,
                                    IMongoCollection<Account> accountRepo,
                                    IMongoCollection<Role> roleRepo,
                                    ILogger logger)
        {
            _userRepo = userRepo;
            _accountRepo = accountRepo;
            _roleRepo = roleRepo;
            _logger = logger;
        }

        public async Task Execute(CreateUserModel userModel, IValidator validator)
        {
            try
            {
                // Validate.
                if (userModel.RoleIds.Count != 0)
                {
                    long count = await _roleRepo.CountDocumentsAsync(Builders<Role>.Filter.In(r => r.Id, userModel.RoleIds));
                    if (count != userModel.RoleIds.Count)
                        validator.AddError("Invalid role or roles.");
                }
                if (validator.HasErrors)
                    return;

                // Retrieve account.
                Account account = await _accountRepo.Find(x => x.AccountId == userModel.AccountId && x.AccountStatus != AccountStatus.Inactive)
                                                 .SingleOrDefaultAsync();
                if (account == null)
                    throw new ObjectNotFoundException("Account not found");

                // Retrieve user.
                User user = await _userRepo.Find(x => x.AccountId == userModel.AccountId).SingleOrDefaultAsync();
                if (user == null)
                    throw new ObjectNotFoundException("User not found");

                // Set update definition.
                var updateDef = Builders<User>.Update.Set(x => x.RoleIds, userModel.RoleIds);

                // Update.
                await _userRepo.UpdateOneAsync(x => x.AccountId == userModel.AccountId, updateDef);

                // Publish an event.
                await Publish(new UserRolesChangedEvent()
                {
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    Issuer = "AuthServer.UserSystem.Services.Commands.EditUserRolesCommand",
                    IssuerSystem = "AuthServer.UserSystem",
                    EventDate = DateTime.Now,

                    AccountId = user.AccountId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    RoleIds = user.RoleIds
                });

                return;
            }
            catch (Exception ex)
            {
                // Log.
                await _logger.LogErrorAsync("EditUserRolesCommand.Execute", "Exception occurred", new
                {
                    UserModel = userModel,
                    Exception = ex
                });

                throw;
            }
        }
    }
}
