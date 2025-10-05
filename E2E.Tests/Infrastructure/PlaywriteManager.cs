using System.Diagnostics;
using Microsoft.Playwright;

namespace E2E.Tests.Infrastructure;

public sealed class PlaywriteManager : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private static bool IsDebugging => Debugger.IsAttached;
    internal IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();

        var options = new BrowserTypeLaunchOptions
        {
            Headless = !IsDebugging
        };

        Browser = await _playwright.Chromium.LaunchAsync(options);
    }

    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        await Browser.DisposeAsync();
        _playwright?.Dispose();
    }
}