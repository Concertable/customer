using System.Net;
using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Customer.Ticket.Application.Responses;
using Xunit.Abstractions;

namespace Concertable.Customer.Ticket.IntegrationTests;

[Collection("Integration")]
public class TicketApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public TicketApiTests(ApiFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
        fixture.AttachOutput(output);
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() { fixture.DetachOutput(); return Task.CompletedTask; }

    #region Purchase

    [Fact]
    public async Task Purchase_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PostAsync("/api/ticket/purchase", new TicketPurchaseParams
        {
            PaymentMethodId = "pm_test",
            ConcertId = 1,
            Quantity = 1
        });

        // Assert
        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Purchase_ShouldReturn403_WhenUserNotInDatabase()
    {
        // Arrange - Customer header set but not seeded in UserDb
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.PostAsync("/api/ticket/purchase", new TicketPurchaseParams
        {
            PaymentMethodId = "pm_test",
            ConcertId = 1,
            Quantity = 1
        });

        // Assert
        await response.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Purchase_ShouldReturn200_WithPaymentResponse()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        var concert = await fixture.SeedConcertAsync(1);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.PostAsync("/api/ticket/purchase", new TicketPurchaseParams
        {
            PaymentMethodId = "pm_test",
            ConcertId = concert.Id,
            Quantity = 1
        });

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadAsync<TicketPaymentResponse>();
        Assert.NotNull(result);
        Assert.Equal("pi_mock_pay", result.TransactionId);
        Assert.False(result.RequiresAction);
    }

    #endregion

    #region Checkout

    [Fact]
    public async Task Checkout_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PostAsync("/api/ticket/checkout", new TicketCheckoutRequest(1, 1));

        // Assert
        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Checkout_ShouldReturn403_WhenUserNotInDatabase()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.PostAsync("/api/ticket/checkout", new TicketCheckoutRequest(1, 1));

        // Assert
        await response.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Checkout_ShouldReturn200_WithCheckoutSession()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        var concert = await fixture.SeedConcertAsync(1);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.PostAsync("/api/ticket/checkout", new TicketCheckoutRequest(concert.Id, 1));

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadAsync<TicketCheckout>();
        Assert.NotNull(result);
        Assert.Equal(concert.Id, result.ConcertId);
        Assert.Equal(1, result.Quantity);
        Assert.Equal("pi_mock_secret", result.Session.ClientSecret);
    }

    #endregion

    #region GetUserUpcoming

    [Fact]
    public async Task GetUserUpcoming_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/ticket/upcoming/user");

        // Assert
        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserUpcoming_ShouldReturn200_WithUpcomingTickets()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        await fixture.SeedTicketAsync(fixture.Customer.Id, 1, upcoming: true);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.GetAsync("/api/ticket/upcoming/user");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = await response.Content.ReadAsync<IEnumerable<TicketDto>>();
        Assert.NotNull(tickets);
        Assert.Single(tickets);
    }

    [Fact]
    public async Task GetUserUpcoming_ShouldNotReturnPastTickets()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        await fixture.SeedTicketAsync(fixture.Customer.Id, 1, upcoming: false);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.GetAsync("/api/ticket/upcoming/user");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = await response.Content.ReadAsync<IEnumerable<TicketDto>>();
        Assert.NotNull(tickets);
        Assert.Empty(tickets);
    }

    #endregion

    #region GetUserHistory

    [Fact]
    public async Task GetUserHistory_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/ticket/history/user");

        // Assert
        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserHistory_ShouldReturn200_WithPastTickets()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        await fixture.SeedTicketAsync(fixture.Customer.Id, 1, upcoming: false);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.GetAsync("/api/ticket/history/user");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = await response.Content.ReadAsync<IEnumerable<TicketDto>>();
        Assert.NotNull(tickets);
        Assert.Single(tickets);
    }

    [Fact]
    public async Task GetUserHistory_ShouldNotReturnUpcomingTickets()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        await fixture.SeedTicketAsync(fixture.Customer.Id, 1, upcoming: true);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.GetAsync("/api/ticket/history/user");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = await response.Content.ReadAsync<IEnumerable<TicketDto>>();
        Assert.NotNull(tickets);
        Assert.Empty(tickets);
    }

    #endregion

    #region CanPurchase

    [Fact]
    public async Task CanPurchase_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/ticket/concert/1/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CanPurchase_ShouldReturn200True_WhenConcertIsAvailable()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        var concert = await fixture.SeedConcertAsync(1, posted: true, availableTickets: 50);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.GetAsync($"/api/ticket/concert/{concert.Id}/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.True(await response.Content.ReadAsync<bool>());
    }

    [Fact]
    public async Task CanPurchase_ShouldReturn200False_WhenConcertIsSoldOut()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        var concert = await fixture.SeedConcertAsync(1, posted: true, availableTickets: 0);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.GetAsync($"/api/ticket/concert/{concert.Id}/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.False(await response.Content.ReadAsync<bool>());
    }

    [Fact]
    public async Task CanPurchase_ShouldReturn200False_WhenConcertIsNotPosted()
    {
        // Arrange
        await fixture.SeedUserAsync(fixture.Customer);
        var concert = await fixture.SeedConcertAsync(1, posted: false, availableTickets: 50);
        var client = fixture.CreateClient(fixture.Customer);

        // Act
        var response = await client.GetAsync($"/api/ticket/concert/{concert.Id}/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.False(await response.Content.ReadAsync<bool>());
    }

    #endregion
}
