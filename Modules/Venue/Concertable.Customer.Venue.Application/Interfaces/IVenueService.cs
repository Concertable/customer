using Concertable.Customer.Venue.Application.Dtos;

namespace Concertable.Customer.Venue.Application.Interfaces;

internal interface IVenueService
{
    Task<VenueDetail?> GetByIdAsync(int venueId);
}
