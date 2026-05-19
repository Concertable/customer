using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Review.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerReviewModule(this IServiceCollection services)
    {
        return services;
    }
}
