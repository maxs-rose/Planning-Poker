using System.Net;
using Api.JiraIntegration.Clients;
using Api.JiraIntegration.Options;
using Api.Options;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Api.Endpoints.Jira;

internal sealed class LoggedInEndpoint(
    IJiraApi jiraApi,
    ILogger<LoggedInEndpoint> logger
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

        try
        {
            await jiraApi.GetAccessibleResources(accessToken, ct);
        }
        catch (ApiException ex) when(ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(ex, "Jira API - Unauthorized");
            await Send.UnauthorizedAsync(ct);
            return;
        }
        
        await Send.OkAsync(cancellation: ct);
    }
}