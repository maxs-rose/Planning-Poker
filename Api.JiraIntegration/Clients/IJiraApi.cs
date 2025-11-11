using Api.JiraIntegration.Contracts.Request;
using Api.JiraIntegration.Contracts.Response;
using Refit;

namespace Api.JiraIntegration.Clients;

public interface IJiraApi
{
    [Get("/oauth/token/accessible-resources")]
    Task<IEnumerable<JiraResourceDto>> GetAccessibleResources(
        [Authorize] string authorization,
        CancellationToken cancellationToken = default);
    
    [Get("/ex/jira/{cloudId}/rest/api/3/issue/picker")]
    Task<JiraIssuePickerResponse> SearchIssues(
        [Authorize] string authorization,
        [AliasAs("cloudId")] string resourceId,
        string query,
        bool showSubTasks = true,
        CancellationToken cancellationToken = default);
    
    [Get("/ex/jira/{cloudId}/rest/api/3/issue/{issueId}")]
    Task<JiraGetIssueResponse> GetIssue(
        [Authorize] string authorization,
        [AliasAs("cloudId")] string resourceId,
        string issueId,
        string expand = "renderedFields",
        CancellationToken cancellationToken = default);
    
    
    [Post("/ex/jira/{cloudId}/rest/api/3/issue/bulkfetch")]
    Task<JiraGetIssueBulkResponse> GetIssues(
        [Authorize] string authorization,
        [AliasAs("cloudId")] string resourceId,
        [Body] JiraGetIssueBulkRequest request,
        CancellationToken cancellationToken = default);
}