using Concertable.Customer.Artist.Application.DTOs;
using Concertable.Customer.Artist.Domain.Entities;
using Concertable.Customer.Artist.Infrastructure.Data;
using Concertable.Customer.Artist.Infrastructure.Mappers;
using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Artist.Infrastructure.Repositories;

internal sealed class ArtistReadRepository : QueryableReadRepository<ArtistEntity, int>, IArtistReadRepository
{
    private readonly IArtistReadDbContext context;

    public ArtistReadRepository(IArtistReadDbContext context) : base(context.Artists)
    {
        this.context = context;
    }

    public Task<ArtistDetails?> GetDetailsByIdAsync(int artistId) =>
        context.Artists
            .Where(a => a.Id == artistId)
            .ToDetails()
            .FirstOrDefaultAsync();
}
