using Concertable.Customer.IntegrationTests.Fixtures.Mocks;
using Concertable.Kernel.Notifications;
using Concertable.Customer.Artist.Infrastructure.Extensions;
using Concertable.Customer.Concert.Infrastructure.Extensions;
using Concertable.Customer.Preference.Infrastructure.Extensions;
using Concertable.Customer.Review.Infrastructure.Extensions;
using Concertable.Customer.Ticket.Infrastructure.Extensions;
using Concertable.Customer.User.Domain;
using Concertable.Customer.User.Infrastructure.Extensions;
using Concertable.Customer.Venue.Infrastructure.Extensions;
using Concertable.DataAccess.Application;
using Concertable.Messaging.Contracts;
using Concertable.Payment.Client;
using Concertable.Customer.Seed.Infrastructure;
using Concertable.Shared.Email.Application;
using Concertable.Shared.Geocoding.Application;
using Concertable.Testing.Integration;
using Concertable.Testing.Integration.Logging;
using Concertable.Testing.Integration.Mocks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Concertable.Customer.IntegrationTests.Fixtures;

public class ApiFixture : IAsyncLifetime
{
    private SqlFixture sqlFixture = null!;
    private WebApplicationFactory<Program> factory = null!;
    private IServiceScope? scope;
    private readonly XunitOutputAccessor outputAccessor = new();

    public void AttachOutput(ITestOutputHelper output) => outputAccessor.Output = output;
    public void DetachOutput() => outputAccessor.Output = null;

    public IMockNotificationClient NotificationClient { get; } = new MockNotificationClient();
    public SeedState SeedState { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        sqlFixture = new SqlFixture();
        await sqlFixture.InitializeAsync();

        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:CustomerDb"] = sqlFixture.ConnectionString,
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddLogging(b =>
                {
                    b.ClearProviders();
                    b.AddProvider(new XunitLoggerProvider(outputAccessor));
                    b.SetMinimumLevel(LogLevel.Information);
                });

                var asbDescriptors = services
                    .Where(d => d.ServiceType == typeof(IHostedService) &&
                                d.ImplementationType?.Name == "AzureServiceBusReceiver")
                    .ToList();
                foreach (var d in asbDescriptors)
                    services.Remove(d);

                services.Replace(ServiceDescriptor.Singleton<IBusTransport, MockBusTransport>());
                services.Replace(ServiceDescriptor.Scoped<IGeocodingService, MockGeocodingService>());
                services.AddScoped<ICustomerPaymentClient, MockCustomerPaymentClient>();
                services.AddSingleton<IEmailSender, MockEmailSender>();
                services.Replace(ServiceDescriptor.Singleton<INotificationClient>(NotificationClient));

                services.AddScoped<IDbInitializer, TestDbInitializer>();
                services.AddScoped<SeedState>();
                services.AddCustomerUserTestSeeder();
                services.AddCustomerVenueProjectionTestSeeder();
                services.AddCustomerArtistProjectionTestSeeder();
                services.AddCustomerConcertProjectionTestSeeder();
                services.AddCustomerTicketTestSeeder();
                services.AddCustomerReviewTestSeeder();
                services.AddCustomerPreferenceTestSeeder();

                services.PostConfigure<AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    opts.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    opts.DefaultScheme = TestAuthHandler.SchemeName;
                });
                services.AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
            });
        });

        _ = factory.Services;
        await sqlFixture.InitializeRespawnerAsync();
    }

    public async Task ResetAsync()
    {
        await sqlFixture.ResetAsync();
        NotificationClient.Reset();

        scope?.Dispose();
        scope = factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<IDbInitializer>().InitializeAsync();
        SeedState = sp.GetRequiredService<SeedState>();
    }

    public async Task DisposeAsync()
    {
        scope?.Dispose();
        await factory.DisposeAsync();
        await sqlFixture.DisposeAsync();
    }

    public HttpClient CreateClient() => factory.CreateClient();

    public HttpClient CreateClient(UserEntity user) => CreateClient(user.Id);

    public HttpClient CreateClient(Guid userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        return client;
    }

    public IServiceProvider Services => factory.Services;
}
