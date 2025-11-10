using Api;
using Api.Clients;
using Api.Models.Configuration;
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

builder.Services.Configure<JiraOptions>(builder.Configuration.GetSection(JiraOptions.SectionName));

builder.Services
    .AddScoped<Moniker>()
    .AddSingleton<HtmlSanitizer>()
    .AddSingleton<RoomManager>();

builder.Services.AddFastEndpoints(o => o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All);

var jiraOptions = builder.Configuration.GetSection(JiraOptions.SectionName).Get<JiraOptions>() ?? new JiraOptions();
builder.Services.AddRefitClient<IJiraApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(jiraOptions.ApiBaseUrl));
builder.Services.AddRefitClient<IJiraAuthApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri(jiraOptions.AuthBaseUrl));

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