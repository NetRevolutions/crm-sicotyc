using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Application.Common.Exceptions;
using Jarasoft.Sicotyc.Application.Exceptions;
using Jarasoft.Sicotyc.Application.Features.Users.Services;

namespace Jarasoft.Sicotyc.Application.Features.Companies.Commands.DeactivateCompanyUsers;

public sealed class DeactivateCompanyUsersHandler(
    ICurrentUser currentUser,
    IIdentityService identityService,
    ICompanyRepository companyRepository,
    AdministratorProtectionService administratorProtection)
{
    public async Task<DeactivateCompanyUsersResult> HandleAsync(
        DeactivateCompanyUsersCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar autenticación.
        if (!currentUser.IsAuthenticated)
        {
            throw new UnauthorizedException(
                "Usuario no autenticado.");
        }

        // 2. Solo SuperAdministrator puede
        //    ejecutar una desactivación masiva.
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException(
                "Solo un SuperAdministrator puede desactivar " +
                "todos los usuarios de una empresa.");
        }

        // 3. Validar la existencia de la Company.
        var companyExists =
            await companyRepository.ExistsByIdAsync(
                command.CompanyId,
                cancellationToken);

        if (!companyExists)
        {
            throw new NotFoundException(
                "La empresa no existe.");
        }

        // 4. Impedir que el SuperAdministrator
        //    desactive su propia Company.
        if (currentUser.CompanyId == command.CompanyId)
        {
            throw new ValidationException(
                "No puede desactivar todos los usuarios " +
                "de la empresa a la que pertenece su propia cuenta.");
        }

        // 5. Conservar el número de usuarios afectados
        //    para construir la respuesta original.
        var affectedUsers = 0;

        // 6. Ejecutar la operación bajo el bloqueo
        //    exclusivo de la Company.
        await administratorProtection.ExecuteAsync(
            command.CompanyId,
            async ct =>
            {
                // 7. Revalidar la restricción de
                //    la Company del solicitante.
                if (currentUser.CompanyId == command.CompanyId)
                {
                    throw new ValidationException(
                        "No puede desactivar todos los usuarios " +
                        "de la empresa a la que pertenece su propia cuenta.");
                }

                // 8. Ejecutar la desactivación masiva
                //    dentro de la transacción existente.
                var result =
                    await identityService
                        .DeactivateUsersByCompanyAsync(
                            command.CompanyId,
                            ct);

                // 9. Cualquier error provoca rollback
                //    mediante AdministratorProtectionService.
                if (!result.Succeeded)
                {
                    throw new ValidationException(
                        string.Join(
                            ", ",
                            result.Errors));
                }

                // 10. Guardar el número de usuarios
                //     afectados por la operación.
                affectedUsers = result.AffectedUsers;
            },
            cancellationToken);

        // 11. ExecuteAsync ya confirmó la transacción.
        return new DeactivateCompanyUsersResult(
            command.CompanyId,
            affectedUsers);
    }
}
