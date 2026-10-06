using Jarasoft.Sicotyc.Application.Abstractions.Persistence;

namespace Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetProvinciasByDepartamento
{
    public sealed class GetProvinciasByDepartamentoHandler
    {
        private readonly IUbigeoRepository _repository;

        public GetProvinciasByDepartamentoHandler(
            IUbigeoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<ProvinciaDto>> HandleAsync(
            GetProvinciasByDepartamentoQuery query,
            CancellationToken cancellationToken)
        {
            var provincias =
                await _repository.GetProvinciasByDepartamentoAsync(
                    query.Departamento,
                    cancellationToken);

            return provincias
                .Select(x => new ProvinciaDto(x))
                .ToList();
        }
    }
}
