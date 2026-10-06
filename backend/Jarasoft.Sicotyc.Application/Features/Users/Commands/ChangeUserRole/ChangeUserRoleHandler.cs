
using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Common.Exceptions;
using Jarasoft.Sicotyc.Application.Exceptions;
using Jarasoft.Sicotyc.Application.Features.Users.Services;

namespace Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserRole;

public sealed class ChangeUserRoleHandler(
    ICurrentUser currentUser,
    IIdentityService identityService,
    AdministratorProtectionService administratorProtection)
{
    public async Task<ChangeUserRoleResult> HandleAsync(
        ChangeUserRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar autenticación.
        if (!currentUser.IsAuthenticated)
        {
            throw new UnauthorizedException(
                "Usuario no autenticado.");
        }

        // 2. Validar permisos administrativos.
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
                "No tiene permisos para modificar roles.");
        }

        // 3. Obtener el usuario objetivo.
        var targetUser =
            await identityService.GetUserByIdAsync(
                command.UserId,
                cancellationToken);

        if (targetUser is null)
        {
            throw new NotFoundException(
                "El usuario no existe.");
        }

        // 4. Validar permisos sobre el usuario.
        ValidateTargetUser(
            targetUser,
            isSuperAdministrator);

        // 5. Validar que el rol puede asignarse.
        ValidateAssignableRole(
            command.Role,
            isSuperAdministrator);

        // 6. Ejecutar el cambio dentro de una
        //    operación protegida por Company.
        await administratorProtection.ExecuteAsync(
            targetUser.CompanyId,
            async ct =>
            {
                // 7. Volver a consultar al usuario
                //    después de adquirir el bloqueo.
                var currentTarget =
                    await identityService.GetUserByIdAsync(
                        command.UserId,
                        ct);

                if (currentTarget is null)
                {
                    throw new NotFoundException(
                        "El usuario no existe.");
                }

                // 8. Verificar que el usuario continúa
                //    perteneciendo a la misma Company.
                if (currentTarget.CompanyId !=
                    targetUser.CompanyId)
                {
                    throw new NotFoundException(
                        "El usuario no existe.");
                }

                // 9. Revalidar los permisos utilizando
                //    los datos actualizados.
                ValidateTargetUser(
                    currentTarget,
                    isSuperAdministrator);

                // 10. Comprobar si ya tiene
                //     exactamente el rol solicitado.
                var alreadyHasRole =
                    currentTarget.Roles.Count == 1 &&
                    currentTarget.Roles.Contains(
                        command.Role,
                        StringComparer.OrdinalIgnoreCase);

                if (alreadyHasRole)
                {
                    return;
                }

                // 11. Proteger al último
                //     Administrator activo.
                await ValidateAdministratorInvariantAsync(
                    currentTarget,
                    command.Role,
                    ct);

                // 12. Reemplazar el rol.
                var result =
                    await identityService.SetUserRoleAsync(
                        currentTarget.Id,
                        command.Role,
                        ct);

                if (!result.Succeeded)
                {
                    throw new ValidationException(
                        string.Join(", ", result.Errors));
                }
            },
            cancellationToken);

        // 13. La operación protegida terminó
        //     correctamente.
        return new ChangeUserRoleResult(
            targetUser.Id,
            command.Role);
    }

    private void ValidateTargetUser(
        IdentityUserListItem targetUser,
        bool isSuperAdministrator)
    {
        if (isSuperAdministrator)
        {
            return;
        }

        // Un Administrator solo puede administrar
        // usuarios de su propia Company.
        if (!currentUser.CompanyId.HasValue ||
            targetUser.CompanyId !=
            currentUser.CompanyId.Value)
        {
            throw new NotFoundException(
                "El usuario no existe.");
        }

        // Un Administrator no puede modificar
        // a un SuperAdministrator.
        if (targetUser.Roles.Contains(
                ApplicationRoles.SuperAdministrator,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ForbiddenException(
                "No tiene permisos para modificar este usuario.");
        }
    }

    private static void ValidateAssignableRole(
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

    private async Task ValidateAdministratorInvariantAsync(
        IdentityUserListItem targetUser,
        string newRole,
        CancellationToken cancellationToken)
    {
        var currentlyAdministrator =
            targetUser.Roles.Contains(
                ApplicationRoles.Administrator,
                StringComparer.OrdinalIgnoreCase);

        var remainsAdministrator =
            string.Equals(
                newRole,
                ApplicationRoles.Administrator,
                StringComparison.OrdinalIgnoreCase);

        // No es necesario contar administradores
        // si el usuario no pierde ese rol
        // o si ya está desactivado.
        if (!currentlyAdministrator ||
            remainsAdministrator ||
            !targetUser.IsActive)
        {
            return;
        }

        var activeAdministrators =
            await identityService.CountActiveUsersInRoleAsync(
                targetUser.CompanyId,
                ApplicationRoles.Administrator,
                cancellationToken);

        // Conservar el contrato HTTP original:
        // ValidationException -> 400 Bad Request.
        if (activeAdministrators <= 1)
        {
            throw new ValidationException(
                "No se puede quitar el rol Administrator " +
                "al último Administrator activo de la empresa.");
        }
    }
}
