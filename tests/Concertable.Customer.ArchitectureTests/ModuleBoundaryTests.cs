using Concertable.Customer.Web;
using Concertable.Testing;
using Xunit;

namespace Concertable.Customer.ArchitectureTests;

public sealed class ModuleBoundaryTests
{
    [Fact]
    public void Web_ReferencesNoModuleInfrastructureAssembly() =>
        Assert.Empty(typeof(CustomerWebHostExtensions).Assembly.ModuleInfrastructureReferences("Seed"));
}
