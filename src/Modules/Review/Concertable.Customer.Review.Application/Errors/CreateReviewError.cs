using Concertable.Kernel.Errors;
using Dunet;

namespace Concertable.Customer.Review.Application.Errors;

[Union(EnableImplicitConversions = false)]
internal abstract partial record CreateReviewError : IError
{
    public ErrorDefinition Definition => this switch
    {
        TicketNotFound => ErrorDefinition.NotFound<TicketNotFound>(),
        ConcertNotReviewableYet =>
            ErrorDefinition.Conflict<ConcertNotReviewableYet>("The concert is not reviewable yet."),
        ReviewAlreadyExists =>
            ErrorDefinition.Conflict<ReviewAlreadyExists>("A review already exists for this ticket.")
    };

    [ErrorCode("review.ticket_not_found")]
    public partial record TicketNotFound;

    [ErrorCode("review.concert_not_reviewable_yet")]
    public partial record ConcertNotReviewableYet;

    [ErrorCode("review.already_exists")]
    public partial record ReviewAlreadyExists;
}
