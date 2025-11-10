using Api.Clients;
using Api.Contracts.Response;
using Api.Models.Configuration;
using FastEndpoints;
using JetBrains.Annotations;

namespace Api.Endpoints.Jira;

internal sealed class SearchTicketsEndpoint(
    IJiraApi jiraApi
) : Endpoint<SearchTicketsRequest, SearchTicketsResponse>
{
    
    public override void Configure()
    {
        Get("/jira/issues");
    }
    
    public override async Task HandleAsync(SearchTicketsRequest req, CancellationToken ct)
    {
        var accessToken = HttpContext.Session.GetString(JiraSession.TokenKey);
        if (string.IsNullOrEmpty(accessToken))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var issues = await jiraApi.SearchIssues(
            accessToken,
            req.ResourceId,
            req.Query,
            cancellationToken: ct);

        var tickets = issues.Sections.SelectMany(section =>
            section.Issues.Select(issue =>
                new SearchTicketsResult(
                    issue.Id,
                    section.IssueTypeLabel,
                    BuildAvatarUrl(req.ResourceUrl, issue.TypeAvatarUrl),
                    issue.Key,
                    issue.MatchSummary)))
            .ToArray();
        
        var response = new SearchTicketsResponse(
            tickets, 
            tickets.Length == 0 ? issues.Sections.FirstOrDefault()?.NoSuggestionsFoundMessage : null);
        
        await Send.OkAsync(response, cancellation: ct);
    }

    private static string BuildAvatarUrl(string resourceUrl, string url)
    {
        var baseUri = new Uri(resourceUrl);
        return $"{new Uri(baseUri, url)}";
    }
}

[PublicAPI]
public sealed record SearchTicketsRequest(
    [property: QueryParam] string ResourceId,
    [property: QueryParam] string ResourceUrl,
    [property: QueryParam] string Query);