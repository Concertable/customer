using Concertable.Kernel.Errors;

namespace Concertable.Customer.Review.Application.Errors;

internal sealed record CreateReviewError(ErrorDefinition Definition) : IError
{
    public static readonly CreateReviewError TicketNotFound = new(
        ErrorDefinition.NotFound(
            "review.ticket_not_found",
            "Ticket not found."));

    public static readonly CreateReviewError ConcertNotReviewableYet = new(
        ErrorDefinition.Conflict(
            "review.concert_not_reviewable_yet",
            "The concert is not reviewable yet."));

    public static readonly CreateReviewError ReviewAlreadyExists = new(
        ErrorDefinition.Conflict(
            "review.already_exists",
            "A review already exists for this ticket."));
}
