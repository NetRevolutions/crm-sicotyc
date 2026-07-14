using Jarasoft.Sicotyc.Api.Contracts.Users;
using Jarasoft.Sicotyc.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace Jarasoft.Sicotyc.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(
    ISystemUserRegistrationService registrationService,
    IApplicationUserQueryService userQueryService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ApplicationUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationUserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await userQueryService.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(new ApplicationUserResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            user.FirstName,
            user.LastName,
            user.DocumentType,
            user.DocumentNumber,
            user.ApplicationRoleId,
            user.ApplicationRoleName,
            user.CompanyIds));
    }

    [HttpPost("register")]
    [ProducesResponseType<RegisterSystemUserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<RegisterSystemUserResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<RegisterSystemUserResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAsync(RegisterSystemUserRequest request, CancellationToken cancellationToken)
    {
        var result = await registrationService.RegisterAsync(
            new RegisterSystemUserCommand(
                request.User.FirstName,
                request.User.LastName,
                request.User.DocumentType,
                request.User.DocumentNumber,
                request.User.UserName,
                request.User.Email,
                request.User.PhoneNumber,
                request.User.ApplicationRoleId,
                request.User.Password,
                request.Company.Ruc,
                request.Company.BusinessName,
                request.Company.Address,
                request.Company.Email,
                request.Company.Phone,
                request.Company.IsTransportCompany),
            cancellationToken);

        var response = new RegisterSystemUserResponse(
            result.Succeeded,
            result.RequiresCompanyContact,
            result.Message,
            result.UserId,
            result.CompanyId,
            result.ApplicationRoleId,
            result.ApplicationRoleName,
            result.Contact is null
                ? null
                : new RegisterCompanyContactResponse(result.Contact.FirstName, result.Contact.LastName, result.Contact.Email),
            result.Errors);

        if (result.Succeeded)
        {
            return CreatedAtAction(nameof(GetByIdAsync), new { id = result.UserId }, response);
        }

        if (result.RequiresCompanyContact)
        {
            return Conflict(response);
        }

        return BadRequest(response);
    }
}