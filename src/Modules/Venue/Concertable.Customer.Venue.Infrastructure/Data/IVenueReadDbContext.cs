using Concertable.Customer.Venue.Domain.Entities;

namespace Concertable.Customer.Venue.Infrastructure.Data;

internal interface IVenueReadDbContext
{
    IQueryable<VenueEntity> Venues { get; }
}
