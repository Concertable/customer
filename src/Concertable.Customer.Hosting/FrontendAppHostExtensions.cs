using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.DevTunnels;
using Concertable.Frontend.Hosting;
using Microsoft.Extensions.Configuration;

namespace Concertable.Customer.Hosting.Frontend;

public static class FrontendAppHostExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        public IResourceBuilder<NodeAppResource> AddCustomerSpa(
            IResourceBuilder<IResourceWithServiceDiscovery> backend,
            IResourceBuilder<IResourceWithServiceDiscovery> customerWeb,
            IResourceBuilder<IResourceWithServiceDiscovery> auth) =>
            builder.AddSpaSurface(CustomerLocalSpaSurfaces.Customer, ["app", "web", "customer"], backend, auth)
                   .WithReference(customerWeb)
                   .WaitFor(customerWeb);

        public IResourceBuilder<NodeAppResource> AddCustomerMobileSurface(
            IResourceBuilder<IResourceWithServiceDiscovery> api,
            IResourceBuilder<IResourceWithServiceDiscovery> auth,
            IResourceBuilder<IResourceWithServiceDiscovery> customerWeb,
            IResourceBuilder<IResourceWithServiceDiscovery> paymentWeb,
            IResourceBuilder<DevTunnelResource> tunnel) =>
            builder.AddMobileSurface("mobile-customer", ["app", "mobile", "customer"], api, tunnel,
                new(api, "EXPO_PUBLIC_API_URL"),
                new(auth, "EXPO_PUBLIC_AUTH_AUTHORITY"),
                new(customerWeb, "EXPO_PUBLIC_CUSTOMER_API_URL"),
                new(paymentWeb, "EXPO_PUBLIC_PAYMENT_API_URL"));
        public IResourceBuilder<DevTunnelResource>? AddMobileCustomer(
            IResourceBuilder<IResourceWithServiceDiscovery> customerWeb,
            IResourceBuilder<IResourceWithServiceDiscovery> auth,
            IResourceBuilder<IResourceWithServiceDiscovery> paymentWeb)
        {
            if (!builder.Configuration.GetValue<bool>("RunMobile"))
                return null;

            var tunnel = builder.AddMobileTunnel("customer-dev", auth, customerWeb, paymentWeb);
            builder.AddCustomerMobileSurface(customerWeb, auth, customerWeb, paymentWeb, tunnel);
            return tunnel;
        }
    }
}
