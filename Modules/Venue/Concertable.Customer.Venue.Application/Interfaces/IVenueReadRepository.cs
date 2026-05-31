using Concertable.Customer.Venue.Domain.Entities;

namespace Concertable.Customer.Venue.Application.Interfaces;

internal interface IVenueReadRepository
{
    Task<VenueEntity?> GetByIdAsync(int venueId);
}
