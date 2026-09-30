namespace Jarasoft.Sicotyc.API.Contracts.Authentication;

public sealed record LoginRequest(
    string Email,
    string Password);