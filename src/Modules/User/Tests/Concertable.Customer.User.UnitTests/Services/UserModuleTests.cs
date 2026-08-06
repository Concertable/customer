using Concertable.Customer.User.Application.Interfaces;
using Concertable.Customer.User.Domain.Entities;
using Concertable.Customer.User.Infrastructure;
using Moq;

namespace Concertable.Customer.User.UnitTests.Services;

public sealed class UserModuleTests
{
    private readonly Mock<IUserRepository> userRepository;
    private readonly UserModule sut;

    public UserModuleTests()
    {
        this.userRepository = new Mock<IUserRepository>();
        this.sut = new UserModule(userRepository.Object);
    }

    [Fact]
    public async Task GetByIdsAsync_ExistingUsers_ReturnsMaterializedList()
    {
        var user = UserEntity.FromRegistration(Guid.NewGuid(), "customer@test.com");
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
}
