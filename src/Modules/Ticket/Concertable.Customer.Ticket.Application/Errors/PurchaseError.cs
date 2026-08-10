using Concertable.Payment.Contracts.Errors;
using Dunet;
using Reunion.Errors;

namespace Concertable.Customer.Ticket.Application.Errors;

[Union(EnableImplicitConversions = false)]
internal abstract partial record PurchaseError : IError
{
    public ErrorDefinition Definition => this switch
    {
        ConcertNotFound(var concertId) =>
            ErrorDefinition.NotFound<ConcertNotFound>($"Concert {concertId} was not found."),
        Invalid(var errors) =>
            ErrorDefinition.Validation<Invalid>("The ticket purchase is invalid.", errors),
        PaymentRejected => ErrorDefinition.PaymentRequired<PaymentRejected>("The payment was rejected."),
        PaymentFailure(var error) => error.Definition
    };

    [ErrorCode("ticket.concert_not_found")]
    public partial record ConcertNotFound(int ConcertId);

    [ErrorCode("ticket.purchase_invalid")]
    public partial record Invalid(ValidationErrors Errors);

    [ErrorCode("ticket.payment_rejected")]
    public partial record PaymentRejected;

    public partial record PaymentFailure(PaymentError Error);
}
