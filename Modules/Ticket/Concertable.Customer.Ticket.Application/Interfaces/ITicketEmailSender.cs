namespace Concertable.Customer.Ticket.Application.Interfaces;

internal interface ITicketEmailSender
{
    Task SendTicketsAsync(string email, IReadOnlyList<Guid> ticketIds);
}
