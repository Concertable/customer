using Concertable.Customer.DataAccess.Infrastructure;

namespace Concertable.Customer.Migrations;

internal static class Program
{
    private static Task Main() =>
        CustomerMigrationJob.RunAsync(
            Environment.GetEnvironmentVariable($"ConnectionStrings__{Db.Name}")
            ?? throw new InvalidOperationException(
                $"Connection string 'ConnectionStrings__{Db.Name}' is required for the Customer migration job."));
}
