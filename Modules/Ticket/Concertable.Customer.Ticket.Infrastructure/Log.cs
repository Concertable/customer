using Microsoft.Extensions.Logging;

namespace Concertable.Customer.Ticket.Infrastructure;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "[TicketPaymentProcessor] fromUserId={FromUserId}")]
    internal static partial void TicketPaymentProcessing(this ILogger logger, string fromUserId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ticket payment failed for user {UserId}: [{FailureCode}] {FailureMessage}")]
    internal static partial void TicketPaymentFailed(this ILogger logger, string userId, string? failureCode, string? failureMessage);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ticket {TicketId} not found for review submitted event")]
    internal static partial void TicketNotFoundForReviewEvent(this ILogger logger, Guid ticketId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Duplicate inbox message {MessageId}; skipping")]
    internal static partial void DuplicateInboxMessage(this ILogger logger, Guid messageId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to send ticket confirmation email to {Email} for tickets {TicketIds}")]
    internal static partial void TicketEmailFailed(this ILogger logger, Exception ex, string email, IReadOnlyList<Guid> ticketIds);
}
