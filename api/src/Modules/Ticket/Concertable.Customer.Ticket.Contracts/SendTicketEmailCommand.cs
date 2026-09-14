using Concertable.Messaging.Contracts;

namespace Concertable.Customer.Ticket.Contracts;

[MessageType("concertable.customer.send-ticket-email.v1")]
public sealed record SendTicketEmailCommand(
    string Email,
    IReadOnlyList<Guid> TicketIds) : IIntegrationCommand;
