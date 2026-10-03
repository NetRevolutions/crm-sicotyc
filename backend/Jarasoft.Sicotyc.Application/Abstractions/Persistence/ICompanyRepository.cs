using Jarasoft.Sicotyc.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jarasoft.Sicotyc.Application.Abstractions.Persistence
{
    public interface ICompanyRepository
    {
        Task<Company?> GetByRucAsync(
            string ruc,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Company company,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
