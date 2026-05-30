using Concertable.B2B.Artist.Contracts.Events;
using Concertable.B2B.Concert.Contracts.Events;
using Concertable.B2B.Seed.Contracts;
using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Web;
using Concertable.Customer.Artist.Infrastructure.Data;
using Concertable.Customer.Artist.Infrastructure.Extensions;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Extensions;
using Concertable.Customer.Preference.Infrastructure.Data;
using Concertable.Customer.Review.Infrastructure.Data;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Customer.User.Infrastructure.Data;
using Concertable.Customer.Venue.Infrastructure.Data;
using Concertable.Customer.Venue.Infrastructure.Extensions;
using Concertable.Customer.Preference.Api.Extensions;
using Concertable.Customer.Preference.Infrastructure.Extensions;
using Concertable.Customer.User.Infrastructure.Extensions;
using Concertable.Customer.User.Api.Extensions;
using Concertable.Customer.Review.Infrastructure.Extensions;
using Concertable.Customer.Ticket.Infrastructure.Extensions;
using Concertable.B2B.Venue.Contracts.Events;
using Concertable.Messaging.Infrastructure.Extensions;
using Concertable.Messaging.Infrastructure.Inbox;
using Concertable.Messaging.Infrastructure.Outbox;
using Concertable.Payment.Client.Extensions;
using Concertable.Payment.Domain.Events;
using Concertable.Auth.Contracts.Events;
using Concertable.Shared.Email.Infrastructure.Extensions;
using Concertable.Shared.Geocoding.Infrastructure.Extensions;
using Concertable.Shared.Pdf.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Concertable.ServiceDefaults;
using Concertable.DataAccess.Infrastructure.Data;
using Concertable.Messaging.Application.Extensions;
using Concertable.Messaging.AzureServiceBus.Extensions;
using Concertable.Kernel.Extensions;
using Concertable.Shared.Notification.Infrastructure.Hubs;
using Concertable.Shared.Notification.Infrastructure.Extensions;
using Concertable.DataAccess.Application;
using Concertable.Seed.Shared;
using Concertable.Seed.Shared.Extensions;
using Concertable.Customer.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers()
    .AddApplicationPart(typeof(Concertable.Shared.Api.Controllers.GenreController).Assembly)
    .AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithOrigins(corsOrigins);
    });
});

var services = builder.Services;

services.AddScoped<IKeyedServiceProvider>(sp => (IKeyedServiceProvider)sp);
services.AddSingleton(TimeProvider.System);
services.AddSingleton<SeedCatalog>();
services.AddSharedInfrastructure(builder.Configuration);
services.AddGeometry();
services.AddClientCredentials(opts =>
{
    opts.Authority = builder.Configuration["Auth:Authority"] ?? builder.Configuration["services__auth__https__0"] ?? "";
    opts.ClientId = builder.Configuration["ServiceAuth:ClientId"] ?? "";
    opts.ClientSecret = builder.Configuration["ServiceAuth:ClientSecret"] ?? "";
});
services.AddSharedEmail(builder.Configuration);
services.AddSharedGeocoding();
services.AddSharedPdf();
services.AddAzureServiceBusTransport(
    opts =>
    {
        opts.ConnectionString = builder.Configuration.GetConnectionString("asb") ?? "";
        opts.ServiceName = "concertable-customer";
    },
    reg =>
    {
        reg.Publishes<CustomerReviewSubmittedEvent>();
        reg.SubscribeTo<CustomerReviewSubmittedEvent>();

        reg.SubscribeTo<ConcertChangedEvent>();
        reg.SubscribeTo<ConcertPostedEvent>();
        reg.SubscribeTo<VenueChangedEvent>();
        reg.SubscribeTo<ArtistChangedEvent>();
        reg.SubscribeTo<VenueRatingUpdatedEvent>();
        reg.SubscribeTo<ArtistRatingUpdatedEvent>();
        reg.SubscribeTo<ConcertRatingUpdatedEvent>();
        reg.SubscribeTo<CredentialRegisteredEvent>();
        reg.SubscribeTo<PaymentSucceededEvent>();
        reg.SubscribeTo<PaymentFailedEvent>();
    });
services.AddDirectBusKeyed("webhook");
services.AddOutbox(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDb")));
services.AddInbox(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDb")));
services.AddScoped<AuditInterceptor>();
services.AddScoped<IDomainEventDispatchInterceptor, DomainEventDispatchInterceptor>();
services.AddSeedingInfrastructure();
if (!builder.Environment.IsEnvironment("Testing"))
{
    services.AddScoped<IDbInitializer, DevDbInitializer>();
    services.AddScoped<SeedState>();
    services.AddCustomerPreferenceDevSeeder();
}

services.AddCustomerConcertModule(builder.Configuration);
services.AddCustomerTicketModule(builder.Configuration);
services.AddCustomerReviewModule(builder.Configuration);
services.AddCustomerUserModule(builder.Configuration);
services.AddCustomerUserApi();
services.AddCustomerPreferenceModule(builder.Configuration);
services.AddCustomerPreferenceApi();
services.AddCustomerVenueModule(builder.Configuration);
services.AddCustomerArtistModule(builder.Configuration);

services.AddNotificationClient();
services.AddCurrentUser();
services.AddPaymentClient(builder.Configuration);

services.AddExceptionHandler<GlobalExceptionHandler>();
services.AddProblemDetails();

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.MapInboundClaims = false;
        opts.Authority = builder.Configuration["Auth:Authority"] ?? builder.Configuration["services__auth__https__0"];
        opts.Audience = "concertable.customer.api";
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = !builder.Environment.IsDevelopment(),
            RoleClaimType = "role"
        };
        opts.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });
services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

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

public partial class Program { }
