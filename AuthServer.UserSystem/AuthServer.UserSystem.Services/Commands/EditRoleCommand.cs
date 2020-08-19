using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using AuthServer.UserSystem.Services.Models;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using AuthServer.Common.Validation;
using TacitusLogger;
using AuthServer.UserSystem.Models.Events;
using AuthServer.Infrastructure.Eventing;

namespace AuthServer.UserSystem.Services.Commands
{
    public class EditRoleCommand : EventPublisher<RoleCreatedOrEditedEvent>, IEditRoleCommand
    {
        private readonly IMongoCollection<Role> _roleRepo;
        private readonly IValidationStrategy<RoleCreateModel> _validationStrategy;
        private readonly ILogger _logger;

        public EditRoleCommand(IMongoCollection<Role> roleRepo, IValidationStrategy<RoleCreateModel> validationStrategy, ILogger logger)
        {
            _roleRepo = roleRepo;
            _validationStrategy = validationStrategy;
            _logger = logger;
        }

        public async Task Execute(string roleId, RoleCreateModel changes, IValidator validator)
        {
            try
            {
                //Validate fields
                _validationStrategy.Validate(changes, validator);
                if (validator.HasErrors)
                    return;

                //Check if role with this name and consumer already exists 
                long count = await _roleRepo.CountDocumentsAsync(x => x.Name == changes.Name && x.Consumer == changes.Consumer && x.Id != roleId);
                if (count != 0)
                {
                    validator.AddError("A role with this name already exists");
                    return;
                }

                IAsyncCursor<Role> cursor = await _roleRepo.FindAsync(x => x.Id == roleId);

                Role role = await cursor.SingleOrDefaultAsync();
                if (role == null)
                    throw new Exception("Role not found");

                var updateDef = Builders<Role>.Update.Set(r => r.Name, changes.Name)
                                                     .Set(r => r.Description, changes.Description)
                                                     .Set(r => r.Consumer, changes.Consumer)
                                                     .Set(r => r.Status, changes.Status);

                //Persist created role
                await _roleRepo.UpdateOneAsync(r => r.Id == roleId, updateDef);

                await Publish(new RoleCreatedOrEditedEvent()
                {
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    IssuerSystem = "AuthServer.UserSystem",
                    Issuer = "AuthServer.UserSystem.Services.Commands.EditRoleCommand",
                    EventDate = DateTime.Now,

                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    Consumer = role.Consumer,
                    RoleIsActive = (role.Status == RoleStatus.Active)
                });

                return;
            }
            catch (Exception ex)
            {
                //Log error
                await _logger.LogErrorAsync("EditRoleCommand.Execute", "Exception was thrown", new
                {
                    RoleId = roleId,
                    Changes = changes,
                    Exception = ex
                });

                throw;
            }
        }

    }
}
