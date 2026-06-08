using Concertable.Customer.User.Application.Mappers;
using Concertable.Customer.User.Contracts;
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

    public async Task<CustomerDto> SaveLocationAsync(double latitude, double longitude)
    {
        var user = await userRepository.GetByIdAsync(currentUser.GetId())
            ?? throw new UnauthorizedAccessException("User not found.");

        var locationDto = await geocodingService.GetLocationAsync(latitude, longitude);
        user.UpdateLocation(
            geometryProvider.CreatePoint(latitude, longitude),
            new Address(locationDto.County, locationDto.Town));

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        return user.ToDto();
    }

    public async Task<CustomerDto?> GetMeAsync()
    {
        var user = await userRepository.GetByIdAsync(currentUser.GetId());
        return user?.ToDto();
    }
}
