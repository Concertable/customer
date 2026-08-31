using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Concertable.Auth.Hosting;
using Concertable.Customer.Web;
using Concertable.Payment.Hosting;
using Concertable.Testing;
using Concertable.Testing.Architecture;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        var jwtOptions = app.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);
        Assert.False(jwtOptions.RequireHttpsMetadata);
        var invalidBuilder = WebApplication.CreateBuilder(CompositionTestArguments.Create());
        invalidBuilder.AddCustomerWebHost();
        invalidBuilder.Services.AddInvalidLifetimeGraph();
        Assert.ThrowsAny<Exception>(() => invalidBuilder.Build());
    }

    [Fact]
    public void Web_ProductionEnvironment_RequiresHttpsMetadata()
    {
        var arguments = CompositionTestArguments.Create();
        arguments[0] = "--environment=Production";
        var builder = WebApplication.CreateBuilder(arguments);
        builder.AddCustomerWebHost();
        using var app = builder.Build();
        var jwtOptions = app.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.True(jwtOptions.RequireHttpsMetadata);
    }

    [Fact]
    public void AppHost_ProductionGraphAndStrictValidation_AreValid()
    {
        var validBuilder = AppHost.CreateBuilder([]);
        AssertImageEndpoint(validBuilder, AuthConstants.Resource, "https");
        AssertImageEndpoint(validBuilder, PaymentConstants.WebResource, "https");
        AssertImageEndpoint(validBuilder, PaymentConstants.WebResource, "http");
        using var app = validBuilder.Build();
        var builder = AppHost.CreateBuilder([]);
        builder.Services.AddInvalidLifetimeGraph();
        Assert.ThrowsAny<Exception>(() => builder.Build());
    }

    [Fact]
    public void AppHost_PublishGraphWithStripeCli_IsValid()
    {
        var builder = AppHost.CreateBuilder(
            ["--publisher", "manifest", "--Stripe:SecretKey=sk_test_composition"]);

        Assert.True(builder.ExecutionContext.IsPublishMode);
        Assert.Single(builder.Resources, resource => resource.Name == PaymentConstants.StripeCliResource);
        using var app = builder.Build();
    }

    [Fact]
    public void Web_ReferencesNoModuleInfrastructureAssembly() =>
        Assert.Empty(typeof(CustomerWebHostExtensions).Assembly.ModuleInfrastructureReferences("Seed"));

    private static void AssertImageEndpoint(
        IDistributedApplicationBuilder builder,
        string resourceName,
        string endpointName)
    {
        var resource = Assert.IsType<ServiceContainerResource>(
            builder.Resources.Single(resource => resource.Name == resourceName));
        var endpoint = Assert.Single(
            resource.Annotations.OfType<EndpointAnnotation>(),
            endpoint => endpoint.Name == endpointName);

        Assert.Equal(endpointName, endpoint.Name);
        Assert.Equal("http", endpoint.UriScheme);
        Assert.Equal(8080, endpoint.TargetPort);
    }
}
