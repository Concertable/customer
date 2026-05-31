using Concertable.B2B.Artist.Contracts.Events;
using Concertable.Customer.Artist.Contracts;
using Concertable.Customer.Artist.Infrastructure.Data;
using Concertable.Customer.Artist.Infrastructure.Data.Seeders;
using Concertable.Customer.Artist.Infrastructure.Handlers;
using Concertable.Customer.Artist.Infrastructure.Repositories;
using Concertable.Customer.Artist.Infrastructure.Services;
using Concertable.Seed.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Artist.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddArtistModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ArtistDbContext>((sp, opts) =>
            opts.UseSqlServer(configuration.GetConnectionString("CustomerDb"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<IDomainEventDispatchInterceptor>()));

        services.AddScoped<IUnitOfWork<ArtistDbContext>, UnitOfWork<ArtistDbContext>>();
        services.AddScoped<IUnitOfWorkBehavior, UnitOfWorkBehavior>();

        services.AddScoped<IArtistReadRepository, ArtistReadRepository>();
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<ICustomerArtistModule, CustomerArtistModule>();
        services.AddScoped<IIntegrationEventHandler<ArtistChangedEvent>, ArtistProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<ArtistRatingUpdatedEvent>, ArtistRatingProjectionHandler>();

        services.AddSingleton<ArtistConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<ArtistConfigurationProvider>());

        return services;
    }

    public static IServiceCollection AddArtistProjectionTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<ITestSeeder, ArtistProjectionTestSeeder>();
        return services;
    }
}
