using System.Collections.Concurrent;
using Jarasoft.Sicotyc.Application.Abstractions.Persistence;

namespace Jarasoft.Sicotyc.Test.Common;

public sealed class TestCompanyAdministrationLock
    : ICompanyAdministrationLock
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim>
        CompanyLocks = new();

    private SemaphoreSlim? _currentLock;

    public async Task AcquireAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        if (_currentLock is not null)
        {
            throw new InvalidOperationException(
                "Ya existe un bloqueo adquirido.");
        }

        var semaphore = CompanyLocks.GetOrAdd(
            companyId,
            _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync(cancellationToken);

        _currentLock = semaphore;
    }

    public Task ReleaseAsync()
    {
        var semaphore = _currentLock;

        _currentLock = null;

        semaphore?.Release();

        return Task.CompletedTask;
    }
}