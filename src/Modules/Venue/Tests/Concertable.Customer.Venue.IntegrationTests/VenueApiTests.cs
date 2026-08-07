using System.Net;
using Concertable.Customer.Venue.Api.Responses;
using Xunit.Abstractions;

namespace Concertable.Customer.Venue.IntegrationTests;

[Collection("Integration")]
public sealed class VenueApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public VenueApiTests(ApiFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        fixture.AttachOutput(output);
    }

    public Task InitializeAsync() => fixture.ResetAsync();

    public Task DisposeAsync()
    {
        fixture.DetachOutput();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetDetailsById_SeededVenue_Returns200WithDetails()
    {
        var venue = fixture.SeedState.Venue;
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"/api/venue/{venue.Id}");

        await response.ShouldBe(HttpStatusCode.OK);
        var details = await response.Content.ReadAsync<DetailsResponse>();
        Assert.NotNull(details);
        Assert.Equal(venue.Id, details.Id);
        Assert.Equal(venue.Name, details.Name);
    }

    [Fact]
    public async Task GetDetailsById_MissingVenue_Returns404()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/venue/2147483647");

        await response.ShouldBe(HttpStatusCode.NotFound);
    }
}
