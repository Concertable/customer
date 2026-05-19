using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Concert.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerConcertModule(this IServiceCollection services)
    {
        return services;
    }
}
