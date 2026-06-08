using Concertable.Customer.User.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.User.Infrastructure.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly UserDbContext context;

    public UserRepository(UserDbContext context)
    {
        this.context = context;
    }

    public async Task<UserEntity?> GetByIdAsync(Guid id) =>
        await context.Users.FindAsync(id);

    public async Task<IReadOnlyCollection<UserEntity>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        await context.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

    public void Update(UserEntity entity) => context.Users.Update(entity);

    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}
