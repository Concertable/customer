using Concertable.Customer.Review.Api.Controllers;
using Concertable.Customer.Review.Infrastructure.Extensions;
using Concertable.Shared.Api.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Review.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddReviewApi(IConfiguration configuration)
        {
            services.AddReviewModule(configuration);
            services.AddControllers()
                .AddInternalControllers(typeof(ConcertReviewsController).Assembly);
            return services;
        }
    }
}
