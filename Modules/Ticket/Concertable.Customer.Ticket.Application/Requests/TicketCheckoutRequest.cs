namespace Concertable.Customer.Ticket.Application.Requests;

internal record TicketCheckoutRequest(int ConcertId, int Quantity);
