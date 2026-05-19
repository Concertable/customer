namespace Concertable.Customer.Concert.Application.Interfaces;

internal interface IConcertRepository
{
    Task<ConcertEntity?> GetByIdAsync(int concertId);
    Task AddAsync(ConcertEntity concert);
    Task SaveChangesAsync();
}
