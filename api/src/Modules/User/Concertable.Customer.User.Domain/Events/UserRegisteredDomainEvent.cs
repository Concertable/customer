using Concertable.Kernel;

namespace Concertable.Customer.User.Domain.Events;

public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Email) : IDomainEvent;
