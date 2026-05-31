using System.Net;
using Concertable.Customer.User.Application.Requests;
using Concertable.Customer.User.Contracts;
using Xunit.Abstractions;

namespace Concertable.Customer.User.IntegrationTests;

[Collection("Integration")]
public class UserApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public UserApiTests(ApiFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        fixture.AttachOutput(output);
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() { fixture.DetachOutput(); return Task.CompletedTask; }

    #region Me

    [Fact]
    public async Task Me_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/user/me");

        // Assert
        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_ShouldReturn403_WhenUserNotInDatabase()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.GetAsync("/api/user/me");

        // Assert
        await response.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Me_ShouldReturn200_WithUserDetails()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.GetAsync("/api/user/me");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var user = await response.Content.ReadAsync<CustomerDto>();
        Assert.NotNull(user);
        Assert.Equal(fixture.Customer.Id, user.Id);
        Assert.Equal(fixture.Customer.Email, user.Email);
    }

    #endregion

    #region UpdateLocation

    [Fact]
    public async Task UpdateLocation_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PutAsync("/api/user/location", new UpdateLocationRequest(51.5, -0.1));

        // Assert
        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateLocation_ShouldReturn403_WhenUserNotInDatabase()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.PutAsync("/api/user/location", new UpdateLocationRequest(51.5, -0.1));

        // Assert
        await response.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateLocation_ShouldReturn200_WithUpdatedCoordinates()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.PutAsync("/api/user/location", new UpdateLocationRequest(51.5074, -0.1278));

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var user = await response.Content.ReadAsync<CustomerDto>();
        Assert.NotNull(user);
        Assert.Equal(51.5074, user.Latitude);
        Assert.Equal(-0.1278, user.Longitude);
    }

    #endregion
}
