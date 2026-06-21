using Concertable.Customer.Concert.Application.DTOs;
using Concertable.Customer.Concert.Domain.Entities;

namespace Concertable.Customer.Concert.Application.Mappers;

internal static class ConcertDetailsMapper
{
    public static ConcertDetails ToDetails(this ConcertEntity concert, ConcertVenue venue, ConcertArtist artist) => new(
        concert.Id,
        concert.Name,
        concert.About,
        concert.BannerUrl,
        concert.Avatar,
        concert.AverageRating,
        concert.Price,
        concert.TotalTickets,
        concert.AvailableTickets,
        concert.Period.Start,
        concert.Period.End,
        concert.DatePosted,
        venue,
        artist,
        concert.Genres.Select(g => g.Genre).ToArray());
}
