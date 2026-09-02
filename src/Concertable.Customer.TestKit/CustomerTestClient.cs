using System.Net.Http.Json;

namespace Concertable.Customer.TestKit;

public sealed class CustomerTestClient
{
    private readonly HttpClient httpClient;

    public CustomerTestClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task PurchaseTicketAsync(
        CustomerTicketPurchaseRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await this.httpClient.PostAsJsonAsync(
            "api/ticket/purchase",
            request,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<CustomerTicket>> GetUpcomingTicketsAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await this.httpClient.GetAsync(
            "api/ticket/upcoming/user",
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CustomerTicket[]>(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Customer returned an empty upcoming-ticket response.");
    }
}
