using Concertable.Customer.Artist.Application.DTOs;
using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Customer.Artist.Infrastructure.Mappers;
using Concertable.DataAccess.Application;
using Concertable.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Repositories;

internal sealed class ArtistReadRepository : ReadRepository<ArtistEntity, int>, IArtistReadRepository
{
    public ArtistReadRepository(IReadDbContext context) : base(context) { }

    public Task<ArtistDetails?> GetDetailsByIdAsync(int artistId) =>
        base.context.Query<ArtistEntity>()
            .Where(a => a.Id == artistId)
            .ToDetails()
            .FirstOrDefaultAsync();
}
