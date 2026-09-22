namespace Concertable.Customer.DataAccess.Infrastructure;

/// <summary>The Customer service's database connection string name — must match the AppHost resource name.</summary>
public static class Db
{
    public const string Name = "CustomerDb";

    public static IReadOnlyList<string> Schemas { get; } =
    [
        "artist",
        "concert",
        "preference",
        "review",
        "ticket",
        "user",
        "venue",
        MigrationsHistory.MessagingSchema,
    ];
}
