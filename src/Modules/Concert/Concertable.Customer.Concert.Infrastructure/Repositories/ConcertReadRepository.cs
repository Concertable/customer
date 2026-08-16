using Concertable.Customer.Concert.Application.DTOs;
using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Concert.Domain.Entities;
using Concertable.Customer.Concert.Domain.ReadModels;
using Concertable.Customer.Concert.Infrastructure.Data;
using Concertable.Customer.Concert.Infrastructure.Mappers;
using Concertable.Customer.DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Concert.Infrastructure.Repositories;

internal sealed class ConcertReadRepository(IConcertReadDbContext context)
    : QueryableReadRepository<ConcertEntity, int>(context.Concerts), IConcertReadRepository
{
    public override Task<ConcertEntity?> GetByIdAsync(int id, CancellationToken ct = default) =>
        context.Concerts.Include(c => c.Genres).FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<ConcertDto?> GetDtoAsync(int concertId, CancellationToken ct = default) =>
        context.Concerts
            .Where(c => c.Id == concertId)
            .ToDto()
            .FirstOrDefaultAsync(ct);

    public Task<ConcertDetails?> GetDetailsAsync(int concertId, CancellationToken ct = default) =>
        context.Concerts
            .Where(c => c.Id == concertId)
            .ToDetails(context.VenueReadModels, context.ArtistReadModels)
            .FirstOrDefaultAsync(ct);
}
