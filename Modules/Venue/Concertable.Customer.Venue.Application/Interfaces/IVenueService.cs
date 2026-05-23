using Concertable.Customer.Venue.Application.Dtos;

namespace Concertable.Customer.Venue.Application.Interfaces;

internal interface IVenueService
{
    Task<VenueDetailDto?> GetByIdAsync(int venueId);
}
