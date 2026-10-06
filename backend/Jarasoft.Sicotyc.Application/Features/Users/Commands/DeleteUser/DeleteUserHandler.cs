
using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Common.Exceptions;
using Jarasoft.Sicotyc.Application.Exceptions;
using Jarasoft.Sicotyc.Application.Features.Users.Services;

namespace Jarasoft.Sicotyc.Application.Features.Users.Commands.DeleteUser;

public sealed class DeleteUserHandler(
    ICurrentUser currentUser,
    IIdentityService identityService,
    AdministratorProtectionService administratorProtection)
{
    public async Task HandleAsync(
        DeleteUserCommand command,
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
                "No tiene permisos para eliminar usuarios.");
        }

        // 3. Validar la identidad del solicitante.
        if (!currentUser.UserId.HasValue)
        {
            throw new UnauthorizedException(
                "No se pudo identificar al usuario autenticado.");
        }

        // 4. Obtener el usuario objetivo.
        var targetUser =
            await identityService.GetUserByIdAsync(
                command.UserId,
                cancellationToken);

        if (targetUser is null)
        {
            throw new NotFoundException(
                "El usuario no existe.");
        }

        // 5. Validar permisos preliminares.
        ValidateTargetUser(
            targetUser,
            isSuperAdministrator);

        // 6. Impedir la autoeliminación.
        ValidateSelfDeletion(targetUser);

        // 7. Ejecutar la eliminación dentro de
        //    una operación protegida por Company.
        await administratorProtection.ExecuteAsync(
            targetUser.CompanyId,
            async ct =>
            {
                // 8. Volver a consultar al usuario
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

                // 9. Comprobar que el usuario sigue
                //    perteneciendo a la misma Company.
                if (currentTarget.CompanyId !=
                    targetUser.CompanyId)
                {
                    throw new NotFoundException(
                        "El usuario no existe.");
                }

                // 10. Revalidar los permisos.
                ValidateTargetUser(
                    currentTarget,
                    isSuperAdministrator);

                // 11. Revalidar la autoeliminación.
                ValidateSelfDeletion(currentTarget);

                // 12. Proteger al último
                //     Administrator activo.
                await ValidateAdministratorInvariantAsync(
                    currentTarget,
                    ct);

                // 13. Ejecutar la eliminación
                //     dentro de la transacción.
                var result =
                    await identityService.DeleteUserAsync(
                        currentTarget.Id,
                        ct);

                if (!result.Succeeded)
                {
                    throw new ValidationException(
                        string.Join(", ", result.Errors));
                }
            },
            cancellationToken);
    }

    private void ValidateTargetUser(
        IdentityUserListItem targetUser,
        bool isSuperAdministrator)
    {
        // SuperAdministrator puede administrar
        // usuarios de cualquier Company.
        if (isSuperAdministrator)
        {
            return;
        }

        // Administrator solo puede administrar
        // usuarios de su propia Company.
        if (!currentUser.CompanyId.HasValue ||
            targetUser.CompanyId !=
            currentUser.CompanyId.Value)
        {
            throw new NotFoundException(
                "El usuario no existe.");
        }

        // Administrator no puede eliminar
        // a un SuperAdministrator.
        if (targetUser.Roles.Contains(
                ApplicationRoles.SuperAdministrator,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ForbiddenException(
                "No tiene permisos para eliminar este usuario.");
        }
    }

    private void ValidateSelfDeletion(
        IdentityUserListItem targetUser)
    {
        // Ningún usuario puede eliminarse
        // mediante este endpoint.
        if (targetUser.Id == currentUser.UserId)
        {
            throw new ValidationException(
                "No puede eliminar su propio usuario.");
        }
    }

    private async Task ValidateAdministratorInvariantAsync(
        IdentityUserListItem targetUser,
        CancellationToken cancellationToken)
    {
        // Solo necesitamos contar administradores
        // cuando se elimina uno que está activo.
        var isActiveAdministrator =
            targetUser.IsActive &&
            targetUser.Roles.Contains(
                ApplicationRoles.Administrator,
                StringComparer.OrdinalIgnoreCase);

        if (!isActiveAdministrator)
        {
            return;
        }

        // El recuento se ejecuta dentro
        // de la operación protegida.
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
                "No se puede eliminar al último " +
                "Administrator activo de la empresa.");
        }
    }
}
