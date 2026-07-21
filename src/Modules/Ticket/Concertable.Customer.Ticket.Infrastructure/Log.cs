using Microsoft.Extensions.Logging;

namespace Concertable.Customer.Ticket.Infrastructure;

internal static partial class Log
{
    #region Payment processors

    [LoggerMessage(Level = LogLevel.Information, Message = "[TicketPaymentProcessor] fromUserId={FromUserId}")]
    internal static partial void TicketPaymentProcessing(this ILogger logger, string fromUserId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ticket payment failed for user {UserId}: [{FailureCode}] {FailureMessage}")]
    internal static partial void TicketPaymentFailed(this ILogger logger, string userId, string? failureCode, string? failureMessage);

    #endregion

    #region CustomerReviewSubmittedEventHandler

    [LoggerMessage(Level = LogLevel.Warning, Message = "Ticket {TicketId} not found for review submitted event")]
    internal static partial void TicketNotFoundForReviewEvent(this ILogger logger, Guid ticketId);

    #endregion

    #region Shared

    [LoggerMessage(Level = LogLevel.Debug, Message = "Duplicate inbox message {MessageId}; skipping")]
    internal static partial void DuplicateInboxMessage(this ILogger logger, Guid messageId);

    #endregion
}
