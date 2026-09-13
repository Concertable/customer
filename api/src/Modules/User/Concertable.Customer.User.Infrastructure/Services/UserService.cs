using Concertable.Customer.User.Application.Mappers;
using Concertable.Customer.User.Contracts;
using Reunion;
using Concertable.Kernel.Geometry;
using Concertable.Kernel.Identity;
using Concertable.Kernel.Services.Geometry;
using Concertable.Shared.Geocoding.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.User.Infrastructure.Services;

internal sealed class UserService : IUserService
{
    private readonly IUserRepository userRepository;
    private readonly ICurrentUser currentUser;
    private readonly IGeocodingClient geocodingClient;
    private readonly IGeometryProvider geometryProvider;

    public UserService(
        IUserRepository userRepository,
        ICurrentUser currentUser,
        IGeocodingClient geocodingClient,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider)
    {
        this.userRepository = userRepository;
        this.currentUser = currentUser;
        this.geocodingClient = geocodingClient;
        this.geometryProvider = geometryProvider;
    }

    public async Task<CustomerDto> SaveLocationAsync(double latitude, double longitude)
    {
        var user = await userRepository.GetByIdAsync(currentUser.GetId())
            ?? throw new UnauthorizedAccessException("User not found.");

        var address = await geocodingClient.GetLocationAsync(latitude, longitude);
        user.UpdateLocation(
            geometryProvider.CreatePoint(latitude, longitude),
            address);

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        return user.ToDto();
    }

    public async Task<Option<CustomerDto>> GetMeAsync()
    {
        var user = await userRepository.GetByIdAsync(currentUser.GetId());
        return user.ToOption().Map(value => value.ToDto());
    }

    public async Task<IReadOnlyList<CustomerDto>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var users = await userRepository.GetByIdsAsync(ids);
        return users.Select(user => user.ToDto()).ToList();
    }
}
