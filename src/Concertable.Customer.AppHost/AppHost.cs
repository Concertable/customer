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
    private const string PaymentWebDigest = "sha256:24503f1c17bce5a67b4544455ee3886e76c6711f1eed57b5c6c74d2c45febdbd";
    private const string PaymentWorkersImage = "ghcr.io/concertable/payment-workers";
    private const string PaymentWorkersDigest = "sha256:5b31c52965054dce47c1cd8a1d1feabd976222f31be28bd8d185c281d014e52f";
    private const string B2BSeedingSimulatorImage = "ghcr.io/concertable/b2b-seeding-simulator";
    private const string B2BSeedingSimulatorDigest = "sha256:a232e5f6a111e3c81479c53cc79d49c54a0bf18c4dcb75a2cbaa7bf3ec1a0957";

    public static IDistributedApplicationBuilder CreateBuilder(string[] args) =>
        CreateBuilder<Projects.Concertable_Customer_Web>(args);

    public static IDistributedApplicationBuilder CreateBuilder<TCustomerWeb>(string[] args)
        where TCustomerWeb : IProjectMetadata, new()
    {
        var builder = StrictDistributedApplication.CreateBuilder(args);
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
