using Concertable.B2B.Artist.Contracts.Events;
using Concertable.B2B.Concert.Contracts.Events;
using Concertable.B2B.Venue.Contracts.Events;
using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Data.Seeders;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Customer.Concert.Infrastructure.Repositories;
using Concertable.Customer.Concert.Infrastructure.Services;
using Concertable.Customer.Ticket.Contracts.Events;
using Concertable.DataAccess.Application;
using Concertable.DataAccess.Infrastructure.Data;
using Concertable.Messaging.Contracts;
using Concertable.Seed.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Concert.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConcertModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ConcertDbContext>((sp, opts) =>
            opts.UseSqlServer(configuration.GetConnectionString("CustomerDb"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<IDomainEventDispatchInterceptor>()));

        services.AddScoped<IUnitOfWork<ConcertDbContext>, UnitOfWork<ConcertDbContext>>();
        services.AddScoped<IUnitOfWorkBehavior, UnitOfWorkBehavior>();

        services.AddScoped<IConcertReadRepository, ConcertReadRepository>();
        services.AddScoped<IConcertService, ConcertService>();
        services.AddScoped<IConcertModule, ConcertModule>();
        services.AddScoped<IIntegrationEventHandler<ConcertChangedEvent>, ConcertProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<ConcertRatingUpdatedEvent>, ConcertRatingProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<TicketPurchasedEvent>, TicketPurchasedHandler>();
        services.AddScoped<IIntegrationEventHandler<VenueChangedEvent>, VenueReadModelProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<ArtistChangedEvent>, ArtistReadModelProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<ArtistRatingUpdatedEvent>, ArtistRatingReadModelProjectionHandler>();

        services.AddHealthChecks().AddCheck<ConcertProjectionHealthCheck>("concert-projection");

        services.AddSingleton<ConcertConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<ConcertConfigurationProvider>());

        return services;
    }

    public static IServiceCollection AddConcertProjectionTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<ITestSeeder, ConcertProjectionTestSeeder>();
        return services;
    }
}
