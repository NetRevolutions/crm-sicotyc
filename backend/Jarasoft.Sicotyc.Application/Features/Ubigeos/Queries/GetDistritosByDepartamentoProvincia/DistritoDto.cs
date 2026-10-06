namespace Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetDistritosByDepartamentoProvincia
{
    public sealed record DistritoDto(
        string Id,
        string Nombre,
        string Capital,
        int CodigoRegionNatural,
        string RegionNatural
    );
}
