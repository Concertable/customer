using Concertable.DataAccess.Application;
using Concertable.Customer.User.Domain.Entities;

namespace Concertable.Customer.User.Application.Interfaces;

internal interface IUserRepository : IRepository<UserEntity, Guid>
{
    Task<IReadOnlyList<UserEntity>> GetByIdsAsync(IEnumerable<Guid> ids);
}
