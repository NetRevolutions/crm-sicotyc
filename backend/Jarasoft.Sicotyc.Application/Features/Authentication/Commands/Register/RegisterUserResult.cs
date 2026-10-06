namespace Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Register;

public sealed record RegisterUserResult(
    Guid UserId,
    Guid CompanyId,
    string Email);