using Concertable.Payment.Client;
using Concertable.Payment.Contracts;
using Concertable.Payment.Contracts.Errors;
using Reunion;

namespace Concertable.Customer.IntegrationTests.Fixtures.Mocks;

public sealed class MockPaymentSessionClient : IPaymentSessionOperationsClient
{
    public List<PaymentSessionOperationRequest> Sessions { get; } = [];
    public PaymentOperationError? CreateError { get; set; }

    public void Reset()
    {
        Sessions.Clear();
        CreateError = null;
    }

    public Task<Result<PaymentMethodSetup, PaymentOperationError>> SetupPaymentMethodAsync(
        PaymentMethodSetupRequest request,
        CancellationToken ct = default) =>
        Task.FromResult(
            Result<PaymentMethodSetup, PaymentOperationError>.Failure(
                new PaymentOperationError.Unknown()));

    public Task<UnitResult<PaymentOperationError>> ValidatePaymentMethodAsync(
        PaymentMethodValidationRequest request,
        CancellationToken ct = default) =>
        Task.FromResult(
            UnitResult<PaymentOperationError>.Failure(
                new PaymentOperationError.Unknown()));

    public Task<Result<PaymentSessionDescriptor, PaymentOperationError>> CreateAsync(
        PaymentSessionOperationRequest request,
        CancellationToken ct = default)
    {
        Sessions.Add(request);
        if (CreateError is not null)
            return Task.FromResult(
                Result<PaymentSessionDescriptor, PaymentOperationError>.Failure(CreateError));

        return Task.FromResult(
            Result<PaymentSessionDescriptor, PaymentOperationError>.Success(
                new PaymentSessionDescriptor(
                    new PaymentOperationIdentity(request.OperationId, Guid.CreateVersion7(), 1),
                    request.Kind,
                    "payment-session-secret",
                    "customer-session-secret",
                    "customer-token")));
    }

    public Task<Result<PaymentSessionDescriptor, PaymentOperationError>> RetryAsync(
        PaymentSessionRetryRequest request,
        CancellationToken ct = default) =>
        Task.FromResult(
            Result<PaymentSessionDescriptor, PaymentOperationError>.Failure(
                new PaymentOperationError.Unknown()));

    public Task<Result<PaymentOperationSnapshot, PaymentOperationError>> GetStatusAsync(
        PaymentSessionStatusRequest request,
        CancellationToken ct = default) =>
        Task.FromResult(
            Result<PaymentOperationSnapshot, PaymentOperationError>.Failure(
                new PaymentOperationError.Unknown()));
}
