namespace Jarasoft.Sicotyc.Application.Abstractions.Authentication;

public sealed record JwtTokenResult(
    string AccessToken,
    DateTime ExpiresAt);