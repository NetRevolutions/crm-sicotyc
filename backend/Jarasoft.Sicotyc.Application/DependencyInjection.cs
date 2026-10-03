using Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Login;
using Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Register;
using Jarasoft.Sicotyc.Application.Features.Companies.Commands.DeactivateCompanyUsers;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetAllDepartamentos;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetDistritosByDepartamentoProvincia;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetProvinciasByDepartamento;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserRole;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.ChangeUserStatus;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.CreateUser;
using Jarasoft.Sicotyc.Application.Features.Users.Commands.DeleteUser;
using Jarasoft.Sicotyc.Application.Features.Users.Queries.GetUserById;
using Jarasoft.Sicotyc.Application.Features.Users.Queries.GetUsers;
using Jarasoft.Sicotyc.Application.Features.Users.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Jarasoft.Sicotyc.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // Registraremos aquí nuestros handlers       
        services.AddScoped<GetAllDepartamentosHandler>();
        services.AddScoped<GetProvinciasByDepartamentoHandler>();
        services.AddScoped<GetDistritosByDepartamentoProvinciaHandler>();
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<GetUserByIdHandler>();
        services.AddScoped<ChangeUserStatusHandler>();
        services.AddScoped<ChangeUserRoleHandler>();
        services.AddScoped<DeleteUserHandler>();
        services.AddScoped<DeactivateCompanyUsersHandler>();        

        // Registraremos aquí nuestros servicios
        services.AddScoped<AdministratorProtectionService>();

        return services;
    }
}
