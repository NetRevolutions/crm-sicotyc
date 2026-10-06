
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
        // 1. Validar los argumentos.
        ArgumentNullException.ThrowIfNull(command);

        cancellationToken.ThrowIfCancellationRequested();

        if (command.CompanyId == Guid.Empty)
        {
            throw new ValidationException(
                "El identificador de la empresa es obligatorio.");
        }

        // 2. Validar autenticación.
        if (!currentUser.IsAuthenticated)
        {
            throw new UnauthorizedException(
                "Usuario no autenticado.");
        }

        // 3. Validar autorización.
        if (!currentUser.IsSuperAdministrator)
        {
            throw new ForbiddenException(
                "Solo un SuperAdministrator puede desactivar " +
                "todos los usuarios de una empresa.");
        }

        // 4. Impedir que el SuperAdministrator
        //    desactive su propia Company.
        if (currentUser.CompanyId == command.CompanyId)
        {
            throw new ValidationException(
                "No puede desactivar todos los usuarios " +
                "de la empresa a la que pertenece su propia cuenta.");
        }

        var affectedUsers = 0;

        // 5. Ejecutar la operación bajo el bloqueo
        //    exclusivo de la Company.
        await administratorProtection.ExecuteAsync(
            command.CompanyId,
            async ct =>
            {
                ct.ThrowIfCancellationRequested();

                // 6. Comprobar la existencia de la
                //    Company dentro de la transacción.
                var companyExists =
                    await companyRepository.ExistsByIdAsync(
                        command.CompanyId,
                        ct);

                if (!companyExists)
                {
                    throw new NotFoundException(
                        "La empresa no existe.");
                }

                // 7. Revalidar la restricción de
                //    la Company del solicitante.
                if (currentUser.CompanyId == command.CompanyId)
                {
                    throw new ValidationException(
                        "No puede desactivar todos los usuarios " +
                        "de la empresa a la que pertenece su propia cuenta.");
                }

                // 8. Desactivar los usuarios dentro
                //    de la transacción existente.
                var result =
                    await identityService
                        .DeactivateUsersByCompanyAsync(
                            command.CompanyId,
                            ct);

                // 9. Ante cualquier error,
                //    AdministratorProtectionService
                //    ejecutará el rollback.
                if (!result.Succeeded)
                {
                    throw new ValidationException(
                        string.Join(
                            ", ",
                            result.Errors));
                }

                affectedUsers = result.AffectedUsers;
            },
            cancellationToken);

        // 10. La transacción ya fue confirmada.
        return new DeactivateCompanyUsersResult(
            command.CompanyId,
            affectedUsers);
    }
}
