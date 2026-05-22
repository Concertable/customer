namespace Concertable.Customer.User.Contracts;

public interface IUserModule
{
    Task<IReadOnlyCollection<UserDto>> GetByIdsAsync(IEnumerable<Guid> ids);
}
