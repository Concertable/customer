using Concertable.Customer.Ticket.Application.Errors;
using Concertable.Payment.Contracts.Errors;
using Reunion.Errors;

namespace Concertable.Customer.Ticket.UnitTests;

public sealed class TicketErrorDefinitionTests
{
    [Fact]
    public void Definition_PurchaseConcertNotFound_ReturnsPublishedContract()
    {
        PurchaseError error = new PurchaseError.ConcertNotFound(42);

        var definition = error.Definition;

        Assert.Equal("ticket.concert_not_found", definition.Code);
        Assert.Equal("Concert 42 was not found.", definition.Message);
        Assert.Equal(ErrorKind.NotFound, definition.Kind);
    }

    [Fact]
    public void Definition_PurchaseInvalid_ReturnsStructuredValidationContract()
    {
        PurchaseError error = new PurchaseError.Invalid(CreateValidationErrors("purchase"));

        var definition = Assert.IsType<ValidationError>(error.Definition);

        Assert.Equal("ticket.purchase_invalid", definition.Code);
        Assert.Equal("The ticket purchase is invalid.", definition.Message);
        Assert.Equal(ErrorKind.Invalid, definition.Kind);
        Assert.Equal(["Not enough tickets available."], definition.Errors.Errors["purchase"]);
    }

    [Fact]
    public void Definition_PurchasePaymentFailure_ForwardsOperationContract()
    {
        PurchaseError error = new PurchaseError.PaymentFailure(new PaymentOperationError.Declined());

        var definition = error.Definition;

        Assert.Equal("payment.operation.declined", definition.Code);
        Assert.Equal("The payment was declined.", definition.Message);
        Assert.Equal(ErrorKind.PaymentRequired, definition.Kind);
    }

    [Fact]
    public void Definition_CheckoutPaymentFailure_ForwardsOperationContract()
    {
        CheckoutError error = new CheckoutError.PaymentFailure(new PaymentOperationError.OperationConflict());

        var definition = error.Definition;

        Assert.Equal("payment.operation.conflict", definition.Code);
        Assert.Equal("The operation identity conflicts with an existing payment operation.", definition.Message);
        Assert.Equal(ErrorKind.Conflict, definition.Kind);
    }

    [Fact]
    public void Definition_CheckoutConcertNotFound_ReturnsPublishedContract()
    {
        CheckoutError error = new CheckoutError.ConcertNotFound(42);

        var definition = error.Definition;

        Assert.Equal("ticket.concert_not_found", definition.Code);
        Assert.Equal("Concert 42 was not found.", definition.Message);
        Assert.Equal(ErrorKind.NotFound, definition.Kind);
    }

    [Fact]
    public void Definition_CheckoutInvalid_ReturnsStructuredValidationContract()
    {
        CheckoutError error = new CheckoutError.Invalid(CreateValidationErrors("checkout"));

        var definition = Assert.IsType<ValidationError>(error.Definition);

        Assert.Equal("ticket.checkout_invalid", definition.Code);
        Assert.Equal("The ticket checkout is invalid.", definition.Message);
        Assert.Equal(ErrorKind.Invalid, definition.Kind);
        Assert.Equal(["Not enough tickets available."], definition.Errors.Errors["checkout"]);
    }

    [Fact]
    public void Definition_EligibilityConcertNotFound_ReturnsPublishedContract()
    {
        EligibilityError error = new EligibilityError.ConcertNotFound(42);

        var definition = error.Definition;

        Assert.Equal("ticket.concert_not_found", definition.Code);
        Assert.Equal("Concert 42 was not found.", definition.Message);
        Assert.Equal(ErrorKind.NotFound, definition.Kind);
    }

    private static ValidationErrors CreateValidationErrors(string field) =>
        new(new Dictionary<string, string[]> { [field] = ["Not enough tickets available."] });
}
