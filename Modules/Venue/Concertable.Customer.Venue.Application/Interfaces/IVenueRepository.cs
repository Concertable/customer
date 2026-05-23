namespace Concertable.Customer.Venue.Application.Interfaces;

internal interface IVenueRepository
{
    Task<VenueEntity?> GetByIdAsync(int venueId);
    Task AddAsync(VenueEntity venue);
    Task SaveChangesAsync();
}
