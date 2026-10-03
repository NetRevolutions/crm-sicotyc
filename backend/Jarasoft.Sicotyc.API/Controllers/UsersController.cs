using Jarasoft.Sicotyc.API.Contracts.Users;
using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserRole;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserStatus;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.CreateUser;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.DeleteUser;
using Jarasoft.Sicotyc.Application.Features.Users.Queries.GetUserById;
using Jarasoft.Sicotyc.Application.Features.Users.Queries.GetUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jarasoft.Sicotyc.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(
    CreateUserHandler createUserHandler
    , GetUsersHandler getUsersHandler
    , GetUserByIdHandler getUserByIdHandler
    , ChangeUserStatusHandler changeUserStatusHandler
    , ChangeUserRoleHandler changeUserRoleHandler
    , DeleteUserHandler deleteUserHandler)
    : ControllerBase
{

    [HttpDelete("{id:guid}")]
    [Authorize(
    Roles =
        ApplicationRoles.Administrator
        + ","
        + ApplicationRoles.SuperAdministrator)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await deleteUserHandler.HandleAsync(
            new DeleteUserCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/role")]
    [Authorize(
    Roles =
        ApplicationRoles.Administrator
        + ","
        + ApplicationRoles.SuperAdministrator)]
    public async Task<ActionResult<ChangeUserRoleResult>> ChangeRole(
        Guid id,
        ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new ChangeUserRoleCommand(
                id,
                request.Role);

        var result =
            await changeUserRoleHandler.HandleAsync(
                command,
                cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(
    Roles =
        ApplicationRoles.Administrator
        + ","
        + ApplicationRoles.SuperAdministrator)]
    public async Task<ActionResult<ChangeUserStatusResult>> ChangeStatus(
        Guid id,
        ChangeUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new ChangeUserStatusCommand(
                id,
                request.IsActive);

        var result =
            await changeUserStatusHandler.HandleAsync(
                command,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(
    Roles =
        ApplicationRoles.Administrator
        + ","
        + ApplicationRoles.SuperAdministrator)]
    public async Task<ActionResult<GetUserByIdResult>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query =
            new GetUserByIdQuery(id);

        var result =
            await getUserByIdHandler.HandleAsync(
                query,
                cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [Authorize(
    Roles =
        ApplicationRoles.Administrator
        + ","
        + ApplicationRoles.SuperAdministrator)]
    public async Task<
    ActionResult<IReadOnlyCollection<UserListItem>>> GetAll(
        [FromQuery] Guid? companyId,
        CancellationToken cancellationToken)
    {
        var query =
            new GetUsersQuery(companyId);

        var result =
            await getUsersHandler.HandleAsync(
                query,
                cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(
        Roles =
            ApplicationRoles.Administrator
            + ","
            + ApplicationRoles.SuperAdministrator)]
    public async Task<
        ActionResult<CreateUserResult>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new CreateUserCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                request.Role,
                request.CompanyId);

        var result =
            await createUserHandler.HandleAsync(
                command,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
}