namespace Concertable.Customer.User.Contracts;

public interface IUserModule
{
    Task<IReadOnlyList<CustomerDto>> GetByIdsAsync(IEnumerable<Guid> ids);
}
