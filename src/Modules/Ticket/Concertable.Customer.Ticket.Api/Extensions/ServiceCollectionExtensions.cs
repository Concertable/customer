using Concertable.Customer.Ticket.Api.Controllers;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Customer.Ticket.Infrastructure.Extensions;
using Concertable.Shared.Api.Extensions;
using Microsoft.EntityFrameworkCore;
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

        public IServiceCollection AddTicketMigrations(IConfiguration configuration)
        {
            services.AddSingleton<TicketConfigurationProvider>();
            services.AddDbContext<TicketDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("CustomerDb")));
            return services;
        }
    }

    extension(IServiceProvider services)
    {
        public Task MigrateTicketModuleAsync(CancellationToken cancellationToken = default)
            => services.GetRequiredService<TicketDbContext>().Database.MigrateAsync(cancellationToken);
    }
}
