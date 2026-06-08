using Concertable.Customer.User.Application.Mappers;

namespace Concertable.Customer.User.Infrastructure;

internal sealed class UserModule : IUserModule
{
    private readonly IUserRepository userRepository;

    public UserModule(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<IReadOnlyCollection<CustomerDto>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var users = await userRepository.GetByIdsAsync(ids);
        return users.Select(u => u.ToDto()).ToList();
    }
}
