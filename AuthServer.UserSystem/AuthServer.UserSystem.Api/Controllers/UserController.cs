using AuthServer.Common.Exceptions;
using TacitusLogger;
using AuthServer.UserSystem.Services.Commands.Interfaces;
using AuthServer.UserSystem.Services.Models;
using AuthServer.UserSystem.Services.Queries.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AuthServer.Common.Messages;
using AuthServer.Common.Validation;

namespace AuthServer.UserSystem.Api.Controllers
{
    [Route("api/v1/user")]
    public class UserController : Controller
    {
        private readonly ILogger _logger;

        public UserController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetDetails([FromRoute] string accountId,
                                                    [FromServices] IGetUserDetailsQuery getUserDetailsQuery)
        {
            try
            {
                if (accountId == null)
                    return BadRequest();

                UserDetailedModel userModel = await getUserDetailsQuery.Execute(accountId);

                return Ok(userModel);
            }
            catch (ObjectNotFoundException ex)
            {
                return NotFound(new Message("User not found."));
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("UserController.GetDetails", "Exception was thrown", new
                {
                    AccountId = accountId,
                    Exception = ex
                });

                return BadRequest(new Message("Something bad happened. Try again."));
            }
        }

        [HttpGet("")]
        public async Task<IActionResult> GetList([FromQuery] UserQueryModel query,
                                                 [FromServices] IGetUsersQuery getUsersQuery)
        {
            try
            {
                if (query == null)
                    return BadRequest(new Message("Something bad happened. Try again."));

                IPage<UserListModel> page = await getUsersQuery.Execute(query);
                return Ok(page);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("UserController.Create", "Exception was thrown.", new
                {
                    UserQuery = query,
                    Exception = ex
                });

                return BadRequest(new Message("Something bad happened. Try again."));
            }
        }


        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] CreateUserModel userModel,
                                                [FromServices] IValidatorFactory validatorFactory,
                                                [FromServices] ICreateUserCommand createUserCommand)
        {
            try
            {
                if (userModel == null)
                    return BadRequest(new Message("Something bad happened. Try again."));

                IValidator validator = validatorFactory.Create();
                string accountId = await createUserCommand.Execute(userModel.AccountId, userModel, validator);

                if (validator.HasErrors)
                    return BadRequest(validator.Errors);
                else
                    return Created("", accountId);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("UserController.Create", "Exception was thrown", new
                {
                    UserModel = userModel,
                    Exception = ex
                });

                return BadRequest(new Message("Something bad happened. Try again"));
            }
        }

        [HttpPut("{accountId}/{component}")]
        public async Task<IActionResult> EditUser([FromRoute] string accountId,
                                                  [FromRoute] string component,
                                                  [FromBody] CreateUserModel userModel,
                                                  [FromServices] IValidatorFactory validatorFactory,
                                                  [FromServices] IEditUserRolesCommand editUserRolesCommand,
                                                  [FromServices] IEditUserDataAndContactsCommand editUserDataAndContactsCommand)
        {
            try
            {
                if (userModel == null || accountId == null || (component != "roles" && component != "data"))
                    return BadRequest(new Message("Something bad happened. Try again."));

                userModel.AccountId = accountId;

                IValidator validator = validatorFactory.Create();
                if (component == "roles")
                    await editUserRolesCommand.Execute(userModel, validator);
                else
                    await editUserDataAndContactsCommand.Execute(userModel, validator);

                if (validator.HasErrors)
                    return BadRequest(validator.Errors);
                else
                    return Created("", accountId);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("UserController.EditUser", "Exception was thrown", new
                {
                    Component = component,
                    UserModel = userModel,
                    Exception = ex
                });

                return BadRequest(new Message("Something bad happened. Try again"));
            }
        }

    }
}
