using Concertable.Customer.Artist.Contracts;
using Concertable.Customer.Concert.Application.Dtos;
using Concertable.Customer.Venue.Contracts;

namespace Concertable.Customer.Concert.Infrastructure.Services;

internal sealed class ConcertService : IConcertService
{
    private readonly IConcertReadRepository concertRepository;
    private readonly ICustomerVenueModule venueModule;
    private readonly ICustomerArtistModule artistModule;

    public ConcertService(
        IConcertReadRepository concertRepository,
        ICustomerVenueModule venueModule,
        ICustomerArtistModule artistModule)
    {
        this.concertRepository = concertRepository;
        this.venueModule = venueModule;
        this.artistModule = artistModule;
    }

    public async Task<ConcertDetailDto?> GetByIdAsync(int concertId, CancellationToken ct = default)
    {
        var concert = await concertRepository.GetByIdAsync(concertId);
        if (concert is null)
            return null;

        var venueTask = venueModule.GetSummaryAsync(concert.VenueId, ct);
        var artistTask = artistModule.GetSummaryAsync(concert.ArtistId, ct);
        await Task.WhenAll(venueTask, artistTask);

        var venue = venueTask.Result;
        var artist = artistTask.Result;

        var venueDto = venue is null
            ? new ConcertVenueDto(concert.VenueId, concert.VenueName, "", "", 0, 0)
            : new ConcertVenueDto(venue.Id, venue.Name, venue.County, venue.Town, venue.Latitude, venue.Longitude);

        var artistDto = artist is null
            ? new ConcertArtistDto(concert.ArtistId, concert.ArtistName, null, 0, "", "", [])
            : new ConcertArtistDto(artist.Id, artist.Name, artist.Avatar, artist.Rating, artist.County, artist.Town, artist.Genres);

        return new ConcertDetailDto(
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
            venueDto,
            artistDto,
            concert.Genres.Select(g => g.Genre).ToArray());
    }
}
