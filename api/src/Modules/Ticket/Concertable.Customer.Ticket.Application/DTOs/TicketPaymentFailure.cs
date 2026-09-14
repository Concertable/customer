using Concertable.Payment.Contracts;

namespace Concertable.Customer.Ticket.Application.DTOs;

internal sealed record TicketPaymentFailure(
    PaymentOperationReference Reference,
    string? FailureCode,
    string? FailureMessage);
