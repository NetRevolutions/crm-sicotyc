using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Domain.Entities;
using Jarasoft.Sicotyc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jarasoft.Sicotyc.Infrastructure.Persistence.Repositories
{
    public sealed class UbigeoRepository : IUbigeoRepository
    {
        private readonly SicotycDbContext _dbContext;

        public UbigeoRepository(SicotycDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Ubigeo>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Ubigeos
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<string>> GetDepartamentosAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Ubigeos
                .AsNoTracking()
                .Select(u => u.NombreDepartamento)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Ubigeo>> GetDistritosByDepartamentoProvinciaAsync(
            string departamento, string provincia, CancellationToken cancellationToken)
        {
            return await _dbContext.Ubigeos
                .AsNoTracking()
                .Where(u => u.NombreDepartamento == departamento && u.NombreProvincia == provincia)
                .OrderBy(u => u.NombreDistrito)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<string>> GetProvinciasByDepartamentoAsync(string departamento, CancellationToken cancellationToken)
        {
            return await _dbContext.Ubigeos
                .AsNoTracking()
                .Where(u => u.NombreDepartamento == departamento)
                .Select(u => u.NombreProvincia)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync(cancellationToken);
        }

        public Task<bool> ExistsByIdDistAsync(string idDist, CancellationToken cancellationToken = default)
        {
            return _dbContext.Ubigeos
                .AnyAsync(
                    x => x.IdDist == idDist,
                    cancellationToken);
        }
    }
}
