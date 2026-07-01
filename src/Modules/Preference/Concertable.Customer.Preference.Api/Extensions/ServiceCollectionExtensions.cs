using Concertable.Customer.Preference.Api.Controllers;
using Concertable.Shared.Api.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Preference.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPreferenceApi(this IServiceCollection services)
    {
        services.AddControllers()
            .AddInternalControllers(typeof(PreferenceController).Assembly);
        return services;
    }
}
