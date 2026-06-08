using Concertable.Customer.Ticket.Application.Requests;
using FluentValidation;

namespace Concertable.Customer.Ticket.Application.Validators;

internal sealed class TicketPurchaseParamsValidator : AbstractValidator<TicketPurchaseParams>
{
    public TicketPurchaseParamsValidator()
    {
        RuleFor(x => x.ConcertId)
            .GreaterThan(0)
            .WithMessage("Concert ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be at least 1");

        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage("Payment method is required");
    }
}
