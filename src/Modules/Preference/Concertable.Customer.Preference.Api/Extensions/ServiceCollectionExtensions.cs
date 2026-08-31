using Concertable.Customer.Preference.Api.Controllers;
using Concertable.Customer.Preference.Infrastructure.Data;
using Concertable.Customer.Preference.Infrastructure.Extensions;
using Concertable.Shared.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Preference.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPreferenceApi(IConfiguration configuration)
        {
            services.AddPreferenceModule(configuration);
            services.AddControllers()
                .AddInternalControllers(typeof(PreferenceController).Assembly);
            return services;
        }

        public IServiceCollection AddPreferenceDevSeeding()
            => services.AddPreferenceDevSeeder();

        public IServiceCollection AddPreferenceMigrations(IConfiguration configuration)
        {
            services.AddSingleton<PreferenceConfigurationProvider>();
            services.AddDbContext<PreferenceDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("CustomerDb")));
            return services;
        }
    }

    extension(IServiceProvider services)
    {
        public Task MigratePreferenceModuleAsync(CancellationToken cancellationToken = default)
            => services.GetRequiredService<PreferenceDbContext>().Database.MigrateAsync(cancellationToken);
    }
}
