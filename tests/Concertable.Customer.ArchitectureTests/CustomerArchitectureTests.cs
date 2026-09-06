using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Concertable.Auth.Hosting;
using Concertable.Customer.Hosting;
using Concertable.Customer.Hosting.Frontend;
using Concertable.Customer.Web;
using Concertable.Payment.Hosting;
using Concertable.Testing;
using Concertable.Testing.Architecture;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Concertable.Customer.ArchitectureTests;

public sealed class CustomerArchitectureTests
{
    [Fact]
    public void Web_ProductionGraphAndStrictValidation_AreValid()
    {
        var builder = WebApplication.CreateBuilder(CompositionTestArguments.Create());
        builder.AddCustomerWebHost();
        using var app = builder.Build();
        builder.Services.ValidateComposition(app.Services, new CompositionValidationOptions
        {
            RootAssemblies = [typeof(CustomerWebHostExtensions).Assembly]
        });
        var jwtOptions = app.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);
        Assert.False(jwtOptions.RequireHttpsMetadata);
        var invalidBuilder = WebApplication.CreateBuilder(CompositionTestArguments.Create());
        invalidBuilder.AddCustomerWebHost();
        invalidBuilder.Services.AddInvalidLifetimeGraph();
        Assert.ThrowsAny<Exception>(() => invalidBuilder.Build());
    }

    [Fact]
    public void Web_ProductionEnvironment_RequiresHttpsMetadata()
    {
        var arguments = CompositionTestArguments.Create();
        arguments[0] = "--environment=Production";
        var builder = WebApplication.CreateBuilder(arguments);
        builder.AddCustomerWebHost();
        using var app = builder.Build();
        var jwtOptions = app.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.True(jwtOptions.RequireHttpsMetadata);
    }

    [Fact]
    public async Task AppHost_ProductionGraphAndStrictValidation_AreValid()
    {
        var validBuilder = AppHost.CreateBuilder([]);
        AssertImageEndpoint(validBuilder, AuthConstants.Resource, "https", scheme: "https");
        AssertContainerRuntimeArgs(validBuilder, AuthConstants.Resource, "--user", "root");
        AssertUsesDeveloperCertificate(validBuilder, AuthConstants.Resource);
        AssertImageEndpoint(
            validBuilder,
            PaymentConstants.WebResource,
            "https",
            scheme: "http",
            targetPort: PaymentConstants.HttpPort);
        AssertImageEndpoint(validBuilder, PaymentConstants.WebResource, "http");
        AssertImageEndpoint(validBuilder, PaymentConstants.WebResource, "grpc", targetPort: PaymentConstants.GrpcPort);
        var payment = validBuilder.Resources.Single(resource => resource.Name == PaymentConstants.WebResource);
        var paymentEnvironment = await GetRawEnvironmentAsync(payment, CancellationToken.None);
        Assert.Equal("8080;8081", paymentEnvironment["ASPNETCORE_HTTP_PORTS"]);
        Assert.Equal("8081", paymentEnvironment["PaymentTransport__GrpcPort"]);
        var customer = validBuilder.Resources.Single(resource => resource.Name == CustomerConstants.WebResource);
        var customerEnvironment = await GetRawEnvironmentAsync(customer, CancellationToken.None);
        Assert.Equal(bool.TrueString, customerEnvironment[PaymentConstants.AllowInsecureHttpClientEnvironmentVariable]);
        Assert.DoesNotContain(validBuilder.Resources.OfType<NodeAppResource>(),
            resource => resource.Name.StartsWith("mobile-", StringComparison.Ordinal));
        Assert.DoesNotContain(validBuilder.Resources, resource => resource.Name == "customer-dev");
        var auth = validBuilder.Resources.Single(resource => resource.Name == AuthConstants.Resource);
        var authEnvironment = await GetRawEnvironmentAsync(auth, CancellationToken.None);
        Assert.DoesNotContain("Auth__PublicUrl", authEnvironment.Keys);
        using var app = validBuilder.Build();
        var builder = AppHost.CreateBuilder([]);
        builder.Services.AddInvalidLifetimeGraph();
        Assert.ThrowsAny<Exception>(() => builder.Build());
    }

    [Fact]
    public async Task AppHost_PublishGraphWithStripeCli_IsValid()
    {
        var builder = AppHost.CreateBuilder(
            ["--publisher", "manifest", "--Stripe:SecretKey=sk_test_composition"]);

        Assert.True(builder.ExecutionContext.IsPublishMode);
        Assert.Single(builder.Resources, resource => resource.Name == PaymentConstants.StripeCliResource);
        var customer = builder.Resources.Single(resource => resource.Name == CustomerConstants.WebResource);
        var customerEnvironment = await GetRawEnvironmentAsync(customer, CancellationToken.None);
        Assert.DoesNotContain(PaymentConstants.AllowInsecureHttpClientEnvironmentVariable, customerEnvironment.Keys);
        using var app = builder.Build();
    }

    [Fact]
    public async Task AppHost_MobileGraph_ContainsOnlyCustomerSurfaces()
    {
        var builder = AppHost.CreateBuilder(["--RunMobile=true"]);
        Assert.Equal(
            new[] { "customer", "mobile-customer" },
            builder.Resources.OfType<NodeAppResource>().Select(resource => resource.Name).Order());
        AssertNodeAppDirectory(builder, "customer", "app", "web", "customer");
        AssertNodeAppDirectory(builder, "mobile-customer", "app", "mobile", "customer");
        Assert.Single(builder.Resources, resource => resource.Name == "customer-dev");
        Assert.DoesNotContain(builder.Resources, resource => resource.Name is "b2b-web" or "search-web");
        using var app = builder.Build();

        AllocateTunnelEndpoints(builder, "customer-dev");
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var mobile = Assert.Single(builder.Resources.OfType<NodeAppResource>(), resource => resource.Name == "mobile-customer");
        AssertClearMetroCacheCommand(mobile);
        var mobileEnvironment = await GetResolvedEnvironmentAsync(mobile, cancellation.Token);
        Assert.Equal("localhost", mobileEnvironment["REACT_NATIVE_PACKAGER_HOSTNAME"]);
        AssertTunnelUrl(mobileEnvironment, "EXPO_PUBLIC_API_URL", "customer-dev-customer-web-http");
        AssertTunnelUrl(mobileEnvironment, "EXPO_PUBLIC_AUTH_AUTHORITY", "customer-dev-auth-https");
        AssertTunnelUrl(mobileEnvironment, "EXPO_PUBLIC_CUSTOMER_API_URL", "customer-dev-customer-web-http");
        AssertTunnelUrl(mobileEnvironment, "EXPO_PUBLIC_PAYMENT_API_URL", "customer-dev-payment-web-https");
        Assert.DoesNotContain("EXPO_PUBLIC_SEARCH_API_URL", mobileEnvironment.Keys);

        var auth = builder.Resources.Single(resource => resource.Name == AuthConstants.Resource);
        var authEnvironment = await GetRawEnvironmentAsync(auth, cancellation.Token);
        await AssertTunnelValueAsync(authEnvironment, "Auth__PublicUrl", "customer-dev-auth-https", cancellation.Token);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FrontendWorkspaces_ExtractedAndMonorepoLayouts_ResolveEveryProductionCandidate(
        bool includeMonorepoLayout)
    {
        var root = Directory.CreateTempSubdirectory("concertable-customer-frontend-");
        try
        {
            CreateDirectories(root, [["app", "web"], ["app", "mobile"]]);
            if (includeMonorepoLayout)
                CreateDirectories(root, [["app", "web", "customer"], ["app", "mobile", "customer"]]);

            var builder = CreateFrontendBuilder(root);
            IResourceBuilder<IResourceWithServiceDiscovery> api =
                builder.AddResource(new ServiceContainerResource("customer-api"));
            IResourceBuilder<IResourceWithServiceDiscovery> auth =
                builder.AddResource(new ServiceContainerResource("auth"));
            IResourceBuilder<IResourceWithServiceDiscovery> payment =
                builder.AddResource(new ServiceContainerResource("payment"));
            builder.AddCustomerSpa(api, api, auth);
            Assert.NotNull(builder.AddMobileCustomer(api, auth, payment));

            AssertNodeAppDirectory(builder, "customer",
                includeMonorepoLayout ? ["app", "web", "customer"] : ["app", "web"]);
            AssertNodeAppDirectory(builder, "mobile-customer",
                includeMonorepoLayout ? ["app", "mobile", "customer"] : ["app", "mobile"]);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task AppHost_CustomerSpaOrigin_MatchesAuthRegistration()
    {
        var builder = AppHost.CreateBuilder([]);
        var surface = CustomerLocalSpaSurfaces.Customer;
        var registration = Assert.Single(CustomerLocalSpaSurfaces.AuthClients);
        Assert.Equal((surface, "Customer"), registration);
        var spa = Assert.Single(builder.Resources.OfType<NodeAppResource>());
        var endpoint = Assert.Single(spa.Annotations.OfType<EndpointAnnotation>());
        Assert.Equal(surface.ResourceName, spa.Name);
        Assert.Equal(5174, endpoint.Port);
        Assert.Equal("https", endpoint.UriScheme);
        var auth = Assert.IsAssignableFrom<IResourceWithEnvironment>(
            builder.Resources.Single(resource => resource.Name == AuthConstants.Resource));
        var configuration = await ExecutionConfigurationBuilder.Create(auth)
            .WithEnvironmentVariablesConfig()
            .BuildAsync(new DistributedApplicationExecutionContext(DistributedApplicationOperation.Publish),
                NullLogger.Instance, CancellationToken.None);
        var environment = configuration.EnvironmentVariables.ToDictionary();
        Assert.Equal("true", environment["Auth__SpaClients__RestrictToEnabledClients"]);
        Assert.Equal("Customer", environment["Auth__SpaClients__EnabledClients__0"]);
        Assert.DoesNotContain("Auth__SpaClients__EnabledClients__1", environment.Keys);
        Assert.Equal(surface.Origin + "/auth/callback", environment["Auth__SpaClients__Customer__RedirectUri"]);
        Assert.DoesNotContain(environment.Keys, key => key.StartsWith("Auth__SpaClients__Venue__", StringComparison.Ordinal));
    }

    [Fact]
    public void Web_ReferencesNoModuleInfrastructureAssembly() =>
        Assert.Empty(typeof(CustomerWebHostExtensions).Assembly.ModuleInfrastructureReferences("Seed"));

    private static void AssertContainerRuntimeArgs(
        IDistributedApplicationBuilder builder,
        string resourceName,
        params object[] expected)
    {
        var resource = Assert.IsType<ServiceContainerResource>(
            builder.Resources.Single(resource => resource.Name == resourceName));
        var args = new List<object>();
        foreach (var annotation in resource.Annotations.OfType<ContainerRuntimeArgsCallbackAnnotation>())
            annotation.Callback(new ContainerRuntimeArgsCallbackContext(args, CancellationToken.None))
                .GetAwaiter()
                .GetResult();

        Assert.Equal(expected, args);
    }

    private static IDistributedApplicationBuilder CreateFrontendBuilder(DirectoryInfo root) =>
        DistributedApplication.CreateBuilder(new DistributedApplicationOptions
        {
            Args = ["--environment", "Development", "--RunMobile=true"],
            DisableDashboard = true,
            ProjectDirectory = root.FullName
        });

    private static void CreateDirectories(DirectoryInfo root, IReadOnlyList<string[]> relativePaths)
    {
        foreach (var relativePath in relativePaths)
            Directory.CreateDirectory(Path.Combine([root.FullName, .. relativePath]));
    }

    private static void AllocateTunnelEndpoints(IDistributedApplicationBuilder builder, string tunnelName)
    {
        var ports = builder.Resources.OfType<Aspire.Hosting.DevTunnels.DevTunnelPortResource>()
            .Where(port => port.Name.StartsWith(tunnelName + "-", StringComparison.Ordinal))
            .ToArray();
        Assert.NotEmpty(ports);
        foreach (var port in ports)
            foreach (var endpoint in port.Annotations.OfType<EndpointAnnotation>())
                endpoint.AllocatedEndpoint = new AllocatedEndpoint(endpoint, $"{port.Name}.example.test", 443);
    }

    private static void AssertNodeAppDirectory(
        IDistributedApplicationBuilder builder,
        string resourceName,
        params string[] relativePath)
    {
        var repoRoot = new DirectoryInfo(builder.AppHostDirectory);
        while (repoRoot is not null && !Directory.Exists(Path.Combine(repoRoot.FullName, "app")))
            repoRoot = repoRoot.Parent;

        Assert.NotNull(repoRoot);
        var expected = Path.GetFullPath(Path.Combine([repoRoot.FullName, .. relativePath]));
        var resource = Assert.Single(builder.Resources.OfType<NodeAppResource>(), resource => resource.Name == resourceName);
        var actual = Path.GetFullPath(resource.WorkingDirectory);
        Assert.True(string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase),
            $"Expected '{resourceName}' to use '{expected}', but it uses '{actual}'.");
        Assert.True(Directory.Exists(actual), $"Frontend directory '{actual}' does not exist.");
    }

    private static void AssertClearMetroCacheCommand(NodeAppResource mobile)
    {
        var command = Assert.Single(mobile.Annotations.OfType<ResourceCommandAnnotation>(),
            command => command.Name == "clear-metro-cache");
        Assert.Equal("Clear Metro Cache", command.DisplayName);
        Assert.Equal("ArrowCounterclockwise", command.IconName);
    }

    private static async Task<Dictionary<string, string>> GetResolvedEnvironmentAsync(
        IResource resource, CancellationToken cancellationToken)
    {
        var environmentResource = Assert.IsAssignableFrom<IResourceWithEnvironment>(resource);
        var configuration = await ExecutionConfigurationBuilder.Create(environmentResource)
            .WithEnvironmentVariablesConfig()
            .BuildAsync(new DistributedApplicationExecutionContext(DistributedApplicationOperation.Run),
                NullLogger.Instance, cancellationToken);
        return configuration.EnvironmentVariables.ToDictionary();
    }

    private static async Task<Dictionary<string, object>> GetRawEnvironmentAsync(
        IResource resource, CancellationToken cancellationToken)
    {
        var environment = new Dictionary<string, object>();
        var context = new EnvironmentCallbackContext(
            new DistributedApplicationExecutionContext(DistributedApplicationOperation.Run),
            resource, environment, cancellationToken);
        foreach (var annotation in resource.Annotations.OfType<EnvironmentCallbackAnnotation>().ToArray())
            await annotation.Callback(context);
        return environment;
    }

    private static void AssertTunnelUrl(
        IReadOnlyDictionary<string, string> environment,
        string key,
        string endpointName)
    {
        Assert.True(environment.TryGetValue(key, out var url),
            $"Missing '{key}'. Available keys: {string.Join(", ", environment.Keys.Order())}");
        Assert.Equal($"https://{endpointName}.example.test:443", url);
    }

    private static async Task AssertTunnelValueAsync(
        IReadOnlyDictionary<string, object> environment,
        string key,
        string endpointName,
        CancellationToken cancellationToken)
    {
        var url = await Assert.IsAssignableFrom<IValueProvider>(environment[key]).GetValueAsync(cancellationToken);
        Assert.Equal($"https://{endpointName}.example.test:443", url);
    }

#pragma warning disable ASPIRECERTIFICATES001 // experimental API; asserts the temporary Auth image bridge
    private static void AssertUsesDeveloperCertificate(
        IDistributedApplicationBuilder builder,
        string resourceName)
    {
        var resource = Assert.IsType<ServiceContainerResource>(
            builder.Resources.Single(resource => resource.Name == resourceName));
        var certificate = Assert.Single(resource.Annotations.OfType<HttpsCertificateAnnotation>());

        Assert.True(certificate.UseDeveloperCertificate);
    }
#pragma warning restore ASPIRECERTIFICATES001

    private static void AssertImageEndpoint(
        IDistributedApplicationBuilder builder,
        string resourceName,
        string endpointName,
        string scheme = "http",
        int targetPort = PaymentConstants.HttpPort)
    {
        var resource = Assert.IsType<ServiceContainerResource>(
            builder.Resources.Single(resource => resource.Name == resourceName));
        var endpoint = Assert.Single(
            resource.Annotations.OfType<EndpointAnnotation>(),
            endpoint => endpoint.Name == endpointName);

        Assert.Equal(endpointName, endpoint.Name);
        Assert.Equal(scheme, endpoint.UriScheme);
        Assert.Equal(targetPort, endpoint.TargetPort);
    }
}
