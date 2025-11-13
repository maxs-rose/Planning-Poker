using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Api.JiraIntegration.Contracts.Response;

[PublicAPI]
public sealed record JiraGetIssueResponse(
    string Id,
    string Key,
    [property: JsonPropertyName("self")]
    string Url,
    JiraGetIssueResponseFields Fields,
    JiraIssueRenderedFields RenderedFields);


[PublicAPI]
public sealed record JiraGetIssueResponseFields(
    string Summary,
    [property: JsonPropertyName("sub-tasks")]
    JiraIssueFieldSubtask[] Subtasks,
    JiraIssueFieldPriority Priority,
    string[] Labels,
    [property: JsonPropertyName("issuetype")]
    JiraIssueFieldType IssueType,
    JiraIssueFieldProject Project);

[PublicAPI]
public sealed record JiraIssueRenderedFields(string Description);

[PublicAPI]
public sealed record JiraIssueFieldPriority(string Name, string Id, string IconUrl);

[PublicAPI]
public sealed record JiraIssueFieldType(string Name, string Id, string IconUrl);

[PublicAPI]
public sealed record JiraIssueFieldProject(string Name, string Id, string Key);

[PublicAPI]
public sealed record JiraIssueFieldSubtask(
    string Id,
    JiraIssueFieldSubtaskOutward OutwardIssue);

[PublicAPI]
public record JiraIssueFieldSubtaskOutward(
    string Id,
    string Key,
    [property: JsonPropertyName("self")]
    string Url);