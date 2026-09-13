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
    private const string AuthDigest = "sha256:d888cb59d806241611eb36a5d1eeb293d472cd94e4a89bf6dab6b9b23c0c5f82";
    private const string PaymentWebImage = "ghcr.io/concertable/payment-web";
    private const string PaymentWebDigest = "sha256:2c7a9b30291d4adb9d94dcf0d047b86809ad487d1c7848ffc340b226497d578d";
    private const string PaymentWorkersImage = "ghcr.io/concertable/payment-workers";
    private const string PaymentWorkersDigest = "sha256:09b1d4d0f9bf175f61dcaafde0d06b7eb7f8a710e0e775d4b385957b9e865cfc";
    private const string B2BSeedingSimulatorImage = "ghcr.io/concertable/b2b-seeding-simulator";
    private const string B2BSeedingSimulatorDigest = "sha256:d6f7ad971e3ee7299e419360238528ee129e219561d2f0eb5eff491570b0db6b";

    public static IDistributedApplicationBuilder CreateBuilder(string[] args) =>
        ConfigureBuilder<Projects.Concertable_Customer_Web>(StrictDistributedApplication.CreateBuilder(args));

    public static IDistributedApplicationBuilder CreateE2EBuilder<TCustomerWeb>()
        where TCustomerWeb : IProjectMetadata, new() =>
        ConfigureBuilder<TCustomerWeb>(DistributedApplication.CreateBuilder(new DistributedApplicationOptions
        {
            Args = ["--environment", "Development"],
            AssemblyName = typeof(AppHost).Assembly.GetName().Name!,
            DisableDashboard = true,
        }));

    private static IDistributedApplicationBuilder ConfigureBuilder<TCustomerWeb>(IDistributedApplicationBuilder builder)
        where TCustomerWeb : IProjectMetadata, new()
    {
        var sql = builder.AddSqlServerContainer("concertable-customer-sql-data");
        var authDb = sql.AddDatabase(AuthConstants.Database);
        var customerDb = sql.AddDatabase(CustomerConstants.Database);
        var paymentDb = sql.AddDatabase(PaymentConstants.Database);
        var asb = builder.AddServiceBus();
        asb.Topology().AddCustomerTopology().AddSearchTopology().AddPaymentTopology().AddAuthTopology().RunAsEmulator();
        var auth = builder.AddAuth(AuthImage, AuthDigest, authDb, asb)
                          .WithContainerRuntimeArgs("--user", "root")
                          .WithHttpsEndpoint(targetPort: AuthConstants.ContainerPort, name: "https");
        auth.WithEndpoint("https", endpoint => endpoint.Port = 7093);
        auth.WithSpaClients(CustomerLocalSpaSurfaces.AuthClients);
        var paymentWeb = builder.AddPaymentWeb(PaymentWebImage, PaymentWebDigest, auth, paymentDb, asb);
        paymentWeb.WithEndpoint("https", endpoint => endpoint.Port = 7098);
        var customerWeb = builder.AddCustomerWeb<TCustomerWeb>(auth, customerDb, asb, paymentWeb);
        if (builder.ExecutionContext.IsRunMode)
            customerWeb.WithEnvironment(PaymentConstants.AllowInsecureHttpClientEnvironmentVariable, bool.TrueString);
        auth.WithEnvironment("ServiceAuth__AuthClientId", "concertable-auth");
        auth.WithEnvironment("Services__CustomerApiUrl", customerWeb.GetEndpoint("https"));
        builder.AddPaymentWorkers(PaymentWorkersImage, PaymentWorkersDigest, paymentDb, asb);
        builder.AddB2BSeedingSimulator(B2BSeedingSimulatorImage, B2BSeedingSimulatorDigest, asb);
        builder.AddCustomerSpa(customerWeb, customerWeb, auth);
        if (builder.AddMobileCustomer(customerWeb, auth, paymentWeb) is { } mobileTunnel)
            auth.WithMobilePublicUrl(mobileTunnel.GetEndpoint(auth, "https"));
        builder.AddStripeCli(paymentWeb);
        return builder;
    }
}
