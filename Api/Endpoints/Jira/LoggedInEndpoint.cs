using Api.JiraIntegration.Clients;
using Api.JiraIntegration.Options;
using Api.Options;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints.Jira;

internal sealed class LoggedInEndpoint(
    IJiraApi jiraApi
) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Head("/jira/user");
        FeatureFlag<JiraEnabledFlag>();
        ResponseCache(0, ResponseCacheLocation.None, noStore: true);
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var accessToken = HttpContext.Session.GetString(JiraSession.TokenKey);
        if (string.IsNullOrEmpty(accessToken))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await jiraApi.GetAccessibleResources(accessToken, ct);
        
        await Send.OkAsync(cancellation: ct);
    }
}