using System.Net;
using System.Text.Json;
using Concertable.Contracts;
using Concertable.Customer.Review.Application.Requests;
using Shouldly;
using Xunit.Abstractions;

namespace Concertable.Customer.Review.IntegrationTests;

[Collection("Integration")]
public sealed class ReviewApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ReviewApiTests(ApiFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        fixture.AttachOutput(output);
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() { fixture.DetachOutput(); return Task.CompletedTask; }

    #region GetConcertReviews

    [Fact]
    public async Task GetConcertReviews_ShouldReturn200_WithEmptyList_WhenNoReviews()
    {
        // Arrange
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/concerts/{concert.Id}/reviews");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadAsync<Pagination<ReviewDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalCount);
    }

    #endregion

    #region GetConcertReviewSummary

    [Fact]
    public async Task GetConcertReviewSummary_ShouldReturn200_WithNoReviews()
    {
        // Arrange
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/concerts/{concert.Id}/reviews/summary");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var summary = await response.Content.ReadAsync<ReviewSummary>();
        Assert.NotNull(summary);
        Assert.Equal(0, summary.TotalReviews);
        Assert.Null(summary.AverageRating);
    }

    #endregion

    #region GetConcertReviewEligibility

    [Fact]
    public async Task GetConcertReviewEligibility_ShouldReturn200False_WhenUserHasNoTicket()
    {
        // Arrange - Customer2 holds no tickets
        var concert = fixture.SeedState.PastFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer2);

        // Act
        var response = await client.GetAsync($"/api/concerts/{concert.Id}/reviews/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.False(await response.Content.ReadAsync<bool>());
    }

    [Fact]
    public async Task GetConcertReviewEligibility_ShouldReturn200False_WhenUnauthenticated()
    {
        // Arrange - no bearer token (a manager-app or anonymous caller)
        var concert = fixture.SeedState.PastFlatFeeConcert;
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/concerts/{concert.Id}/reviews/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.False(await response.Content.ReadAsync<bool>());
    }

    [Fact]
    public async Task GetConcertReviewEligibility_ShouldReturn200False_WhenConcertHasNotHappenedYet()
    {
        // Arrange - Customer1 holds an upcoming ticket for this concert
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        // Act
        var response = await client.GetAsync($"/api/concerts/{concert.Id}/reviews/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.False(await response.Content.ReadAsync<bool>());
    }

    [Fact]
    public async Task GetConcertReviewEligibility_ShouldReturn200True_WhenConcertPassedAndNoReviewYet()
    {
        // Arrange - Customer1 holds a past, unreviewed ticket for this concert
        var concert = fixture.SeedState.PastFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        // Act
        var response = await client.GetAsync($"/api/concerts/{concert.Id}/reviews/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.True(await response.Content.ReadAsync<bool>());
    }

    #endregion

    #region CreateConcertReview

    [Fact]
    public async Task CreateConcertReview_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange - no bearer token; the write path must reject at the auth boundary, not reach the service
        var concert = fixture.SeedState.PastFlatFeeConcert;
        var client = fixture.CreateClient();

        // Act
        var response = await client.PostAsync($"/api/concerts/{concert.Id}/reviews", new CreateReviewRequest
        {
            Stars = 4,
            Details = "Great concert"
        });

        // Assert
        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateConcertReview_ShouldReturn404_WhenUserHasNoTicket()
    {
        // Arrange - Customer2 holds no tickets
        var concert = fixture.SeedState.PastFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer2);

        // Act
        var response = await client.PostAsync($"/api/concerts/{concert.Id}/reviews", new CreateReviewRequest
        {
            Stars = 4,
            Details = "Great concert"
        });

        // Assert
        await response.ShouldBe(HttpStatusCode.NotFound);
        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.NotFound,
            "Not Found",
            "Ticket not found.",
            "review.ticket_not_found");
    }

    [Fact]
    public async Task CreateConcertReview_ShouldReturn409_WhenConcertHasNotHappenedYet()
    {
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.PostAsync($"/api/concerts/{concert.Id}/reviews", new CreateReviewRequest
        {
            Stars = 4,
            Details = "Great concert"
        });

        await response.ShouldBe(HttpStatusCode.Conflict);
        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.Conflict,
            "Conflict",
            "The concert is not reviewable yet.",
            "review.concert_not_reviewable_yet");
    }

    [Fact]
    public async Task CreateConcertReview_ShouldReturn409_WhenTicketAlreadyReviewed()
    {
        var concert = fixture.SeedState.PastFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);
        var request = new CreateReviewRequest
        {
            Stars = 4,
            Details = "Great concert"
        };
        var created = await client.PostAsync($"/api/concerts/{concert.Id}/reviews", request);
        await created.ShouldBe(HttpStatusCode.Created);

        var response = await client.PostAsync($"/api/concerts/{concert.Id}/reviews", request);

        await response.ShouldBe(HttpStatusCode.Conflict);
        await AssertProblemDetailsAsync(
            response,
            HttpStatusCode.Conflict,
            "Conflict",
            "A review already exists for this ticket.",
            "review.already_exists");
    }

    [Fact]
    public async Task CreateConcertReview_ShouldMakeTicketIneligibleForAnotherReview()
    {
        var concert = fixture.SeedState.PastFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);
        var created = await client.PostAsync($"/api/concerts/{concert.Id}/reviews", new CreateReviewRequest
        {
            Stars = 4,
            Details = "Great concert"
        });
        await created.ShouldBe(HttpStatusCode.Created);

        var response = await client.GetAsync($"/api/concerts/{concert.Id}/reviews/eligibility");

        await response.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsync<bool>()).ShouldBeFalse();
    }

    [Fact]
    public async Task CreateConcertReview_ShouldReturn201_WithReviewDetails()
    {
        // Arrange - Customer1 holds a past, unreviewed ticket for this concert
        var concert = fixture.SeedState.PastFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        // Act
        var response = await client.PostAsync($"/api/concerts/{concert.Id}/reviews", new CreateReviewRequest
        {
            Stars = 4,
            Details = "Great concert"
        });

        // Assert
        await response.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldBe(new Uri($"http://localhost/api/concerts/{concert.Id}/reviews"));
        var review = await response.Content.ReadAsync<ReviewDto>();
        Assert.NotNull(review);
        Assert.Equal(4, review.Stars);
        Assert.Equal("Great concert", review.Details);
        Assert.Equal($"/api/concerts/{concert.Id}/reviews", response.Headers.Location?.OriginalString);
    }

    #endregion

    private static async Task AssertProblemDetailsAsync(
        HttpResponseMessage response,
        HttpStatusCode status,
        string title,
        string detail,
        string code)
    {
        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        var problem = document.RootElement;

        problem.GetProperty("status").GetInt32().ShouldBe((int)status);
        problem.GetProperty("title").GetString().ShouldBe(title);
        problem.GetProperty("detail").GetString().ShouldBe(detail);
        problem.GetProperty("code").GetString().ShouldBe(code);
    }
}
