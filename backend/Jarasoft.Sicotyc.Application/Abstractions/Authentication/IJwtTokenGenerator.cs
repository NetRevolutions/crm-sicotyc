namespace Jarasoft.Sicotyc.Application.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(
        Guid userId,
        Guid companyId,
        string email,
        string firstName,
        string lastName,
        IReadOnlyCollection<string> roles);
}