using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Application.Exceptions;
using Jarasoft.Sicotyc.Domain.Entities;

namespace Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Register;

public sealed class RegisterUserHandler(
    ICompanyRepository companyRepository,
    IUbigeoRepository ubigeoRepository,
    IIdentityService identityService,
    IUnitOfWork unitOfWork)
{
    public async Task<RegisterUserResult> HandleAsync(
    RegisterUserCommand command,
    CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim();
        var ruc = command.Ruc.Trim();
        var idDist = command.IdDist.Trim();

        if (await identityService.EmailExistsAsync(
                email,
                cancellationToken))
        {
            throw new ConflictException(
                "Ya existe un usuario registrado con ese correo.");
        }

        await unitOfWork.BeginTransactionAsync(
            cancellationToken);

        try
        {
            var company = await companyRepository.GetByRucAsync(
                ruc,
                cancellationToken);

            if (company is null)
            {
                var ubigeoExists =
                    await ubigeoRepository.ExistsByIdDistAsync(
                        idDist,
                        cancellationToken);

                if (!ubigeoExists)
                {
                    throw new ValidationException(
                        $"El Ubigeo '{idDist}' no existe.");
                }

                company = new Company(
                    ruc,
                    command.NombreEmpresa,
                    command.Direccion,
                    idDist,
                    command.CompanyEmail);

                await companyRepository.AddAsync(
                    company,
                    cancellationToken);

                await unitOfWork.SaveChangesAsync(
                    cancellationToken);
            }

            var registration =
                await identityService.CreateUserAsync(
                    company.Id,
                    command.FirstName,
                    command.LastName,
                    command.Email,
                    command.Password,
                    ApplicationRoles.User,
                    cancellationToken);

            if (!registration.Succeeded)
            {
                throw new ValidationException(
                    string.Join("; ", registration.Errors));
            }

            await unitOfWork.CommitTransactionAsync(
                cancellationToken);

            return new RegisterUserResult(
                registration.UserId!.Value,
                company.Id,
                email);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(
                cancellationToken);

            throw;
        }
    }
}