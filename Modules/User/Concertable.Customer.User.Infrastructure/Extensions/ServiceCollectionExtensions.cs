using Concertable.Auth.Contracts.Events;
using Concertable.Customer.User.Application.Validators;
using Concertable.Customer.User.Infrastructure.Authorization;
using Concertable.Customer.User.Infrastructure.Data;
using Concertable.Customer.User.Infrastructure.Data.Seeders;
using Concertable.Customer.User.Infrastructure.Events;
using Concertable.Customer.User.Infrastructure.Repositories;
using Concertable.Customer.User.Infrastructure.Services;
using Concertable.DataAccess.Infrastructure.Data;
using Concertable.Messaging.Contracts;
using Concertable.Seed.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.User.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerUserModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UserDbContext>((sp, opts) =>
            opts.UseSqlServer(
                    configuration.GetConnectionString("CustomerDb"),
                    sqlOpts => sqlOpts.UseNetTopologySuite())
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<IDomainEventDispatchInterceptor>()));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserModule, UserModule>();

        services.AddScoped<IIntegrationEventHandler<CredentialRegisteredEvent>, UserCreationHandler>();

        services.AddSingleton<UserConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<UserConfigurationProvider>());

        services.AddValidatorsFromAssemblyContaining<UpdateLocationRequestValidator>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Customer", p => p.AddRequirements(new CustomerUserRequirement()));
        });
        services.AddScoped<IAuthorizationHandler, CustomerUserHandler>();

        return services;
    }

    public static IServiceCollection AddCustomerUserTestSeeder(this IServiceCollection services)
    {
        services.AddScoped<ITestSeeder, UserTestSeeder>();
        return services;
    }
}
