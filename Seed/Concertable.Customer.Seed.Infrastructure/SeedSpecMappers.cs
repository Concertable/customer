using Concertable.B2B.Seed.Contracts.Specs;
using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Seed.Infrastructure.Factories;
using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Customer.Venue.Domain.Entities;

namespace Concertable.Customer.Seed.Infrastructure;

public static class SeedSpecMappers
{
    public static VenueEntity ToEntity(this VenueSeedSpec spec) =>
        VenueEntity.Create(
            spec.VenueId, spec.UserId, spec.Name, spec.About,
            spec.Avatar, spec.BannerUrl, spec.County, spec.Town,
            spec.Latitude, spec.Longitude, spec.Email);

    public static ArtistEntity ToEntity(this ArtistSeedSpec spec)
    {
        var artist = ArtistEntity.Create(
            spec.ArtistId, spec.UserId, spec.Name, spec.About,
            spec.Avatar, spec.BannerUrl, spec.County, spec.Town,
            spec.Latitude, spec.Longitude, spec.Email);

        foreach (var genre in spec.Genres)
            artist.Genres.Add(new ArtistGenreEntity { ArtistId = spec.ArtistId, Genre = genre });

        return artist;
    }

    public static ConcertEntity ToEntity(this ConcertSeedSpec spec)
    {
        var concert = ConcertEntity.Create(
            spec.ConcertId, spec.Name, spec.About, spec.BannerUrl, spec.Avatar,
            spec.TotalTickets, spec.Price, spec.Period, spec.DatePosted,
            spec.ArtistId, spec.ArtistName, spec.VenueId, spec.VenueName, spec.PayeeUserId, spec.PayeeOwnerId);

        foreach (var genre in spec.Genres)
            concert.Genres.Add(new ConcertGenreEntity { ConcertId = spec.ConcertId, Genre = genre });

        return concert;
    }

    public static TicketEntity ToTicket(this ConcertEntity concert, Guid id, Guid userId, DateTime purchaseDate) =>
        TicketFactory.Create(
            id, userId, concert.Id, purchaseDate,
            concert.Name, concert.Price, concert.Period,
            concert.ArtistId, concert.ArtistName, concert.VenueId, concert.VenueName);
}
