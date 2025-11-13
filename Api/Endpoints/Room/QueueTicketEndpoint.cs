using Api.Contracts.Response;
using Api.JiraIntegration.Clients;
using Api.JiraIntegration.Contracts.Request;
using Api.JiraIntegration.Contracts.Response;
using Api.JiraIntegration.Options;
using Api.Models;
using Api.Options;
using Api.Services;
using FastEndpoints;
using Ganss.Xss;
using JetBrains.Annotations;

namespace Api.Endpoints.Room;

internal sealed class QueueTicketEndpoint(RoomManager roomManager, IJiraApi jiraApi, HtmlSanitizer sanitizer) : Endpoint<QueueTicketRequest>
{
    public override void Configure()
    {
        Post("/rooms/{Code}/queue");
        FeatureFlag<JiraEnabledFlag>();
    }

    public override async Task HandleAsync(QueueTicketRequest req, CancellationToken ct)
    {
        var room = roomManager.GetRoom(Route<string>("Code")!)!;
        var accessToken = HttpContext.Session.GetString(JiraSession.TokenKey);
        if (string.IsNullOrEmpty(accessToken))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        
        var requestBody = new JiraGetIssueBulkRequest(req.Ids);
        
        var resourceUrl = (await jiraApi.GetAccessibleResources(accessToken, ct)).Single(resource => resource.Id == req.ResourceId).Url;
        var issuesResponse = await jiraApi.GetIssues(accessToken, req.ResourceId, requestBody, cancellationToken: ct);
        foreach (var issue in issuesResponse.Issues)
            room.QueueTicket(CreateTicket(issue, resourceUrl));
        
        await Send.OkAsync(new ModifyTicketQueueResponse(room.Tickets, true), ct);
    }

    private Ticket CreateTicket(JiraGetIssueResponse issue, string baseUrl)
    {
        var descriptionRaw = issue.RenderedFields.Description
            .Replace(" src=\"/", $" src=\"{baseUrl}/")
            .Replace(" href=\"/", $" target=\"_blank\" href=\"{baseUrl}/");
        var descriptionSanitized = sanitizer.Sanitize(descriptionRaw);
        return new Ticket(
            issue.Id,
            issue.Key,
            issue.Fields.IssueType.Name,
            issue.Fields.Summary,
            issue.Fields.IssueType.IconUrl,
            descriptionSanitized,
            $"{baseUrl}/browse/{issue.Key}",
            issue.Fields.Labels);
    }
}

[PublicAPI]
public sealed record QueueTicketRequest([property: QueryParam] string ResourceId, [property: QueryParam] string[] Ids);