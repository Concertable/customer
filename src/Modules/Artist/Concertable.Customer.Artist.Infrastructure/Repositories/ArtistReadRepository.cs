using Concertable.Customer.Artist.Application.DTOs;
using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Customer.Artist.Infrastructure.Mappers;
using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Repositories;

internal sealed class ArtistReadRepository : ReadRepository<ArtistEntity>, IArtistReadRepository
{
    public ArtistReadRepository(IReadDbContext context) : base(context) { }

    public Task<ArtistDetails?> GetDetailsByIdAsync(int artistId) =>
        context.Query<ArtistEntity>()
            .Where(a => a.Id == artistId)
            .ToDetails()
            .FirstOrDefaultAsync();
}
