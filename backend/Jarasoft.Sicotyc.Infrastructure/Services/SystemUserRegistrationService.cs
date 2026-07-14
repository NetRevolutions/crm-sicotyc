using Jarasoft.Sicotyc.Application.Users;
using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Domain.Enums;
using Jarasoft.Sicotyc.Domain.ValueObjects;
using Jarasoft.Sicotyc.Infraestructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Jarasoft.Sicotyc.Infraestructure.Services;

public sealed class SystemUserRegistrationService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : ISystemUserRegistrationService
{
    private const string AdministratorRoleName = "Administrador";
    private const string ClientCompanyTypeName = "Cliente";
    private const string TransportCompanyTypeName = "Transportista";

    public async Task<RegisterSystemUserResult> RegisterAsync(RegisterSystemUserCommand command, CancellationToken cancellationToken = default)
    {
        var validationErrors = await ValidateAsync(command, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return RegisterSystemUserResult.Failure("No se pudo completar el registro del usuario.", validationErrors);
        }

        if (!Enum.TryParse<DocumentTypeEnum>(command.DocumentType, true, out var documentType))
        {
            return RegisterSystemUserResult.Failure(
                "El tipo de documento no es válido.",
                new Dictionary<string, string[]>
                {
                    [nameof(command.DocumentType)] = ["El tipo de documento no es válido."]
                });
        }

        var normalizedRuc = command.Ruc.Trim();
        var existingCompany = await context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(company => company.RUC.Value == normalizedRuc, cancellationToken);

        if (existingCompany is not null)
        {
            var contact = await context.UserCompanies
                .AsNoTracking()
                .Where(userCompany => userCompany.CompanyId == existingCompany.Id)
                .Select(userCompany => userCompany.User)
                .OrderBy(user => user!.Tracking!.CreatedAt)
                .Select(user => new RegistrationContactInfo(
                    user!.FirstName != null ? user.FirstName.Value : null,
                    user.LastName != null ? user.LastName.Value : null,
                    user.Email))
                .FirstOrDefaultAsync(cancellationToken);

            return RegisterSystemUserResult.ContactRequired(
                "La empresa ingresada ya fue registrada. Para crear una cuenta debe contactar a la persona que registró el RUC.",
                contact);
        }

        var administratorRole = await EnsureAdministratorRoleAsync(cancellationToken);
        var companyType = await EnsureCompanyTypeAsync(command.IsTransportCompany, cancellationToken);

        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var user = ApplicationUser.Register(
            userId,
            command.UserName.Trim(),
            command.Email.Trim(),
            command.PhoneNumber.Trim(),
            new FirstName(command.FirstName.Trim()),
            new LastName(command.LastName.Trim()),
            new DocumentIdentity(documentType, command.DocumentNumber.Trim()));

        user.SetApplicationRole(administratorRole);

        var company = new Company(
            Guid.NewGuid(),
            companyType.Id,
            new BusinessName(command.BusinessName.Trim()),
            new TradeName(command.BusinessName.Trim()),
            new Ruc(normalizedRuc),
            new Address(command.Address.Trim()),
            new Email(command.CompanyEmail.Trim()),
            new Phone(command.CompanyPhone.Trim()),
            command.IsTransportCompany,
            true,
            new Tracking(now, userId, null, null, false, null, null));

        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await context.Companies.AddAsync(company, cancellationToken);

            var identityResult = await userManager.CreateAsync(user, command.Password);
            if (!identityResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return RegisterSystemUserResult.Failure(
                    "No se pudo crear el usuario en ASP.NET Core Identity.",
                    identityResult.Errors
                        .GroupBy(error => error.Code)
                        .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray()));
            }

            var roleResult = await userManager.AddToRoleAsync(user, administratorRole.Name!);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return RegisterSystemUserResult.Failure(
                    "No se pudo asignar el rol del usuario.",
                    roleResult.Errors
                        .GroupBy(error => error.Code)
                        .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray()));
            }

            await context.UserCompanies.AddAsync(new UserCompany(user.Id, company.Id), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return RegisterSystemUserResult.Success(
                "Usuario registrado correctamente. Como el RUC no existía, la cuenta fue creada con rol Administrador.",
                user.Id,
                company.Id,
                administratorRole.Id,
                administratorRole.Name);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<Dictionary<string, string[]>> ValidateAsync(RegisterSystemUserCommand command, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(command.FirstName))
        {
            errors[nameof(command.FirstName)] = ["El nombre es obligatorio."];
        }

        if (string.IsNullOrWhiteSpace(command.LastName))
        {
            errors[nameof(command.LastName)] = ["El apellido es obligatorio."];
        }

        if (string.IsNullOrWhiteSpace(command.DocumentType))
        {
            errors[nameof(command.DocumentType)] = ["El tipo de documento es obligatorio."];
        }

        if (string.IsNullOrWhiteSpace(command.DocumentNumber))
        {
            errors[nameof(command.DocumentNumber)] = ["El número de documento es obligatorio."];
        }

        if (string.IsNullOrWhiteSpace(command.UserName))
        {
            errors[nameof(command.UserName)] = ["El nombre de usuario es obligatorio."];
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            errors[nameof(command.Email)] = ["El correo del usuario es obligatorio."];
        }

        if (string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            errors[nameof(command.PhoneNumber)] = ["El teléfono del usuario es obligatorio."];
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            errors[nameof(command.Password)] = ["La contraseña es obligatoria."];
        }

        if (command.ApplicationRoleId == Guid.Empty)
        {
            errors[nameof(command.ApplicationRoleId)] = ["El rol de aplicación es obligatorio."];
        }

        if (string.IsNullOrWhiteSpace(command.Ruc))
        {
            errors[nameof(command.Ruc)] = ["El RUC es obligatorio."];
        }

        if (string.IsNullOrWhiteSpace(command.BusinessName))
        {
            errors[nameof(command.BusinessName)] = ["La razón social es obligatoria."];
        }

        if (string.IsNullOrWhiteSpace(command.Address))
        {
            errors[nameof(command.Address)] = ["La dirección es obligatoria."];
        }

        if (string.IsNullOrWhiteSpace(command.CompanyEmail))
        {
            errors[nameof(command.CompanyEmail)] = ["El correo de la empresa es obligatorio."];
        }

        if (string.IsNullOrWhiteSpace(command.CompanyPhone))
        {
            errors[nameof(command.CompanyPhone)] = ["El teléfono de la empresa es obligatorio."];
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        if (await userManager.FindByNameAsync(command.UserName.Trim()) is not null)
        {
            errors[nameof(command.UserName)] = ["El nombre de usuario ya existe."];
        }

        if (await userManager.FindByEmailAsync(command.Email.Trim()) is not null)
        {
            errors[nameof(command.Email)] = ["El correo del usuario ya existe."];
        }

        var documentAlreadyRegistered = await context.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.DocumentIdentity != null
                    && user.DocumentIdentity.DocumentNumber == command.DocumentNumber.Trim()
                    && user.DocumentIdentity.DocumentType.ToString() == command.DocumentType.Trim(),
                cancellationToken);

        if (documentAlreadyRegistered)
        {
            errors[nameof(command.DocumentNumber)] = ["El documento del usuario ya se encuentra registrado."];
        }

        return errors;
    }

    private async Task<ApplicationRole> EnsureAdministratorRoleAsync(CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByNameAsync(AdministratorRoleName);
        if (role is not null)
        {
            return role;
        }

        role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = AdministratorRoleName,
            NormalizedName = AdministratorRoleName.ToUpperInvariant()
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"No se pudo crear el rol '{AdministratorRoleName}'.");
        }

        return role;
    }

    private async Task<CompanyType> EnsureCompanyTypeAsync(bool isTransportCompany, CancellationToken cancellationToken)
    {
        var companyTypeName = isTransportCompany ? TransportCompanyTypeName : ClientCompanyTypeName;

        var companyType = await context.CompanyTypes
            .FirstOrDefaultAsync(entity => entity.Name == companyTypeName, cancellationToken);

        if (companyType is not null)
        {
            return companyType;
        }

        companyType = CompanyType.Create(
            companyTypeName,
            isTransportCompany ? "Empresa transportista creada durante el autoregistro." : "Empresa cliente creada durante el autoregistro.");

        await context.CompanyTypes.AddAsync(companyType, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return companyType;
    }
}