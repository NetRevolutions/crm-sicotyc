using Jarasoft.Sicotyc.Application.Common.DTOs;

namespace Jarasoft.Sicotyc.Application.Abstractions.Identity;

public interface IIdentityService
{
    Task<IdentityRegistrationResult> CreateUserAsync(
        Guid companyId,
        string firstName,
        string lastName,
        string email,
        string password,
        string role,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<IdentityLoginResult> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<IdentityUserListItem>> GetUsersByCompanyAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<IdentityUserListItem?> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> SetUserActiveStatusAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<int> CountActiveUsersInRoleAsync(
        Guid companyId,
        string role,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> SetUserRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> DeleteUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IdentityBulkOperationResult> DeactivateUsersByCompanyAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    // IIdentityService
    Task<IReadOnlyList<IdentityUserDto>> GetUsersByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);
}