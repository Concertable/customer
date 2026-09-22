using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Concertable.Auth.Hosting;
using Concertable.Customer.Hosting;
using Concertable.Payment.Hosting;
using Xunit;

namespace Concertable.Customer.StartupTests;

/// <summary>Covers the image overloads of Customer's own resources, which no AppHost in this
/// repository composes — every standalone AppHost runs its own service from source and only foreign
/// services by image. That gap is why the overload shipped declaring no endpoint at all while the
/// project overload gets Aspire's defaults for free: a consumer composing Customer by image got a
/// service nothing could reach, and GetEndpoint("https") threw.</summary>
public sealed class ImageTests
{
    private const string Digest = "sha256:0000000000000000000000000000000000000000000000000000000000000000";

    [Fact]
    public void AddCustomerWeb_ByImage_DeclaresTheEndpointConsumersResolve()
    {
        var builder = DistributedApplication.CreateBuilder();
        var (web, _) = ComposeByImage(builder);

        var endpoint = Assert.Single(
            web.Resource.Annotations.OfType<EndpointAnnotation>(),
            endpoint => endpoint.Name == "https");

        Assert.Equal("http", endpoint.UriScheme);
        Assert.Equal(CustomerConstants.ContainerPort, endpoint.TargetPort);
    }

    [Fact]
    public void AddCustomerWeb_ByImage_WaitsForItsOwnMigrationsToComplete()
    {
        var builder = DistributedApplication.CreateBuilder();
        var (web, migrations) = ComposeByImage(builder);

        var wait = Assert.Single(
            web.Resource.Annotations.OfType<WaitAnnotation>(),
            annotation => annotation.Resource.Name == migrations.Resource.Name);

        Assert.Equal(WaitType.WaitForCompletion, wait.WaitType);
        Assert.Equal(0, wait.ExitCode);
    }

    private static (IResourceBuilder<ServiceContainerResource> Web, IResourceBuilder<ServiceContainerResource> Migrations)
        ComposeByImage(IDistributedApplicationBuilder builder)
    {
        var postgres = builder.AddPostgresContainer("composition-test-data");
        var asb = builder.AddServiceBus();
        var auth = builder.AddContainerImage(AuthConstants.Resource, "ghcr.io/concertable/auth", Digest)
                          .WithHttpEndpoint(targetPort: AuthConstants.ContainerPort, name: "https");
        var paymentWeb = builder.AddPaymentWeb(
            "ghcr.io/concertable/payment-web",
            Digest,
            auth,
            postgres.AddDatabase(PaymentConstants.Database),
            asb);

        var customerDb = postgres.AddDatabase(CustomerConstants.Database);
        var migrations = builder.AddCustomerMigrations(
            "ghcr.io/concertable/customer-migrations",
            Digest,
            customerDb);
        var web = builder.AddCustomerWeb(
            "ghcr.io/concertable/customer-web",
            Digest,
            auth,
            customerDb,
            migrations,
            asb,
            paymentWeb);

        return (web, migrations);
    }
}
