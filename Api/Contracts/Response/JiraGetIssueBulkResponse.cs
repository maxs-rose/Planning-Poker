using JetBrains.Annotations;

namespace Api.Contracts.Response;

[PublicAPI]
public sealed record JiraGetIssueBulkResponse(
    string Expand,
    string[] IssueErrors,
    JiraGetIssueResponse[] Issues);

