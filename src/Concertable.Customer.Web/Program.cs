using Concertable.Customer.Web;

var builder = WebApplication.CreateBuilder(args);
builder.AddCustomerWebHost();

var app = builder.Build();
await app.UseCustomerWebHost();

app.Run();

public sealed partial class Program
{ }
