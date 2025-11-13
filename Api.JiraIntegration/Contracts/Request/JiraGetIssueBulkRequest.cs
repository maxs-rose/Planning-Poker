using JetBrains.Annotations;

namespace Api.JiraIntegration.Contracts.Request;

[PublicAPI]
public record JiraGetIssueBulkRequest(string[] IssueIdsOrKeys)
{
    public string[] Expand { get; init; } = ["renderedFields"];
}