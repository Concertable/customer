using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Ticket.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerTicketModule(this IServiceCollection services)
    {
        return services;
    }
}
