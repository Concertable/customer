using Concertable.Customer.Concert.Contracts;
using Concertable.Customer.Ticket.Application.Errors;
using Reunion;
using Reunion.Errors;
using Reunion.Validation;

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

    public ValidationResult CanBePurchased(ConcertDto concert) =>
        new[]
        {
            Validate(
                concert.DatePosted is not null,
                "concert",
                "Concert is not posted yet"),
            Validate(
                concert.Period.Start >= timeProvider.GetUtcNow(),
                "concert",
                "You cannot purchase a Ticket for a Concert that's already passed"),
            Validate(
                concert.AvailableTickets > 0,
                "concert",
                "No Tickets Available for Concert")
        }.Combine();

    public Task<Result<ValidationResult, EligibilityError>> CanBePurchasedAsync(int concertId) =>
        concertModule.GetByIdAsync(concertId)
            .OrFailure<ConcertDto, EligibilityError>(new EligibilityError.ConcertNotFound(concertId))
            .Map(CanBePurchased);

    public ValidationResult CanPurchaseTickets(ConcertDto concert, int quantity)
    {
        var concertValidation = CanBePurchased(concert);
        if (concertValidation.IsInvalid)
            return concertValidation;

        return Validate(
            concert.AvailableTickets - quantity >= 0,
            "quantity",
            $"Not enough tickets available. Only {concert.AvailableTickets} tickets are available");
    }

    private static ValidationResult Validate(bool isValid, string field, string message) =>
        isValid
            ? ValidationResult.Valid()
            : ValidationResult.Invalid(new ValidationErrors([new(field, message)]));
}
