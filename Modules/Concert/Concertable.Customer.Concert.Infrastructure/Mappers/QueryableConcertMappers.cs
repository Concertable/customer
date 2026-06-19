using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Concert.Domain.Entities;

namespace Concertable.Customer.Concert.Infrastructure.Mappers;

internal static class QueryableConcertMappers
{
    public static IQueryable<ConcertDto> ToDto(this IQueryable<ConcertEntity> query) =>
        query.Select(c => new ConcertDto(
            c.Id,
            c.Name,
            c.Price,
            c.Period,
            c.DatePosted,
            c.AvailableTickets,
            c.ArtistId,
            c.ArtistName,
            c.VenueId,
            c.VenueName,
            c.PayeeUserId,
            c.PayeeOwnerId));
}
