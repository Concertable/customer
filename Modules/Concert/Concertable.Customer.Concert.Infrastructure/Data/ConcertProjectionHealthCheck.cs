using Concertable.B2B.Seeding.Fixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Concertable.Customer.Concert.Infrastructure.Data;

internal sealed class ConcertProjectionHealthCheck : IHealthCheck
{
    private readonly ConcertDbContext context;
    private readonly B2BSeedFixture fixture;

    public ConcertProjectionHealthCheck(ConcertDbContext context, B2BSeedFixture fixture)
    {
        this.context = context;
        this.fixture = fixture;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var seedConcertId = fixture.Concerts.First(c => c.Name == "Upcoming FlatFee Show").ConcertId;

        var exists = await this.context.Concerts
            .AnyAsync(c => c.Id == seedConcertId, cancellationToken);

        return exists
            ? HealthCheckResult.Healthy($"seed concert {seedConcertId} projected")
            : HealthCheckResult.Unhealthy($"seed concert {seedConcertId} not yet projected");
    }
}
