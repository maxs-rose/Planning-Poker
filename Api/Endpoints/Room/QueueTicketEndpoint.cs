using Api.Clients;
using Api.Contracts.Request;
using Api.Contracts.Response;
using Api.Models;
using Api.Models.Configuration;
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
        
        var issuesResponse = await jiraApi.GetIssues(accessToken, req.ResourceId, requestBody, cancellationToken: ct);
        foreach (var issue in issuesResponse.Issues)
            room.QueueTicket(new Ticket(
                issue.Id,
                issue.Key,
                issue.Fields.IssueType.Name,
                issue.Fields.Summary,
                issue.Fields.IssueType.IconUrl,
                sanitizer.Sanitize(issue.RenderedFields.Description),
                issue.Fields.Labels));
        
        await Send.OkAsync(new ModifyTicketQueueResponse(room.Tickets, true), ct);
    }
}

[PublicAPI]
public sealed record QueueTicketRequest([property: QueryParam] string ResourceId, [property: QueryParam] string[] Ids);