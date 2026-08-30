using System.Security.Cryptography;
using System.Text;
using Concertable.Customer.Seed.Infrastructure;
using Concertable.DataAccess.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;

namespace Concertable.Customer.E2ETests.Server;

public static class E2EAdminExtensions
{
    private const string AdminKeyHeader = "X-Concertable-E2E-Key";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddCustomerE2EAdmin(IConfiguration configuration)
        {
            services.AddSingleton(new E2EAdminOptions(
                configuration["E2E:AdminKey"]
                    ?? throw new InvalidOperationException("E2E:AdminKey is required by the Customer E2E host."),
                configuration.GetConnectionString("CustomerDb")
                    ?? throw new InvalidOperationException("Connection string 'CustomerDb' is required by the Customer E2E host.")));
            services.AddScoped<CustomerDatabaseResetter>();
            return services;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication MapCustomerE2EAdmin()
        {
            var group = app.MapGroup("/_e2e")
                .AddEndpointFilter(AuthorizeAsync);
            group.MapPost("/reset", ResetAsync);
            group.MapGet("/seed-state", GetSeedState);
            return app;
        }
    }

    private static async ValueTask<object?> AuthorizeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var options = context.HttpContext.RequestServices.GetRequiredService<E2EAdminOptions>();
        var supplied = context.HttpContext.Request.Headers[AdminKeyHeader].ToString();
        var expectedBytes = Encoding.UTF8.GetBytes(options.AdminKey);
        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        if (expectedBytes.Length != suppliedBytes.Length
            || !CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes))
        {
            return Results.NotFound();
        }

        return await next(context);
    }

    private static async Task<IResult> ResetAsync(
        CustomerDatabaseResetter resetter,
        IDbInitializer initializer,
        CancellationToken cancellationToken)
    {
        await resetter.ResetAsync(cancellationToken);
        await initializer.InitializeAsync();
        return Results.NoContent();
    }

    private static IResult GetSeedState(SeedState seed) => Results.Ok(new
    {
        Customer1 = new { seed.Customer1.Id, seed.Customer1.Email },
        UpcomingFlatFeeConcert = new { seed.UpcomingFlatFeeConcert.Id },
    });
}

internal sealed record E2EAdminOptions(string AdminKey, string ConnectionString);

internal sealed class CustomerDatabaseResetter
{
    private readonly E2EAdminOptions options;

    public CustomerDatabaseResetter(E2EAdminOptions options)
    {
        this.options = options;
    }

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            TablesToIgnore =
            [
                "__EFMigrationsHistory",
                new Table("concert", "Concerts"),
                new Table("concert", "ConcertGenres"),
                new Table("concert", "VenueReadModels"),
                new Table("concert", "ArtistReadModels"),
                new Table("concert", "ArtistReadModelGenres"),
                new Table("artist", "Artists"),
                new Table("artist", "ArtistGenres"),
                new Table("venue", "Venues"),
                new Table("user", "Users"),
                new Table("messaging", "Inbox"),
            ],
            DbAdapter = DbAdapter.SqlServer,
            WithReseed = true,
        });
        await respawner.ResetAsync(connection);
    }
}
