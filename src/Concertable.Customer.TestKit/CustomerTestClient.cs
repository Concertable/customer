using System.Net.Http.Json;

namespace Concertable.Customer.TestKit;

public sealed class CustomerTestClient
{
    public const string AdminKeyHeader = "X-Concertable-E2E-Key";

    private readonly HttpClient httpClient;

    public CustomerTestClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public CustomerTestClient(HttpClient httpClient, string adminKey)
        : this(httpClient)
    {
        this.httpClient.DefaultRequestHeaders.Add(AdminKeyHeader, adminKey);
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        using var response = await this.httpClient.PostAsync("/_e2e/reset", null, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task<SeedState> GetSeedStateAsync(CancellationToken cancellationToken = default) =>
        await this.httpClient.GetFromJsonAsync<SeedState>("/_e2e/seed-state", cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("The Customer E2E seed-state response was empty.");

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
