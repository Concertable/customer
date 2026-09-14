namespace Concertable.Customer.Concert.Infrastructure;

internal static class Schema
{
    public const string Name = "concert";

    public static class Tables
    {
        public const string Concerts = "Concerts";
        public const string ConcertGenres = "ConcertGenres";
        public const string VenueReadModels = "VenueReadModels";
        public const string ArtistReadModels = "ArtistReadModels";
        public const string ArtistReadModelGenres = "ArtistReadModelGenres";
    }
}
