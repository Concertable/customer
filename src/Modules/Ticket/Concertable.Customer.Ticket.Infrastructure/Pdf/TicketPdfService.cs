using Concertable.Shared.Pdf.Application;

namespace Concertable.Customer.Ticket.Infrastructure.Pdf;

internal sealed class TicketPdfService : ITicketPdfService
{
    private readonly IPdfRenderer pdfRenderer;
    private readonly IQrCodeService qrCodeService;

    public TicketPdfService(IPdfRenderer pdfRenderer, IQrCodeService qrCodeService)
    {
        this.pdfRenderer = pdfRenderer;
        this.qrCodeService = qrCodeService;
    }

    public async Task<byte[]> RenderTicketReceiptAsync(string email, Guid ticketId)
    {
        byte[] qrCode = await qrCodeService.GetByTicketIdAsync(ticketId);
        return pdfRenderer.Render(new TicketReceiptDocument(email, ticketId, qrCode));
    }
}
