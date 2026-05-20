using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Concertable.Customer.Ticket.Infrastructure.Pdf;

internal sealed class TicketReceiptDocument : IDocument
{
    private readonly string email;
    private readonly Guid ticketId;
    private readonly byte[] qrCode;

    public TicketReceiptDocument(string email, Guid ticketId, byte[] qrCode)
    {
        this.email = email;
        this.ticketId = ticketId;
        this.qrCode = qrCode;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(20);
            page.Size(PageSizes.A5);

            page.Header()
                .Text("Ticket")
                .FontSize(20)
                .Bold()
                .AlignCenter();

            page.Content()
                .Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text($"Email: {email}").FontSize(14);
                    column.Item().Text($"TicketId: {ticketId}").FontSize(14);

                    if (qrCode != null)
                        column.Item().Image(qrCode);

                    column.Item().Text("Show this QR code at entrance").Italic();
                });

            page.Footer()
                .AlignCenter()
                .Text("Thank you for using Concertable");
        });
    }
}
