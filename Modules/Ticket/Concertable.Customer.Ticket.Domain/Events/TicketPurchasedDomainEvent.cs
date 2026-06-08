using Concertable.Kernel;

namespace Concertable.Customer.Ticket.Domain.Events;

public sealed record TicketPurchasedDomainEvent(
    Guid TicketId,
    Guid UserId,
    int ConcertId,
    decimal Price,
    DateTime PurchaseDate) : IDomainEvent;
