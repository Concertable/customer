using Concertable.Application.Interfaces;

namespace Concertable.Customer.Web.Services;

internal sealed class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> logger;

    public FakeEmailService(ILogger<FakeEmailService> logger)
    {
        this.logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string body)
    {
        logger.LogInformation("[FakeEmail] To: {Email} | Subject: {Subject}\n{Body}", toEmail, subject, body);
        return Task.CompletedTask;
    }

    public Task SendTicketsToEmailAsync(string toEmail, IEnumerable<Guid> ticketIds)
    {
        logger.LogInformation("[FakeEmail] Tickets to: {Email} | TicketIds: {Ids}", toEmail, string.Join(", ", ticketIds));
        return Task.CompletedTask;
    }

    public Task SendVerificationAsync(string toEmail, string token, string verifyBaseUrl, CancellationToken ct = default)
    {
        logger.LogInformation("[FakeEmail] Verification email skipped for {Email} (Customer.Web does not own auth flows)", toEmail);
        return Task.CompletedTask;
    }
}
