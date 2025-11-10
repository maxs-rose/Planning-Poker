using JetBrains.Annotations;

namespace Api.Contracts.Response;

[PublicAPI]
public record JiraResourceDto(
    string Id, 
    string Name,
    string Url,
    IEnumerable<string> Scopes,
    string AvatarUrl);