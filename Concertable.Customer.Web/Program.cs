using Concertable.Customer.Concert.Infrastructure.Extensions;
using Concertable.Customer.Ticket.Infrastructure.Extensions;
using Concertable.Customer.Review.Infrastructure.Extensions;
using Concertable.Customer.Profile.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllers();

builder.Services.AddCustomerConcertModule(builder.Configuration);
builder.Services.AddCustomerTicketModule(builder.Configuration);
builder.Services.AddCustomerReviewModule(builder.Configuration);
builder.Services.AddCustomerProfileModule(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapControllers();

app.Run();

public partial class Program { }
