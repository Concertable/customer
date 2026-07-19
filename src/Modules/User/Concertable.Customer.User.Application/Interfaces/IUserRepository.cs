using Concertable.Customer.User.Domain.Entities;

namespace Concertable.Customer.User.Application.Interfaces;

internal interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<UserEntity>> GetByIdsAsync(IEnumerable<Guid> ids);
    void Update(UserEntity entity);
    Task SaveChangesAsync();
}
