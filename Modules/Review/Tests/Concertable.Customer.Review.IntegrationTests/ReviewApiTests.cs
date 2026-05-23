using System.Net;
using Concertable.Contracts;
using Concertable.Customer.Review.Application.Requests;

namespace Concertable.Customer.Review.IntegrationTests;

[Collection("Integration")]
public class ReviewApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ReviewApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    #region GetConcertReviews

    [Fact]
    public async Task GetConcertReviews_ShouldReturn200_WithEmptyList_WhenNoReviews()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/concerts/1/reviews");

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
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/concerts/1/reviews/summary");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var summary = await response.Content.ReadAsync<ReviewSummaryDto>();
        Assert.NotNull(summary);
        Assert.Equal(0, summary.TotalReviews);
        Assert.Null(summary.AverageRating);
    }

    #endregion

    #region GetConcertReviewEligibility

    [Fact]
    public async Task GetConcertReviewEligibility_ShouldReturn200False_WhenUserHasNoTicket()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.GetAsync("/api/concerts/1/reviews/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.False(await response.Content.ReadAsync<bool>());
    }

    [Fact]
    public async Task GetConcertReviewEligibility_ShouldReturn200False_WhenConcertHasNotHappenedYet()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        await fixture.SeedTicketAsync(fixture.Customer.Id, 1, upcoming: true);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.GetAsync("/api/concerts/1/reviews/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.False(await response.Content.ReadAsync<bool>());
    }

    [Fact]
    public async Task GetConcertReviewEligibility_ShouldReturn200True_WhenConcertPassedAndNoReviewYet()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        await fixture.SeedTicketAsync(fixture.Customer.Id, 1, upcoming: false);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.GetAsync("/api/concerts/1/reviews/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.True(await response.Content.ReadAsync<bool>());
    }

    #endregion

    #region CreateConcertReview

    [Fact]
    public async Task CreateConcertReview_ShouldReturn404_WhenUserHasNoTicket()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.PostAsync("/api/concerts/1/reviews", new CreateReviewRequest
        {
            Stars = 4,
            Details = "Great concert"
        });

        // Assert
        await response.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateConcertReview_ShouldReturn201_WithReviewDetails()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        await fixture.SeedTicketAsync(fixture.Customer.Id, 1, upcoming: false);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.PostAsync("/api/concerts/1/reviews", new CreateReviewRequest
        {
            Stars = 4,
            Details = "Great concert"
        });

        // Assert
        await response.ShouldBe(HttpStatusCode.Created);
        var review = await response.Content.ReadAsync<ReviewDto>();
        Assert.NotNull(review);
        Assert.Equal(4, review.Stars);
        Assert.Equal("Great concert", review.Details);
    }

    #endregion
}
