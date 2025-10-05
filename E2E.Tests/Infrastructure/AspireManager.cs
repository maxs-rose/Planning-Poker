using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Projects;

namespace E2E.Tests.Infrastructure;

public sealed class AspireManager : IAsyncLifetime
{
    internal PlaywriteManager PlaywriteManager { get; } = new();
    internal DistributedApplication App { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await PlaywriteManager.InitializeAsync();

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<AppHost>();

        App = await builder.BuildAsync();
        await App.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await PlaywriteManager.DisposeAsync();
        await App.DisposeAsync();
    }
}