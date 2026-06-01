using Concertable.Customer.Venue.Domain.Entities;
using Concertable.DataAccess.Application;

namespace Concertable.Customer.Venue.Application.Interfaces;

internal interface IVenueReadRepository : IReadRepository<VenueEntity>;
