using Concertable.Customer.Hosting;
using Concertable.Payment.Hosting;
using Concertable.Search.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServerContainer("concertable-customer-sql-data");
var authDb = sql.AddDatabase(AuthConstants.Database);
var customerDb = sql.AddDatabase(CustomerConstants.Database);
var paymentDb = sql.AddDatabase(PaymentConstants.Database);
var b2bDb = sql.AddDatabase("B2BDb");

var asb = builder.AddServiceBus();

asb.Topology()
   .AddCustomerTopology()
   .AddSearchTopology()
   .AddPaymentTopology()
   .AddAuthTopology();

var auth = builder.AddAuth<Projects.Concertable_Auth>(authDb, b2bDb, asb);
auth.WithEndpoint("https", endpoint => endpoint.Port = 7093);

var paymentWeb = builder.AddPaymentWeb<Projects.Concertable_Payment_Web>(auth, paymentDb, asb);
paymentWeb.WithEndpoint("https", endpoint => endpoint.Port = 7098);

var customerWeb = builder.AddCustomerWeb<Projects.Concertable_Customer_Web>(auth, customerDb, asb, paymentWeb);

auth.WithEnvironment("ServiceAuth__AuthClientId", "concertable-auth");
auth.WithEnvironment("Services__CustomerApiUrl", customerWeb.GetEndpoint("https"));

builder.AddPaymentWorkers<Projects.Concertable_Payment_Workers>(paymentDb, asb);
builder.AddB2BSeedingSimulator<Projects.Concertable_B2B_Seed_Simulator>(asb);
builder.AddCustomerSpa(customerWeb, customerWeb, auth);
builder.AddMobileCustomer(customerWeb, auth, paymentWeb);
builder.AddStripeCli(paymentWeb);

builder.Build().Run();
