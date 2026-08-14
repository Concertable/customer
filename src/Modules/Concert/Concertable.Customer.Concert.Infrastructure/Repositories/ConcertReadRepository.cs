using Concertable.Customer.Concert.Application.DTOs;
using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Domain.ReadModels;
using Concertable.Customer.Concert.Infrastructure.Mappers;
using Concertable.DataAccess.Application;
using Concertable.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Repositories;

internal sealed class ConcertReadRepository : ReadRepository<ConcertEntity, int>, IConcertReadRepository
{
    public ConcertReadRepository(IReadDbContext context) : base(context) { }

    public override Task<ConcertEntity?> GetByIdAsync(int id, CancellationToken ct = default) =>
        base.context.Query<ConcertEntity>().Include(c => c.Genres).FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<ConcertDto?> GetDtoAsync(int concertId, CancellationToken ct = default) =>
        base.context.Query<ConcertEntity>()
            .Where(c => c.Id == concertId)
            .ToDto()
            .FirstOrDefaultAsync(ct);

    public Task<ConcertDetails?> GetDetailsAsync(int concertId, CancellationToken ct = default) =>
        base.context.Query<ConcertEntity>()
            .Where(c => c.Id == concertId)
            .ToDetails(base.context.Query<VenueReadModel>(), base.context.Query<ArtistReadModel>())
            .FirstOrDefaultAsync(ct);
}
