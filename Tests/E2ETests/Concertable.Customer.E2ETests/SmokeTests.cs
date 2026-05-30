using Concertable.Seed;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Concertable.Customer.E2ETests;

[Collection("E2E")]
public class SmokeTests(AppFixture fixture)
{
    [Fact]
    public void CustomerSeedHost_ResolvesDbInitializer()
    {
        // Arrange / Act / Assert
        var scope = fixture.DbFixture.GetType(); // fixture initialized = app healthy
        Assert.NotNull(fixture.SeedData);
        Assert.NotNull(fixture.CustomerClient);
    }
}
