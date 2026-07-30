using Concertable.Customer.Ticket.Application.Commands;
using Concertable.Customer.Ticket.Application.Interfaces;
using Concertable.Messaging.Contracts;
using Concertable.Shared.Email.Application;

namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal sealed class SendTicketEmailCommandHandler : IIntegrationCommandHandler<SendTicketEmailCommand>
{
    private readonly ITicketPdfService ticketPdfService;
    private readonly IEmailTransport transport;

    public SendTicketEmailCommandHandler(ITicketPdfService ticketPdfService, IEmailTransport transport)
    {
        this.ticketPdfService = ticketPdfService;
        this.transport = transport;
    }

    public async Task HandleAsync(SendTicketEmailCommand command, MessageEnvelope envelope, CancellationToken ct = default)
    {
        var attachments = new List<EmailAttachment>();
        foreach (var ticketId in command.TicketIds)
        {
            var pdf = await ticketPdfService.RenderTicketReceiptAsync(command.Email, ticketId);
            attachments.Add(new EmailAttachment(pdf, $"Ticket-{ticketId}.pdf"));
        }

        await transport.SendEmailAsync(
            command.Email,
            "Your Ticket Receipt",
            $"<p>Thank you for your order! Your {command.TicketIds.Count} ticket(s) are attached.</p>",
            attachments);
    }
}
