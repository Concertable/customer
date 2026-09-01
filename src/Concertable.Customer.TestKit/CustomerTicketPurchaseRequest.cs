namespace Concertable.Customer.TestKit;

public sealed record CustomerTicketPurchaseRequest(
    string PaymentMethodId,
    int ConcertId,
    int Quantity = 1);
