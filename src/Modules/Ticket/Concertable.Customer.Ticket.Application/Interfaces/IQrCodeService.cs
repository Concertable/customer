namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface IQrCodeService
{
    byte[] GenerateFromTicketId(Guid id);
    Task<byte[]> GetByTicketIdAsync(Guid ticketId);
}
