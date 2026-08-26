using Concertable.Customer.User.Api.Controllers;
using Concertable.Customer.User.Infrastructure.Data;
using Concertable.Customer.User.Infrastructure.Extensions;
using Concertable.Shared.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.User.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddUserApi(IConfiguration configuration)
        {
            services.AddUserModule(configuration);
            services.AddAuthorization(options =>
            {
                options.AddPolicy("UserClaimsScope", p =>
                    p.RequireClaim("scope", "user:claims"));
            });
            services.AddControllers()
                .AddInternalControllers(typeof(UserController).Assembly);
            return services;
        }
    }

    extension(IServiceProvider services)
    {
        public Task MigrateUserModuleAsync(CancellationToken cancellationToken = default)
            => services.GetRequiredService<UserDbContext>().Database.MigrateAsync(cancellationToken);
    }
}
