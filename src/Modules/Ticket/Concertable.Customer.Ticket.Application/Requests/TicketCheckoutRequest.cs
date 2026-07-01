namespace Concertable.Customer.Ticket.Application.Requests;

internal sealed record TicketCheckoutRequest(int ConcertId, int Quantity);
