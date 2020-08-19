using AuthServer.Common.Exceptions;
using AuthServer.SecurityTokens.Services.Commands.Interfaces;
using AuthServer.SecurityTokens.Services.Providers.Interfaces;
using AuthServer.SecurityTokens.Services.Queries.Interfaces;
using TacitusLogger;
using AuthServer.SecurityTokens.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthServer.Common.Messages;
using AuthServer.UserSystem.Models.MQ;

namespace AuthServer.SecurityTokens.Api.Controllers
{
    [Route("api/v1/rtoken")]
    public class RefreshTokenController : Controller
    {
        private readonly ILogger _logger;

        public RefreshTokenController(ILogger logger)
        {
            _logger = logger;
        }

        internal ILogger Logger => _logger;

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetTokensInfoForAccount([FromRoute] string accountId,
                                                                 [FromServices] IGetAllTokensForAccountQuery getAllTokensForAccountQuery)
        {
            try
            {
                if (accountId == null)
                    return BadRequest(new Message("Something went wrong."));

                IEnumerable<AccountTokenModel> tokenModels = await getAllTokensForAccountQuery.Execute(accountId);
                return Ok((new Page<AccountTokenModel>(tokenModels, tokenModels.Count())));
            }
            catch (Exception ex)
            {
                //Log error
                await _logger.LogErrorAsync("RefreshTokenController.GetTokensInfoForAccount", "Exception was thrown", new
                {
                    AccountId = accountId,
                    Exception = ex
                });

                return BadRequest(new Message("Something went wrong."));
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] CredModel model,
                                                [FromServices] IRequestClient<AuthValidationMQRequest> authValidationReqClient,
                                                [FromServices] IRequestClient<UserClaimsMQRequest> userClaimsReqClient,
                                                [FromServices] IGenerateRefreshTokenCommand generateRefreshTokenCommand)
        {
            string correlationId = null;
            try
            {
                if (model == null)
                    return BadRequest("Something went wrong");
                correlationId = Guid.NewGuid().ToString("N");

                // Validate username and password.
                Response<AuthValidationMQResponse> authValidationResponse = await authValidationReqClient.GetResponse<AuthValidationMQResponse>(new AuthValidationMQRequest()
                {
                    CorrelationId = correlationId,
                    UsernameOrEmail = model.Username,
                    Password = model.Password
                });

                if (authValidationResponse.Message.OpSuccess != true)
                    throw new Exception("Error with authValidationReqClient request");

                if (!authValidationResponse.Message.IsValid)
                    return BadRequest("Invalid username or password");

                // Get user and generate user claims.
                Response<UserClaimsMQResponse> claimsResponse = await userClaimsReqClient.GetResponse<UserClaimsMQResponse>(new UserClaimsMQRequest()
                {
                    CorrelationId = correlationId,
                    AccountId = authValidationResponse.Message.AccountId,
                    ClaimsConsumers = model.ClaimsConsumers
                });
                if (claimsResponse.Message.OpSuccess != true)
                    throw new Exception("Error with userClaimsReqClient request");

                // Generate refresh token.
                var claims = claimsResponse.Message.GetClaims();

                var req = ControllerContext.HttpContext.Request;
                TokenAdditionalData additionalData = new TokenAdditionalData()
                {
                    DeviceInfo = "Device info",
                    RequesterIPv4 = "ipv4",
                    RequesterIPv6 = "ipv6"
                };

                TokenResult rTokenResult = await generateRefreshTokenCommand.Execute(authValidationResponse.Message.AccountId, claims, additionalData);
                return Created("", rTokenResult);
            }
            catch (Exception ex)
            {
                if (model != null)
                    model.Password = "text hidden";

                //Log error
                _logger.LogError("RefreshTokenController.Create", "Exception was thrown", new
                {
                    CorrelationId = correlationId,
                    CredModel = model,
                    Exception = ex
                });

                return BadRequest("Something went wrong");
            }
        }

        [HttpPut("")]
        public async Task<IActionResult> Renew([FromBody] RTokenRenewModel renewModel,
                                               [FromServices] IRequestClient<UserClaimsMQRequest> userClaimsReqClient,
                                               [FromServices] IGenerateRefreshTokenCommand generateRefreshTokenCommand,
                                               [FromServices] ITokenExtractor refreshTokenExtractor)
        {
            string correlationId = null;
            try
            {
                if (renewModel == null)
                    return BadRequest(ValueResponse<TokenResult>.GeneralError("Something went wrong"));
                correlationId = Guid.NewGuid().ToString("N");

                //Old token verification and accountId retrieval goes here
                if (!refreshTokenExtractor.TryExractToken(renewModel.OldRToken, out List<Claim> oldClaims))
                    return BadRequest(ValueResponse<TokenResult>.GeneralError("Something went wrong"));

                string accountId = oldClaims.Single(x => x.Type == "Account").Value;
                //Problem with dates
                DateTime expiredOld = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified).AddSeconds((long.Parse(oldClaims.Single(x => x.Type == "exp").Value)));

                //Get user and generate user claims
                Response<UserClaimsMQResponse> claimsResp = await userClaimsReqClient.GetResponse<UserClaimsMQResponse>(new UserClaimsMQRequest()
                {
                    CorrelationId = correlationId,
                    AccountId = accountId,
                    ClaimsConsumers = renewModel.ClaimsConsumers
                });

                if (claimsResp.Message.OpSuccess != true)
                    throw new Exception("Error with userClaimsReqClient request");

                //Generate refresh token
                var newClaims = claimsResp.Message.GetClaims();

                var req = ControllerContext.HttpContext.Request;
                TokenAdditionalData additionalData = new TokenAdditionalData()
                {
                    DeviceInfo = "Device info",
                    RequesterIPv4 = "ipv4",
                    RequesterIPv6 = "ipv6"
                };

                TokenResult rTokenResult = await generateRefreshTokenCommand.Execute(accountId, newClaims, additionalData, expiredOld);
                return Created("", rTokenResult);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("RefreshTokenController.Renew", "Exception was thrown", new
                {
                    CorrelationId = correlationId,
                    TokenRenewModel = renewModel,
                    Exception = ex
                });

                return BadRequest(ValueResponse<TokenResult>.GeneralError("Something went wrong"));
            }
        }

        [HttpDelete("account/{accountId}/token/{tokenId}", Order = 2)]
        public async Task<IActionResult> Revoke([FromRoute] string accountId,
                                                [FromRoute] string tokenId,
                                                [FromServices] IRevokeTokenCommand revokeTokenCommand)
        {
            try
            {
                //Validate rights

                await revokeTokenCommand.Execute(accountId, tokenId);
                return Ok(AuthServer.Common.Messages.Response.Success());
            }
            catch (InsufficientPermissionsException ex)
            {
                return Ok(AuthServer.Common.Messages.Response.Success());
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("RefreshTokenController.Revoke", "Exception was thrown", new
                {
                    AccountId = accountId,
                    TokenId = tokenId,
                    Exception = ex
                });

                return BadRequest("Something went wrong");
            }
        }

        [HttpDelete("account/{accountId}/all", Order = 1)]
        public async Task<IActionResult> RevokeAll([FromRoute] string accountId,
                                                   [FromServices] IRevokeAllTokensForAccountCommand revokeAllTokensForAccountCommand)
        {
            try
            {
                //Validate rights

                await revokeAllTokensForAccountCommand.Execute(accountId);
                return Ok(AuthServer.Common.Messages.Response.Success());
            }
            catch (InsufficientPermissionsException ex)
            {
                return Ok(AuthServer.Common.Messages.Response.Success());
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("RefreshTokenController.RevokeAll", "Exception was thrown", new
                {
                    AccountId = accountId,
                    Exception = ex
                });

                return BadRequest("Something went wrong");
            }
        }

    }
}
