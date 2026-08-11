using Dunet;
using Reunion.Errors;

namespace Concertable.Customer.Ticket.Application.Errors;

[Union(EnableImplicitConversions = false)]
internal abstract partial record EligibilityError : IError
{
    public ErrorDefinition Definition => this switch
    {
        ConcertNotFound(var concertId) =>
            ErrorDefinition.NotFound<ConcertNotFound>($"Concert {concertId} was not found.")
    };

    [ErrorCode("ticket.concert_not_found")]
    public partial record ConcertNotFound(int ConcertId);
}
