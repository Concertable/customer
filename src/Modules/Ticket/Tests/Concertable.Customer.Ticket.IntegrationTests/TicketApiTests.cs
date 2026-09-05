using System.Net;
using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Payment.Contracts;
using Concertable.Payment.Contracts.Errors;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace Concertable.Customer.Ticket.IntegrationTests;

[Collection("Integration")]
public sealed class TicketApiTests : IAsyncLifetime
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
    public async Task Purchase_Unauthenticated_ReturnsUnauthorized()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsync("/api/ticket/purchase", new TicketPurchaseParams
        {
            ConcertId = fixture.SeedState.UpcomingFlatFeeConcert.Id,
            Quantity = 1
        });

        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Purchase_UnregisteredUser_ReturnsForbidden()
    {
        var client = fixture.CreateClient(Guid.NewGuid());

        var response = await client.PostAsync("/api/ticket/purchase", new TicketPurchaseParams
        {
            ConcertId = fixture.SeedState.UpcomingFlatFeeConcert.Id,
            Quantity = 1
        });

        await response.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Purchase_AvailableConcert_ReturnsPaymentSessionReference()
    {
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var buyer = fixture.SeedState.Customer1;
        var client = fixture.CreateClient(buyer);

        var response = await client.PostAsync("/api/ticket/purchase", new TicketPurchaseParams
        {
            ConcertId = concert.Id,
            Quantity = 1
        });

        await response.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadAsync<TicketPayment>();
        Assert.NotNull(result);
        Assert.Equal("ticket-purchase", result.Reference.OperationType);
        Assert.Equal("payment-session-secret", result.ClientSecret);
        Assert.Equal(concert.Id, result.ConcertId);
        Assert.Equal(concert.Price, result.Amount);
        Assert.Equal("GBP", result.Currency);

        var request = Assert.Single(fixture.PaymentSessionClient.Sessions);
        Assert.Equal(result.Reference, request.Reference);
        Assert.Equal(PaymentSessionKind.Payment, request.Kind);
        Assert.Equal(PaymentSession.OnSession, request.Session);
        Assert.Equal(buyer.Id, request.PayerOwnerId);
        Assert.Equal(concert.PayeeOwnerId, request.PayeeOwnerId);
        Assert.Equal(
            Concertable.Kernel.ValueObjects.Money.Gbp(concert.Price).ToMinorUnits(),
            request.AmountMinor);
        Assert.Equal(Concertable.Kernel.ValueObjects.Currency.Gbp, request.Currency);
        Assert.Equal(PaymentSessionFundsRouting.Destination, request.FundsRouting);
    }

    [Fact]
    public async Task Purchase_MissingConcert_ReturnsNotFoundProblem()
    {
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.PostAsync("/api/ticket/purchase", new TicketPurchaseParams
        {
            ConcertId = int.MaxValue,
            Quantity = 1
        });

        await AssertProblemCodeAsync(response, HttpStatusCode.NotFound, "ticket.concert_not_found");
    }

    [Fact]
    public async Task Purchase_InsufficientAvailability_ReturnsValidationProblem()
    {
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.PostAsync("/api/ticket/purchase", new TicketPurchaseParams
        {
            ConcertId = concert.Id,
            Quantity = concert.AvailableTickets + 1
        });

        var problem = await AssertValidationProblemAsync(
            response,
            "ticket.purchase_invalid",
            "purchase");
        Assert.StartsWith("Not enough tickets available.", Assert.Single(problem.Errors["purchase"]));
    }

    [Fact]
    public async Task Purchase_DeclinedPaymentSession_ReturnsPaymentProblem()
    {
        fixture.PaymentSessionClient.CreateError = new PaymentOperationError.Declined();
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.PostAsync("/api/ticket/purchase", new TicketPurchaseParams
        {
            ConcertId = fixture.SeedState.UpcomingFlatFeeConcert.Id,
            Quantity = 1
        });

        await AssertProblemCodeAsync(response, HttpStatusCode.PaymentRequired, "payment.operation.declined");
    }

    #endregion

    #region Checkout

    [Fact]
    public async Task Checkout_Unauthenticated_ReturnsUnauthorized()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsync("/api/ticket/checkout",
            new TicketCheckoutRequest(fixture.SeedState.UpcomingFlatFeeConcert.Id, 1));

        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Checkout_UnregisteredUser_ReturnsForbidden()
    {
        var client = fixture.CreateClient(Guid.NewGuid());

        var response = await client.PostAsync("/api/ticket/checkout",
            new TicketCheckoutRequest(fixture.SeedState.UpcomingFlatFeeConcert.Id, 1));

        await response.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Checkout_AvailableConcert_ReturnsPaymentSessionReference()
    {
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var buyer = fixture.SeedState.Customer1;
        var client = fixture.CreateClient(buyer);

        var response = await client.PostAsync("/api/ticket/checkout", new TicketCheckoutRequest(concert.Id, 1));

        await response.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadAsync<TicketCheckout>();
        Assert.NotNull(result);
        Assert.Equal(concert.Id, result.ConcertId);
        Assert.Equal(1, result.Quantity);
        Assert.Equal("ticket-purchase", result.Reference.OperationType);
        Assert.Equal("payment-session-secret", result.Session.ClientSecret);
        Assert.Equal("customer-session-secret", result.Session.CustomerSession);
        Assert.Equal("customer-token", result.Session.CustomerId);

        var request = Assert.Single(fixture.PaymentSessionClient.Sessions);
        Assert.Equal(result.Reference, request.Reference);
        Assert.Equal(PaymentSessionKind.Payment, request.Kind);
        Assert.Equal(PaymentSession.OnSession, request.Session);
        Assert.Equal(buyer.Id, request.PayerOwnerId);
        Assert.Equal(concert.PayeeOwnerId, request.PayeeOwnerId);
        Assert.Equal(
            Concertable.Kernel.ValueObjects.Money.Gbp(concert.Price).ToMinorUnits(),
            request.AmountMinor);
        Assert.Equal(Concertable.Kernel.ValueObjects.Currency.Gbp, request.Currency);
        Assert.Equal(PaymentSessionFundsRouting.Destination, request.FundsRouting);
    }

    [Fact]
    public async Task Checkout_MissingConcert_ReturnsNotFoundProblem()
    {
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.PostAsync(
            "/api/ticket/checkout",
            new TicketCheckoutRequest(int.MaxValue, 1));

        await AssertProblemCodeAsync(response, HttpStatusCode.NotFound, "ticket.concert_not_found");
    }

    [Fact]
    public async Task Checkout_InsufficientAvailability_ReturnsValidationProblem()
    {
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.PostAsync(
            "/api/ticket/checkout",
            new TicketCheckoutRequest(concert.Id, concert.AvailableTickets + 1));

        var problem = await AssertValidationProblemAsync(
            response,
            "ticket.checkout_invalid",
            "checkout");
        Assert.StartsWith("Not enough tickets available.", Assert.Single(problem.Errors["checkout"]));
    }

    #endregion

    #region GetUserUpcoming

    [Fact]
    public async Task GetUserUpcoming_Unauthenticated_ReturnsUnauthorized()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/ticket/upcoming/user");

        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserUpcoming_UpcomingTicket_ReturnsTicket()
    {
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.GetAsync("/api/ticket/upcoming/user");

        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = await response.Content.ReadAsync<IEnumerable<TicketDto>>();
        Assert.NotNull(tickets);
        var ticket = Assert.Single(tickets);
        Assert.Equal(fixture.SeedState.UpcomingFlatFeeConcert.Id, ticket.Concert.Id);
    }

    [Fact]
    public async Task GetUserUpcoming_PastTickets_OmitsTickets()
    {
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.GetAsync("/api/ticket/upcoming/user");

        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = (await response.Content.ReadAsync<IEnumerable<TicketDto>>())?.ToList();
        Assert.NotNull(tickets);
        Assert.DoesNotContain(tickets, t => t.Concert.Id == fixture.SeedState.PastDoorSplitConcert.Id);
        Assert.DoesNotContain(tickets, t => t.Concert.Id == fixture.SeedState.PastFlatFeeConcert.Id);
    }

    #endregion

    #region GetUserHistory

    [Fact]
    public async Task GetUserHistory_Unauthenticated_ReturnsUnauthorized()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/ticket/history/user");

        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserHistory_PastTickets_ReturnsTickets()
    {
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.GetAsync("/api/ticket/history/user");

        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = (await response.Content.ReadAsync<IEnumerable<TicketDto>>())?.ToList();
        Assert.NotNull(tickets);
        Assert.Contains(tickets, t => t.Concert.Id == fixture.SeedState.PastDoorSplitConcert.Id);
        Assert.Contains(tickets, t => t.Concert.Id == fixture.SeedState.PastFlatFeeConcert.Id);
    }

    [Fact]
    public async Task GetUserHistory_UpcomingTicket_OmitsTicket()
    {
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.GetAsync("/api/ticket/history/user");

        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = await response.Content.ReadAsync<IEnumerable<TicketDto>>();
        Assert.NotNull(tickets);
        Assert.DoesNotContain(tickets, t => t.Concert.Id == fixture.SeedState.UpcomingFlatFeeConcert.Id);
    }

    #endregion

    #region CanPurchase

    [Fact]
    public async Task CanPurchase_Unauthenticated_ReturnsUnauthorized()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"/api/ticket/concert/{fixture.SeedState.UpcomingFlatFeeConcert.Id}/eligibility");

        await response.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CanPurchase_AvailableConcert_ReturnsTrue()
    {
        var concert = fixture.SeedState.UpcomingFlatFeeConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.GetAsync($"/api/ticket/concert/{concert.Id}/eligibility");

        await response.ShouldBe(HttpStatusCode.OK);
        Assert.True(await response.Content.ReadAsync<bool>());
    }

    [Fact]
    public async Task CanPurchase_PastConcert_ReturnsFalse()
    {
        var concert = fixture.SeedState.PastDoorSplitConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.GetAsync($"/api/ticket/concert/{concert.Id}/eligibility");

        await response.ShouldBe(HttpStatusCode.OK);
        Assert.False(await response.Content.ReadAsync<bool>());
    }

    [Fact]
    public async Task CanPurchase_MissingConcert_ReturnsNotFoundProblem()
    {
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.GetAsync($"/api/ticket/concert/{int.MaxValue}/eligibility");

        await AssertProblemCodeAsync(response, HttpStatusCode.NotFound, "ticket.concert_not_found");
    }

    #endregion

    private static async Task AssertProblemCodeAsync(
        HttpResponseMessage response,
        HttpStatusCode statusCode,
        string code)
    {
        await response.ShouldBe(statusCode);
        var problem = await response.Content.ReadAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(code, problem.Extensions["code"]?.ToString());
    }

    private static async Task<ValidationProblemDetails> AssertValidationProblemAsync(
        HttpResponseMessage response,
        string code,
        string field)
    {
        await response.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(code, problem.Extensions["code"]?.ToString());
        Assert.Equal([field], problem.Errors.Keys);
        return problem;
    }
}
