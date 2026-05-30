using Concertable.Seeding.Identity;
using Xunit;

namespace Concertable.Customer.E2ETests.Payments;

[Collection("E2E")]
public class TicketPurchaseTests(AppFixture fixture) : IAsyncLifetime
{
    public async Task InitializeAsync() => await fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ShouldCreateTicket_WhenPaymentSucceeds()
    {
        // Arrange
        var client = await fixture.CreateAuthenticatedClientAsync(SeedCustomers.Customer1.Email);
        var upcomingConcertId = fixture.B2BSeed.Concerts.First(c => c.Name == "Upcoming FlatFee Show").ConcertId;

        // Act
        await client.PostAsSuccessAsync("/api/Ticket/purchase", new
        {
            ConcertId = upcomingConcertId,
            Quantity = 1,
            PaymentMethodId = AppFixture.TestPaymentMethodId
        });

        // Assert
        await fixture.Polling.UntilAsync(
            () => client.GetAssertAsync<IEnumerable<UpcomingTicket>>("/api/Ticket/upcoming/user"),
            tickets => tickets is not null && tickets.Any(t => t.Concert.Id == upcomingConcertId),
            timeout: TimeSpan.FromSeconds(30));
    }

    private record UpcomingTicket(Guid Id, UpcomingConcert Concert);
    private record UpcomingConcert(int Id, string Name);
}
