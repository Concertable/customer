using Concertable.Shared.Email.Application;

namespace Concertable.Customer.Ticket.Infrastructure.Pdf;

internal sealed class TicketEmailSender : ITicketEmailSender
{
    private readonly IEmailSender emailSender;
    private readonly ITicketPdfService ticketPdfService;

    public TicketEmailSender(IEmailSender emailSender, ITicketPdfService ticketPdfService)
    {
        this.emailSender = emailSender;
        this.ticketPdfService = ticketPdfService;
    }

    public async Task SendTicketsAsync(string email, IReadOnlyList<Guid> ticketIds)
    {
        var attachments = new List<EmailAttachment>();
        foreach (var ticketId in ticketIds)
        {
            var pdf = await ticketPdfService.RenderTicketReceiptAsync(email, ticketId);
            attachments.Add(new EmailAttachment(pdf, $"Ticket-{ticketId}.pdf"));
        }

        await emailSender.SendEmailAsync(
            email,
            "Your Ticket Receipt",
            $"<p>Thank you for your order! Your {ticketIds.Count} ticket(s) are attached.</p>",
            attachments);
    }
}
