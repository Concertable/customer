using Concertable.Customer.Artist.Api.Extensions;
using Concertable.Customer.Concert.Api.Extensions;
using Concertable.Customer.Preference.Api.Extensions;
using Concertable.Customer.Review.Api.Extensions;
using Concertable.Customer.Ticket.Api.Extensions;
using Concertable.Customer.User.Api.Extensions;
using Concertable.Customer.Venue.Api.Extensions;
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
    await sp.MigrateArtistModuleAsync();
    await sp.MigrateConcertModuleAsync();
    await sp.MigratePreferenceModuleAsync();
    await sp.MigrateReviewModuleAsync();
    await sp.MigrateTicketModuleAsync();
    await sp.MigrateUserModuleAsync();
    await sp.MigrateVenueModuleAsync();
    if (app.Environment.IsDevelopment())
        await sp.GetRequiredService<IDbInitializer>().InitializeAsync();
}

app.Run();

public sealed partial class Program
{ }
