using Concertable.Customer.Ticket.Api.Controllers;
using Concertable.Customer.Ticket.Infrastructure.Extensions;
using Concertable.Shared.Api.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Ticket.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddTicketApi(IConfiguration configuration)
        {
            services.AddTicketModule(configuration);
            services.AddControllers()
                .AddInternalControllers(typeof(TicketController).Assembly);
            return services;
        }

        public IServiceCollection AddTicketDevSeeding()
            => services.AddTicketDevSeeder();
    }
}
