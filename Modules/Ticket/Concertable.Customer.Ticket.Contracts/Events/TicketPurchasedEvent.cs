using Concertable.Messaging.Contracts;

namespace Concertable.Customer.Ticket.Contracts.Events;

[MessageType("concertable.customer.ticket-purchased.v1")]
public sealed record TicketPurchasedEvent(
    Guid TicketId,
    Guid UserId,
    int ConcertId,
    decimal Price,
    DateTime PurchaseDate) : IIntegrationEvent;
