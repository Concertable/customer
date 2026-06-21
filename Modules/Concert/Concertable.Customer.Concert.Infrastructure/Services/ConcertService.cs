using Concertable.Customer.Artist.Contracts;
using Concertable.Customer.Concert.Application.DTOs;
using Concertable.Customer.Concert.Application.Mappers;
using Concertable.Customer.Venue.Contracts;

namespace Concertable.Customer.Concert.Infrastructure.Services;

internal sealed class ConcertService : IConcertService
{
    private readonly IConcertReadRepository concertRepository;
    private readonly IVenueModule venueModule;
    private readonly IArtistModule artistModule;

    public ConcertService(
        IConcertReadRepository concertRepository,
        IVenueModule venueModule,
        IArtistModule artistModule)
    {
        this.concertRepository = concertRepository;
        this.venueModule = venueModule;
        this.artistModule = artistModule;
    }

    public async Task<ConcertDetails?> GetDetailsByIdAsync(int concertId, CancellationToken ct = default)
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
            ? new ConcertVenue(concert.VenueId, concert.VenueName, "", "", 0, 0)
            : new ConcertVenue(venue.Id, venue.Name, venue.County, venue.Town, venue.Latitude, venue.Longitude);

        var artistDto = artist is null
            ? new ConcertArtist(concert.ArtistId, concert.ArtistName, null, 0, "", "", [])
            : new ConcertArtist(artist.Id, artist.Name, artist.Avatar, artist.Rating, artist.County, artist.Town, artist.Genres);

        return concert.ToDetails(venueDto, artistDto);
    }
}
