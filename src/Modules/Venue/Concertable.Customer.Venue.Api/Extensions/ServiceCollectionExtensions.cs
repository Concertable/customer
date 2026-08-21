using Concertable.Customer.Venue.Api.Controllers;
using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.Customer.Venue.Infrastructure.Extensions;
using Concertable.Shared.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Venue.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddVenueApi(IConfiguration configuration)
        {
            services.AddVenueModule(configuration);
            services.AddControllers()
                .AddInternalControllers(typeof(VenueController).Assembly);
            return services;
        }
    }

    extension(IServiceProvider services)
    {
        public Task MigrateVenueModuleAsync(CancellationToken cancellationToken = default)
            => services.GetRequiredService<VenueDbContext>().Database.MigrateAsync(cancellationToken);
    }
}
