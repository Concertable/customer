using Concertable.Seeding.Identity;

namespace Concertable.Customer.Seeding;

public sealed class SeedData
{
    public const string TestPassword = "Password11!";
    public const int UpcomingConcertId = 13;

    public SeedCustomer Customer { get; }
    public IReadOnlyList<Guid> CustomerIds { get; }

    public SeedData()
    {
        Customer = SeedCustomers.Customer1;
        CustomerIds = [.. SeedCustomers.All.Select(c => c.Id)];
    }
}
