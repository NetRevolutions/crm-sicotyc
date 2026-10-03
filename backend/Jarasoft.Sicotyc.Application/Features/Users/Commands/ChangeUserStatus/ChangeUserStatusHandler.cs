using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Common.Exceptions;
using Jarasoft.Sicotyc.Application.Exceptions;
using Jarasoft.Sicotyc.Application.Features.Users.Services;

namespace Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserStatus;

public sealed class ChangeUserStatusHandler(
    ICurrentUser currentUser,
    IIdentityService identityService,
    AdministratorProtectionService administratorProtection)
{
    public async Task<ChangeUserStatusResponse> HandleAsync(
        ChangeUserStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar autenticación.
        if (!currentUser.IsAuthenticated)
        {
            throw new UnauthorizedException(
                "El usuario no está autenticado.");
        }

        // 2. Validar permisos administrativos.
        var isAdministrator = currentUser.Roles.Contains(
            ApplicationRoles.Administrator);

        if (!currentUser.IsSuperAdministrator &&
            !isAdministrator)
        {
            throw new ForbiddenException(
                "No tiene permisos para administrar usuarios.");
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

        // 4. Validar el aislamiento entre Companies.
        if (!currentUser.IsSuperAdministrator &&
            currentUser.CompanyId != targetUser.CompanyId)
        {
            throw new NotFoundException(
                "El usuario no existe.");
        }

        // 5. Un Administrator no puede modificar
        //    a un SuperAdministrator.
        if (!currentUser.IsSuperAdministrator &&
            targetUser.Roles.Contains(
                ApplicationRoles.SuperAdministrator))
        {
            throw new ForbiddenException(
                "No puede modificar un SuperAdministrator.");
        }

        // 6. Impedir la autodesactivación.
        if (!command.IsActive &&
            currentUser.UserId == targetUser.Id)
        {
            throw new ValidationException(
                "No puede desactivar su propia cuenta.");
        }

        // 7. Las activaciones no reducen el número
        //    de Administrators activos.
        if (command.IsActive)
        {
            if (!targetUser.IsActive)
            {
                await ChangeStatusAsync(
                    targetUser.Id,
                    true,
                    cancellationToken);
            }

            return new ChangeUserStatusResponse(
                targetUser.Id,
                true,
                "Usuario activado correctamente.");
        }

        // 8. Todas las desactivaciones se ejecutan
        //    mediante la operación protegida.
        await administratorProtection.ExecuteAsync(
            targetUser.CompanyId,
            async ct =>
            {
                // 9. Volver a consultar el usuario
                //    dentro de la transacción.
                var currentTarget =
                    await identityService.GetUserByIdAsync(
                        command.UserId,
                        ct);

                if (currentTarget is null)
                {
                    throw new NotFoundException(
                        "El usuario no existe.");
                }

                // 10. Verificar que el usuario
                //     sigue perteneciendo a la misma Company.
                if (currentTarget.CompanyId !=
                    targetUser.CompanyId)
                {
                    throw new ConflictException(
                        "La empresa del usuario ha cambiado.");
                }

                // 11. Revalidar el aislamiento entre Companies.
                if (!currentUser.IsSuperAdministrator &&
                    currentUser.CompanyId !=
                    currentTarget.CompanyId)
                {
                    throw new NotFoundException(
                        "El usuario no existe.");
                }

                // 12. Revalidar la protección
                //     del SuperAdministrator.
                if (!currentUser.IsSuperAdministrator &&
                    currentTarget.Roles.Contains(
                        ApplicationRoles.SuperAdministrator))
                {
                    throw new ForbiddenException(
                        "No puede modificar un SuperAdministrator.");
                }

                // 13. Revalidar la autodesactivación.
                if (currentUser.UserId == currentTarget.Id)
                {
                    throw new ValidationException(
                        "No puede desactivar su propia cuenta.");
                }

                // 14. Determinar si estamos desactivando
                //     a un Administrator activo.
                var isActiveAdministrator =
                    currentTarget.IsActive &&
                    currentTarget.Roles.Contains(
                        ApplicationRoles.Administrator);

                if (isActiveAdministrator)
                {
                    // 15. Contar Administrators activos
                    //     dentro de la operación protegida.
                    var activeAdministrators =
                        await identityService
                            .CountActiveUsersInRoleAsync(
                                currentTarget.CompanyId,
                                ApplicationRoles.Administrator,
                                ct);

                    // 16. Proteger al último Administrator.
                    if (activeAdministrators <= 1)
                    {
                        throw new ValidationException(
                            "La empresa debe conservar al menos " +
                            "un Administrator activo.");
                    }
                }

                if (!currentTarget.IsActive)
                {
                    return;
                }

                // 17. Persistir la desactivación.
                await ChangeStatusAsync(
                    currentTarget.Id,
                    false,
                    ct);
            },
            cancellationToken);

        return new ChangeUserStatusResponse(
            targetUser.Id,
            false,
            "Usuario desactivado correctamente.");
            }        

    private async Task ChangeStatusAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var result =
            await identityService.SetUserActiveStatusAsync(
                userId,
                isActive,
                cancellationToken);

        if (!result.Succeeded)
        {
            throw new ValidationException(
                string.Join(", ", result.Errors));
        }
    }
}