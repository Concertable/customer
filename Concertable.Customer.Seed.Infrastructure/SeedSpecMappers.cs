using Concertable.B2B.Seed.Contracts.Specs;
using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Seed.Infrastructure.Factories;
using Concertable.Customer.Ticket.Domain.Entities;
using Concertable.Customer.Venue.Domain.Entities;

namespace Concertable.Customer.Seed.Infrastructure;

public static class SeedSpecMappers
{
    public static VenueReadModel ToReadModel(this VenueSeedSpec spec) =>
        VenueReadModel.Create(
            spec.VenueId, spec.UserId, spec.Name, spec.About,
            spec.Avatar, spec.BannerUrl, spec.County, spec.Town,
            spec.Latitude, spec.Longitude, spec.Email);

    public static ArtistReadModel ToReadModel(this ArtistSeedSpec spec)
    {
        var artist = ArtistReadModel.Create(
            spec.ArtistId, spec.UserId, spec.Name, spec.About,
            spec.Avatar, spec.BannerUrl, spec.County, spec.Town,
            spec.Latitude, spec.Longitude, spec.Email);

        foreach (var genre in spec.Genres)
            artist.Genres.Add(new ArtistGenreReadModel { ArtistId = spec.ArtistId, Genre = genre });

        return artist;
    }

    public static ConcertReadModel ToReadModel(this ConcertSeedSpec spec)
    {
        var concert = ConcertReadModel.Create(
            spec.ConcertId, spec.Name, spec.About, spec.BannerUrl, spec.Avatar,
            spec.TotalTickets, spec.Price, spec.Period, spec.DatePosted,
            spec.ArtistId, spec.ArtistName, spec.VenueId, spec.VenueName);

        foreach (var genre in spec.Genres)
            concert.Genres.Add(new ConcertGenreReadModel { ConcertId = spec.ConcertId, Genre = genre });

        return concert;
    }

    public static TicketEntity ToTicket(this ConcertReadModel concert, Guid id, Guid userId, DateTime purchaseDate) =>
        TicketFactory.Create(
            id, userId, concert.Id, purchaseDate,
            concert.Name, concert.Price, concert.Period,
            concert.ArtistId, concert.ArtistName, concert.VenueId, concert.VenueName);
}
