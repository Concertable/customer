using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Seed.Contracts;
using Concertable.Kernel;
using Concertable.Messaging.Application.Extensions;
using Concertable.Messaging.AzureServiceBus.Extensions;
using Concertable.Messaging.Contracts;
using Concertable.ServiceDefaults;

namespace Concertable.Customer.Seed.Simulator;

public static class HostExtensions
{
    extension(HostApplicationBuilder builder)
    {
        public HostApplicationBuilder AddCustomerSeedSimulator()
        {
            builder.AddServiceDefaults();
            builder.Configuration.AddEnvironmentVariables();
            builder.Services.AddSingleton<DevFixture>();

            if (builder.Configuration.GetValue<bool>("SeedSimulator:Smoke"))
            {
                builder.Services.AddSingleton<IBusTransport, SmokeBusTransport>();
            }
            else
            {
                builder.Services.AddAzureServiceBusTransport(
                    options =>
                    {
                        options.ConnectionString = builder.Configuration.GetConnectionString("asb")
                            ?? (builder.Environment.IsIntegration() ? null!
                                : throw new InvalidOperationException("Connection string 'asb' is required."));
                        options.ServiceName = "concertable-customer-seed-simulator";
                    },
                    registry => registry.Publishes<CustomerReviewSubmittedEvent>());
            }

            builder.Services.AddHostedService<SeedEventPublisher>();
            return builder;
        }
    }
}
