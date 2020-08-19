using AuthServer.Common.Exceptions;
using AuthServer.SecurityTokens.Services.Commands.Interfaces;
using TacitusLogger;
using AuthServer.SecurityTokens.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AuthServer.Common.Messages;

namespace AuthServer.SecurityTokens.Api.Controllers
{
    [Route("api/v1/token")]
    public class ShortTokenController : Controller
    {
        private readonly ILogger _logger;

        public ShortTokenController(ILogger logger)
        {
            _logger = logger;
        }

        internal ILogger Logger => _logger;

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] CreateSTokenModel model, [FromServices] IGenerateShortTokenCommand generateShortTokenCommand)
        {
            try
            {
                if (model == null || model.RToken == null)
                    return BadRequest(new Message("Invalid refresh token provided."));

                TokenResult sTokenRes = await generateShortTokenCommand.Execute(model.RToken);
                return Created("", sTokenRes);
            }
            catch (Exception ex)
            {
                //Log error
                await _logger.LogErrorAsync("ShortTokenController.Create", "Exception was thrown", new
                {
                    CreateSTokenModel = model,
                    Exception = ex
                });

                return BadRequest(new Message("Something went wrong."));
            }
        }
    }
}
