using Dunet;
using Reunion.Errors;

namespace Concertable.Customer.Ticket.Application.Errors;

[Union(EnableImplicitConversions = false)]
internal abstract partial record CheckoutError : IError
{
    public ErrorDefinition Definition => this switch
    {
        ConcertNotFound(var concertId) =>
            ErrorDefinition.NotFound<ConcertNotFound>($"Concert {concertId} was not found."),
        Invalid(var errors) => ErrorDefinition.Validation<Invalid>("The ticket checkout is invalid.", errors)
    };

    [ErrorCode("ticket.concert_not_found")]
    public partial record ConcertNotFound(int ConcertId);

    [ErrorCode("ticket.checkout_invalid")]
    public partial record Invalid(ValidationErrors Errors);
}
