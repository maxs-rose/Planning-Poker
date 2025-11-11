using Api;
using Api.JiraIntegration;
using Api.Services;
using FastEndpoints;
using Ganss.Xss;
using Refit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, config) =>
{
    config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services);
});

builder.Services.ConfigureJira(builder.Configuration);

builder.Services
    .AddScoped<Moniker>()
    .AddSingleton<HtmlSanitizer>()
    .AddSingleton<RoomManager>();

builder.Services.AddFastEndpoints(o => o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All);
builder.Services.AddJiraClients(builder.Configuration);

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseResponseCaching().UseFastEndpoints(o => 
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

app.UseSession();

app.Run();