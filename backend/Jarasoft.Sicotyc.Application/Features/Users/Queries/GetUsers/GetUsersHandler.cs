using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Application.Common.Exceptions;
using Jarasoft.Sicotyc.Application.Exceptions;

namespace Jarasoft.Sicotyc.Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersHandler(
    ICurrentUser currentUser,
    IIdentityService identityService,
    ICompanyRepository companyRepository)
{
    public async Task<IReadOnlyCollection<UserListItem>>
        HandleAsync(
            GetUsersQuery query,
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
                "No tiene permisos para consultar usuarios.");
        }

        Guid companyId;

        if (isSuperAdministrator)
        {
            if (!query.CompanyId.HasValue)
            {
                throw new ValidationException(
                    "CompanyId es obligatorio para un SuperAdministrator.");
            }

            companyId =
                query.CompanyId.Value;
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

        var users =
            await identityService.GetUsersByCompanyAsync(
                companyId,
                cancellationToken);

        return users
            .Select(x =>
                new UserListItem(
                    x.Id,
                    x.CompanyId,
                    x.FirstName,
                    x.LastName,
                    x.Email,
                    x.IsActive,
                    x.Roles))
            .ToArray();
    }
}