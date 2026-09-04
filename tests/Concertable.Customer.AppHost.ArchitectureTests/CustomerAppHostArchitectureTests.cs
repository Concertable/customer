using Concertable.Testing;
using Concertable.Testing.Architecture;
using Xunit;

namespace Concertable.Customer.AppHost.ArchitectureTests;

public sealed class CustomerAppHostArchitectureTests
{
    [Fact]
    public void ProductionGraphAndStrictValidation_AreValid()
    {
        using var app = CustomerAppHost.CreateBuilder([]).Build();
        var builder = CustomerAppHost.CreateBuilder([]);
        builder.Services.AddInvalidLifetimeGraph();
        Assert.ThrowsAny<Exception>(() => builder.Build());
    }
}
