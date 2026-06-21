using Concertable.Customer.Artist.Application.DTOs;
using Concertable.Customer.Artist.Contracts;
using Concertable.Customer.Artist.Domain.Entities;
using Concertable.DataAccess.Application;

namespace Concertable.Customer.Artist.Application.Interfaces;

internal interface IArtistReadRepository : IReadRepository<ArtistEntity>
{
    Task<ArtistSummary?> GetSummaryAsync(int artistId);

    Task<ArtistDetails?> GetDetailsByIdAsync(int artistId);
}
