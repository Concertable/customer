using System.Net;
using Concertable.Customer.Ticket.Application.DTOs;
using Concertable.Customer.Ticket.Application.Requests;
using Concertable.Payment.Contracts;
using Concertable.Payment.Contracts.Errors;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
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
        var result = (await response.Content.ReadAsync<TicketPayment>()).ShouldNotBeNull();
        result.Reference.OperationType.ShouldBe("ticket-purchase");
        result.ClientSecret.ShouldBe("payment-session-secret");
        result.ConcertId.ShouldBe(concert.Id);
        result.Amount.ShouldBe(concert.Price);
        result.Currency.ShouldBe("GBP");

        var request = fixture.PaymentSessionClient.Sessions.ShouldHaveSingleItem();
        request.Reference.ShouldBe(result.Reference);
        request.Kind.ShouldBe(PaymentSessionKind.Payment);
        request.Session.ShouldBe(PaymentSession.OnSession);
        request.PayerOwnerId.ShouldBe(buyer.Id);
        request.PayeeOwnerId.ShouldBe(concert.PayeeOwnerId);
        request.AmountMinor.ShouldBe(
            Concertable.Kernel.ValueObjects.Money.Gbp(concert.Price).ToMinorUnits());
        request.Currency.ShouldBe(Concertable.Kernel.ValueObjects.Currency.Gbp);
        request.FundsRouting.ShouldBe(PaymentSessionFundsRouting.Destination);
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
        problem.Errors["purchase"].ShouldHaveSingleItem().ShouldStartWith("Not enough tickets available.");
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
        var result = (await response.Content.ReadAsync<TicketCheckout>()).ShouldNotBeNull();
        result.ConcertId.ShouldBe(concert.Id);
        result.Quantity.ShouldBe(1);
        result.Reference.OperationType.ShouldBe("ticket-purchase");
        result.Session.ClientSecret.ShouldBe("payment-session-secret");
        result.Session.CustomerSession.ShouldBe("customer-session-secret");
        result.Session.CustomerId.ShouldBe("customer-token");

        var request = fixture.PaymentSessionClient.Sessions.ShouldHaveSingleItem();
        request.Reference.ShouldBe(result.Reference);
        request.Kind.ShouldBe(PaymentSessionKind.Payment);
        request.Session.ShouldBe(PaymentSession.OnSession);
        request.PayerOwnerId.ShouldBe(buyer.Id);
        request.PayeeOwnerId.ShouldBe(concert.PayeeOwnerId);
        request.AmountMinor.ShouldBe(
            Concertable.Kernel.ValueObjects.Money.Gbp(concert.Price).ToMinorUnits());
        request.Currency.ShouldBe(Concertable.Kernel.ValueObjects.Currency.Gbp);
        request.FundsRouting.ShouldBe(PaymentSessionFundsRouting.Destination);
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
        problem.Errors["checkout"].ShouldHaveSingleItem().ShouldStartWith("Not enough tickets available.");
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
        var tickets = (await response.Content.ReadAsync<IEnumerable<TicketDto>>()).ShouldNotBeNull();
        var ticket = tickets.ShouldHaveSingleItem();
        ticket.Concert.Id.ShouldBe(fixture.SeedState.UpcomingFlatFeeConcert.Id);
    }

    [Fact]
    public async Task GetUserUpcoming_PastTickets_OmitsTickets()
    {
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.GetAsync("/api/ticket/upcoming/user");

        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = (await response.Content.ReadAsync<IEnumerable<TicketDto>>()).ShouldNotBeNull();
        tickets.ShouldNotContain(ticket => ticket.Concert.Id == fixture.SeedState.PastDoorSplitConcert.Id);
        tickets.ShouldNotContain(ticket => ticket.Concert.Id == fixture.SeedState.PastFlatFeeConcert.Id);
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
        var tickets = (await response.Content.ReadAsync<IEnumerable<TicketDto>>()).ShouldNotBeNull();
        tickets.ShouldContain(ticket => ticket.Concert.Id == fixture.SeedState.PastDoorSplitConcert.Id);
        tickets.ShouldContain(ticket => ticket.Concert.Id == fixture.SeedState.PastFlatFeeConcert.Id);
    }

    [Fact]
    public async Task GetUserHistory_UpcomingTicket_OmitsTicket()
    {
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.GetAsync("/api/ticket/history/user");

        await response.ShouldBe(HttpStatusCode.OK);
        var tickets = (await response.Content.ReadAsync<IEnumerable<TicketDto>>()).ShouldNotBeNull();
        tickets.ShouldNotContain(ticket => ticket.Concert.Id == fixture.SeedState.UpcomingFlatFeeConcert.Id);
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
        (await response.Content.ReadAsync<bool>()).ShouldBeTrue();
    }

    [Fact]
    public async Task CanPurchase_PastConcert_ReturnsFalse()
    {
        var concert = fixture.SeedState.PastDoorSplitConcert;
        var client = fixture.CreateClient(fixture.SeedState.Customer1);

        var response = await client.GetAsync($"/api/ticket/concert/{concert.Id}/eligibility");

        await response.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsync<bool>()).ShouldBeFalse();
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
        var problem = (await response.Content.ReadAsync<ProblemDetails>()).ShouldNotBeNull();
        problem.Extensions["code"]?.ToString().ShouldBe(code);
    }

    private static async Task<ValidationProblemDetails> AssertValidationProblemAsync(
        HttpResponseMessage response,
        string code,
        string field)
    {
        await response.ShouldBe(HttpStatusCode.BadRequest);
        var problem = (await response.Content.ReadAsync<ValidationProblemDetails>()).ShouldNotBeNull();
        problem.Extensions["code"]?.ToString().ShouldBe(code);
        problem.Errors.Keys.ShouldBe([field]);
        return problem;
    }
}
