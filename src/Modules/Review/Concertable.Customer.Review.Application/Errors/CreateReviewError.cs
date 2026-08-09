using Dunet;
using Reunion.Errors;

namespace Concertable.Customer.Review.Application.Errors;

[Union(EnableImplicitConversions = false)]
internal abstract partial record CreateReviewError : IError
{
    private static readonly ErrorDefinitions<CreateReviewError> Definitions =
        ErrorDefinition.For<CreateReviewError>();

    public ErrorDefinition Definition => this switch
    {
        TicketNotFound => Definitions.NotFound<TicketNotFound>(),
        ConcertNotReviewableYet =>
            Definitions.Conflict<ConcertNotReviewableYet>("The concert is not reviewable yet."),
        ReviewAlreadyExists =>
            Definitions.Conflict<ReviewAlreadyExists>("A review already exists for this ticket.")
    };

    [ErrorCode("review.ticket_not_found")]
    public partial record TicketNotFound;

    [ErrorCode("review.concert_not_reviewable_yet")]
    public partial record ConcertNotReviewableYet;

    [ErrorCode("review.already_exists")]
    public partial record ReviewAlreadyExists;
}
