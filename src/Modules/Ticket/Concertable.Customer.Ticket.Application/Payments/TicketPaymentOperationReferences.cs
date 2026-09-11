using System.Globalization;
using Concertable.Payment.Contracts;

namespace Concertable.Customer.Ticket.Application.Payments;

internal enum TicketPaymentOperationType
{
    Purchase
}

internal readonly record struct TicketPurchaseReference(Guid BuyerId, int ConcertId, int Quantity);

internal static class TicketPaymentOperationReferences
{
    private const string PurchaseOperationType = "ticket-purchase";
    private const string BuyerPrefix = "buyer:";
    private const string ConcertMarker = ":concert:";
    private const string QuantityMarker = ":quantity:";

    public static PaymentOperationReference Create(
        TicketPaymentOperationType operationType,
        Guid buyerId,
        int concertId,
        int quantity) =>
        operationType switch
        {
            TicketPaymentOperationType.Purchase => new(
                PurchaseOperationType,
                $"{BuyerPrefix}{buyerId:N}{ConcertMarker}{concertId.ToString(CultureInfo.InvariantCulture)}{QuantityMarker}{quantity.ToString(CultureInfo.InvariantCulture)}"),
            _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
        };

    extension(PaymentOperationReference reference)
    {
        public bool TryGetPurchase(out TicketPurchaseReference purchase)
        {
            purchase = default;
            if (!string.Equals(reference.OperationType, PurchaseOperationType, StringComparison.Ordinal))
                return false;

            var clientReference = reference.ClientReference;
            if (!clientReference.StartsWith(BuyerPrefix, StringComparison.Ordinal))
                return false;

            var concertMarkerIndex = clientReference.IndexOf(ConcertMarker, StringComparison.Ordinal);
            var quantityMarkerIndex = clientReference.IndexOf(QuantityMarker, StringComparison.Ordinal);
            if (concertMarkerIndex <= BuyerPrefix.Length
                || quantityMarkerIndex <= concertMarkerIndex + ConcertMarker.Length)
                return false;

            var buyerSpan = clientReference.AsSpan(BuyerPrefix.Length, concertMarkerIndex - BuyerPrefix.Length);
            var concertSpan = clientReference.AsSpan(
                concertMarkerIndex + ConcertMarker.Length,
                quantityMarkerIndex - concertMarkerIndex - ConcertMarker.Length);
            var quantitySpan = clientReference.AsSpan(quantityMarkerIndex + QuantityMarker.Length);
            if (!Guid.TryParseExact(buyerSpan, "N", out var buyerId)
                || !int.TryParse(concertSpan, NumberStyles.None, CultureInfo.InvariantCulture, out var concertId)
                || !int.TryParse(quantitySpan, NumberStyles.None, CultureInfo.InvariantCulture, out var quantity)
                || buyerId == Guid.Empty
                || concertId <= 0
                || quantity <= 0)
                return false;

            purchase = new(buyerId, concertId, quantity);
            return true;
        }
    }
}
