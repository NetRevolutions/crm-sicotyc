using Jarasoft.Sicotyc.API.Contracts.Users;
using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Features.Companies.Commands.DeactivateCompanyUsers;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserRole;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserStatus;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.CreateUser;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.DeleteUser;
using Jarasoft.Sicotyc.Application.Features.Users.Queries.GetUserById;
using Jarasoft.Sicotyc.Application.Features.Users.Queries.GetUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jarasoft.Sicotyc.API.Controllers
{
    [ApiController]
    [Route("api/companies")]
    [Authorize]
    public class CompaniesController(
        DeactivateCompanyUsersHandler deactivateCompanyUsersHandler)
        : ControllerBase
    {
        [HttpPatch("{companyId:guid}/users/deactivate")]
        [Authorize(Roles = ApplicationRoles.SuperAdministrator)]
        public async Task<ActionResult<DeactivateCompanyUsersResult>>DeactivateUsers(
            Guid companyId,
            CancellationToken cancellationToken)
        {
            var result =
                await deactivateCompanyUsersHandler.HandleAsync(
                    new DeactivateCompanyUsersCommand(companyId),
                    cancellationToken);

            return Ok(result);
        }
    }
}
