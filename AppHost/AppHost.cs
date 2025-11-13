using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Api>("Api")
    .WithEnvironment("Serilog__WriteTo__0__Args__Configure__0__Args__theme", "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Sixteen, Serilog.Sinks.Console")
    .WithEnvironment("Serilog__WriteTo__0__Args__Configure__0__Args__applyThemeToRedirectedOutput", "true");

builder.AddViteApp("Client", Path.Join(builder.AppHostDirectory, "..", "Client"))
    .WithPnpm()
    .WithEnvironment("VITE_BACKEND_API_URL", api.GetEndpoint("https"))
    .WaitFor(api);

builder.Build().Run();