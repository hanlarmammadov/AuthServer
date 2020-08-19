using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Services.Models;
using AuthServer.UserSystem.Services.Queries.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Common.Messages;
using System.Linq.Expressions;

namespace AuthServer.UserSystem.Services.Queries
{
    public class GetRolesQuery : MongoListQueryBase, IGetRolesQuery
    {
        private readonly IMongoCollection<Role> _roleRepo;
        private readonly ILogger _logger;
        private Dictionary<string, Expression<Func<Role, object>>> _sortDictionary;

        public GetRolesQuery(IMongoCollection<Role> roleRepo, ILogger logger)
        {
            _roleRepo = roleRepo;
            _logger = logger;

            _sortDictionary = new Dictionary<string, Expression<Func<Role, object>>>()
            {
                {"name",  r => r.Name},
                {"consumer",  r => r.Consumer},
                {"status",  r => r.Status},
                {"default",  r => r.CreateDate}
            };
        }

        public async Task<IPage<RoleCreateModel>> Execute(RoleQueryModel queryModel)
        {
            try
            {
                if (queryModel == null)
                    throw new ArgumentNullException("queryModel");

                ValidateAndCorrectListQueryModel(queryModel);

                //Create filters list
                List<FilterDefinition<Role>> filterDefList = new List<FilterDefinition<Role>>();
                filterDefList.Add(Builders<Role>.Filter.Empty);
                if (queryModel.Name != null)
                    filterDefList.Add(Builders<Role>.Filter.Eq(r => r.Name, queryModel.Name));
                if (queryModel.Consumer != null)
                    filterDefList.Add(Builders<Role>.Filter.Eq(r => r.Consumer, queryModel.Consumer));
                if (queryModel.Status != RoleStatus.NotSet)
                    filterDefList.Add(Builders<Role>.Filter.Eq(r => r.Status, queryModel.Status));

                var filterDef = Builders<Role>.Filter.And(filterDefList);

                // Create query using the filters.
                IFindFluent<Role, Role> query = _roleRepo.Find(filterDef);

                // Add sorting.
                AddSortingToQuery(query, _sortDictionary, queryModel.Order, queryModel.IsDesc);

                // Add paging. 
                AddPagingToQuery(query, queryModel.Page, queryModel.PageSize);

                var list = await query.ToListAsync();

                var roles = from r in list
                            select new RoleCreateModel()
                            {
                                Id = r.Id,
                                Name = r.Name,
                                Consumer = r.Consumer,
                                Description = r.Description,
                                Status = r.Status
                            };

                long totalItems = await _roleRepo.CountDocumentsAsync(filterDef);

                return new Page<RoleCreateModel>(roles, totalItems);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("GetRolesQuery.Execute", "Exception was thrown", new
                {
                    RoleQueryModel = queryModel,
                    Exception = ex
                });

                throw;
            }
        }
    }
}
