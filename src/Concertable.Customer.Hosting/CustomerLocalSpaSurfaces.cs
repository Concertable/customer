namespace Concertable.Customer.Hosting;

public static class CustomerLocalSpaSurfaces
{
    public static SpaSurface Customer { get; } = new("customer", 5174);

    public static IReadOnlyList<SpaSurface> All { get; } = Array.AsReadOnly([Customer]);

    public static IReadOnlyList<(SpaSurface Surface, string ClientName)> AuthClients { get; } =
        Array.AsReadOnly<(SpaSurface Surface, string ClientName)>([(Customer, "Customer")]);
}
