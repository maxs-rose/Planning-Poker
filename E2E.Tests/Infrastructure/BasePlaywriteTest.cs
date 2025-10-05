using Aspire.Hosting.Testing;
using Microsoft.Playwright;

namespace E2E.Tests.Infrastructure;

public abstract class BasePlaywriteTest(AspireManager aspireManager) : IClassFixture<AspireManager>
{
    protected AspireManager AspireManager { get; } = aspireManager;
    protected PlaywriteManager PlaywriteManager => AspireManager.PlaywriteManager;

    protected async Task InteractWithPage(Func<IPage, CancellationToken, Task> test, ViewportSize? size = null, CancellationToken ct = default)
    {
        var clientUrl = AspireManager.App.GetEndpoint("Client");

        await AspireManager.App.ResourceNotifications.WaitForResourceHealthyAsync("Client", ct);

        await using var context = await GetPageContext(clientUrl, size);
        var page = await context.NewPageAsync();

        try
        {
            await test(page, ct);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private async Task<IBrowserContext> GetPageContext(Uri url, ViewportSize? size = null)
    {
        return await PlaywriteManager.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ColorScheme = ColorScheme.Dark,
            ViewportSize = size,
            BaseURL = url.ToString()
        });
    }
}