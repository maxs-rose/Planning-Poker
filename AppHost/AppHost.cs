using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Api>("Api")
    .WithEnvironment("Serilog__WriteTo__0__Args__Configure__0__Args__theme", "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Sixteen, Serilog.Sinks.Console")
    .WithEnvironment("Serilog__WriteTo__0__Args__Configure__0__Args__applyThemeToRedirectedOutput", "true");

builder.AddViteApp("Client", Path.Join(builder.AppHostDirectory, "..", "Client"), "pnpm")
    .WaitFor(api);

builder.Build().Run();