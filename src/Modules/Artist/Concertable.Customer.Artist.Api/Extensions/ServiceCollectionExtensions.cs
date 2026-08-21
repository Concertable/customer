using Concertable.Customer.Artist.Api.Controllers;
using Concertable.Customer.Artist.Infrastructure.Data;
using Concertable.Customer.Artist.Infrastructure.Extensions;
using Concertable.Shared.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Artist.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddArtistApi(IConfiguration configuration)
        {
            services.AddArtistModule(configuration);
            services.AddControllers()
                .AddInternalControllers(typeof(ArtistController).Assembly);
            return services;
        }
    }

    extension(IServiceProvider services)
    {
        public Task MigrateArtistModuleAsync(CancellationToken cancellationToken = default)
            => services.GetRequiredService<ArtistDbContext>().Database.MigrateAsync(cancellationToken);
    }
}
