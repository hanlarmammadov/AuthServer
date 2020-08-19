using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using AuthServer.UserSystem.Services.Models;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using AuthServer.Common.Validation;
using AuthServer.UserSystem.Models.Events;
using AuthServer.Infrastructure.Eventing;

namespace AuthServer.UserSystem.Services.Commands
{
    public class CreateRoleCommand : EventPublisher<RoleCreatedOrEditedEvent>, ICreateRoleCommand
    {
        private readonly IMongoCollection<Role> _roleRepo;
        private readonly IValidationStrategy<RoleCreateModel> _validationStrategy;
        private readonly ILogger _logger;

        public CreateRoleCommand(IMongoCollection<Role> roleRepo, IValidationStrategy<RoleCreateModel> validationStrategy, ILogger logger)
        {
            _roleRepo = roleRepo;
            _validationStrategy = validationStrategy;
            _logger = logger;
        }

        public async Task<string> Execute(RoleCreateModel roleModel, IValidator validator)
        {
            try
            {
                // Validate fields.
                _validationStrategy.Validate(roleModel, validator);
                if (validator.HasErrors)
                    return null;

                // Check if role with this name and consumer already exists.
                long count = await _roleRepo.CountDocumentsAsync(x => x.Name == roleModel.Name && x.Consumer == roleModel.Consumer);
                if (count != 0)
                {
                    validator.AddError("A role with this name already exists");
                    return null;
                }

                // Create role entity.
                Role role = new Role()
                {
                    Id = roleModel.Id,
                    Name = roleModel.Name,
                    Description = roleModel.Description,
                    Consumer = roleModel.Consumer,
                    Status = roleModel.Status
                };

                // Persist created role.
                await _roleRepo.InsertOneAsync(role);

                // Issue an event describing the created role.
                await Publish(new RoleCreatedOrEditedEvent()
                {
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    IssuerSystem = "AuthServer.UserSystem",
                    Issuer = "AuthServer.UserSystem.Services.Commands.CreateRoleCommand",
                    EventDate = DateTime.Now,

                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    Consumer = role.Consumer,
                    RoleIsActive = (role.Status == RoleStatus.Active)
                });
                 
                // Return role id.
                return role.Id;
            }
            catch (Exception ex)
            {
                //Log error
                await _logger.LogErrorAsync("CreateRoleCommand.Execute", "Exception was thrown", new
                {
                    RoleModel = roleModel,
                    Exception = ex
                });

                throw;
            }
        }
    }
}
