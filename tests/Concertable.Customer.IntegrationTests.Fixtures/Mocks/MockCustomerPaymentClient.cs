using Concertable.Kernel.ValueObjects;
using Concertable.Payment.Client;
using Concertable.Payment.Contracts;
using Concertable.Payment.Contracts.Errors;
using Reunion;

namespace Concertable.Customer.IntegrationTests.Fixtures;

internal sealed class MockCustomerPaymentClient : ICustomerPaymentOperationsClient
{
    public Task<Result<PaymentOutcome, PaymentError>> PayAsync(Guid payerId, int concertId, Guid payeeId, Money amount, IReadOnlyDictionary<string, string> metadata, string paymentMethodId, CancellationToken ct = default) =>
        Task.FromResult(Result<PaymentOutcome, PaymentError>.Success(new PaymentOutcome { RequiresAction = false, TransactionId = "pi_mock_pay" }));

    public Task<CheckoutSession> CreatePaymentSessionAsync(Guid payerId, int concertId, Guid payeeId, IReadOnlyDictionary<string, string> metadata, CancellationToken ct = default) =>
        Task.FromResult(new CheckoutSession("pi_mock_secret", "cuss_mock_secret", "cus_mock"));
}
