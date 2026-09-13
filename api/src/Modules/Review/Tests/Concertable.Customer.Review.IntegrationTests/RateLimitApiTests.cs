using System.Net;
using Concertable.Customer.Review.Application.Requests;
using Shouldly;
using Xunit.Abstractions;

namespace Concertable.Customer.Review.IntegrationTests;

public sealed class RateLimitApiTests : IClassFixture<RateLimitApiFixture>, IAsyncLifetime
{
    private readonly RateLimitApiFixture fixture;

    public RateLimitApiTests(RateLimitApiFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        fixture.AttachOutput(output);
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() { fixture.DetachOutput(); return Task.CompletedTask; }

    #region PublicRead (per-IP)

    [Fact]
    public async Task PublicRead_PerIp_Returns429WithRetryAfter_OncePermitLimitExceeded()
    {
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var client = fixture.CreateClient();
        var url = $"/api/concerts/{concert.Id}/reviews";

        for (var i = 0; i < RateLimitApiFixture.PermitLimit; i++)
            (await client.GetAsync(url)).StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);

        var limited = await client.GetAsync(url);

        limited.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        limited.Headers.RetryAfter.ShouldNotBeNull();
    }

    #endregion

    #region ReviewWrite (per-user)

    [Fact]
    public async Task ReviewWrite_PerUser_Returns429WithRetryAfter_OncePermitLimitExceeded()
    {
        var concert = fixture.SeedState.PastFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);
        var url = $"/api/concerts/{concert.Id}/reviews";
        var request = new CreateReviewRequest { Stars = 4, Details = "Great concert" };

        for (var i = 0; i < RateLimitApiFixture.PermitLimit; i++)
            (await client.PostAsync(url, request)).StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);

        var limited = await client.PostAsync(url, request);

        limited.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        limited.Headers.RetryAfter.ShouldNotBeNull();
    }

    #endregion
}
