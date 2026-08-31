using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Concertable.Auth.Hosting;
using Concertable.Customer.Web;
using Concertable.Payment.Hosting;
using Concertable.Testing;
using Concertable.Testing.Architecture;
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
        var validBuilder = CustomerAppHost.CreateBuilder([]);
        AssertImageEndpoint(validBuilder, AuthConstants.Resource);
        AssertImageEndpoint(validBuilder, PaymentConstants.WebResource);
        using var app = validBuilder.Build();
        var builder = CustomerAppHost.CreateBuilder([]);
        builder.Services.AddInvalidLifetimeGraph();
        Assert.ThrowsAny<Exception>(() => builder.Build());
    }

    [Fact]
    public void Web_ReferencesNoModuleInfrastructureAssembly() =>
        Assert.Empty(typeof(CustomerWebHostExtensions).Assembly.ModuleInfrastructureReferences("Seed"));

    private static void AssertImageEndpoint(
        IDistributedApplicationBuilder builder,
        string resourceName)
    {
        var resource = Assert.IsType<ServiceContainerResource>(
            builder.Resources.Single(resource => resource.Name == resourceName));
        var endpoint = Assert.Single(resource.Annotations.OfType<EndpointAnnotation>());

        Assert.Equal("https", endpoint.Name);
        Assert.Equal("http", endpoint.UriScheme);
        Assert.Equal(8080, endpoint.TargetPort);
    }
}
