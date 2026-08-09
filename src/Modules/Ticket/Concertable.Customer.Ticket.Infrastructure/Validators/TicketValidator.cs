using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Ticket.Application.Errors;
using Reunion;

namespace Concertable.Customer.Ticket.Infrastructure.Validators;

internal sealed class TicketValidator : ITicketValidator
{
    private readonly IConcertModule concertModule;
    private readonly TimeProvider timeProvider;

    public TicketValidator(IConcertModule concertModule, TimeProvider timeProvider)
    {
        this.concertModule = concertModule;
        this.timeProvider = timeProvider;
    }

    public bool CanBePurchased(ConcertDto concert) => GetPurchaseErrors(concert).Count == 0;

    public async Task<Result<bool, EligibilityError>> CanBePurchasedAsync(int concertId)
    {
        var concert = await concertModule.GetByIdAsync(concertId);
        if (concert is null)
            return Result<bool, EligibilityError>.Failure(new EligibilityError.ConcertNotFound(concertId));

        return Result<bool, EligibilityError>.Success(CanBePurchased(concert));
    }

    public UnitResult<IReadOnlyList<string>> CanPurchaseTickets(ConcertDto concert, int quantity)
    {
        var errors = GetPurchaseErrors(concert);
        if (errors.Count > 0)
            return UnitResult<IReadOnlyList<string>>.Failure(errors);

        return concert.AvailableTickets - quantity < 0
            ? UnitResult<IReadOnlyList<string>>.Failure(
                [$"Not enough tickets available. Only {concert.AvailableTickets} tickets are available"])
            : UnitResult<IReadOnlyList<string>>.Success();
    }

    private IReadOnlyList<string> GetPurchaseErrors(ConcertDto concert)
    {
        var errors = new List<string>();

        if (concert.DatePosted is null)
            errors.Add("Concert is not posted yet");

        if (concert.Period.Start < timeProvider.GetUtcNow())
            errors.Add("You cannot purchase a Ticket for a Concert that's already passed");

        if (concert.AvailableTickets <= 0)
            errors.Add("No Tickets Available for Concert");

        return errors;
    }
}
