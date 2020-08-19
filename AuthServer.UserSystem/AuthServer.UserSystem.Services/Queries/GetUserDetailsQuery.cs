using AuthServer.Common.Exceptions;
using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Services.Models;
using AuthServer.UserSystem.Services.Queries.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.UserSystem.Services.Queries
{
    public class GetUserDetailsQuery : IGetUserDetailsQuery
    {
        private readonly IMongoCollection<User> _userRepo;
        private readonly IMongoCollection<Role> _roleRepo;
        private readonly ILogger _logger;

        public GetUserDetailsQuery(IMongoCollection<User> userRepo,
                              IMongoCollection<Role> roleRepo,
                              ILogger logger)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _logger = logger;
        }

        protected ProjectionDefinition<Role, Role> GetRoleProjections()
        {
            var roleProjBuilder = Builders<Role>.Projection;
            roleProjBuilder.Include(x => x.Id);
            roleProjBuilder.Include(x => x.Name);
            roleProjBuilder.Include(x => x.Consumer);
            roleProjBuilder.Include(x => x.Description);
            roleProjBuilder.Include(x => x.Status);
            ProjectionDefinition<Role, Role> proj = roleProjBuilder.As<Role>();
            return proj;
        }

        protected async Task<List<RoleCreateModel>> GetUserRoles(List<String> roleIds)
        {
            var roleFilter = Builders<Role>.Filter.In(x => x.Id, roleIds);
            var proj = GetRoleProjections();

            var rQuery = _roleRepo.Find(roleFilter).Project(proj);

            var dblist = await rQuery.ToListAsync();
            return dblist.Select(x => new RoleCreateModel()
            {
                Id = x.Id,
                Consumer = x.Consumer,
                Name = x.Name,
                Description = x.Description,
                Status = x.Status
            }).ToList();
        }

        public async Task<UserDetailedModel> Execute(string accountId)
        {
            try
            { 
                //Get user
                User user = await (await _userRepo.FindAsync(x => x.AccountId == accountId)).SingleOrDefaultAsync();
                if (user == null)
                    throw new ObjectNotFoundException("User not found");

                UserDetailedModel userModel = new UserDetailedModel()
                {
                    AccountId = user.AccountId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    CreateDate = user.CreateDate,
                    ModifiedDate = user.ModifiedDate
                };

                //Fill contacts if any
                if (user.Contacts.Count != 0)
                    userModel.Contacts = user.Contacts.Select(c => new ContactModel() { Type = c.Type, Value = c.Value }).ToList(); 

                //Get user roles if any 
                if (user.RoleIds.Count != 0) 
                    userModel.Roles = await GetUserRoles(user.RoleIds);


                return userModel; 
            }
            catch (ObjectNotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                //Log
                _logger.LogError("GetUserDetailsQuery.Execute", "Exception occurred", new
                {
                    AccountId = accountId,
                    Exception = ex
                });

                throw;
            }
        }
    }
}
