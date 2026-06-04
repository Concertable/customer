using Concertable.Customer.Venue.Contracts;
using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.Customer.Venue.Infrastructure.Data.Seeders;
using Concertable.Customer.Venue.Infrastructure.Handlers;
using Concertable.Customer.Venue.Infrastructure.Repositories;
using Concertable.Customer.Venue.Infrastructure.Services;
using Concertable.B2B.Venue.Contracts.Events;
using Concertable.Seed.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Venue.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVenueModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<VenueDbContext>((sp, opts) =>
            opts.UseSqlServer(configuration.GetConnectionString("CustomerDb"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<IDomainEventDispatchInterceptor>()));

        services.AddScoped<IUnitOfWork<VenueDbContext>, UnitOfWork<VenueDbContext>>();
        services.AddScoped<IUnitOfWorkBehavior, UnitOfWorkBehavior>();

        services.AddScoped<IVenueReadRepository, VenueReadRepository>();
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<IVenueModule, VenueModule>();
        services.AddScoped<IIntegrationEventHandler<VenueChangedEvent>, VenueProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<VenueRatingUpdatedEvent>, VenueRatingProjectionHandler>();

        services.AddSingleton<VenueConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<VenueConfigurationProvider>());

        return services;
    }

    public static IServiceCollection AddVenueProjectionTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<ITestSeeder, VenueProjectionTestSeeder>();
        return services;
    }
}
