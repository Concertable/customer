using Concertable.Customer.Profile.Infrastructure.Authorization;
using Concertable.Customer.Profile.Infrastructure.Data;
using Concertable.Customer.Profile.Infrastructure.Events;
using Concertable.Auth.Contracts.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Profile.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerProfileModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProfileDbContext>((sp, opts) =>
            opts.UseSqlServer(configuration.GetConnectionString("CustomerDb"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IIntegrationEventHandler<CredentialRegisteredEvent>, CustomerProfileCreationHandler>();

        services.AddSingleton<ProfileConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<ProfileConfigurationProvider>());

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Customer", p => p.AddRequirements(new CustomerProfileRequirement()));
        });
        services.AddScoped<IAuthorizationHandler, CustomerProfileHandler>();

        return services;
    }
}
