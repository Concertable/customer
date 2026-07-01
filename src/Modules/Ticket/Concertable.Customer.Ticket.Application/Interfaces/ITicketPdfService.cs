namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketPdfService
{
    Task<byte[]> RenderTicketReceiptAsync(string email, Guid ticketId);
}
