using Concertable.Customer.Venue.Application.DTOs;
using Concertable.Customer.Venue.Contracts;
using Concertable.Customer.Venue.Domain.Entities;
using Concertable.DataAccess.Application;

namespace Concertable.Customer.Venue.Application.Interfaces;

internal interface IVenueReadRepository : IReadRepository<VenueEntity>
{
    Task<VenueSummary?> GetSummaryAsync(int venueId);

    Task<VenueDetails?> GetDetailsByIdAsync(int venueId);
}
