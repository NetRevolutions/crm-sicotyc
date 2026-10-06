using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Common.Exceptions;


namespace Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Login;

public sealed class LoginHandler(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator)
{
    public async Task<LoginResult> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim();

        var login =
            await identityService.ValidateCredentialsAsync(
                email,
                command.Password,
                cancellationToken);

        if (!login.Succeeded)
        {
            throw new UnauthorizedException(
                "Credenciales inválidas.");
        }

        var token =
            jwtTokenGenerator.GenerateToken(
                login.UserId!.Value,
                login.CompanyId!.Value,
                login.Email!,
                login.FirstName!,
                login.LastName!,
                login.Roles);

        return new LoginResult(
            login.UserId.Value,
            login.CompanyId.Value,
            login.Email!,
            login.FirstName!,
            login.LastName!,
            login.Roles,
            token.AccessToken,
            token.ExpiresAt);
    }
}