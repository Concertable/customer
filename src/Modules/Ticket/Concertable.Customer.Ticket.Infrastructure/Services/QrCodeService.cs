using Concertable.Kernel.Exceptions;
using QRCoder;

namespace Concertable.Customer.Ticket.Infrastructure.Services;

internal sealed class QrCodeService : IQrCodeService
{
    private readonly QRCodeGenerator qrCodeGenerator;
    private readonly ITicketRepository ticketRepository;

    public QrCodeService(QRCodeGenerator qrCodeGenerator, ITicketRepository ticketRepository)
    {
        this.qrCodeGenerator = qrCodeGenerator;
        this.ticketRepository = ticketRepository;
    }

    public byte[] GenerateFromTicketId(Guid id)
    {
        QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(id.ToString(), QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }

    public async Task<byte[]> GetByTicketIdAsync(Guid ticketId)
    {
        return await ticketRepository.GetQrCodeByIdAsync(ticketId)
            .OrNotFound("QR Code");
    }
}
