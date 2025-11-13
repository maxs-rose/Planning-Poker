using JetBrains.Annotations;

namespace Api.JiraIntegration.Contracts.Response;

[PublicAPI]
public sealed record SearchTicketsResponse(
    IEnumerable<SearchTicketsResult> Results,
    string? NoSuggestionsFoundMessage = null);

[PublicAPI]
public sealed record SearchTicketsResult(
    int Id,
    string Type,
    string TypeAvatarUrl,
    string Key,
    string MatchSummary);