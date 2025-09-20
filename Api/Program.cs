using Api;
using Api.Services;
using FastEndpoints;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, config) =>
{
    config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services);
});

builder.Services
    .AddScoped<Moniker>()
    .AddSingleton<RoomManager>();

builder.Services.AddFastEndpoints(o => o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All);

var app = builder.Build();

app.UseFastEndpoints(o =>
{
    o.Endpoints.RoutePrefix = "api";
    o.Endpoints.Configurator = x => { x.AllowAnonymous(); };
});


app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/api"))
    {
        await next(ctx);
        return;
    }

    await next(ctx);

    if (ctx.Response.StatusCode == 404)
        ctx.Response.Redirect("/");
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();