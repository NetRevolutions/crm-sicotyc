using Jarasoft.Sicotyc.Api.Contracts.Roles;
using Jarasoft.Sicotyc.Application.Roles;
using Microsoft.AspNetCore.Mvc;

namespace Jarasoft.Sicotyc.Api.Controllers;

[ApiController]
[Route("api/application-roles")]
public sealed class ApplicationRolesController(IApplicationRoleQueryService roleQueryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<ApplicationRoleResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ApplicationRoleResponse>>> GetAsync(CancellationToken cancellationToken)
    {
        var roles = await roleQueryService.ListAsync(cancellationToken);

        return Ok(roles.Select(role => new ApplicationRoleResponse(role.Id, role.Name, role.NormalizedName)).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApplicationRoleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationRoleResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await roleQueryService.GetByIdAsync(id, cancellationToken);
        if (role is null)
        {
            return NotFound();
        }

        return Ok(new ApplicationRoleResponse(role.Id, role.Name, role.NormalizedName));
    }
}