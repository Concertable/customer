using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Concertable.Messaging.AzureServiceBus.Options;
using Microsoft.Extensions.Configuration;

namespace Concertable.Customer.Hosting;

public static class AppHostExtensions
{
    public static IResourceBuilder<ProjectResource> AddCustomerWeb<TProject>(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> auth,
        IResourceBuilder<SqlServerDatabaseResource> customerDb,
        IResourceBuilder<AzureServiceBusResource> asb,
        IResourceBuilder<ProjectResource> paymentWeb)
        where TProject : IProjectMetadata, new()
    {
        var customerSecret = builder.Configuration["ServiceAuth:CustomerClientSecret"];
        return builder.AddProject<TProject>(CustomerConstants.WebResource)
                      .WithReference(auth)
                      .WaitFor(auth)
                      .WithReference(customerDb)
                      .WaitFor(customerDb)
                      .WithReference(asb)
                      .WaitFor(asb)
                      .WithReference(paymentWeb)
                      .WaitFor(paymentWeb)
                      .WithEnvironment("Auth__Authority", auth.GetEndpoint("https"))
                      .WithEnvironment(AzureServiceBusOptions.ServiceNameEnvVar, CustomerConstants.ServiceName)
                      .WithEnvironment("ServiceAuth__ClientId", "concertable-customer")
                      .WithOptionalEnvironment("ServiceAuth__ClientSecret", customerSecret);
    }
}
