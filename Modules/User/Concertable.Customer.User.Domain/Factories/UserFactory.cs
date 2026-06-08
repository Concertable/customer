namespace Concertable.Customer.User.Domain.Factories;

public static class UserFactory
{
    public static UserEntity FromRegistration(Guid id, string email) =>
        UserEntity.FromRegistration(id, email);
}
