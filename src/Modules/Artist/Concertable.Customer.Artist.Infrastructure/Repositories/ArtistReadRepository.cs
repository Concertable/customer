using Concertable.Customer.Artist.Application.DTOs;
using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Customer.Artist.Infrastructure.Data;
using Concertable.Customer.Artist.Infrastructure.Mappers;
using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Repositories;

internal sealed class ArtistReadRepository(IArtistReadDbContext context)
    : QueryableReadRepository<ArtistEntity, int>(context.Artists), IArtistReadRepository
{
    public Task<ArtistDetails?> GetDetailsByIdAsync(int artistId) =>
        context.Artists
            .Where(a => a.Id == artistId)
            .ToDetails()
            .FirstOrDefaultAsync();
}
