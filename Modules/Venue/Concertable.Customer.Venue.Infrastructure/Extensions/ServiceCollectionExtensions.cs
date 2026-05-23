using Concertable.Customer.Venue.Contracts;
using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.Customer.Venue.Infrastructure.Handlers;
using Concertable.Customer.Venue.Infrastructure.Repositories;
using Concertable.Customer.Venue.Infrastructure.Services;
using Concertable.Venue.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Venue.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerVenueModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<VenueDbContext>((sp, opts) =>
            opts.UseSqlServer(configuration.GetConnectionString("CustomerDb"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IUnitOfWork<VenueDbContext>, UnitOfWork<VenueDbContext>>();
        services.AddScoped<IUnitOfWorkBehavior, UnitOfWorkBehavior>();

        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<ICustomerVenueModule, CustomerVenueModule>();
        services.AddScoped<IIntegrationEventHandler<VenueChangedEvent>, VenueProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<VenueRatingUpdatedEvent>, VenueRatingProjectionHandler>();

        services.AddSingleton<VenueConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<VenueConfigurationProvider>());

        return services;
    }
}
