using Concertable.Customer.Ticket.Contracts;
using Concertable.Customer.Ticket.Domain.Entities;

namespace Concertable.Customer.Ticket.Infrastructure.Mappers;

internal static class QueryableTicketMappers
{
    public static IQueryable<TicketSummary> ToSummary(this IQueryable<TicketEntity> query) =>
        query.Select(t => new TicketSummary(t.Id, t.ConcertId, t.ArtistId, t.VenueId, t.Period.Start));
}
