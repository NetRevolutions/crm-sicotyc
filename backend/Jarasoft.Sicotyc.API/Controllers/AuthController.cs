using Jarasoft.Sicotyc.API.Contracts.Authentication;
using Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Login;
using Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Register;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Jarasoft.Sicotyc.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    RegisterUserHandler registerUserHandler,
    LoginHandler loginHandler
    /*,UserManager<ApplicationUser> userManager*/)
    : ControllerBase
{

    [Authorize(Roles = "Administrator")]
    [HttpGet("admin-test")]
    public IActionResult AdminTest()
    {
        return Ok(new
        {
            Message = "Acceso de administrador autorizado."
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId =
            User.FindFirstValue(
                JwtRegisteredClaimNames.Sub);

        var email =
            User.FindFirstValue(
                JwtRegisteredClaimNames.Email);

        var companyId =
            User.FindFirstValue(
                "company_id");

        var firstName =
            User.FindFirstValue(
                JwtRegisteredClaimNames.GivenName);

        var lastName =
            User.FindFirstValue(
                JwtRegisteredClaimNames.FamilyName);

        var roles =
            User.FindAll(ClaimTypes.Role)
                .Select(x => x.Value)
                .ToArray();

        return Ok(new
        {
            UserId = userId,
            CompanyId = companyId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Roles = roles
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResult>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new RegisterUserCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                request.Ruc,
                request.NombreEmpresa,
                request.Direccion,
                request.IdDist,
                request.CompanyEmail);

        var result =
            await registerUserHandler.HandleAsync(
                command,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new LoginCommand(
                request.Email,
                request.Password);

        var result =
            await loginHandler.HandleAsync(
                command,
                cancellationToken);

        return Ok(result);
    }

    /*
    [HttpPost("dev/reset-password")]
    public async Task<IActionResult> ResetPassword(
    string email,
    string newPassword)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return NotFound("Usuario no encontrado.");
        }

        var token =
            await userManager.GeneratePasswordResetTokenAsync(user);

        var result =
            await userManager.ResetPasswordAsync(
                user,
                token,
                newPassword);

        if (!result.Succeeded)
        {
            return BadRequest(
                result.Errors.Select(x => x.Description));
        }

        return Ok("Contraseña actualizada.");
    }
    
    [HttpPost("dev/change-password")]
    public async Task<IActionResult> ChangePasswordForDevelopment(
    string email,
    string newPassword)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return NotFound("Usuario no encontrado.");
        }

        var removeResult =
            await userManager.RemovePasswordAsync(user);

        if (!removeResult.Succeeded)
        {
            return BadRequest(
                removeResult.Errors.Select(x => x.Description));
        }

        var addResult =
            await userManager.AddPasswordAsync(
                user,
                newPassword);

        if (!addResult.Succeeded)
        {
            return BadRequest(
                addResult.Errors.Select(x => x.Description));
        }

        return Ok("Contraseña actualizada.");
    }
    */
}