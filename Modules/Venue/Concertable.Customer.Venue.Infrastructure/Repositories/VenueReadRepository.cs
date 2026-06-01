using Concertable.Customer.Venue.Domain.Entities;
using Concertable.Customer.Venue.Infrastructure.Data;

namespace Concertable.Customer.Venue.Infrastructure.Repositories;

internal class VenueReadRepository : ReadRepository<VenueEntity>, IVenueReadRepository
{
    public VenueReadRepository(VenueDbContext context) : base(context) { }
}
