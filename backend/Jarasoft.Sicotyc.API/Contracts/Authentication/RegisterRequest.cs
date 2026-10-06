namespace Jarasoft.Sicotyc.API.Contracts.Authentication
{
    public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Ruc,
    string NombreEmpresa,
    string Direccion,
    string IdDist,
    string? CompanyEmail);
}
