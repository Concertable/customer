using Concertable.Customer.Venue.Application.DTOs;
using Concertable.Kernel.Functional;

namespace Concertable.Customer.Venue.Application.Interfaces;

internal interface IVenueService
{
    Task<Option<VenueDetails>> GetDetailsByIdAsync(int venueId);
}
