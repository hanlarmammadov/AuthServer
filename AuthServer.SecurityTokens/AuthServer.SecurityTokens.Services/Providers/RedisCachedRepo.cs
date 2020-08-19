using AuthServer.Infrastructure.Serialization;
using AuthServer.SecurityTokens.Services.Providers.Interfaces;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.SecurityTokens.Services.Providers
{
    public class RedisCachedRepo<TEntity>: ICachedRepo<TEntity>
    {
        private readonly IDatabase _redisDb;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
        private readonly string _collectionPrefix;

        public RedisCachedRepo(IDatabase redisDb, string collectionPrefix, ISerializer serializer, ILogger logger)
        {
            _collectionPrefix = (collectionPrefix != null) ? collectionPrefix + ":" : "";
            _redisDb = redisDb;
            _serializer = serializer;
            _logger = logger;
        }

        public async Task Add(string key, TEntity entity, DateTime? expires = null)
        {
            try
            {
                var fullKey = _collectionPrefix + key;
                bool addRes = await _redisDb.StringSetAsync(fullKey, _serializer.Serialize(entity));
                if (expires != null)
                    await _redisDb.KeyExpireAsync(fullKey, expires);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("RedisExpiringEntityRepo.Add", "Exception was thrown", new
                {
                    Key = key,
                    Entity = entity,
                    Exception = ex
                });

                throw;
            }
        }

        public async Task<bool> Exists(string key)
        {
            try
            {
                var fullKey = _collectionPrefix + key;
                bool exists = await _redisDb.KeyExistsAsync(fullKey);
#if DEBUG
                if (exists)
                {
                    var val = _redisDb.StringGet(fullKey);
                    var ttl = _redisDb.KeyTimeToLive(fullKey);
                }
#endif
                return exists;
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("RedisExpiringEntityRepo.Exists", "Exception was thrown", new
                {
                    Key = key,
                    Exception = ex
                });

                throw;
            }
        }
    }
}
