using Api.JiraIntegration.Contracts.Request;
using Api.JiraIntegration.Options;
using Api.Options;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Api.Endpoints.Jira;

internal sealed class LoginEndpoint(
    IOptions<JiraOptions> jiraOptions
) : EndpointWithoutRequest
{
    private readonly JiraOptions _options = jiraOptions.Value;
    
    public override void Configure()
    {
        Get("/jira/login");
        FeatureFlag<JiraEnabledFlag>();
        ResponseCache(0, ResponseCacheLocation.None, noStore: true);
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var state = $"{Guid.NewGuid():D}";
        
        HttpContext.Session.SetString(JiraSession.AuthStateKey, state);

        var nextUrl = $"{_options.AuthBaseUrl}{_options.AuthorizeUrl}";
        var queryParams = new JiraAuthorizeRequest(_options.Audience, _options.ClientId, _options.Scope, 
            $"https://{HttpContext.Request.Host}{JiraOptions.CallbackUrl}", state, "code", 
            "consent");
        
        var redirectUri = QueryHelpers.AddQueryString(nextUrl, queryParams.ToDictionary());
        await Send.RedirectAsync(redirectUri, true, true);
    }
}