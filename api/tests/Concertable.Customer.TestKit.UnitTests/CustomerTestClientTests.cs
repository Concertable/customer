using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Concertable.Customer.TestKit.UnitTests;

public sealed class CustomerTestClientTests
{
    [Fact]
    public async Task PurchaseTicketAsync_Request_SendsPublicPurchaseContract()
    {
        HttpRequestMessage? observedRequest = null;
        string? observedContent = null;
        using var handler = new StubHttpMessageHandler(async request =>
        {
            observedRequest = request;
            observedContent = await request.Content!.ReadAsStringAsync().ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://customer.test/")
        };
        var client = new CustomerTestClient(httpClient);

        await client.PurchaseTicketAsync(new CustomerTicketPurchaseRequest("pm_card_visa", 42, 2));

        Assert.NotNull(observedRequest);
        Assert.Equal(HttpMethod.Post, observedRequest.Method);
        Assert.Equal("https://customer.test/api/ticket/purchase", observedRequest.RequestUri!.AbsoluteUri);
        using var document = JsonDocument.Parse(observedContent!);
        Assert.Equal("pm_card_visa", document.RootElement.GetProperty("paymentMethodId").GetString());
        Assert.Equal(42, document.RootElement.GetProperty("concertId").GetInt32());
        Assert.Equal(2, document.RootElement.GetProperty("quantity").GetInt32());
    }

    [Fact]
    public async Task GetUpcomingTicketsAsync_PublicResponse_ReturnsObservableTickets()
    {
        using var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal(
                "https://customer.test/api/ticket/upcoming/user",
                request.RequestUri!.AbsoluteUri);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new[]
                {
                    new CustomerTicket
                    {
                        Id = Guid.Parse("4bc95506-a4e1-44ec-90cf-25fcab02586d"),
                        PurchaseDate = new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                        QrCode = [1, 2, 3],
                        UserId = Guid.Parse("8a4f8a36-8184-43f5-9d11-16391dfeb33a"),
                        Concert = new CustomerTicketConcert
                        {
                            Id = 42,
                            Name = "Customer Test Concert",
                            Price = 19.50m,
                            StartDate = new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
                            EndDate = new DateTime(2026, 10, 1, 22, 0, 0, DateTimeKind.Utc),
                            VenueName = "Test Venue",
                            ArtistName = "Test Artist"
                        }
                    }
                })
            });
        });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://customer.test/")
        };
        var client = new CustomerTestClient(httpClient);

        var tickets = await client.GetUpcomingTicketsAsync();

        var ticket = Assert.Single(tickets);
        Assert.Equal(42, ticket.Concert.Id);
        Assert.Equal("Customer Test Concert", ticket.Concert.Name);
    }

    [Fact]
    public async Task GetUpcomingTicketsAsync_ErrorResponse_ThrowsHttpRequestException()
    {
        using var handler = new StubHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)));
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://customer.test/")
        };
        var client = new CustomerTestClient(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetUpcomingTicketsAsync());
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> send;

        public StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send)
        {
            this.send = send;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            this.send(request);
    }
}
