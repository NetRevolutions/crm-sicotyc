using Jarasoft.Sicotyc.Application.Abstractions.Persistence;

namespace Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetDistritosByDepartamentoProvincia
{
    public sealed class GetDistritosByDepartamentoProvinciaHandler
    {
        private readonly IUbigeoRepository _repository;
        public GetDistritosByDepartamentoProvinciaHandler(IUbigeoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DistritoDto>> HandleAsync(GetDistritosByDepartamentoProvinciaQuery query, CancellationToken cancellationToken)
        {
            var distritos = await _repository
                                .GetDistritosByDepartamentoProvinciaAsync(
                                    query.Departamento, 
                                    query.Provincia, 
                                    cancellationToken);

            return distritos
                .Select(d => new DistritoDto(
                    d.IdDist, 
                    d.NombreDistrito, 
                    d.NombreCapital, 
                    d.CodigoRegionNatural, 
                    d.RegionNatural))
                .ToList();
        }
    }
}
