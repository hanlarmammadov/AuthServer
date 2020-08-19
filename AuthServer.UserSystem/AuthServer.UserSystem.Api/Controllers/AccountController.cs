using TacitusLogger;
using AuthServer.UserSystem.Models;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using AuthServer.UserSystem.Services.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AuthServer.Common.Messages;
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Api.Controllers
{
    [Route("api/v1/account")]
    public class AccountController : Controller
    {
        private readonly ILogger _logger;

        public AccountController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] AccountModel model,
                                                [FromServices] IValidatorFactory validatorFactory,
                                                [FromServices] ICreateAccountCommand createAccountCommand)
        {
            try
            {
                //Validate request
                if (model == null)
                    return BadRequest();

                //Create account
                IValidator validator = validatorFactory.Create();
                string accountId = await createAccountCommand.Execute(model, validator);

                //Return result
                if (validator.HasErrors)
                    return BadRequest(ValueResponse<string>.ValidationError(validator.Errors));
                else
                    return Created("", ValueResponse<string>.Success(accountId));
            }
            catch (Exception ex)
            {
                _logger.LogError("AccountController.Create", "Exception occurred", new
                {
                    Exception = ex,
                    AccountModel = model
                });
                return BadRequest(ValueResponse<string>.GeneralError("Something went wrong"));
            }
        }

        [HttpGet("confirmEmail/{code}")]
        public async Task<IActionResult> ConfirmEmail([FromRoute] string code,
                                                      [FromServices] IConfirmEmailCommand confirmEmailCommand)
        {
            try
            {
                if (code == null)
                    return BadRequest();

                bool result = await confirmEmailCommand.Execute(code);
                if (result)
                    return Ok();
                else
                    return BadRequest(AuthServer.Common.Messages.Response.GeneralError("Something went wrong"));
            }
            catch (Exception ex)
            {
                //Log
                _logger.LogError("AccountController.ConfirmEmail", "Exception occurred", new
                {
                    Exception = ex,
                    Code = code
                });

                return BadRequest(AuthServer.Common.Messages.Response.GeneralError("Something went wrong"));
            }
        }

        [HttpPut("{accountId}/changePass")]
        public async Task<IActionResult> ChangeAccountPassword([FromRoute] string accountId,
                                                               [FromBody] PasswordChangeModel model,
                                                               [FromServices] IValidatorFactory validatorFactory,
                                                               [FromServices] IChangeAccountPasswordCommand changeAccountPasswordCommand)
        {
            try
            {
                if (accountId == null || model == null)
                    return BadRequest(AuthServer.Common.Messages.Response.GeneralError("Something went wrong"));

                var validator = validatorFactory.Create();
                await changeAccountPasswordCommand.Execute(accountId, model, validator);

                if (validator.HasErrors)
                    return BadRequest(AuthServer.Common.Messages.Response.ValidationError(validator.Errors));
                else
                    return Ok(AuthServer.Common.Messages.Response.Success());
            }
            catch (Exception ex)
            {
                //Log
                _logger.LogError("AccountController.ChangeAccountPassword", "Exception occurred", new
                {
                    AccountId = accountId,
                    Exception = ex,
                });

                return BadRequest(AuthServer.Common.Messages.Response.GeneralError("Something went wrong"));
            }
        }

        [HttpPut("{accountId}/changeEmail")]
        public async Task<IActionResult> ChangeAccountEmail([FromRoute] string accountId,
                                                            [FromBody] ChangeEmailModel model,
                                                            [FromServices] IValidatorFactory validatorFactory,
                                                            [FromServices] IChangeAccountEmailCommand changeAccountEmailCommand)
        {
            try
            {
                if (accountId == null || model == null)
                    return BadRequest(AuthServer.Common.Messages.Response.GeneralError("Something went wrong"));

                var validator = validatorFactory.Create();
                await changeAccountEmailCommand.Execute(accountId, model, validator);

                if (validator.HasErrors)
                    return BadRequest(AuthServer.Common.Messages.Response.ValidationError(validator.Errors));
                else
                    return Ok(AuthServer.Common.Messages.Response.Success());
            }
            catch (Exception ex)
            {
                //Log
                _logger.LogError("AccountController.ChangeAccountEmail", "Exception occurred", new
                {
                    AccountId = accountId,
                    ChangeEmailModel = model,
                    Exception = ex,
                });

                return BadRequest(AuthServer.Common.Messages.Response.GeneralError("Something went wrong"));
            }
        }

        [HttpPut("undoEmailChange/{emailChangeRecordId}")]
        public async Task<IActionResult> UndoChangeAccountEmail([FromRoute] string emailChangeRecordId,
                                                                [FromServices] IUndoChangeAccountEmailCommand undoChangeAccountEmailCommand)
        {
            try
            {
                if (emailChangeRecordId == null)
                    return BadRequest(AuthServer.Common.Messages.Response.GeneralError("Something went wrong"));

                bool success = await undoChangeAccountEmailCommand.Execute(emailChangeRecordId);

                if (success)
                    return Ok(AuthServer.Common.Messages.Response.Success());
                else
                    return BadRequest(AuthServer.Common.Messages.Response.GeneralError("Something went wrong"));
            }
            catch (Exception ex)
            {
                //Log
                _logger.LogError("AccountController.ChangeAccountEmail", "Exception occurred", new
                {
                    EmailChangeRecordId = emailChangeRecordId,
                    Exception = ex,
                });

                return BadRequest(AuthServer.Common.Messages.Response.GeneralError("Something went wrong"));
            }
        }

    }
}
