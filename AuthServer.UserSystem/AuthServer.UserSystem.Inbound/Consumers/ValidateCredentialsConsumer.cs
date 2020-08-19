using AuthServer.UserSystem.Models;
using AuthServer.UserSystem.Models.MQ;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using MassTransit;
using System;
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.UserSystem.Inbound.Consumers
{
    public class ValidateCredentialsConsumer : IConsumer<AuthValidationMQRequest>
    {
        private readonly IValidateCredentialsCommand _validateCredentialsCommand;
        private readonly ILogger _logger;

        public ValidateCredentialsConsumer(IValidateCredentialsCommand validateCredentialsCommand, ILogger logger)
        {
            _validateCredentialsCommand = validateCredentialsCommand;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuthValidationMQRequest> context)
        {
            try
            {
                AuthValidationMQResponse response = new AuthValidationMQResponse();
                try
                {
                    response.OpSuccess = true;
                    AuthValidationRequest request = new AuthValidationRequest()
                    {
                        CorrelationId = context.Message.CorrelationId,
                        UsernameOrEmail = context.Message.UsernameOrEmail,
                        Password = context.Message.Password
                    };
                    (bool Result, string AccountId) validationRes = await _validateCredentialsCommand.Execute(request);

                    if (validationRes.Result)
                    {
                        response.IsValid = true;
                        response.AccountId = validationRes.AccountId;
                    }
                    else
                    {
                        response.IsValid = false;
                    }
                    await _logger.LogEventAsync("ValidateCredentialsConsumer.Consume", $"Validation request for {context.Message.UsernameOrEmail} served.");
                }
                catch (Exception ex)
                {
                    //Log error
                    await _logger.LogErrorAsync("ValidateCredentialsConsumer.Consume", "Exception was thrown.", new
                    {
                        ConsumeContext = context,
                        Exception = ex
                    });

                    response.OpSuccess = false;
                }

                await context.RespondAsync<AuthValidationMQResponse>(response);
            }
            catch (Exception ex)
            {
                //Log error
                await _logger.LogErrorAsync("ValidateCredentialsConsumer.Consume", "Exception was thrown", new
                {
                    ConsumeContext = context,
                    Exception = ex
                });
            }
        }
    }
}
