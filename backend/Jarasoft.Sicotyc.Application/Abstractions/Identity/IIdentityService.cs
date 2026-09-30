namespace Jarasoft.Sicotyc.Application.Abstractions.Identity;

public interface IIdentityService
{
    Task<IdentityRegistrationResult> CreateUserAsync(
        Guid companyId,
        string firstName,
        string lastName,
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<IdentityLoginResult> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}