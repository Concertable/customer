using Concertable.Customer.Ticket.Contracts;
using Reunion.Errors;
using Reunion.Validation;

namespace Concertable.Customer.Review.Infrastructure.Validators;

internal sealed class ReviewValidator : IReviewValidator
{
    private readonly IConcertReviewRepository concertReviewRepository;
    private readonly ITicketModule ticketModule;
    private readonly TimeProvider timeProvider;

    public ReviewValidator(
        IConcertReviewRepository concertReviewRepository,
        ITicketModule ticketModule,
        TimeProvider timeProvider)
    {
        this.concertReviewRepository = concertReviewRepository;
        this.ticketModule = ticketModule;
        this.timeProvider = timeProvider;
    }

    public ValidationResult ValidateReviewPeriod(TicketSummary ticket) =>
        ticket.PeriodStart <= timeProvider.GetUtcNow()
            ? ValidationResult.Valid()
            : Invalid("ConcertId", "The concert is not reviewable yet.");

    public async Task<ValidationResult> ValidateTicketNotReviewedAsync(Guid ticketId) =>
        await concertReviewRepository.HasReviewForTicketAsync(ticketId)
            ? Invalid("TicketId", "A review already exists for this ticket.")
            : ValidationResult.Valid();

    public async Task<ValidationResult> ValidateArtistAsync(Guid userId, int artistId) =>
        await ticketModule.CanReviewArtistAsync(userId, artistId)
            ? ValidationResult.Valid()
            : Invalid("ArtistId", "No reviewable ticket exists for this artist.");

    public async Task<ValidationResult> ValidateVenueAsync(Guid userId, int venueId) =>
        await ticketModule.CanReviewVenueAsync(userId, venueId)
            ? ValidationResult.Valid()
            : Invalid("VenueId", "No reviewable ticket exists for this venue.");

    private static ValidationResult Invalid(string field, string message) =>
        ValidationResult.Invalid(new ValidationErrors([new(field, message)]));
}
