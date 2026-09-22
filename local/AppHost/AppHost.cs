using Concertable.Customer.Hosting.Frontend;
using Aspire.Hosting;
using Concertable.Auth.Hosting;
using Concertable.B2B.Hosting;
using Concertable.Customer.Hosting;
using Concertable.Payment.Hosting;
using Concertable.Search.Hosting;

public static class AppHost
{
    private const string AuthImage = "ghcr.io/concertable/auth";
    private const string AuthDigest = "sha256:cbd7c429da9d9dd2cc674177760690c53d1414e8057e368eefc3631dfcb62be6";
    private const string AuthMigrationsImage = "ghcr.io/concertable/auth-migrations";
    private const string AuthMigrationsDigest = "sha256:090b1bb80dc7b708508a03883cdfb8e8805b36918589e6d14f2f350cc61c5dcb";
    private const string PaymentWebImage = "ghcr.io/concertable/payment-web";
    private const string PaymentWebDigest = "sha256:13bc1a58a647e01618822935985097e3c667b3ddb365def899ba83cf5d5b574f";
    private const string PaymentWorkersImage = "ghcr.io/concertable/payment-workers";
    private const string PaymentWorkersDigest = "sha256:586f87d0866bcf325622793379f15f4b94abdd34091907ca94b4e15fa5b2ab64";
    private const string PaymentMigrationsImage = "ghcr.io/concertable/payment-migrations";
    private const string PaymentMigrationsDigest = "sha256:b22e1a0d498e01d12b49e0bc911f3e4c7e44a0d5e3c4d5c8917e854e1fccdc96";
    private const string SearchWebImage = "ghcr.io/concertable/search-web";
    private const string SearchWebDigest = "sha256:f345c7e05209d4a72f7aee23f311867327fbc9b8b5daec7323506c3ef517d50a";
    private const string SearchWorkersImage = "ghcr.io/concertable/search-workers";
    private const string SearchWorkersDigest = "sha256:179918a414cb6aa398dc3ffd5035728a47d2780649b88c0b27468cdf71223515";
    private const string SearchMigrationsImage = "ghcr.io/concertable/search-migrations";
    private const string SearchMigrationsDigest = "sha256:351365915b0afbfd604db29d4dcff252b9bb8d2beeff3bd6d38392de176413b7";
    private const string B2BSeedingSimulatorImage = "ghcr.io/concertable/b2b-seeding-simulator";
    private const string B2BSeedingSimulatorDigest = "sha256:240d9569035152fd7e63695bcb0913ca5bad85fef902d9669c7e3bc288d2e244";

    public static IDistributedApplicationBuilder CreateBuilder(string[] args) =>
        ConfigureBuilder<Projects.Concertable_Customer_Web, Projects.Concertable_Customer_Migrations>(
            StrictDistributedApplication.CreateBuilder(args));

    public static IDistributedApplicationBuilder CreateE2EBuilder<TCustomerWeb>()
        where TCustomerWeb : IProjectMetadata, new() =>
        ConfigureBuilder<TCustomerWeb, Projects.Concertable_Customer_Migrations>(
            DistributedApplication.CreateBuilder(new DistributedApplicationOptions
            {
                Args = ["--environment", "Development"],
                AssemblyName = typeof(AppHost).Assembly.GetName().Name!,
                DisableDashboard = true,
            }));

    private static IDistributedApplicationBuilder ConfigureBuilder<TCustomerWeb, TCustomerMigrations>(
        IDistributedApplicationBuilder builder)
        where TCustomerWeb : IProjectMetadata, new()
        where TCustomerMigrations : IProjectMetadata, new()
    {
        var postgres = builder.AddPostgresContainer("concertable-customer-postgres-data").WithPostGis();
        var authDb = postgres.AddDatabase(AuthConstants.Database);
        var customerDb = postgres.AddDatabase(CustomerConstants.Database);
        var paymentDb = postgres.AddDatabase(PaymentConstants.Database);
        var searchDb = postgres.AddDatabase(SearchConstants.Database);
        var asb = builder.AddServiceBus();
        asb.Topology().AddCustomerTopology().AddSearchTopology().AddPaymentTopology().AddAuthTopology().RunAsEmulator();
        var authMigrations = builder.AddAuthMigrations(AuthMigrationsImage, AuthMigrationsDigest, authDb);
        var auth = builder.AddAuth(AuthImage, AuthDigest, authDb, authMigrations, asb)
                          .WithContainerRuntimeArgs("--user", "root")
                          .WithHttpEndpoint(targetPort: AuthConstants.ContainerPort, name: "https");
        auth.WithEndpoint("https", endpoint => endpoint.Port = 7093);
        auth.WithSpaClients(CustomerLocalSpaSurfaces.AuthClients);
        var paymentMigrations = builder.AddPaymentMigrations(
            PaymentMigrationsImage,
            PaymentMigrationsDigest,
            paymentDb);
        var paymentWeb = builder.AddPaymentWeb(PaymentWebImage, PaymentWebDigest, auth, paymentDb, asb)
            .WaitForCompletion(paymentMigrations);
        paymentWeb.WithEndpoint("https", endpoint => endpoint.Port = 7098);
        var searchMigrations = builder.AddSearchMigrations(
            SearchMigrationsImage,
            SearchMigrationsDigest,
            searchDb);
        var searchWeb = builder.AddSearchWeb(SearchWebImage, SearchWebDigest, auth, searchDb)
            .WaitForCompletion(searchMigrations);
        searchWeb.WithEndpoint("https", endpoint => endpoint.Port = 7097);
        var customerMigrations = builder.AddCustomerMigrations<TCustomerMigrations>(customerDb);
        var customerWeb = builder.AddCustomerWeb<TCustomerWeb>(auth, customerDb, customerMigrations, asb, paymentWeb);
        if (builder.ExecutionContext.IsRunMode)
            customerWeb.WithEnvironment(PaymentConstants.AllowInsecureHttpClientEnvironmentVariable, bool.TrueString);
        auth.WithEnvironment("ServiceAuth__AuthClientId", "concertable-auth");
        auth.WithEnvironment("Services__CustomerApiUrl", customerWeb.GetEndpoint("https"));
        builder.AddPaymentWorkers(PaymentWorkersImage, PaymentWorkersDigest, paymentDb, asb)
            .WaitForCompletion(paymentMigrations);
        builder.AddSearchWorkers(SearchWorkersImage, SearchWorkersDigest, searchDb, asb)
            .WaitForCompletion(searchMigrations);
        builder.AddB2BSeedingSimulator(B2BSeedingSimulatorImage, B2BSeedingSimulatorDigest, asb);
        builder.AddCustomerSpa(customerWeb, searchWeb, auth);
        if (builder.AddMobileCustomer(customerWeb, auth, searchWeb, paymentWeb) is { } mobileTunnel)
            auth.WithMobilePublicUrl(mobileTunnel.GetEndpoint(auth, "https"));
        builder.AddStripeCli(paymentWeb);
        return builder;
    }
}
