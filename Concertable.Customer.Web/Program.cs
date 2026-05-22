using Concertable.Concert.Contracts.Events;
using Concertable.Customer.Concert.Infrastructure.Extensions;
using Concertable.Messaging.Application;
using Concertable.Messaging.AzureServiceBus;
using Concertable.Customer.Profile.Infrastructure.Extensions;
using Concertable.Customer.Review.Infrastructure.Extensions;
using Concertable.Customer.Ticket.Infrastructure.Extensions;
using Concertable.DataAccess.Infrastructure;
using Concertable.Messaging.Infrastructure.Extensions;
using Concertable.Messaging.Infrastructure.Inbox;
using Concertable.Messaging.Infrastructure.Outbox;
using Concertable.Notification.Infrastructure.Extensions;
using Concertable.Payment.Client.Extensions;
using Concertable.Payment.Domain.Events;
using Concertable.Auth.Contracts.Events;
using Concertable.Shared.Blob.Infrastructure.Extensions;
using Concertable.Shared.Email.Infrastructure.Extensions;
using Concertable.Shared.Geocoding.Infrastructure.Extensions;
using Concertable.Shared.Imaging.Infrastructure.Extensions;
using Concertable.Shared.Pdf.Infrastructure.Extensions;
using Concertable.Shared.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

var services = builder.Services;

services.AddScoped<IKeyedServiceProvider>(sp => (IKeyedServiceProvider)sp);
services.AddSingleton(TimeProvider.System);
services.AddSharedInfrastructure(builder.Configuration);
services.AddClientCredentials(opts =>
{
    opts.Authority = builder.Configuration["Auth:Authority"] ?? builder.Configuration["services__auth__https__0"] ?? "";
    opts.ClientId = builder.Configuration["ServiceAuth:ClientId"] ?? "";
    opts.ClientSecret = builder.Configuration["ServiceAuth:ClientSecret"] ?? "";
});
services.AddSharedBlob(builder.Configuration);
services.AddSharedEmail(builder.Configuration);
services.AddSharedGeocoding();
services.AddSharedImaging();
services.AddSharedPdf();
services.AddAzureServiceBusTransport(
    opts =>
    {
        opts.ConnectionString = builder.Configuration.GetConnectionString("asb") ?? "";
        opts.ServiceName = "concertable-customer";
    },
    reg =>
    {
        reg.Publishes<ReviewSubmittedEvent>();

        reg.SubscribeTo<ConcertChangedEvent>();
        reg.SubscribeTo<CredentialRegisteredEvent>();
        reg.SubscribeTo<PaymentSucceededEvent>();
        reg.SubscribeTo<PaymentFailedEvent>();
    });
services.AddDirectBusKeyed("webhook");
services.AddOutbox(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDb")));
services.AddInbox(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("CustomerDb")));
services.AddScoped<AuditInterceptor>();
services.AddScoped<DomainEventDispatchInterceptor>();

services.AddCustomerConcertModule(builder.Configuration);
services.AddCustomerTicketModule(builder.Configuration);
services.AddCustomerReviewModule(builder.Configuration);
services.AddCustomerProfileModule(builder.Configuration);

services.AddNotificationClient();
services.AddCurrentUser();
services.AddPaymentClient(builder.Configuration);

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.MapInboundClaims = false;
        opts.Authority = builder.Configuration["Auth:Authority"] ?? builder.Configuration["services__auth__https__0"];
        opts.Audience = "concertable.customer.api";
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = !builder.Environment.IsDevelopment()
        };
    });
services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    await sp.GetRequiredService<OutboxDbContext>().Database.MigrateAsync();
    await sp.GetRequiredService<InboxDbContext>().Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapControllers();

app.Run();

public partial class Program { }
