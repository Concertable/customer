namespace Concertable.Customer.User.Contracts;

public interface IUserModule
{
    Task<IReadOnlyCollection<CustomerDto>> GetByIdsAsync(IEnumerable<Guid> ids);
}
