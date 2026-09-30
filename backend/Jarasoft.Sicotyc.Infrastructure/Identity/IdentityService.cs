using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Identity;

namespace Jarasoft.Sicotyc.Infrastructure.Identity
{
    public sealed class IdentityService(
        UserManager<ApplicationUser> userManager) : IIdentityService
    {
        public async Task<bool> EmailExistsAsync(
            string email, 
            CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(email);

            return user is not null;
        }

        public async Task<IdentityRegistrationResult> CreateUserAsync(
            Guid companyId, 
            string firstName, 
            string lastName, 
            string email, 
            string password, 
            CancellationToken cancellationToken = default)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                UserName = email.Trim(),
                Email = email.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // 1. Crear usuario
            var result = await userManager.CreateAsync(
                user,
                password);

            if (!result.Succeeded)
            {
                return new IdentityRegistrationResult(
                    false,
                    null,
                    result.Errors
                        .Select(x => x.Description)
                        .ToArray());
            }

            // 2. Asignar rol por defecto
            var roleResult = await userManager.AddToRoleAsync(
                user, 
                ApplicationRoles.User);

            if (!roleResult.Succeeded)
            {
                return new IdentityRegistrationResult(
                    false,
                    user.Id,
                    roleResult.Errors
                    .Select(e => e.Description)
                    .ToArray());
            }

            return new IdentityRegistrationResult(
                true,
                user.Id,
                Array.Empty<string>());
        }

        public async Task<IdentityLoginResult> ValidateCredentialsAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            var normalizedEmail = email.Trim();

            var user =
                await userManager.FindByEmailAsync(
                    normalizedEmail);

            if (user is null)
            {
                return FailedLogin();
            }

            if (!user.IsActive)
            {
                return FailedLogin();
            }

            var passwordIsValid =
                await userManager.CheckPasswordAsync(
                    user,
                    password);

            if (!passwordIsValid)
            {
                return FailedLogin();
            }

            var roles =
                await userManager.GetRolesAsync(user);

            return new IdentityLoginResult(
                true,
                user.Id,
                user.CompanyId,
                user.Email,
                user.FirstName,
                user.LastName,
                roles.ToArray());
        }

        private static IdentityLoginResult FailedLogin()
        {
            return new IdentityLoginResult(
                false,
                null,
                null,
                null,
                null,
                null,
                Array.Empty<string>());
        }
    }
}
