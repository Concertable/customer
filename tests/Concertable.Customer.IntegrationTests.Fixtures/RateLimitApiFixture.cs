namespace Concertable.Customer.IntegrationTests.Fixtures;

public sealed class RateLimitApiFixture : ApiFixture
{
    public const int PermitLimit = 3;

    protected override int? RateLimitPermit => PermitLimit;
}
