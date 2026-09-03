using Aspire.Hosting;
using Concertable.Auth.Hosting;
using Concertable.B2B.Hosting;
using Concertable.Customer.Hosting;
using Concertable.Frontend.Hosting;
using Concertable.Payment.Hosting;
using Concertable.Search.Hosting;

public static class AppHost
{
    private const string AuthImage = "ghcr.io/concertable/auth";
    private const string AuthDigest = "sha256:8b7ba47efb319e6e1f1b5b86223d4075b9c8e09920933dae24fbf35f72851a63";
    private const string PaymentWebImage = "ghcr.io/concertable/payment-web";
    private const string PaymentWebDigest = "sha256:11f02cfa129cf82709dbceb438a281c7fa66b8594643a32a3d599142958fd696";
    private const string PaymentWorkersImage = "ghcr.io/concertable/payment-workers";
    private const string PaymentWorkersDigest = "sha256:dc9670dffdd9b8f63cbae682c9c81be2b52f0fad394aa239debed632009d144c";
    private const string B2BSeedingSimulatorImage = "ghcr.io/concertable/b2b-seeding-simulator";
    private const string B2BSeedingSimulatorDigest = "sha256:a232e5f6a111e3c81479c53cc79d49c54a0bf18c4dcb75a2cbaa7bf3ec1a0957";

    public static IDistributedApplicationBuilder CreateBuilder(string[] args)
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
        var paymentWeb = builder.AddPaymentWeb(PaymentWebImage, PaymentWebDigest, auth, paymentDb, asb)
                                .WithHttpEndpoint(targetPort: 8080, name: "https")
                                .WithHttpEndpoint(targetPort: 8080, name: "http");
        paymentWeb.WithEndpoint("https", endpoint => endpoint.Port = 7098);
        var customerWeb = builder.AddCustomerWeb<Projects.Concertable_Customer_Web>(auth, customerDb, asb, paymentWeb);
        auth.WithEnvironment("ServiceAuth__AuthClientId", "concertable-auth");
        auth.WithEnvironment("Services__CustomerApiUrl", customerWeb.GetEndpoint("https"));
        builder.AddPaymentWorkers(PaymentWorkersImage, PaymentWorkersDigest, paymentDb, asb);
        builder.AddB2BSeedingSimulator(B2BSeedingSimulatorImage, B2BSeedingSimulatorDigest, asb);
        builder.AddCustomerSpa(customerWeb, customerWeb, auth);
        builder.AddMobileCustomer(customerWeb, auth, paymentWeb);
        builder.AddStripeCli(paymentWeb);
        return builder;
    }
}
