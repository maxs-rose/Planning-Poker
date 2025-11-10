using Api.Clients;
using Api.Contracts.Request;
using Api.Models.Configuration;
using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Api.Endpoints.Jira;

internal sealed class CallbackEndpoint(
    IOptions<JiraOptions> jiraOptions,
    IJiraAuthApi jiraAuthApi
) : Endpoint<CallbackRequest>
{
    private readonly JiraOptions _options = jiraOptions.Value;
    
    public override void Configure()
    {
        Get("/jira/callback");
        ResponseCache(0, ResponseCacheLocation.None, noStore: true);
    }
    
    public override async Task HandleAsync(CallbackRequest req, CancellationToken ct)
    {
        var storedState = HttpContext.Session.GetString(JiraSession.AuthStateKey);

        if (storedState != req.State)
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        
        HttpContext.Session.Remove(JiraSession.AuthStateKey);

        var body = new JiraTokenRequest(
            _options.ClientId,
            _options.ClientSecret,
            req.Code,
            $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{JiraOptions.CallbackUrl}");
        
        var jiraToken = await jiraAuthApi.GetToken(body, ct);
        HttpContext.Session.SetString(JiraSession.TokenKey, jiraToken.AccessToken);

        await Send.RedirectAsync("/");
    }
}

[PublicAPI]
public sealed record CallbackRequest([property: QueryParam] string Code, [property: QueryParam] string State);