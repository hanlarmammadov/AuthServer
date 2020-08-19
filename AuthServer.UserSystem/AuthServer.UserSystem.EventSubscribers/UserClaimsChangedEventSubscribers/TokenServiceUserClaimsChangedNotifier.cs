//using AuthServer.Common.Patterns;
//using TacitusLogger;
//using AuthServer.UserSystem.Models.Events;
//using MassTransit;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AuthServer.UserSystem.EventSubscribers.UserClaimsChangedEventSubscribers
//{ 
//    public class TokenServiceUserClaimsChangedNotifier : IEventSubscriber<UserClaimsChangedEvent>
//    {
//        private readonly IPublishEndpoint _massTransitPublishEndpoint;
//        private readonly ILogger _logger;

//        public TokenServiceUserClaimsChangedNotifier(IPublishEndpoint massTransitPublishEndpoint, ILogger logger)
//        {
//            _massTransitPublishEndpoint = massTransitPublishEndpoint;
//            _logger = logger;
//        }

//        public async Task HandleEvent(UserClaimsChangedEvent evnt)
//        {
//            try
//            {
//                await _massTransitPublishEndpoint.Publish<UserClaimsChangedEvent>(evnt);
//            }
//            catch (Exception ex)
//            {
//                //Log error
//                _logger.LogError("TokenServiceUserClaimsChangedNotifier.HandleEvent", "Exception was thrown", new
//                {
//                    PasswordChangedEvent = evnt,
//                    Exception = ex
//                });

//                throw;
//            }
//        }
//    }
//}
