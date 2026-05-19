using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Profile.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerProfileModule(this IServiceCollection services)
    {
        return services;
    }
}
