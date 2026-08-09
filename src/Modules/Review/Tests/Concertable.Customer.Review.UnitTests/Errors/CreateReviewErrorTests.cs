using Concertable.Customer.Review.Application.Errors;
using Reunion.Errors;

namespace Concertable.Customer.Review.UnitTests.Errors;

public sealed class CreateReviewErrorTests
{
    [Fact]
    public void TicketNotFound_Definition_IsStable()
    {
        var definition = new CreateReviewError.TicketNotFound().Definition;

        Assert.Equal("review.ticket_not_found", definition.Code);
        Assert.Equal("Ticket not found.", definition.Message);
        Assert.Equal(ErrorKind.NotFound, definition.Kind);
    }

    [Fact]
    public void ConcertNotReviewableYet_Definition_IsStable()
    {
        var definition = new CreateReviewError.ConcertNotReviewableYet().Definition;

        Assert.Equal("review.concert_not_reviewable_yet", definition.Code);
        Assert.Equal("The concert is not reviewable yet.", definition.Message);
        Assert.Equal(ErrorKind.Conflict, definition.Kind);
    }

    [Fact]
    public void ReviewAlreadyExists_Definition_IsStable()
    {
        var definition = new CreateReviewError.ReviewAlreadyExists().Definition;

        Assert.Equal("review.already_exists", definition.Code);
        Assert.Equal("A review already exists for this ticket.", definition.Message);
        Assert.Equal(ErrorKind.Conflict, definition.Kind);
    }
}
