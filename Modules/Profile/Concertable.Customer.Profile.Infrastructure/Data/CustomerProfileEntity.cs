namespace Concertable.Customer.Profile.Infrastructure.Data;

internal class CustomerProfileEntity
{
    private CustomerProfileEntity() { }

    public CustomerProfileEntity(Guid sub)
    {
        Sub = sub;
    }

    public Guid Sub { get; private set; }
}
