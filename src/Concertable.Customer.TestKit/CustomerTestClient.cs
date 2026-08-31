using System.Net.Http.Json;

namespace Concertable.Customer.TestKit;

public sealed class CustomerTestClient
{
    public const string AdminKeyHeader = "X-Concertable-E2E-Key";

    private readonly HttpClient client;

    public CustomerTestClient(HttpClient client, string adminKey)
    {
        this.client = client;
        this.client.DefaultRequestHeaders.Add(AdminKeyHeader, adminKey);
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync("/_e2e/reset", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<SeedState> GetSeedStateAsync(CancellationToken cancellationToken = default) =>
        await client.GetFromJsonAsync<SeedState>("/_e2e/seed-state", cancellationToken)
            ?? throw new InvalidOperationException("The Customer E2E seed-state response was empty.");
}
