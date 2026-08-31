using Concertable.Customer.E2ETests.Server;
using Concertable.Customer.Web;

namespace Concertable.Customer.E2ETests.Web;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory,
        });
        builder.AddCustomerWebHost();
        builder.Services.AddCustomerE2EAdmin(builder.Configuration, builder.Environment);

        var app = builder.Build();
        app.MapCustomerE2EAdmin();
        await app.UseCustomerWebHost();
        app.Run();
    }
}
