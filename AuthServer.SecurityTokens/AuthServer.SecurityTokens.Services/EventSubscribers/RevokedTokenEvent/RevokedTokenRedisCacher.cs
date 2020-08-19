using AuthServer.Common.Patterns;
using AuthServer.SecurityTokens.Services.Providers.Interfaces;
using TacitusLogger;
using AuthServer.SecurityTokens.Models;
using System;
using System.Threading.Tasks;
using AuthServer.SecurityTokens.Models.Events;

namespace AuthServer.SecurityTokens.Services.EventSubscribers.RevokedTokenEvent
{
    public class RevokedTokenRedisCacher : IEventSubscriber<RefreshTokenRevokedEvent>
    {
        private readonly ICachedRepo<string> _revokedTokenRepo;
        private readonly ILogger _logger;

        public RevokedTokenRedisCacher(ICachedRepo<string> revokedTokenRepo, ILogger logger)
        {
            _revokedTokenRepo = revokedTokenRepo;
            _logger = logger;
        }

        public async Task HandleEvent(RefreshTokenRevokedEvent evnt)
        {
            try
            {
                //Add token to local revoked tokens list
                await _revokedTokenRepo.Add(evnt.TokenId, evnt.TokenId, evnt.Expires);
            }
            catch (Exception ex)
            {
                //Log error
                await _logger.LogErrorAsync("RevokedTokenRedisCacher.HandleEvent", "Exception was thrown", new
                {
                    RefreshTokenRevokedEvent = evnt,
                    Exception = ex
                });

                throw;
            }
        }
    }
}
