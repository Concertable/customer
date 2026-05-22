using Concertable.Customer.Preference.Api.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Preference.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerPreferenceApi(this IServiceCollection services)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(PreferenceController).Assembly)
            .ConfigureApplicationPartManager(apm =>
                apm.FeatureProviders.Add(new InternalControllerFeatureProvider()));
        return services;
    }
}
