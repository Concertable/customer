using Concertable.Payment.Client;
using FluentResults;

namespace Concertable.Customer.IntegrationTests.Fixtures;

internal sealed class MockCustomerPaymentClient : ICustomerPaymentClient
{
    public Task<Result<PaymentResponse>> PayAsync(Guid payerId, int concertId, Guid payeeId, decimal amount, IDictionary<string, string> metadata, string paymentMethodId, CancellationToken ct = default) =>
        Task.FromResult(Result.Ok(new PaymentResponse { RequiresAction = false, TransactionId = "pi_mock_pay" }));

    public Task<CheckoutSession> CreatePaymentSessionAsync(Guid payerId, int concertId, Guid payeeId, IDictionary<string, string> metadata, CancellationToken ct = default) =>
        Task.FromResult(new CheckoutSession("pi_mock_secret", "cuss_mock_secret", "cus_mock"));
}
