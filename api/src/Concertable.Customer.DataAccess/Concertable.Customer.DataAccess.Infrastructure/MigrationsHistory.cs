namespace Concertable.Customer.DataAccess.Infrastructure;

/// <summary>Each context keeps its history in the schema it owns, so one database carries nine
/// independent histories rather than one shared table nothing can attribute.</summary>
public static class MigrationsHistory
{
    public const string Table = "__EFMigrationsHistory";
    public const string InboxTable = "__EFMigrationsHistory_Inbox";
    public const string OutboxTable = "__EFMigrationsHistory_Outbox";
    public const string MessagingSchema = "messaging";
}
