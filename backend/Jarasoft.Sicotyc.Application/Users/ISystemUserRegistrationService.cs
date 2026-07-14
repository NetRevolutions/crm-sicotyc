namespace Jarasoft.Sicotyc.Application.Users;

public interface ISystemUserRegistrationService
{
    Task<RegisterSystemUserResult> RegisterAsync(RegisterSystemUserCommand command, CancellationToken cancellationToken = default);
}