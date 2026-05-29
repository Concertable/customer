using Concertable.Customer.User.Api.Controllers;
using Concertable.Shared.Api.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.User.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerUserApi(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("UserClaimsScope", p =>
                p.RequireClaim("scope", "user:claims"));
        });
        services.AddControllers()
            .AddInternalControllers(typeof(UserController).Assembly);
        return services;
    }
}
