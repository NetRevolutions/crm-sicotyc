using Jarasoft.Sicotyc.Application.Abstractions.Persistence;

namespace Jarasoft.Sicotyc.Application.Features.Ubigeos.Queries.GetAllDepartamentos
{
    public sealed class GetAllDepartamentosHandler
    {
        private readonly IUbigeoRepository _repository;

        public GetAllDepartamentosHandler(IUbigeoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<DepartamentoDto>> HandleAsync(GetAllDepartamentosQuery query, CancellationToken cancellationToken)
        {
            var departamentos = await _repository.GetDepartamentosAsync(cancellationToken);

            return departamentos
                .Select(d => new DepartamentoDto(d))
                .ToList();
        }
    }
}
