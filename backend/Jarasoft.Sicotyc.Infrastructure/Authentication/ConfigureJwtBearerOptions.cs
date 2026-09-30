using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    }
}