using TacitusLogger;
using AuthServer.UserSystem.Domain.Entities;
using AuthServer.UserSystem.Services.Models;
using AuthServer.UserSystem.Services.Queries.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AuthServer.Common.Messages;

namespace AuthServer.UserSystem.Services.Queries
{
    public class GetUsersQuery : MongoListQueryBase, IGetUsersQuery
    {
        private readonly IMongoCollection<User> _userRepo;
        private readonly ILogger _logger;
        private Dictionary<string, Expression<Func<User, object>>> _sortDictionary;

        public GetUsersQuery(IMongoCollection<User> userRepo, ILogger logger)
        {
            _userRepo = userRepo;
            _logger = logger;

            _sortDictionary = new Dictionary<string, Expression<Func<User, object>>>()
            {
                {"firstname",  r => r.FirstName},
                {"lastname",  r => r.LastName},
                {"gender",  r => r.Gender},
                {"default",  r => r.CreateDate}
            };
        }

        public async Task<IPage<UserListModel>> Execute(UserQueryModel queryModel)
        {
            try
            {
                if (queryModel == null)
                    throw new ArgumentNullException("queryModel");

                ValidateAndCorrectListQueryModel(queryModel);

                //Create filters list
                List<FilterDefinition<User>> filterDefList = new List<FilterDefinition<User>>();

                //Filters goes here
                filterDefList.Add(Builders<User>.Filter.Empty);
                var filterDef = Builders<User>.Filter.And(filterDefList);

                //Filtering and ordering
                var query = _userRepo.Find(filterDef)
                                     .Project(GetUserListProjections());
                // Add sorting.
                AddSortingToQuery(query, _sortDictionary, queryModel.Order, queryModel.IsDesc);

                // Add paging. 
                AddPagingToQuery(query, queryModel.Page, queryModel.PageSize);

                var list = await query.ToListAsync();

                var users = from u in list
                            select new UserListModel()
                            {
                                AccountId = u.AccountId,
                                FirstName = u.FirstName,
                                LastName = u.LastName,
                                Gender = u.Gender
                            };

                long totalItems = await _userRepo.CountDocumentsAsync(filterDef);

                return new Page<UserListModel>(users, totalItems);
            }
            catch (Exception ex)
            {
                //Log
                _logger.LogError("GetUsersQuery.Execute", "Exception occurred", new
                {
                    Exception = ex,
                    UserQuery = queryModel
                });

                throw;
            }
        }
        protected ProjectionDefinition<User, User> GetUserListProjections()
        {
            ProjectionDefinition<User, User> proj = Builders<User>.Projection
                                                            .Include(x => x.AccountId)
                                                            .Include(x => x.FirstName)
                                                            .Include(x => x.LastName)
                                                            .Include(x => x.Gender)
                                                            .Include(x => x.CreateDate);
            return proj;
        }
    }
}
