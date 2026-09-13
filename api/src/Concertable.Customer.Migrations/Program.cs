using Concertable.Customer.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.AddCustomerMigrationHost();

var app = builder.Build();
await using (app.ConfigureAwait(false))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.MigrateCustomerDatabaseAsync().ConfigureAwait(false);
}
