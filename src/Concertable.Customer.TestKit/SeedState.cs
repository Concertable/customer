namespace Concertable.Customer.TestKit;

public sealed record SeedState
{
    public const string TestPassword = "Password11!";

    public required TestUser Customer1 { get; init; }
    public required TestConcert UpcomingFlatFeeConcert { get; init; }
}

public sealed record TestUser(Guid Id, string Email);

public sealed record TestConcert(int Id, Guid PayeeOwnerId);
