using Concertable.Customer.Concert.Contracts;
using Concertable.Kernel.Exceptions;
using FluentResults;

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

    public Result CanBePurchased(ConcertDto concert)
    {
        var errors = new List<string>();

        if (concert.DatePosted is null)
            errors.Add("Concert is not posted yet");

        if (concert.Period.Start < timeProvider.GetUtcNow())
            errors.Add("You cannot purchase a Ticket for a Concert that's already passed");

        if (concert.AvailableTickets <= 0)
            errors.Add("No Tickets Available for Concert");

        return errors.Count > 0 ? Result.Fail(errors) : Result.Ok();
    }

    public async Task<Result> CanBePurchasedAsync(int concertId)
    {
        var concert = await concertModule.GetByIdAsync(concertId)
            ?? throw new NotFoundException("Concert not found");

        return CanBePurchased(concert);
    }

    public Result CanPurchaseTickets(ConcertDto concert, int quantity)
    {
        var baseResult = CanBePurchased(concert);
        if (baseResult.IsFailed)
            return baseResult;

        return concert.AvailableTickets - quantity < 0
            ? Result.Fail($"Not enough tickets available. Only {concert.AvailableTickets} tickets are available")
            : Result.Ok();
    }
}
