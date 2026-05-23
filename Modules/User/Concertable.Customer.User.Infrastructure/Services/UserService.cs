using Concertable.Customer.User.Contracts;
using Concertable.Kernel.Geometry;
using Concertable.Kernel.Identity;
using Concertable.Kernel.Services.Geometry;
using Concertable.Shared.Geocoding.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Customer.User.Infrastructure.Services;

internal class UserService : IUserService
{
    private readonly IUserRepository userRepository;
    private readonly ICurrentUser currentUser;
    private readonly IGeocodingService geocodingService;
    private readonly IGeometryProvider geometryProvider;

    public UserService(
        IUserRepository userRepository,
        ICurrentUser currentUser,
        IGeocodingService geocodingService,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider)
    {
        this.userRepository = userRepository;
        this.currentUser = currentUser;
        this.geocodingService = geocodingService;
        this.geometryProvider = geometryProvider;
    }

    public async Task<UserDto> SaveLocationAsync(double latitude, double longitude)
    {
        var user = await userRepository.GetByIdAsync(currentUser.GetId())
            ?? throw new UnauthorizedAccessException("User not found.");

        var locationDto = await geocodingService.GetLocationAsync(latitude, longitude);
        user.UpdateLocation(
            geometryProvider.CreatePoint(latitude, longitude),
            new Address(locationDto.County, locationDto.Town));

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task<UserDto?> GetMeAsync()
    {
        var user = await userRepository.GetByIdAsync(currentUser.GetId());
        return user is not null ? ToDto(user) : null;
    }

    private static UserDto ToDto(UserEntity user) => new(
        user.Id,
        user.Email,
        user.Location?.Y,
        user.Location?.X,
        user.Address?.County,
        user.Address?.Town);
}
