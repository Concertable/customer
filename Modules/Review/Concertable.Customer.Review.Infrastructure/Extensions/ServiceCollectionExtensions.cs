using Concertable.Customer.Review.Application.Validators;
using Concertable.Customer.Review.Contracts;
using Concertable.Customer.Review.Domain.Events;
using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Events;
using Concertable.Customer.Review.Infrastructure.Repositories;
using Concertable.Customer.Review.Infrastructure.Services;
using Concertable.Customer.Review.Infrastructure.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Review.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerReviewModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReviewDbContext>((sp, opts) =>
            opts.UseSqlServer(configuration.GetConnectionString("CustomerDb"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IUnitOfWork<ReviewDbContext>, UnitOfWork<ReviewDbContext>>();
        services.AddScoped<IUnitOfWorkBehavior, UnitOfWorkBehavior>();

        services.AddScoped<IConcertReviewService, ConcertReviewService>();
        services.AddScoped<IArtistReviewService, ArtistReviewService>();
        services.AddScoped<IVenueReviewService, VenueReviewService>();
        services.AddScoped<IReviewValidator, ReviewValidator>();
        services.AddScoped<IConcertReviewRepository, ConcertReviewRepository>();
        services.AddScoped<IArtistReviewRepository, ArtistReviewRepository>();
        services.AddScoped<IVenueReviewRepository, VenueReviewRepository>();
        services.AddScoped<ICustomerReviewModule, CustomerReviewModule>();

        services.AddScoped<IDomainEventHandler<ReviewCreatedDomainEvent>, ReviewCreatedDomainEventHandler>();

        services.AddSingleton<ReviewConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<ReviewConfigurationProvider>());

        services.AddValidatorsFromAssemblyContaining<CreateReviewRequestValidator>();

        return services;
    }
}
