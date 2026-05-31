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
            ConcertId = fixture.SeedState.UpcomingFlatFeeConcert.Id,
            Quantity = 1
        });

        // Assert
        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Purchase_ShouldReturn403_WhenUserNotInDatabase()
    {
        // Arrange - authenticated as a user that was never seeded
        var client = fixture.CreateClient(Guid.NewGuid());

        // Act
        var response = await client.PostAsync("/api/ticket/purchase", new TicketPurchaseParams
        {
            PaymentMethodId = "pm_test",
            ConcertId = fixture.SeedState.UpcomingFlatFeeConcert.Id,
            Quantity = 1
        });

        // Assert
        await response.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Purchase_ShouldReturn200_WithPaymentResponse()
    {
        // Arrange
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

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
        var response = await client.PostAsync("/api/ticket/checkout",
            new TicketCheckoutRequest(fixture.SeedState.UpcomingFlatFeeConcert.Id, 1));

        // Assert
        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Checkout_ShouldReturn403_WhenUserNotInDatabase()
    {
        // Arrange
        var client = fixture.CreateClient(Guid.NewGuid());

        // Act
        var response = await client.PostAsync("/api/ticket/checkout",
            new TicketCheckoutRequest(fixture.SeedState.UpcomingFlatFeeConcert.Id, 1));

        // Assert
        await response.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Checkout_ShouldReturn200_WithCheckoutSession()
    {
        // Arrange
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

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
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        // Act
        var response = await client.GetAsync("/api/ticket/upcoming/user");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = await response.Content.ReadAsync<IEnumerable<TicketDto>>();
        Assert.NotNull(tickets);
        var ticket = Assert.Single(tickets);
        Assert.Equal(fixture.SeedState.UpcomingFlatFeeConcert.Id, ticket.Concert.Id);
    }

    [Fact]
    public async Task GetUserUpcoming_ShouldNotReturnPastTickets()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        // Act
        var response = await client.GetAsync("/api/ticket/upcoming/user");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = (await response.Content.ReadAsync<IEnumerable<TicketDto>>())?.ToList();
        Assert.NotNull(tickets);
        Assert.DoesNotContain(tickets, t => t.Concert.Id == fixture.SeedState.PastDoorSplitConcert.Id);
        Assert.DoesNotContain(tickets, t => t.Concert.Id == fixture.SeedState.PastFlatFeeConcert.Id);
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
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        // Act
        var response = await client.GetAsync("/api/ticket/history/user");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = (await response.Content.ReadAsync<IEnumerable<TicketDto>>())?.ToList();
        Assert.NotNull(tickets);
        Assert.Contains(tickets, t => t.Concert.Id == fixture.SeedState.PastDoorSplitConcert.Id);
        Assert.Contains(tickets, t => t.Concert.Id == fixture.SeedState.PastFlatFeeConcert.Id);
    }

    [Fact]
    public async Task GetUserHistory_ShouldNotReturnUpcomingTickets()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        // Act
        var response = await client.GetAsync("/api/ticket/history/user");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = await response.Content.ReadAsync<IEnumerable<TicketDto>>();
        Assert.NotNull(tickets);
        Assert.DoesNotContain(tickets, t => t.Concert.Id == fixture.SeedState.UpcomingFlatFeeConcert.Id);
    }

    #endregion

    #region CanPurchase

    [Fact]
    public async Task CanPurchase_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/ticket/concert/{fixture.SeedState.UpcomingFlatFeeConcert.Id}/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CanPurchase_ShouldReturn200True_WhenConcertIsAvailable()
    {
        // Arrange
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        // Act
        var response = await client.GetAsync($"/api/ticket/concert/{concert.Id}/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.True(await response.Content.ReadAsync<bool>());
    }

    [Fact]
    public async Task CanPurchase_ShouldReturn200False_WhenConcertHasPassed()
    {
        // Arrange
        var concert = fixture.SeedState.PastDoorSplitConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        // Act
        var response = await client.GetAsync($"/api/ticket/concert/{concert.Id}/eligibility");

        // Assert
        await response.ShouldBe(HttpStatusCode.OK);
        Assert.False(await response.Content.ReadAsync<bool>());
    }

    #endregion
}
