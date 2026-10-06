using Jarasoft.Sicotyc.Domain.Entities;

namespace Jarasoft.Sicotyc.Application.Abstractions.Persistence
{
    // Aquí aplicamos CQRS sin MediatR.
    public interface IUbigeoRepository
    {
        Task<IReadOnlyList<Ubigeo>> GetAllAsync(CancellationToken cancellationToken);

        Task<IReadOnlyList<string>> GetDepartamentosAsync(CancellationToken cancellationToken);

        Task<IReadOnlyList<string>> GetProvinciasByDepartamentoAsync(string departamento, CancellationToken cancellationToken);

        Task<IReadOnlyList<Ubigeo>> GetDistritosByDepartamentoProvinciaAsync(string departamento, string provincia, CancellationToken cancellationToken);

        Task<bool> ExistsByIdDistAsync(string idDist, CancellationToken cancellationToken = default);

    }    
}
