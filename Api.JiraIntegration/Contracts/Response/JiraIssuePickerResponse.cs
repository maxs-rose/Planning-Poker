using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Api.JiraIntegration.Contracts.Response;


[PublicAPI]
public sealed record JiraIssuePickerResponse(IEnumerable<JiraIssuePickerResponseSection> Sections);


[PublicAPI]
public sealed record JiraIssuePickerResponseSection(
    [property: JsonPropertyName("id")] string IssueTypeId,
    IEnumerable<JiraIssuePickerIssueDto> Issues,
    [property: JsonPropertyName("label")] string IssueTypeLabel,
    [property: JsonPropertyName("msg")] string NoSuggestionsFoundMessage,
    [property: JsonPropertyName("sub")] string SuggestionsFoundMessage);


[PublicAPI]
public sealed record JiraIssuePickerIssueDto(
    int Id,
    [property: JsonPropertyName("img")] string TypeAvatarUrl,
    string Key,
    string KeyHtml,
    [property: JsonPropertyName("summaryText")] string MatchSummary,
    [property: JsonPropertyName("summary")] string MatchSummaryHtml);
    