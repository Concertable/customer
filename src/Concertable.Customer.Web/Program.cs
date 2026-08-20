using Concertable.Customer.Artist.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Preference.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Customer.User.Infrastructure.Data;
using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.Customer.Web;
using Concertable.DataAccess.Application;
using Concertable.Messaging.Infrastructure.Inbox;
using Concertable.Messaging.Infrastructure.Outbox;
using Concertable.ServiceDefaults;
using Concertable.Shared.Notification.Infrastructure.Hubs;
using Microsoft.EntityFrameworkCore;
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
    await sp.GetRequiredService<OutboxDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<InboxDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<ArtistDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<ConcertDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<PreferenceDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<ReviewDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<TicketDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<UserDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<VenueDbContext>().Database.MigrateAsync();
    if (app.Environment.IsDevelopment())
        await sp.GetRequiredService<IDbInitializer>().InitializeAsync();
}

app.Run();

public sealed partial class Program
{ }
