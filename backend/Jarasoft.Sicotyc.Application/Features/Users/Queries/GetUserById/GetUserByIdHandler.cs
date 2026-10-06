using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Common.Exceptions;
using Jarasoft.Sicotyc.Application.Exceptions;

namespace Jarasoft.Sicotyc.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdHandler(
    ICurrentUser currentUser,
    IIdentityService identityService)
{
    public async Task<GetUserByIdResult> HandleAsync(
        GetUserByIdQuery query,
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

        var user =
            await identityService.GetUserByIdAsync(
                query.UserId,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "El usuario no existe.");
        }

        if (!isSuperAdministrator)
        {
            if (!currentUser.CompanyId.HasValue)
            {
                throw new ForbiddenException(
                    "El usuario autenticado no tiene una empresa asociada.");
            }

            if (user.CompanyId != currentUser.CompanyId.Value)
            {
                throw new NotFoundException(
                    "El usuario no existe.");
            }
        }

        return new GetUserByIdResult(
            user.Id,
            user.CompanyId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.IsActive,
            user.Roles);
    }
}