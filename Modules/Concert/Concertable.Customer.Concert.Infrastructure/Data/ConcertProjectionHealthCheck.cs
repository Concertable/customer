using Concertable.B2B.Seeding.Fixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertProjectionHealthCheck : IHealthCheck
{
    private readonly ConcertDbContext context;

    public ConcertProjectionHealthCheck(ConcertDbContext context)
    {
        this.context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var exists = await this.context.Concerts
            .AnyAsync(c => c.Id == B2BSeedFixture.UpcomingConcertId, cancellationToken);

        return exists
            ? HealthCheckResult.Healthy($"seed concert {B2BSeedFixture.UpcomingConcertId} projected")
            : HealthCheckResult.Unhealthy($"seed concert {B2BSeedFixture.UpcomingConcertId} not yet projected");
    }
}
