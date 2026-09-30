using Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Login;
using Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Register;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetAllDepartamentos;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetDistritosByDepartamentoProvincia;
using Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetProvinciasByDepartamento;
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

        return services;
    }
}
