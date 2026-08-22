using Concertable.Customer.Concert.Api.Controllers;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Extensions;
using Concertable.Shared.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Concert.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddConcertApi(IConfiguration configuration)
        {
            services.AddConcertModule(configuration);
            services.AddControllers()
                .AddInternalControllers(typeof(ConcertController).Assembly);
            return services;
        }
    }

    extension(IServiceProvider services)
    {
        public Task MigrateConcertModuleAsync(CancellationToken cancellationToken = default)
            => services.GetRequiredService<ConcertDbContext>().Database.MigrateAsync(cancellationToken);
    }
}
