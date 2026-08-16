using Concertable.Customer.User.Application.Interfaces;

namespace Concertable.Customer.User.Infrastructure;

internal sealed class UserModule : IUserModule
{
    private readonly IUserService userService;

    public UserModule(IUserService userService)
    {
        this.userService = userService;
    }

    public Task<IReadOnlyList<CustomerDto>> GetByIdsAsync(IEnumerable<Guid> ids) =>
        userService.GetByIdsAsync(ids);
}
