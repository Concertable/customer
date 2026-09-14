using Concertable.Auth.Contracts;
using Concertable.Auth.Contracts.Events;
using Concertable.Customer.User.Domain.Entities;
using Concertable.Customer.User.Infrastructure.Data;
using Concertable.Customer.User.Infrastructure.Events;
using Concertable.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.User.UnitTests.Events;

public sealed class UserCreationHandlerTests
{
    private static readonly DateTimeOffset Base = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.NewGuid();

    private static UserDbContext NewContext(string dbName) =>
        new(new DbContextOptionsBuilder<UserDbContext>().UseInMemoryDatabase(dbName).Options,
            new UserConfigurationProvider());

    private static CredentialRegisteredEvent NewEvent(InteractiveClient client) =>
        new(UserId, "customer@test.com", InteractiveClientInfo.Get(client).Id);

    [Theory]
    [InlineData(InteractiveClient.CustomerBrowser)]
    [InlineData(InteractiveClient.CustomerMobile)]
    public async Task HandleAsync_ForCustomerClient_CreatesUserAndRecordsInbox(InteractiveClient client)
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<CredentialRegisteredEvent>(Base);

        // Act
        await using (var context = NewContext(dbName))
            await new UserCreationHandler(context).HandleAsync(NewEvent(client), envelope);

        // Assert
        await using var probe = NewContext(dbName);
        var user = await probe.Users.SingleAsync();
        Assert.Equal(UserId, user.Id);
        Assert.Equal("customer@test.com", user.Email);
        Assert.True(await probe.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(UserCreationHandler)));
    }

    [Theory]
    [InlineData(InteractiveClient.VenueBrowser)]
    [InlineData(InteractiveClient.ArtistMobile)]
    [InlineData(InteractiveClient.Admin)]
    public async Task HandleAsync_ForNonCustomerClient_DoesNothing(InteractiveClient client)
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<CredentialRegisteredEvent>(Base);

        // Act
        await using (var context = NewContext(dbName))
            await new UserCreationHandler(context).HandleAsync(NewEvent(client), envelope);

        // Assert — other services' registrations are not this module's users; nothing persists
        await using var probe = NewContext(dbName);
        Assert.Empty(await probe.Users.ToListAsync());
        Assert.False(await probe.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(UserCreationHandler)));
    }

    [Fact]
    public async Task HandleAsync_WhenMessageAlreadyProcessed_DoesNotCreateUser()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var envelope = MessageEnvelope.Create<CredentialRegisteredEvent>(Base);
        await using (var seed = NewContext(dbName))
        {
            seed.AddInboxMessage(envelope, nameof(UserCreationHandler));
            await seed.SaveChangesAsync();
        }

        // Act
        await using (var context = NewContext(dbName))
            await new UserCreationHandler(context).HandleAsync(NewEvent(InteractiveClient.CustomerBrowser), envelope);

        // Assert
        await using var probe = NewContext(dbName);
        Assert.Empty(await probe.Users.ToListAsync());
    }
}
