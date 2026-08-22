using Concertable.Composition.Testing;
using Concertable.Customer.Web;
using Concertable.Testing;
using Microsoft.AspNetCore.Builder;
using Xunit;

namespace Concertable.Customer.ArchitectureTests;

public sealed class CustomerArchitectureTests
{
    [Fact]
    public void Web_ProductionGraphAndStrictValidation_AreValid()
    {
        var builder = WebApplication.CreateBuilder(CompositionTestArguments.Create());
        builder.AddCustomerWebHost();
        using var app = builder.Build();
        builder.Services.ValidateComposition(app.Services, new CompositionValidationOptions
        {
            RootAssemblies = [typeof(CustomerWebHostExtensions).Assembly]
        });
        var invalidBuilder = WebApplication.CreateBuilder(CompositionTestArguments.Create());
        invalidBuilder.AddCustomerWebHost();
        invalidBuilder.Services.AddInvalidLifetimeGraph();
        Assert.ThrowsAny<Exception>(() => invalidBuilder.Build());
    }

    [Fact]
    public void AppHost_ProductionGraphAndStrictValidation_AreValid()
    {
        using var app = CustomerAppHost.CreateBuilder([]).Build();
        var builder = CustomerAppHost.CreateBuilder([]);
        builder.Services.AddInvalidLifetimeGraph();
        Assert.ThrowsAny<Exception>(() => builder.Build());
    }

    [Fact]
    public void Web_ReferencesNoModuleInfrastructureAssembly() =>
        Assert.Empty(typeof(CustomerWebHostExtensions).Assembly.ModuleInfrastructureReferences("Seed"));
}
