using System.Net;
using Concertable.Customer.Concert.Application.Dtos;
using Xunit.Abstractions;

namespace Concertable.Customer.Concert.IntegrationTests;

[Collection("Integration")]
public class ConcertApiTests : IAsyncLifetime
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
        await fixture.SeedVenueAsync(1);
        await fixture.SeedArtistAsync(1);
        var concert = await fixture.SeedConcertAsync(1);
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/concert/{concert.Id}");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadAsync<ConcertDetailDto>();
        Assert.NotNull(dto);
        Assert.Equal(concert.Id, dto.Id);
        Assert.Equal("Test Concert", dto.Name);
        Assert.Equal("Test Venue", dto.Venue.Name);
        Assert.Equal("Test Artist", dto.Artist.Name);
    }

    [Fact]
    public async Task GetById_ShouldReturn200_WhenVenueAndArtistReadModelsAreMissing()
    {
        // Arrange - concert seeded without venue/artist read models; service falls back to defaults
        var concert = await fixture.SeedConcertAsync(1);
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/concert/{concert.Id}");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadAsync<ConcertDetailDto>();
        Assert.NotNull(dto);
        Assert.Equal(concert.Id, dto.Id);
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
