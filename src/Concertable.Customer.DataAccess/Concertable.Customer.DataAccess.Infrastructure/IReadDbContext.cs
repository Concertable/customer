namespace Concertable.Customer.DataAccess.Infrastructure;

public interface IReadDbContext
{
    IQueryable<T> Query<T>() where T : class;
}
