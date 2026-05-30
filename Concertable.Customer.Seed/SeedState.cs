using Concertable.Seed.Identity;

namespace Concertable.Customer.Seed;

public sealed class SeedState
{
    public const string TestPassword = "Password11!";

    public SeedCustomer Customer { get; }
    public IReadOnlyList<Guid> CustomerIds { get; }

    public SeedState()
    {
        Customer = SeedCustomers.Customer1;
        CustomerIds = [.. SeedCustomers.All.Select(c => c.Id)];
    }
}
