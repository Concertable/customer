using Concertable.Customer.User.Application.Interfaces;
using Concertable.Customer.User.Domain.Entities;
using Concertable.Customer.User.Infrastructure.Services;
using Concertable.Kernel.Geometry;
using Concertable.Kernel.Identity;
using Concertable.Kernel.ValueObjects;
using Concertable.Shared.Geocoding.Application;
using Moq;
using NetTopologySuite.Geometries;

namespace Concertable.Customer.User.UnitTests.Services;

public sealed class UserServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly Mock<IUserRepository> userRepository;
    private readonly Mock<ICurrentUser> currentUser;
    private readonly Mock<IGeocodingClient> geocodingClient;
    private readonly Mock<IGeometryProvider> geometryProvider;
    private readonly UserService sut;

    public UserServiceTests()
    {
        this.userRepository = new Mock<IUserRepository>();
        this.currentUser = new Mock<ICurrentUser>();
        this.geocodingClient = new Mock<IGeocodingClient>();
        this.geometryProvider = new Mock<IGeometryProvider>();
        this.currentUser.SetupGet(user => user.Id).Returns(UserId);
        this.sut = new UserService(
            userRepository.Object,
            currentUser.Object,
            geocodingClient.Object,
            geometryProvider.Object);
    }

    #region GetMeAsync

    [Fact]
    public async Task GetMeAsync_ExistingUser_ReturnsSome()
    {
        this.userRepository
            .Setup(repository => repository.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewUser());

        var result = await this.sut.GetMeAsync();

        Assert.True(result.TryGetValue(out var user));
        Assert.Equal(UserId, user.Id);
        Assert.Equal("customer@test.com", user.Email);
    }

    [Fact]
    public async Task GetMeAsync_MissingUser_ReturnsNone()
    {
        this.userRepository
            .Setup(repository => repository.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var result = await this.sut.GetMeAsync();

        Assert.True(result.IsNone);
    }

    #endregion

    #region SaveLocationAsync

    [Fact]
    public async Task SaveLocationAsync_ExistingUser_UpdatesAndReturnsProfile()
    {
        var user = NewUser();
        var address = new Address("Test County", "Test Town");
        var location = new Point(-0.1278, 51.5074) { SRID = 4326 };
        this.userRepository
            .Setup(repository => repository.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        this.geocodingClient
            .Setup(client => client.GetLocationAsync(51.5074, -0.1278))
            .ReturnsAsync(address);
        this.geometryProvider
            .Setup(provider => provider.CreatePoint(51.5074, -0.1278))
            .Returns(location);

        var result = await this.sut.SaveLocationAsync(51.5074, -0.1278);

        Assert.Equal(UserId, result.Id);
        Assert.Equal(51.5074, result.Latitude);
        Assert.Equal(-0.1278, result.Longitude);
        Assert.Equal("Test County", result.County);
        Assert.Equal("Test Town", result.Town);
        this.userRepository.Verify(repository => repository.Update(user), Times.Once);
        this.userRepository.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveLocationAsync_MissingUser_ThrowsInvariantException()
    {
        this.userRepository
            .Setup(repository => repository.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => this.sut.SaveLocationAsync(51.5074, -0.1278));

        this.geocodingClient.Verify(
            client => client.GetLocationAsync(It.IsAny<double>(), It.IsAny<double>()),
            Times.Never);
    }

    #endregion

    #region GetByIdsAsync

    [Fact]
    public async Task GetByIdsAsync_ExistingUsers_ReturnsMaterializedDtos()
    {
        var user = NewUser();
        var source = new List<UserEntity> { user };
        this.userRepository
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(source);

        var result = await this.sut.GetByIdsAsync([user.Id]);
        source.Add(UserEntity.FromRegistration(Guid.NewGuid(), "other@test.com"));

        var customer = Assert.Single(result);
        Assert.Equal(user.Id, customer.Id);
        Assert.Equal(user.Email, customer.Email);
    }

    [Fact]
    public async Task GetByIdsAsync_MissingUsers_ReturnsEmptyList()
    {
        this.userRepository
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync([]);

        var result = await this.sut.GetByIdsAsync([Guid.NewGuid()]);

        Assert.Empty(result);
    }

    #endregion

    private static UserEntity NewUser() =>
        UserEntity.FromRegistration(UserId, "customer@test.com");
}
