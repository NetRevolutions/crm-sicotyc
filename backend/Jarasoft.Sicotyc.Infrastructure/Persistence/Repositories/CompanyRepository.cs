using Jarasoft.Sicotyc.Application.Abstractions.Persistence;
using Jarasoft.Sicotyc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Jarasoft.Sicotyc.Infrastructure.Persistence.Repositories
{
    public sealed class CompanyRepository(
        SicotycDbContext context)
        : ICompanyRepository
    {
        public async Task<Company?> GetByRucAsync(
            string ruc, 
            CancellationToken cancellationToken = default)
        {
            return await context.Companies
                .FirstOrDefaultAsync(
                    c => c.Ruc == ruc,
                    cancellationToken);

        }

        public async Task AddAsync(
            Company company, 
            CancellationToken cancellationToken = default)
        {
            await context.Companies.AddAsync(
                company, 
                cancellationToken);
        }

        public Task<bool> ExistsByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return context.Companies
                .AnyAsync(
                    x => x.Id == id,
                    cancellationToken);
        }
    }
}
