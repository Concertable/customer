using Concertable.Customer.Venue.Application.DTOs;

namespace Concertable.Customer.Venue.Application.Interfaces;

internal interface IVenueService
{
    Task<VenueDetails?> GetDetailsByIdAsync(int venueId);
}
