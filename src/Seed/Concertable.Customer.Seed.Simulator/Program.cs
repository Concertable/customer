using Concertable.Customer.Seed.Simulator;

var builder = Host.CreateApplicationBuilder(args);
builder.AddCustomerSeedSimulator();

var app = builder.Build();
app.Run();
