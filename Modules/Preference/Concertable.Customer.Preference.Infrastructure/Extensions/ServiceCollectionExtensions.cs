using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Customer.Preference.Infrastructure.Data;
using Concertable.Customer.Preference.Infrastructure.Data.Seeders;
using Concertable.Customer.Preference.Infrastructure.Events;
using Concertable.Customer.Preference.Infrastructure.Notifications;
using Concertable.Customer.Preference.Infrastructure.Repositories;
using Concertable.Customer.Preference.Infrastructure.Services;
using Concertable.Seed.Shared;
using Concertable.Seed.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Preference.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerPreferenceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PreferenceDbContext>((sp, opts) =>
            opts.UseSqlServer(configuration.GetConnectionString("CustomerDb"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<IDomainEventDispatchInterceptor>())
                .UseSeedingSupport(sp));

        services.AddSingleton<PreferenceConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<PreferenceConfigurationProvider>());

        services.AddScoped<IPreferenceRepository, PreferenceRepository>();
        services.AddScoped<IPreferenceService, PreferenceService>();
        services.AddScoped<IConcertPostedNotifier, ConcertPostedNotifier>();

        services.AddScoped<IIntegrationEventHandler<ConcertPostedEvent>, ConcertPostedNotificationHandler>();

        return services;
    }

    public static IServiceCollection AddCustomerPreferenceDevSeeder(this IServiceCollection services)
    {
        services.AddScoped<IDevSeeder, PreferenceDevSeeder>();
        return services;
    }

    public static IServiceCollection AddCustomerPreferenceTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<ITestSeeder, PreferenceTestSeeder>();
        return services;
    }
}
