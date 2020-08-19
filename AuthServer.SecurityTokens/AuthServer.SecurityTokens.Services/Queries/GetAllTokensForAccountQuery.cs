using AuthServer.SecurityTokens.Services.Queries.Interfaces;
using AuthServer.SecurityTokens.Entities;
using AuthServer.SecurityTokens.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.SecurityTokens.Services.Queries
{
    public class GetAllTokensForAccountQuery : IGetAllTokensForAccountQuery
    {
        private readonly IMongoCollection<AccountRTokenInfo> _accountTokenRepo;
        private readonly ILogger _logger;

        public GetAllTokensForAccountQuery(IMongoCollection<AccountRTokenInfo> accountTokenRepo, ILogger logger)
        {
            _accountTokenRepo = accountTokenRepo;
            _logger = logger;
        }

        protected FilterDefinition<AccountRTokenInfo> GetFilterDefinition(string accountId)
        {
            return Builders<AccountRTokenInfo>.Filter.And(Builders<AccountRTokenInfo>.Filter.Eq(x => x.AccountId, accountId),
                                                          Builders<AccountRTokenInfo>.Filter.Or(
                                                               Builders<AccountRTokenInfo>.Filter.Eq(x => x.Status, AccountRTokenStatus.Active),
                                                               Builders<AccountRTokenInfo>.Filter.Eq(x => x.Status, AccountRTokenStatus.ClaimsChanged)));
        }

        public async Task<IEnumerable<AccountTokenModel>> Execute(string accountId)
        {
            try
            {
                List<AccountTokenModel> resultList = new List<AccountTokenModel>();

                List<AccountRTokenInfo> tokens = await _accountTokenRepo.Find(GetFilterDefinition(accountId)).ToListAsync();
              
                if (tokens != null)
                    resultList = tokens.Select(x => new AccountTokenModel()
                    {
                        AccountId = x.AccountId,
                        TokenId = x.TokenId,
                        ExpireDate = x.ExpireDate,
                        CreateDate = x.CreateDate,
                        DeviceInfo = x.DeviceInfo,
                        RequesterIPv4 = x.RequesterIPv4,
                        RequesterIPv6 = x.RequesterIPv6
                    }).ToList();


                return resultList;
            }
            catch(Exception ex)
            { 
                //Log error
                _logger.LogError("RevokeAllAccountTokensCommand.Execute", "Exception was thrown", new
                {
                    AccountId = accountId, 
                    Exception = ex
                });

                throw;
            }
        }
    }
}
