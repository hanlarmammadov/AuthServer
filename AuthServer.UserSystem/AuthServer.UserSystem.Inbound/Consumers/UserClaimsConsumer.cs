using AuthServer.UserSystem.Models;
using AuthServer.UserSystem.Models.MQ;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using MassTransit;
using System;
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.UserSystem.Inbound.Consumers
{
    public class UserClaimsConsumer : IConsumer<UserClaimsMQRequest>
    {
        private readonly IGetUserClaimsCommand _getUserClaimsCommand;
        private readonly ILogger _logger;

        public UserClaimsConsumer(IGetUserClaimsCommand getUserClaimsCommand, ILogger logger)
        {
            _getUserClaimsCommand = getUserClaimsCommand;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserClaimsMQRequest> context)
        {
            try
            {
                UserClaimsMQResponse response = new UserClaimsMQResponse();
                try
                {

                    UserClaimsRequest request = new UserClaimsRequest()
                    {
                        CorrelationId = context.Message.CorrelationId,
                        AccountId = context.Message.AccountId,
                        ClaimsConsumers = context.Message.ClaimsConsumers
                    };
                    response.SetClaims(await _getUserClaimsCommand.Execute(request));
                    response.OpSuccess = true;

                    await _logger.LogEventAsync("UserClaimsConsumer.Consume", $"User claims request fulfilled. AccountId: {context.Message.AccountId}");
                }
                catch (Exception ex)
                {
                    //Log error
                    await _logger.LogErrorAsync("UserClaimsConsumer.Consume.InnerCatch", "Exception was thrown", new
                    {
                        Context = context,
                        Exception = ex
                    });

                    response.OpSuccess = false;
                }

                await context.RespondAsync<UserClaimsMQResponse>(response);
            }
            catch (Exception ex)
            {
                //Log error
                await _logger.LogErrorAsync("UserClaimsConsumer.Consume.OuterCatch", "Exception was thrown", new
                {
                    Context = context,
                    Exception = ex
                });
            }
        }
    }
}
