namespace Concertable.Customer.DataAccess.Infrastructure;

internal static class DesignTimeConfiguration
{
    public static string ConnectionString() =>
        Environment.GetEnvironmentVariable($"ConnectionStrings__{Db.Name}")
        ?? throw new InvalidOperationException(
            $"Design-time connection string 'ConnectionStrings__{Db.Name}' is not set. " +
            "Set it via environment or user-secrets — ./initial-migrations.ps1 exports it for local re-scaffolds.");
}
