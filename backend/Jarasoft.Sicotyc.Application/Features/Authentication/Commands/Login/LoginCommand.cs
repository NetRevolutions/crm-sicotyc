namespace Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password);