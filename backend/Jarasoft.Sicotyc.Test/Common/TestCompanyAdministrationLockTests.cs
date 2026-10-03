using FluentAssertions;

namespace Jarasoft.Sicotyc.Test.Common;

public sealed class TestCompanyAdministrationLockTests
{
    [Fact]
    public async Task AcquireAsync_ShouldWait_WhenCompanyIsLocked()
    {
        var companyId = Guid.NewGuid();

        var firstLock = new TestCompanyAdministrationLock();
        var secondLock = new TestCompanyAdministrationLock();

        await firstLock.AcquireAsync(companyId);

        using var cancellation =
            new CancellationTokenSource();

        var secondAcquisition = secondLock.AcquireAsync(
            companyId,
            cancellation.Token);

        try
        {
            secondAcquisition.IsCompleted
                .Should()
                .BeFalse();

            await firstLock.ReleaseAsync();

            await secondAcquisition;

            secondAcquisition.IsCompletedSuccessfully
                .Should()
                .BeTrue();
        }
        finally
        {
            cancellation.Cancel();

            await firstLock.ReleaseAsync();

            await secondLock.ReleaseAsync();
        }
    }

    [Fact]
    public async Task AcquireAsync_ShouldAllowDifferentCompanies()
    {
        var firstLock = new TestCompanyAdministrationLock();
        var secondLock = new TestCompanyAdministrationLock();

        try
        {
            await firstLock.AcquireAsync(Guid.NewGuid());

            using var cancellation =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(5));

            await secondLock.AcquireAsync(
                Guid.NewGuid(),
                cancellation.Token);
        }
        finally
        {
            await firstLock.ReleaseAsync();
            await secondLock.ReleaseAsync();
        }
    }
}