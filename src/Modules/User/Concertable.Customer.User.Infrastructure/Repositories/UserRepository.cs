using Concertable.Customer.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.User.Infrastructure.Repositories;

internal sealed class UserRepository(UserDbContext context) : Repository<UserEntity>(context), IUserRepository
{
    public async Task<IReadOnlyList<UserEntity>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        await base.Context.Query<UserEntity>().Where(u => ids.Contains(u.Id)).ToListAsync();
}
