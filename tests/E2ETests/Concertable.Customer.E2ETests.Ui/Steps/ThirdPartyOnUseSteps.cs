using Concertable.Customer.E2ETests.Ui.Support;

namespace Concertable.Customer.E2ETests.Ui.Steps;

[Binding]
public sealed class ThirdPartyOnUseSteps
{
    private const string StripeJsHost = "js.stripe.com";
    private const string GoogleMapsHost = "maps.googleapis.com";

    private readonly UiFixture fixture;
    private readonly Browser browser;
    private readonly List<string> requestUrls = [];

    public ThirdPartyOnUseSteps(UiFixture fixture, Browser browser)
    {
        this.fixture = fixture;
        this.browser = browser;
    }

    [Given("a visitor is on the customer landing page")]
    public async Task VisitLandingPage()
    {
        CollectRequests();
        await browser.Page.GotoAsync(
            fixture.App.CustomerSpaUrl,
            new() { WaitUntil = WaitUntilState.NetworkIdle });
    }

    [Given("a visitor is on the find page")]
    public async Task VisitFindPage()
    {
        CollectRequests();
        await browser.Page.RunAndWaitForRequestAsync(
            () => browser.Page.GotoAsync(
                $"{fixture.App.CustomerSpaUrl}/find",
                new() { WaitUntil = WaitUntilState.NetworkIdle }),
            request => request.Url.Contains(GoogleMapsHost, StringComparison.OrdinalIgnoreCase),
            new() { Timeout = 15_000 });
    }

    [Then("Stripe.js is not requested")]
    public void StripeNotRequested() =>
        Assert.DoesNotContain(requestUrls, url => url.Contains(StripeJsHost, StringComparison.OrdinalIgnoreCase));

    [Then("Google Maps is not requested")]
    public void MapsNotRequested() =>
        Assert.DoesNotContain(requestUrls, url => url.Contains(GoogleMapsHost, StringComparison.OrdinalIgnoreCase));

    [Then("Google Maps is requested")]
    public void MapsRequested() =>
        Assert.Contains(requestUrls, url => url.Contains(GoogleMapsHost, StringComparison.OrdinalIgnoreCase));

    [Then("no Stripe fraud cookie is set")]
    public async Task NoStripeFraudCookieSet()
    {
        var cookies = await browser.Context.CookiesAsync();
        Assert.DoesNotContain(
            cookies,
            cookie => cookie.Name.StartsWith("__stripe", StringComparison.OrdinalIgnoreCase));
    }

    private void CollectRequests() =>
        browser.Page.Request += (_, request) => requestUrls.Add(request.Url);
}
