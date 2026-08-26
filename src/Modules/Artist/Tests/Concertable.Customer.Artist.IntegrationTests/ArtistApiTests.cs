using System.Net;
using Concertable.Customer.Artist.Api.Responses;
using Shouldly;
using Xunit.Abstractions;

namespace Concertable.Customer.Artist.IntegrationTests;

[Collection("Integration")]
public sealed class ArtistApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ArtistApiTests(ApiFixture fixture, ITestOutputHelper output)
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
    public async Task GetDetailsById_SeededArtist_Returns200WithDetails()
    {
        var artist = fixture.SeedState.Artist;
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"/api/artist/{artist.Id}");

        await response.ShouldBe(HttpStatusCode.OK);
        var details = (await response.Content.ReadAsync<DetailsResponse>()).ShouldNotBeNull();
        details.Id.ShouldBe(artist.Id);
        details.Name.ShouldBe(artist.Name);
    }

    [Fact]
    public async Task GetDetailsById_MissingArtist_Returns404()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/artist/2147483647");

        await response.ShouldBe(HttpStatusCode.NotFound);
    }
}
