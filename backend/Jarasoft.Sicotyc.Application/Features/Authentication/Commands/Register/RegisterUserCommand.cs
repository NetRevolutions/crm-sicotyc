namespace Jarasoft.Sicotyc.Application.Features.Authentication.Commands.Register;

public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Ruc,
    string NombreEmpresa,
    string Direccion,
    string IdDist,
    string? CompanyEmail);