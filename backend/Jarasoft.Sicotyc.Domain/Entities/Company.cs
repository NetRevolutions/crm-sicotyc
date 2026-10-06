namespace Jarasoft.Sicotyc.Domain.Entities;

public sealed class Company
{
    private Company()
    {
    }

    public Company(
        string ruc,
        string nombreEmpresa,
        string direccion,
        string idDist,
        string? email = null)
    {
        Id = Guid.NewGuid();
        Ruc = ruc.Trim();
        NombreEmpresa = nombreEmpresa.Trim();
        Direccion = direccion.Trim();
        IdDist = idDist.Trim();

        Email = string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim();

        EstadoContribuyente = null;
        CondicionContribuyente = null;
        ActualizadoDeSunat = false;

        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Ruc { get; private set; } = string.Empty;

    public string NombreEmpresa { get; private set; } = string.Empty;
    public string? NombreComercial { get; private set; }
    public string? EstadoContribuyente { get; private set; }

    public string? CondicionContribuyente { get; private set; }

    public bool ActualizadoDeSunat { get; private set; }
    
    public DateTime? FechaActualizacionSunat { get; private set; }
    
    public string Direccion { get; private set; } = string.Empty;

    public string IdDist { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Ubigeo? Ubigeo { get; private set; }

    public void ActualizarDatosDeSunat(
        string nombreEmpresa,
        string? nombreComercial,
        string? estadoContribuyente,
        string? condicionContribuyente)
    {
        if (!string.IsNullOrWhiteSpace(nombreEmpresa))
        {
            NombreEmpresa = nombreEmpresa.Trim();
        }

        NombreComercial = string.IsNullOrWhiteSpace(nombreComercial)
            ? null
            : nombreComercial.Trim();

        EstadoContribuyente = string.IsNullOrWhiteSpace(estadoContribuyente)
            ? null
            : estadoContribuyente.Trim();

        CondicionContribuyente = string.IsNullOrWhiteSpace(condicionContribuyente)
            ? null
            : condicionContribuyente.Trim();

        ActualizadoDeSunat = true;
        FechaActualizacionSunat = DateTime.UtcNow;
    }
}