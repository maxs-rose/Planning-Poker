using Api.Clients;
using Api.Contracts.Response;
using Api.Models.Configuration;
using FastEndpoints;

namespace Api.Endpoints.Jira;

internal sealed class ResourcesEndpoint(
    IJiraApi jiraApi
) : EndpointWithoutRequest<IEnumerable<JiraResourceDto>>
{
    
    public override void Configure()
    {
        Get("/jira/resources");
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var accessToken = HttpContext.Session.GetString(JiraSession.TokenKey);
        if (string.IsNullOrEmpty(accessToken))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var response = await jiraApi.GetAccessibleResources(accessToken, ct);
        
        await Send.OkAsync(response, cancellation: ct);
    }
}