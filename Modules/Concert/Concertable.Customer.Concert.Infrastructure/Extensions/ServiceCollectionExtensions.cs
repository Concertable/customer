using Concertable.B2B.Concert.Contracts.Events;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Handlers;
using Concertable.Customer.Concert.Infrastructure.Repositories;
using Concertable.Customer.Concert.Infrastructure.Services;
using Concertable.DataAccess.Application;
using Concertable.DataAccess.Infrastructure.Data;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Concert.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerConcertModule(this IServiceCollection services, IConfiguration configuration)
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
        services.AddScoped<IIntegrationEventHandler<ConcertChangedEvent>, ConcertProjectionHandler>();
        services.AddScoped<IIntegrationEventHandler<ConcertRatingUpdatedEvent>, ConcertRatingProjectionHandler>();

        services.AddSingleton<ConcertConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<ConcertConfigurationProvider>());

        return services;
    }
}
