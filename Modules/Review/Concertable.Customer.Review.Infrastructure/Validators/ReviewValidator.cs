namespace Concertable.Customer.Review.Infrastructure.Validators;

internal sealed class ReviewValidator(
    IConcertReviewRepository concertReviewRepository,
    IArtistReviewRepository artistReviewRepository,
    IVenueReviewRepository venueReviewRepository) : IReviewValidator
{
    public Task<bool> CanUserReviewConcertAsync(Guid userId, int concertId) =>
        concertReviewRepository.CanUserReviewConcertAsync(userId, concertId);

    public Task<bool> CanUserReviewArtistAsync(Guid userId, int artistId) =>
        artistReviewRepository.CanUserReviewArtistAsync(userId, artistId);

    public Task<bool> CanUserReviewVenueAsync(Guid userId, int venueId) =>
        venueReviewRepository.CanUserReviewVenueAsync(userId, venueId);
}
