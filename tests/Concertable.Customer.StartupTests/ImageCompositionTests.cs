using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Concertable.Auth.Hosting;
using Concertable.Customer.Hosting;
using Concertable.Payment.Hosting;
using Xunit;

namespace Concertable.Customer.StartupTests;

/// <summary>Covers the image overload of Customer's own web service, which no AppHost in this
/// repository composes — every standalone AppHost runs its own service from source and only foreign
/// services by image. That gap is why the overload shipped declaring no endpoint at all while the
/// project overload gets Aspire's defaults for free: a consumer composing Customer by image got a
/// service nothing could reach, and GetEndpoint("https") threw.</summary>
public sealed class ImageCompositionTests
{
    private const string Digest = "sha256:0000000000000000000000000000000000000000000000000000000000000000";

    [Fact]
    public void AddCustomerWeb_ByImage_DeclaresTheEndpointConsumersResolve()
    {
        var builder = DistributedApplication.CreateBuilder();
        var sql = builder.AddSqlServer("sql");
        var asb = builder.AddServiceBus();
        var auth = builder.AddContainerImage(AuthConstants.Resource, "ghcr.io/concertable/auth", Digest)
                          .WithHttpsEndpoint(targetPort: AuthConstants.ContainerPort, name: "https");
        var paymentWeb = builder.AddPaymentWeb(
            "ghcr.io/concertable/payment-web",
            Digest,
            auth,
            sql.AddDatabase(PaymentConstants.Database),
            asb);

        var web = builder.AddCustomerWeb(
            "ghcr.io/concertable/customer-web",
            Digest,
            auth,
            sql.AddDatabase(CustomerConstants.Database),
            asb,
            paymentWeb);

        var endpoint = Assert.Single(
            web.Resource.Annotations.OfType<EndpointAnnotation>(),
            endpoint => endpoint.Name == "https");

        Assert.Equal("http", endpoint.UriScheme);
        Assert.Equal(CustomerConstants.ContainerPort, endpoint.TargetPort);
    }
}
