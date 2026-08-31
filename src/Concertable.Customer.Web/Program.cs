using Concertable.Customer.Web;
using Concertable.DataAccess.Application;
using Concertable.ServiceDefaults;
using Concertable.Shared.Notification.Infrastructure.Hubs;
var builder = WebApplication.CreateBuilder(args);
builder.AddCustomerWebHost();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultRateLimiting();

app.MapDefaultEndpoints();
app.MapControllers();
app.MapHub<NotificationHub>("/hub/notifications");

if (!app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;
    await sp.MigrateCustomerDatabaseAsync().ConfigureAwait(false);
    if (app.Environment.IsDevelopment())
        await sp.GetRequiredService<IDbInitializer>().InitializeAsync();
}

app.Run();

public sealed partial class Program
{ }
