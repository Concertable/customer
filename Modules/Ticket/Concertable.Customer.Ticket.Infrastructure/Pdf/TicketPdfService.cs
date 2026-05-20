using Concertable.Shared.Pdf;

namespace Concertable.Customer.Ticket.Infrastructure.Pdf;

internal sealed class TicketPdfService : ITicketPdfService
{
    private readonly IPdfService pdfService;
    private readonly IQrCodeService qrCodeService;

    public TicketPdfService(IPdfService pdfService, IQrCodeService qrCodeService)
    {
        this.pdfService = pdfService;
        this.qrCodeService = qrCodeService;
    }

    public async Task<byte[]> RenderTicketReceiptAsync(string email, Guid ticketId)
    {
        byte[] qrCode = await qrCodeService.GetByTicketIdAsync(ticketId);
        return pdfService.Render(new TicketReceiptDocument(email, ticketId, qrCode));
    }
}
