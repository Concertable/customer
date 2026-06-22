using Concertable.Customer.Concert.Application.DTOs;
using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Domain.ReadModels;

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

    public static IQueryable<ConcertDetails> ToDetails(
        this IQueryable<ConcertEntity> query,
        IQueryable<VenueReadModel> venues,
        IQueryable<ArtistReadModel> artists) =>
        from c in query
        join v in venues on c.VenueId equals v.Id
        join a in artists on c.ArtistId equals a.Id
        select new ConcertDetails(
            c.Id,
            c.Name,
            c.About,
            c.BannerUrl,
            c.Avatar,
            c.AverageRating,
            c.Price,
            c.TotalTickets,
            c.AvailableTickets,
            c.Period.Start,
            c.Period.End,
            c.DatePosted,
            new ConcertVenue(v.Id, v.Name, v.Address.County, v.Address.Town, v.Latitude, v.Longitude),
            new ConcertArtist(a.Id, a.Name, a.Avatar, a.AverageRating, a.Address.County, a.Address.Town, a.Genres.Select(g => g.Genre).ToArray()),
            c.Genres.Select(g => g.Genre).ToArray());
}
