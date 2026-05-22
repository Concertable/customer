namespace Concertable.Customer.User.Infrastructure;

internal class UserModule : IUserModule
{
    private readonly IUserRepository userRepository;

    public UserModule(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<IReadOnlyCollection<UserDto>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var users = await userRepository.GetByIdsAsync(ids);
        return users.Select(ToDto).ToList();
    }

    private static UserDto ToDto(UserEntity user) =>
        new(user.Id, user.Email, user.Location?.Y, user.Location?.X, user.Address?.County, user.Address?.Town);
}
