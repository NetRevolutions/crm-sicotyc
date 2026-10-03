using Jarasoft.Sicotyc.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Jarasoft.Sicotyc.Infrastructure.Authentication;

public sealed class ConfigureJwtBearerOptions(
    IOptions<JwtOptions> jwtOptions)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions =
        jwtOptions.Value;

    public void Configure(
        string? name,
        JwtBearerOptions options)
    {
        if (name !=
            JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        Configure(options);
    }

    public void Configure(
        JwtBearerOptions options)
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer =
                    _jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience =
                    _jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            _jwtOptions.SecretKey)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero,
                
                NameClaimType =
                JwtRegisteredClaimNames.Email,

                RoleClaimType =
                ClaimTypes.Role

            };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdValue =
                    context.Principal?.FindFirst(
                        JwtRegisteredClaimNames.Sub)?.Value;

                if (!Guid.TryParse(
                    userIdValue,
                    out var userId))
                {
                    context.Fail("Token inválido.");
                    return;
                }

                var identityService =
                    context.HttpContext.RequestServices
                        .GetRequiredService<IIdentityService>();

                var user =
                    await identityService.GetUserByIdAsync(
                        userId,
                        context.HttpContext.RequestAborted);

                if (user is null || !user.IsActive)
                {
                    context.Fail(
                        "El usuario no existe o está inactivo.");
                    return;
                }

                // 5. Validar que la Company del token coincida
                //    con la Company actual del usuario.
                var companyIdValue = context.Principal?
                    .FindFirst("company_id")?
                    .Value;

                if (!Guid.TryParse(companyIdValue, out var companyId)
                    || user.CompanyId != companyId)
                {
                    context.Fail("La empresa asociada al token no es válida.");
                    return;
                }
            }
        };        
    }
}