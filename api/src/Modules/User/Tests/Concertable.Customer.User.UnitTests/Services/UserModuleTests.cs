using Concertable.Customer.User.Application.Interfaces;
using Concertable.Customer.User.Contracts;
using Concertable.Customer.User.Infrastructure;
using Moq;

namespace Concertable.Customer.User.UnitTests.Services;

public sealed class UserModuleTests
{
    private readonly Mock<IUserService> userService;
    private readonly UserModule sut;

    public UserModuleTests()
    {
        this.userService = new Mock<IUserService>();
        this.sut = new UserModule(userService.Object);
    }

    [Fact]
    public async Task GetByIdsAsync_ForwardsIdsAndResult()
    {
        Guid[] ids = [Guid.NewGuid()];
        IReadOnlyList<CustomerDto> expected =
        [
            new CustomerDto { Id = ids[0], Email = "customer@test.com" }
        ];
        this.userService
            .Setup(service => service.GetByIdsAsync(ids))
            .ReturnsAsync(expected);

        var result = await this.sut.GetByIdsAsync(ids);

        Assert.Same(expected, result);
        this.userService.Verify(service => service.GetByIdsAsync(ids), Times.Once);
    }
}
