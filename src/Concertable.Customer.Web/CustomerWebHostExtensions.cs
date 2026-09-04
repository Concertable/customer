using Concertable.Auth.Contracts.Events;
using Concertable.B2B.Artist.Contracts.Events;
using Concertable.B2B.Concert.Contracts.Events;
using Concertable.B2B.Seed.Contracts;
using Concertable.B2B.Venue.Contracts.Events;
using Concertable.Customer.Artist.Api.Extensions;
using Concertable.Customer.Concert.Api.Extensions;
using Concertable.Customer.Preference.Api.Extensions;
using Concertable.Customer.Review.Api.Extensions;
using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Seed.Infrastructure;
using Concertable.Customer.Ticket.Api.Extensions;
using Concertable.Customer.Ticket.Contracts;
using Concertable.Customer.Ticket.Contracts.Events;
using Concertable.Customer.User.Api.Extensions;
using Concertable.Customer.Venue.Api.Extensions;
using Concertable.DataAccess.Application;
using Concertable.DataAccess.Infrastructure.Data;
using Concertable.Kernel;
using Concertable.Kernel.Extensions;
using Concertable.Messaging.Application.Extensions;
using Concertable.Messaging.AzureServiceBus.Extensions;
using Concertable.Messaging.Infrastructure.Extensions;
using Concertable.Messaging.Infrastructure.Inbox;
using Concertable.Messaging.Infrastructure.Outbox;
using Concertable.Payment.Client.Extensions;
using Concertable.Payment.Contracts.Events;
using Concertable.Seed.Shared;
using Concertable.Seed.Shared.Extensions;
using Concertable.ServiceDefaults;
using Concertable.Shared.Api.Exceptions;
using Concertable.Shared.Api.Extensions;
using Concertable.Shared.Email.Infrastructure.Extensions;
using Concertable.Shared.Geocoding.Infrastructure.Extensions;
using Concertable.Shared.Notification.Infrastructure.Extensions;
using Concertable.Shared.Notification.Infrastructure.Hubs;
using Concertable.Shared.Pdf.Infrastructure.Extensions;
using Concertable.Shared.QrCode.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Concertable.Customer.Web;

public static class CustomerWebHostExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddCustomerWebHost()
        {
            builder.AddServiceDefaults();
            builder.Configuration.AddEnvironmentVariables();
            builder.Services.AddProblemDetails();
            builder.Services.AddControllers()
                .AddApplicationPart(typeof(Concertable.Shared.Api.Controllers.GenreController).Assembly)
                .AddApplicationJson();
            var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(corsOrigins));
            });

            var services = builder.Services;
            services.AddScoped<IKeyedServiceProvider>(sp => (IKeyedServiceProvider)sp);
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<SeedCatalog>();
            services.AddSharedInfrastructure(builder.Configuration);
            services.AddGeometry();
            services.AddClientCredentials(opts =>
            {
                opts.Authority = builder.Configuration["Auth:Authority"] ?? builder.Configuration["services__auth__https__0"]
                    ?? (builder.Environment.IsIntegration() ? null!
                        : throw new InvalidOperationException("Auth:Authority is required (no explicit key and no service-discovery fallback)."));
                opts.ClientId = builder.Configuration["ServiceAuth:ClientId"]
                    ?? (builder.Environment.IsIntegration() ? null!
                        : throw new InvalidOperationException("ServiceAuth:ClientId is required."));
                if (builder.Configuration["ServiceAuth:ClientSecret"] is string clientSecret)
                    opts.ClientSecret = clientSecret;
            });
            services.AddSharedEmail(builder.Configuration);
            services.AddSharedGeocoding();
            services.AddSharedPdf();
            services.AddQrCode();
            services.AddAzureServiceBusTransport(
                opts =>
                {
                    opts.ConnectionString = builder.Configuration.GetConnectionString("asb")
                        ?? (builder.Environment.IsIntegration() ? null!
                            : throw new InvalidOperationException("Connection string 'asb' is required."));
                    opts.ServiceName = builder.Configuration["ServiceBus:ServiceName"]
                        ?? (builder.Environment.IsIntegration() ? "concertable-customer"
                            : throw new InvalidOperationException("Configuration 'ServiceBus:ServiceName' is required."));
                },
                reg =>
                {
                    reg.Publishes<CustomerReviewSubmittedEvent>();
                    reg.SubscribeTo<CustomerReviewSubmittedEvent>();
                    reg.Publishes<TicketPurchasedEvent>();
                    reg.SubscribeTo<TicketPurchasedEvent>();
                    reg.HandleCommand<SendTicketEmailCommand>();
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
            if (!builder.Environment.IsIntegration())
            {
                services.AddScoped<IDbInitializer, DevDbInitializer>();
                services.AddScoped<SeedState>();
                services.AddPreferenceDevSeeding();
                services.AddTicketDevSeeding();
            }
            services.AddConcertApi(builder.Configuration);
            services.AddTicketApi(builder.Configuration);
            services.AddReviewApi(builder.Configuration);
            services.AddUserApi(builder.Configuration);
            services.AddPreferenceApi(builder.Configuration);
            services.AddVenueApi(builder.Configuration);
            services.AddArtistApi(builder.Configuration);
            services.AddNotificationClient();
            services.AddCurrentUser();
            if (!builder.Environment.IsIntegration())
                services.AddPaymentClient(builder.Configuration);
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.MapInboundClaims = false;
                    opts.Authority = builder.Configuration["Auth:Authority"] ?? builder.Configuration["services__auth__https__0"];
                    opts.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
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
            services.Configure<ForwardedHeadersOptions>(options =>
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
            builder.AddDefaultRateLimiting();
            builder.AddRateLimitPolicy(RateLimitPolicies.PublicRead, new RateLimitWindow { PermitLimit = 100, WindowSeconds = 60 }, perUser: false);
            builder.AddRateLimitPolicy(RateLimitPolicies.Purchase, new RateLimitWindow { PermitLimit = 20, WindowSeconds = 60 }, perUser: true);
            builder.AddRateLimitPolicy(RateLimitPolicies.Review, new RateLimitWindow { PermitLimit = 10, WindowSeconds = 60 }, perUser: true);
            return builder;
        }
    }

    extension(WebApplication app)
    {
        public async Task UseCustomerWebHost()
        {
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
                await using var scope = app.Services.CreateAsyncScope();
                var services = scope.ServiceProvider;
                await services.GetRequiredService<OutboxDbContext>().Database.MigrateAsync();
                await services.GetRequiredService<InboxDbContext>().Database.MigrateAsync();
                await services.MigrateArtistModuleAsync();
                await services.MigrateConcertModuleAsync();
                await services.MigratePreferenceModuleAsync();
                await services.MigrateReviewModuleAsync();
                await services.MigrateTicketModuleAsync();
                await services.MigrateUserModuleAsync();
                await services.MigrateVenueModuleAsync();
                if (app.Environment.IsDevelopment())
                    await services.GetRequiredService<IDbInitializer>().InitializeAsync();
            }
        }
    }
}
