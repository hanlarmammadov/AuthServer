//using AuthServer.SecurityTokens.Services.Providers.Interfaces;
//using TacitusLogger;
//using AuthServer.SecurityTokens.Models;
//using StackExchange.Redis;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using AuthServer.Infrastructure.Serialization;

//namespace AuthServer.SecurityTokens.Services.Providers
//{
//    public class RevokedTokenRedisRepo: IRevokedTokenRepo
//    {
//        private readonly IDatabase _redisDb;
//        private readonly ISerializer _serializer;
//        private readonly ILogger _logger;
//        private readonly string _collectionPrefix;

//        public RevokedTokenRedisRepo(IDatabase redisDb, string collectionPrefix, ISerializer serializer, ILogger logger)
//        {
//            _collectionPrefix = (collectionPrefix != null) ? collectionPrefix + ":" : "";
//            _redisDb = redisDb;
//            _serializer = serializer;
//            _logger = logger;
//        }

//        public async Task Add(RevokedTokenModel revokedToken)
//        {
//            try
//            {
//                var key = _collectionPrefix + revokedToken.TokenId;
//                bool addRes = await _redisDb.StringSetAsync(key, _serializer.Serialize(revokedToken));
//                bool exp = await _redisDb.KeyExpireAsync(key, revokedToken.Expires);
//            }
//            catch (Exception ex)
//            {
//                //Log error
//                _logger.LogError("RevokedTokensRedisRepo.Add", "Exception was thrown", new
//                {
//                    RevokedToken = revokedToken,
//                    Exception = ex
//                });

//                throw;
//            }
//        }
  
//        public async Task<bool> Exists(string rTokenId)
//        {
//            try
//            { 
//                var key = _collectionPrefix + rTokenId;
//                bool exists = await _redisDb.KeyExistsAsync(key);
//#if DEBUG
//                if (exists)
//                {
//                    var val = _redisDb.StringGet(key);
//                    var ttl = _redisDb.KeyTimeToLive(key);
//                }
//#endif
//                return exists;
//            }
//            catch (Exception ex)
//            {     
//                //Log error
//                _logger.LogError("RevokedTokensRedisRepo.Exists", "Exception was thrown", new
//                {
//                    RTokenId = rTokenId,
//                    Exception = ex
//                });

//                throw;
//            }
//        }
//    }
//}
