using Jarasoft.Sicotyc.Api.Contracts.Users;
using Jarasoft.Sicotyc.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace Jarasoft.Sicotyc.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(ISystemUserRegistrationService registrationService) : ControllerBase
{
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
            return CreatedAtAction(nameof(RegisterAsync), response);
        }

        if (result.RequiresCompanyContact)
        {
            return Conflict(response);
        }

        return BadRequest(response);
    }
}