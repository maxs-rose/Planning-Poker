namespace Api.Contracts.Request;

public record JiraGetIssueBulkRequest(string[] IssueIdsOrKeys)
{
    public string[] Expand { get; init; } = ["renderedFields"];
}