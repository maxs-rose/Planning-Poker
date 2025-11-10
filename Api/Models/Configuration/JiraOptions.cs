namespace Api.Models.Configuration;

public sealed record JiraOptions
{
    public const string SectionName = "Jira";
    public const string CallbackUrl = "/api/jira/callback";


    public string ClientId { get; init; } = string.Empty;
    
    public string ClientSecret { get; init; } = string.Empty;

    public string AuthBaseUrl { get; init; } = "https://auth.atlassian.com";
    
    public string ApiBaseUrl { get; init; } = "https://api.atlassian.com";

    public string AuthorizeUrl { get; init; } = "/authorize";
    
    public string Audience { get; init; } = "api.atlassian.com";
    
    public string Scope { get; init; } = "read:jira-work read:jira-user";
}

public static class JiraSession
{
    public const string AuthStateKey = "jira_auth_state";
    public const string TokenKey = "jira_token";
}