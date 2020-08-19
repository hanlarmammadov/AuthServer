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
    [Route("api/v1/role")]
    public class RoleController : Controller
    {
        private readonly ILogger _logger;

        public RoleController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<ActionResult> Get([FromQuery] RoleQueryModel query,
                                            [FromServices] IGetRolesQuery getRolesQuery)
        {
            try
            {
                if (query == null)
                    return BadRequest();

                IPage<RoleCreateModel> roles = await getRolesQuery.Execute(query);

                return Ok(roles);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("RoleController.Get", "Exception was thrown.", new
                {
                    RoleQuery = query,
                    Exception = ex
                });

                return BadRequest(new Message("Something bad happened. Try again."));
            }
        }

        [HttpPost("")]
        public async Task<ActionResult> Create([FromBody] RoleCreateModel roleModel,
                                               [FromServices] IValidatorFactory validatorFactory,
                                               [FromServices] ICreateRoleCommand createRoleCommand)
        {
            try
            {
                if (roleModel == null)
                    return BadRequest();
                IValidator validator = validatorFactory.Create();
                string roleId = await createRoleCommand.Execute(roleModel, validator);

                if (validator.HasErrors)
                    return BadRequest(validator.Errors);
                else
                    return Created("", roleId);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("RoleController.Create", "Exception was thrown.", new
                {
                    RoleModel = roleModel,
                    Exception = ex
                });

                return BadRequest(new Message("Something bad happened. Try again."));
            }
        }

        [HttpPut("{roleId}")]
        public async Task<ActionResult> Edit([FromRoute] string roleId,
                                             [FromBody] RoleCreateModel changes,
                                             [FromServices] IValidatorFactory validatorFactory,
                                             [FromServices] IEditRoleCommand editRoleCommand)
        {
            try
            {
                if (roleId == null || changes == null)
                    return BadRequest();

                IValidator validator = validatorFactory.Create();
                await editRoleCommand.Execute(roleId, changes, validator);

                if (validator.HasErrors)
                    return BadRequest(validator.Errors);
                else
                    return Created("", roleId);
            }
            catch (Exception ex)
            {
                //Log error
                _logger.LogError("RoleController.Edit", "Exception was thrown", new
                {
                    RoleId = roleId,
                    Changes = changes,
                    Exception = ex
                });

                return BadRequest(new Message("Something bad happened. Try again"));
            }
        }

    }
}
