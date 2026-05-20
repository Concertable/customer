using Concertable.Authorization.Infrastructure.Extensions;
using Concertable.Customer.Concert.Infrastructure.Extensions;
using Concertable.Customer.Profile.Infrastructure.Extensions;
using Concertable.Customer.Review.Infrastructure.Extensions;
using Concertable.Customer.Ticket.Infrastructure.Extensions;
using Concertable.DataAccess.Infrastructure;
using Concertable.Messaging.Infrastructure.Extensions;
using Concertable.Notification.Infrastructure.Extensions;
using Concertable.Payment.Infrastructure.Extensions;
using Concertable.Shared.Blob.Infrastructure.Extensions;
using Concertable.Shared.Email.Infrastructure.Extensions;
using Concertable.Shared.Geocoding.Infrastructure.Extensions;
using Concertable.Shared.Imaging.Infrastructure.Extensions;
using Concertable.Shared.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

var services = builder.Services;

services.AddScoped<IKeyedServiceProvider>(sp => (IKeyedServiceProvider)sp);
services.AddSingleton(TimeProvider.System);
services.AddSharedInfrastructure(builder.Configuration);
services.AddSharedBlob(builder.Configuration);
services.AddSharedEmail(builder.Configuration);
services.AddSharedGeocoding();
services.AddSharedImaging();
services.AddMessaging();
services.AddScoped<AuditInterceptor>();
services.AddScoped<DomainEventDispatchInterceptor>();

services.AddCustomerConcertModule(builder.Configuration);
services.AddCustomerTicketModule(builder.Configuration);
services.AddCustomerReviewModule(builder.Configuration);
services.AddCustomerProfileModule(builder.Configuration);

services.AddNotificationModule();
services.AddAuthorizationModule();
services.AddPaymentModule(builder.Configuration);

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

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapControllers();

app.Run();

public partial class Program { }
