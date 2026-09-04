namespace Concertable.Customer.Hosting;

public static class CustomerLocalSpaSurfaces
{
    public static SpaSurface Customer { get; } = new("customer", 5174, "Customer");

    public static IReadOnlyList<SpaSurface> All { get; } = Array.AsReadOnly([Customer]);
}
