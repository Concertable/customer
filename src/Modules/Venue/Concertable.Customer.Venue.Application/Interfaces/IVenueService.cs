using Concertable.Customer.Venue.Application.DTOs;
using Reunion;

namespace Concertable.Customer.Venue.Application.Interfaces;

internal interface IVenueService
{
    Task<Option<VenueDetails>> GetDetailsByIdAsync(int venueId);
}
