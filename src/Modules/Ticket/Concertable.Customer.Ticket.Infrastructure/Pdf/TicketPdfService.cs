using Concertable.Customer.Ticket.Contracts;
using Concertable.Kernel.Exceptions;
using Concertable.Shared.Pdf.Application;

namespace Concertable.Customer.Ticket.Infrastructure.Pdf;

internal sealed class TicketPdfService : ITicketPdfService
{
    private readonly IPdfRenderer pdfRenderer;
    private readonly ITicketRepository ticketRepository;

    public TicketPdfService(IPdfRenderer pdfRenderer, ITicketRepository ticketRepository)
    {
        this.pdfRenderer = pdfRenderer;
        this.ticketRepository = ticketRepository;
    }

    public async Task<byte[]> RenderTicketReceiptAsync(string email, Guid ticketId)
    {
        byte[] qrCode = await ticketRepository.GetQrCodeByIdAsync(ticketId)
            .OrNotFound(DisplayNames.QrCode);
        return pdfRenderer.Render(new TicketReceiptDocument(email, ticketId, qrCode));
    }
}
