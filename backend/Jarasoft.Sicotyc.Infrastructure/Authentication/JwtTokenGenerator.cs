using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Jarasoft.Sicotyc.Infrastructure.Authentication;

public sealed class JwtTokenGenerator(
    IOptions<JwtOptions> jwtOptions)
    : IJwtTokenGenerator
{
    private readonly JwtOptions _jwtOptions =
        jwtOptions.Value;

    public JwtTokenResult GenerateToken(
        Guid userId,
        Guid companyId,
        string email,
        string firstName,
        string lastName,
        IReadOnlyCollection<string> roles)
    {
        var now = DateTime.UtcNow;

        var expiresAt =
            now.AddMinutes(
                _jwtOptions.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                userId.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                email),

            new(
                "company_id",
                companyId.ToString()),

            new(
                JwtRegisteredClaimNames.GivenName,
                firstName),

            new(
                JwtRegisteredClaimNames.FamilyName,
                lastName),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _jwtOptions.SecretKey));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: credentials);

        var accessToken =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return new JwtTokenResult(
            accessToken,
            expiresAt);
    }
}