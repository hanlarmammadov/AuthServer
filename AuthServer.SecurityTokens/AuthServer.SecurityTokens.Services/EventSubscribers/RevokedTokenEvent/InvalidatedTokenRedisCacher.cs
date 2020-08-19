using AuthServer.Common.Patterns;
using AuthServer.SecurityTokens.Services.Providers.Interfaces;
using TacitusLogger;
using AuthServer.SecurityTokens.Models;
using System;
using System.Threading.Tasks;
using AuthServer.SecurityTokens.Models.Events;

namespace AuthServer.SecurityTokens.Services.EventSubscribers.RevokedTokenEvent
{
    public class InvalidatedTokenRedisCacher : IEventSubscriber<RefreshTokenRevokedEvent>
    {
        private readonly ICachedRepo<string> _invalidatedTokenRepo;
        private readonly ILogger _logger;

        public InvalidatedTokenRedisCacher(ICachedRepo<string> invalidatedTokenRepo, ILogger logger)
        {
            _invalidatedTokenRepo = invalidatedTokenRepo;
            _logger = logger;
        }

        public async Task HandleEvent(RefreshTokenRevokedEvent evnt)
        {
            try
            {
                //Add token to local invalidated tokens list
                await _invalidatedTokenRepo.Add(evnt.TokenId, evnt.TokenId, evnt.Expires);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("RevokedTokenRedisCacher.HandleEvent", "Exception was thrown", new
                {
                    Exception = ex
                });

                throw;
            }
        }
    }
}
