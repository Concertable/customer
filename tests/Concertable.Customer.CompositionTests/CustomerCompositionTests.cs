using Concertable.Composition.Testing;
using Concertable.Customer.Web;
using Microsoft.AspNetCore.Builder;
using Xunit;

namespace Concertable.Customer.CompositionTests;

public sealed class CustomerCompositionTests
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
    public void Web_ReferencesNoModuleInfrastructureAssembly()
    {
        var webAssemblyName = typeof(CustomerWebHostExtensions).Assembly.GetName().Name!;
        var servicePrefix = webAssemblyName[..(webAssemblyName.LastIndexOf('.') + 1)];
        var seedInfrastructureAssembly = $"{servicePrefix}Seed.Infrastructure";

        var moduleInfrastructureReferences = typeof(CustomerWebHostExtensions).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null
                && name.StartsWith(servicePrefix, StringComparison.Ordinal)
                && name.EndsWith(".Infrastructure", StringComparison.Ordinal)
                && name != seedInfrastructureAssembly)
            .ToArray();

        Assert.Empty(moduleInfrastructureReferences);
    }
}
