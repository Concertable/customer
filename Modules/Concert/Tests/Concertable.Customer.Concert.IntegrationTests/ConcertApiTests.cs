using System.Net;
using Concertable.Customer.Concert.Application.Dtos;
using Xunit.Abstractions;

namespace Concertable.Customer.Concert.IntegrationTests;

[Collection("Integration")]
public sealed class ConcertApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ConcertApiTests(ApiFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        fixture.AttachOutput(output);
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() { fixture.DetachOutput(); return Task.CompletedTask; }

    #region GetById

    [Fact]
    public async Task GetById_ShouldReturn200_WithConcertDetails()
    {
        // Arrange
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/concert/{concert.Id}");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadAsync<ConcertDetailDto>();
        Assert.NotNull(dto);
        Assert.Equal(concert.Id, dto.Id);
        Assert.Equal(concert.Name, dto.Name);
        Assert.Equal(concert.VenueName, dto.Venue.Name);
        Assert.Equal(concert.ArtistName, dto.Artist.Name);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenConcertDoesNotExist()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/concert/99999");

        // Assert
        await response.ShouldBe(HttpStatusCode.NotFound);
    }

    #endregion
}
