using Concertable.Customer.Ticket.Application.Validators;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Customer.Ticket.Infrastructure.Pdf;
using Concertable.Customer.Ticket.Infrastructure.Repositories;
using Concertable.Customer.Ticket.Infrastructure.Services;
using Concertable.Customer.Review.Contracts.Events;
using Concertable.Customer.Ticket.Infrastructure.Services.Events;
using Concertable.Customer.Ticket.Infrastructure.Services.Payment;
using Concertable.Customer.Ticket.Infrastructure.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Concertable.DataAccess.Application;
using Concertable.DataAccess.Infrastructure.Data;
using Concertable.Messaging.Contracts;

namespace Concertable.Customer.Ticket.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerTicketModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TicketDbContext>((sp, opts) =>
            opts.UseSqlServer(configuration.GetConnectionString("CustomerDb"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<IDomainEventDispatchInterceptor>()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUnitOfWorkBehavior, UnitOfWorkBehavior>();

        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITicketValidator, TicketValidator>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketNotifier, TicketNotifier>();

        services.AddSingleton<QRCoder.QRCodeGenerator>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<ITicketPdfService, TicketPdfService>();
        services.AddScoped<ITicketEmailSender, TicketEmailSender>();

        services.AddScoped<IIntegrationEventHandler<PaymentSucceededEvent>, TicketPaymentProcessor>();
        services.AddScoped<IIntegrationEventHandler<PaymentFailedEvent>, TicketPaymentFailedProcessor>();
        services.AddScoped<IIntegrationEventHandler<CustomerReviewSubmittedEvent>, CustomerReviewSubmittedEventHandler>();

        services.AddSingleton<TicketConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<TicketConfigurationProvider>());

        services.AddValidatorsFromAssemblyContaining<TicketPurchaseParamsValidator>();

        return services;
    }
}
