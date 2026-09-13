using Concertable.Customer.Review.Application.Requests;
using FluentValidation;

namespace Concertable.Customer.Review.Application.Validators;

internal sealed class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.Stars).InclusiveBetween((byte)1, (byte)5);
    }
}
