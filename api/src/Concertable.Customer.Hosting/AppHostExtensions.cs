using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Concertable.Messaging.AzureServiceBus.Options;
using Microsoft.Extensions.Configuration;

namespace Concertable.Customer.Hosting;

public static class AppHostExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        public IResourceBuilder<ServiceContainerResource> AddCustomerMigrations(
            string image,
            string digest,
            IResourceBuilder<PostgresDatabaseResource> customerDb) =>
            builder.AddContainerImage(CustomerConstants.MigrationsResource, image, digest)
                   .WithReference(customerDb)
                   .WaitFor(customerDb);

        public IResourceBuilder<ProjectResource> AddCustomerMigrations<TProject>(
            IResourceBuilder<PostgresDatabaseResource> customerDb)
            where TProject : IProjectMetadata, new() =>
            builder.AddProject<TProject>(CustomerConstants.MigrationsResource)
                   .WithReference(customerDb)
                   .WaitFor(customerDb);

        public IResourceBuilder<ServiceContainerResource> AddCustomerWeb(
            string image,
            string digest,
            IResourceBuilder<IResourceWithServiceDiscovery> auth,
            IResourceBuilder<PostgresDatabaseResource> customerDb,
            IResourceBuilder<IResource> migrations,
            IResourceBuilder<AzureServiceBusResource> asb,
            IResourceBuilder<IResourceWithServiceDiscovery> paymentWeb)
        {
            var customerSecret = builder.Configuration["ServiceAuth:CustomerClientSecret"];
            return builder.AddContainerImage(CustomerConstants.WebResource, image, digest)
                          .WithHttpEndpoint(targetPort: CustomerConstants.ContainerPort, name: "https")
                          .WithReference(auth)
                          .WaitFor(auth)
                          .WithReference(customerDb)
                          .WaitFor(customerDb)
                          .WaitForCompletion(migrations)
                          .WithReference(asb)
                          .WaitFor(asb)
                          .WithReference(paymentWeb)
                          .WaitFor(paymentWeb)
                          .WithEnvironment("Auth__Authority", auth.GetEndpoint("https"))
                          .WithEnvironment(AzureServiceBusOptions.ServiceNameEnvVar, CustomerConstants.ServiceName)
                          .WithEnvironment("ServiceAuth__ClientId", "concertable-customer")
                          .WithOptionalEnvironment("ServiceAuth__ClientSecret", customerSecret);
        }

        public IResourceBuilder<ProjectResource> AddCustomerWeb<TProject>(
            IResourceBuilder<IResourceWithServiceDiscovery> auth,
            IResourceBuilder<PostgresDatabaseResource> customerDb,
            IResourceBuilder<IResource> migrations,
            IResourceBuilder<AzureServiceBusResource> asb,
            IResourceBuilder<IResourceWithServiceDiscovery> paymentWeb)
            where TProject : IProjectMetadata, new()
        {
            var customerSecret = builder.Configuration["ServiceAuth:CustomerClientSecret"];
            return builder.AddProject<TProject>(CustomerConstants.WebResource)
                          .WithReference(auth)
                          .WaitFor(auth)
                          .WithReference(customerDb)
                          .WaitFor(customerDb)
                          .WaitForCompletion(migrations)
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
}
