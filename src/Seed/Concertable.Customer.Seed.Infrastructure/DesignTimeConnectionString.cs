namespace Concertable.Customer.Seed.Infrastructure;

// Design-time only: the fallback `dotnet ef` uses when Aspire hasn't injected ConnectionStrings__CustomerDb.
// Real hosts always resolve the string from configuration; this is the local/dev fallback.
public static class DesignTimeConnectionString
{
    public static string Customer() =>
        Environment.GetEnvironmentVariable("ConnectionStrings__CustomerDb")
        ?? "Server=localhost,1433;Database=concertable-customer;User Id=sa;Password=Password11!;TrustServerCertificate=True";
}
