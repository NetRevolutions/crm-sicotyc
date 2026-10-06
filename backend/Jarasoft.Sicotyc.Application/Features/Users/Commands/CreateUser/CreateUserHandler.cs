using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Application.Common.Exceptions;
using Jarasoft.Sicotyc.Application.Exceptions;

namespace Jarasoft.Sicotyc.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserHandler(
    ICurrentUser currentUser,
    IIdentityService identityService,
    ICompanyRepository companyRepository)
{
    public async Task<CreateUserResult> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new UnauthorizedException(
                "Usuario no autenticado.");
        }
        
        var isAdministrator =
            currentUser.Roles.Contains(
                ApplicationRoles.Administrator,
                StringComparer.OrdinalIgnoreCase);

        var isSuperAdministrator =
            currentUser.IsSuperAdministrator;

        if (!isAdministrator &&
            !isSuperAdministrator)
        {
            throw new ForbiddenException(
                "No tiene permisos para crear usuarios.");
        }

        Guid companyId;

        if (isSuperAdministrator)
        {
            if (!command.CompanyId.HasValue)
            {
                throw new ValidationException(
                    "CompanyId es obligatorio para un SuperAdministrator.");
            }

            companyId =
                command.CompanyId.Value;
        }
        else
        {
            if (!currentUser.CompanyId.HasValue)
            {
                throw new ValidationException(
                    "El usuario autenticado no tiene una empresa asociada.");
            }

            companyId =
                currentUser.CompanyId.Value;
        }

        var companyExists =
            await companyRepository.ExistsByIdAsync(
                companyId,
                cancellationToken);

        if (!companyExists)
        {
            throw new ValidationException(
                "La empresa indicada no existe.");
        }

        ValidateRole(
            command.Role,
            isSuperAdministrator);

        var emailExists =
            await identityService.EmailExistsAsync(
                command.Email,
                cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(
                "El correo ya se encuentra registrado.");
        }

        var result =
            await identityService.CreateUserAsync(
                companyId,
                command.FirstName,
                command.LastName,
                command.Email,
                command.Password,
                command.Role,
                cancellationToken);

        if (!result.Succeeded ||
            !result.UserId.HasValue)
        {
            throw new ValidationException(
                string.Join(
                    ", ",
                    result.Errors));
        }

        return new CreateUserResult(
            result.UserId.Value,
            companyId,
            command.Email,
            command.Role);
    }

    private static void ValidateRole(
        string role,
        bool isSuperAdministrator)
    {
        var allowedRoles =
            isSuperAdministrator
                ? ApplicationRoleRules
                    .SuperAdministratorAssignableRoles
                : ApplicationRoleRules
                    .AdministratorAssignableRoles;

        if (!allowedRoles.Contains(
                role,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException(
                $"El rol '{role}' no puede ser asignado.");
        }
    }
}