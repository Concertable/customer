using Concertable.Customer.Ticket.Application.Payments;
using Concertable.Payment.Contracts;

namespace Concertable.Customer.Ticket.UnitTests;

public sealed class TicketPaymentOperationReferencesTests
{
    [Fact]
    public void Create_PurchaseReference_RoundTripsPurchaseIdentity()
    {
        var buyerId = Guid.NewGuid();

        var reference = TicketPaymentOperationReferences.Create(
            TicketPaymentOperationType.Purchase,
            buyerId,
            42,
            3);
        var parsed = reference.TryGetPurchase(out var purchase);

        Assert.True(parsed);
        Assert.Equal("ticket-purchase", reference.OperationType);
        Assert.Equal(buyerId, purchase.BuyerId);
        Assert.Equal(42, purchase.ConcertId);
        Assert.Equal(3, purchase.Quantity);
    }

    [Fact]
    public void TryGetPurchase_ForeignOperationType_ReturnsFalse()
    {
        var reference = new PaymentOperationReference("application-fee", "malformed");

        var parsed = reference.TryGetPurchase(out _);

        Assert.False(parsed);
    }

    [Theory]
    [InlineData("malformed")]
    [InlineData("buyer:not-a-guid:concert:42:quantity:1")]
    [InlineData("buyer:00000000000000000000000000000000:concert:42:quantity:1")]
    [InlineData("buyer:00000000000000000000000000000000:concert:0:quantity:1")]
    [InlineData("buyer:00000000000000000000000000000000:concert:42:quantity:0")]
    public void TryGetPurchase_MalformedOwnedReference_ReturnsFalse(string clientReference)
    {
        var reference = new PaymentOperationReference("ticket-purchase", clientReference);

        var parsed = reference.TryGetPurchase(out _);

        Assert.False(parsed);
    }
}
