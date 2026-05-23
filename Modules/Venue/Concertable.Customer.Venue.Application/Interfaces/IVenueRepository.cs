namespace Concertable.Customer.Venue.Application.Interfaces;

internal interface IVenueReadRepository
{
    Task<VenueReadModel?> GetByIdAsync(int venueId);
}
