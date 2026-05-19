using Concertable.Customer.Ticket.Application.Validators;
using Concertable.Customer.Ticket.Infrastructure.Data;
using Concertable.Customer.Ticket.Infrastructure.Repositories;
using Concertable.Customer.Ticket.Infrastructure.Services;
using Concertable.Customer.Ticket.Infrastructure.Services.Payment;
using Concertable.Customer.Ticket.Infrastructure.Validators;
using Concertable.Payment.Contracts.Events;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.Ticket.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerTicketModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TicketDbContext>((sp, opts) =>
            opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .AddInterceptors(
                    sp.GetRequiredService<AuditInterceptor>(),
                    sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IUnitOfWork<TicketDbContext>, UnitOfWork<TicketDbContext>>();
        services.AddScoped<IUnitOfWorkBehavior, UnitOfWorkBehavior>();

        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITicketValidator, TicketValidator>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketNotifier, TicketNotifier>();

        services.AddSingleton<QRCoder.QRCodeGenerator>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IPdfService, PdfService>();

        services.AddScoped<IIntegrationEventHandler<PaymentSucceededEvent>, TicketPaymentProcessor>();

        services.AddSingleton<TicketConfigurationProvider>();
        services.AddSingleton<IEntityTypeConfigurationProvider>(sp => sp.GetRequiredService<TicketConfigurationProvider>());

        services.AddValidatorsFromAssemblyContaining<TicketPurchaseParamsValidator>();

        return services;
    }
}
