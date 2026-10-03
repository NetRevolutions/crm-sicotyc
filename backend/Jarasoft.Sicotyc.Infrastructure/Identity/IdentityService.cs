using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Common.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Jarasoft.Sicotyc.Infrastructure.Identity
{
    public sealed class IdentityService(
        UserManager<ApplicationUser> userManager
        , RoleManager<ApplicationRole> roleManager) : IIdentityService
    {


        public async Task<IReadOnlyList<IdentityUserDto>>
            GetUsersByCompanyIdAsync(
                Guid companyId,
                CancellationToken cancellationToken = default)
        {
            var users = await userManager.Users
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId)
                .OrderBy(x => x.Email)
                .ToListAsync(cancellationToken);

            var result = new List<IdentityUserDto>(
                users.Count);

            foreach (var user in users)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var roles = await userManager
                    .GetRolesAsync(user);

                result.Add(new IdentityUserDto
                {
                    Id = user.Id,
                    CompanyId = user.CompanyId,
                    Email = user.Email ?? string.Empty,
                    IsActive = user.IsActive,
                    Roles = roles.ToArray()
                });
            }

            return result;
        }

        public async Task<IdentityOperationResult> SetUserRoleAsync(
            Guid userId,
            string role,
            CancellationToken cancellationToken = default)
        {
            var user =
                await userManager.FindByIdAsync(
                    userId.ToString());

            if (user is null)
            {
                return new IdentityOperationResult(
                    false,
                    ["El usuario no existe."]);
            }

            if (!await roleManager.RoleExistsAsync(role))
            {
                return new IdentityOperationResult(
                    false,
                    [$"El rol '{role}' no existe."]);
            }

            var currentRoles =
                await userManager.GetRolesAsync(user);

            if (currentRoles.Count == 1 &&
                string.Equals(
                    currentRoles[0],
                    role,
                    StringComparison.OrdinalIgnoreCase))
            {
                return new IdentityOperationResult(
                    true,
                    Array.Empty<string>());
            }

            if (currentRoles.Count > 0)
            {
                var removeResult =
                    await userManager.RemoveFromRolesAsync(
                        user,
                        currentRoles);

                if (!removeResult.Succeeded)
                {
                    return new IdentityOperationResult(
                        false,
                        removeResult.Errors
                            .Select(x => x.Description)
                            .ToArray());
                }
            }

            var addResult =
                await userManager.AddToRoleAsync(
                    user,
                    role);

            if (!addResult.Succeeded)
            {
                return new IdentityOperationResult(
                    false,
                    addResult.Errors
                        .Select(x => x.Description)
                        .ToArray());
            }

            return new IdentityOperationResult(
                true,
                Array.Empty<string>());
        }

        public async Task<IdentityOperationResult> SetUserActiveStatusAsync(
            Guid userId,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            var user =
                await userManager.FindByIdAsync(
                    userId.ToString());

            if (user is null)
            {
                return new IdentityOperationResult(
                    false,
                    ["El usuario no existe."]);
            }

            user.IsActive = isActive;

            var result =
                await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new IdentityOperationResult(
                    false,
                    result.Errors
                        .Select(x => x.Description)
                        .ToArray());
            }

            return new IdentityOperationResult(
                true,
                Array.Empty<string>());
        }

        public async Task<int> CountActiveUsersInRoleAsync(
            Guid companyId,
            string role,
            CancellationToken cancellationToken = default)
        {
            var usersInRole =
                await userManager.GetUsersInRoleAsync(role);

            return usersInRole.Count(
                x => x.CompanyId == companyId &&
                     x.IsActive);
        }

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
            string role,
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

            var result =
                await userManager.CreateAsync(
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

            var roleResult =
                await userManager.AddToRoleAsync(
                    user,
                    role);

            if (!roleResult.Succeeded)
            {
                return new IdentityRegistrationResult(
                    false,
                    user.Id,
                    roleResult.Errors
                        .Select(x => x.Description)
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

        public async Task<IReadOnlyCollection<IdentityUserListItem>>
        GetUsersByCompanyAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
        {
            var users =
                await userManager.Users
                    .Where(x => x.CompanyId == companyId)
                    .OrderBy(x => x.FirstName)
                    .ThenBy(x => x.LastName)
                    .ToListAsync(cancellationToken);

            var result =
                new List<IdentityUserListItem>();

            foreach (var user in users)
            {
                var roles =
                    await userManager.GetRolesAsync(user);

                result.Add(
                    new IdentityUserListItem(
                        user.Id,
                        user.CompanyId,
                        user.FirstName,
                        user.LastName,
                        user.Email ?? string.Empty,
                        user.IsActive,
                        roles.ToArray()));
            }

            return result;
        }

        public async Task<IdentityUserListItem?> GetUserByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user =
                await userManager.Users
                    .FirstOrDefaultAsync(
                        x => x.Id == userId,
                        cancellationToken);

            if (user is null)
                return null;

            var roles =
                await userManager.GetRolesAsync(user);

            return new IdentityUserListItem(
                user.Id,
                user.CompanyId,
                user.FirstName,
                user.LastName,
                user.Email ?? string.Empty,
                user.IsActive,
                roles.ToArray());
        }

        public async Task<IdentityOperationResult> DeleteUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user =
                await userManager.FindByIdAsync(
                    userId.ToString());

            if (user is null)
            {
                return new IdentityOperationResult(
                    false,
                    ["El usuario no existe."]);
            }

            var result =
                await userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return new IdentityOperationResult(
                    false,
                    result.Errors
                        .Select(x => x.Description)
                        .ToArray());
            }

            return new IdentityOperationResult(
                true,
                Array.Empty<string>());
        }


        public async Task<IdentityBulkOperationResult> DeactivateUsersByCompanyAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (companyId == Guid.Empty)
            {
                return new IdentityBulkOperationResult(
                    false,
                    0,
                    new[]
                    {
                "El identificador de la empresa es obligatorio."
                    });
            }

            // 1. Obtener únicamente los usuarios activos
            //    que pertenecen a la Company indicada.
            var users = await userManager.Users
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.IsActive)
                .ToListAsync(cancellationToken);

            if (users.Count == 0)
            {
                return new IdentityBulkOperationResult(
                    true,
                    0,
                    Array.Empty<string>());
            }

            var affectedUsers = 0;

            // 2. Desactivar los usuarios dentro de
            //    la transacción abierta por el handler.
            foreach (var user in users)
            {
                cancellationToken.ThrowIfCancellationRequested();

                user.IsActive = false;

                var result = await userManager.UpdateAsync(user);

                // 3. Detener la operación ante el primer error.
                //    El handler provocará el rollback.
                if (!result.Succeeded)
                {
                    var errors = result.Errors
                        .Select(x =>
                            $"{user.Email}: {x.Description}")
                        .ToArray();

                    return new IdentityBulkOperationResult(
                        false,
                        affectedUsers,
                        errors);
                }

                affectedUsers++;
            }

            // 4. Comprobar la cancelación antes de
            //    devolver el resultado.
            cancellationToken.ThrowIfCancellationRequested();

            return new IdentityBulkOperationResult(
                true,
                affectedUsers,
                Array.Empty<string>());
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
