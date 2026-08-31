using Concertable.Seed.Identity;

namespace Concertable.Customer.Seed.Contracts;

public sealed class DevFixture
{
    public DevFixture()
    {
        this.ConfirmedConcertReview = new ReviewSeedSpec(
            new Guid("e0000000-0000-0000-0000-000000000001"),
            new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new Guid("d0000000-0000-0000-0000-000000000002"),
            1,
            1,
            12,
            5,
            SeedCustomers.CustomerEmail(1),
            "Great show");
        this.Reviews = [this.ConfirmedConcertReview];
    }

    public ReviewSeedSpec ConfirmedConcertReview { get; }
    public IReadOnlyList<ReviewSeedSpec> Reviews { get; }
}
