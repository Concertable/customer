var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServerContainer("concertable-customer-sql-data");
var authDb = sql.AddDatabase("AuthDb");
var customerDb = sql.AddDatabase("CustomerDb");
var searchDb = sql.AddDatabase("SearchDb");
var paymentDb = sql.AddDatabase("PaymentDb");
var b2bDb = sql.AddDatabase("B2BDb");

var asb = builder.AddServiceBus();

asb.Topology()
   .AddCustomerTopology()
   .AddSearchTopology()
   .AddPaymentTopology();

var auth = builder.AddAuth<Projects.Concertable_Auth>(authDb, b2bDb, asb);
var paymentWeb = builder.AddPaymentWeb<Projects.Concertable_Payment_Web>(auth, paymentDb, asb);
var customerWeb = builder.AddCustomerWeb<Projects.Concertable_Customer_Web>(auth, customerDb, asb, paymentWeb);

auth.WithEnvironment("ServiceAuth__AuthClientId", "concertable-auth");
auth.WithEnvironment("Services__CustomerApiUrl", customerWeb.GetEndpoint("https"));

builder.AddPaymentWorkers<Projects.Concertable_Payment_Workers>(paymentDb, asb);
builder.AddSearchWeb<Projects.Concertable_Search_Web>(auth, searchDb);
builder.AddSearchWorkers<Projects.Concertable_Search_Workers>(searchDb, asb);
builder.AddB2BSeedingSimulator<Projects.Concertable_B2B_Seed_Simulator>(asb);
builder.AddCustomerSpa(customerWeb, customerWeb, auth);
builder.AddMobileCustomer(customerWeb, auth);
builder.AddStripeCli(paymentWeb);

builder.Build().Run();
