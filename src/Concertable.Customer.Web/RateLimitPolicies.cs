namespace Concertable.Customer.Web;

public static class RateLimitPolicies
{
    public const string PublicRead = "public-read";
    public const string Purchase = "purchase";
    public const string Review = "review";

    public static readonly IReadOnlyList<string> All = [PublicRead, Purchase, Review];
}
