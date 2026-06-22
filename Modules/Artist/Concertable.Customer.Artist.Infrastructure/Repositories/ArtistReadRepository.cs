using Concertable.Customer.Artist.Application.DTOs;
using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Customer.Artist.Infrastructure.Data;
using Concertable.Customer.Artist.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Repositories;

internal sealed class ArtistReadRepository : ReadRepository<ArtistEntity>, IArtistReadRepository
{
    public ArtistReadRepository(ArtistDbContext context) : base(context) { }

    public Task<ArtistDetails?> GetDetailsByIdAsync(int artistId) =>
        context.Artists
            .Where(a => a.Id == artistId)
            .ToDetails()
            .FirstOrDefaultAsync();
}
