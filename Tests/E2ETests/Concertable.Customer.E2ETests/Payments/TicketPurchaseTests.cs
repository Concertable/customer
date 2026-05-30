using Concertable.B2B.Seeding.Fixture;
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

        // Act
        await client.PostAsSuccessAsync("/api/Ticket/purchase", new
        {
            ConcertId = B2BSeedFixture.UpcomingConcertId,
            Quantity = 1,
            PaymentMethodId = AppFixture.TestPaymentMethodId
        });

        // Assert
        await fixture.Polling.UntilAsync(
            () => client.GetAssertAsync<IEnumerable<UpcomingTicket>>("/api/Ticket/upcoming/user"),
            tickets => tickets is not null && tickets.Any(t => t.Concert.Id == B2BSeedFixture.UpcomingConcertId),
            timeout: TimeSpan.FromSeconds(30));
    }

    private record UpcomingTicket(Guid Id, UpcomingConcert Concert);
    private record UpcomingConcert(int Id, string Name);
}
