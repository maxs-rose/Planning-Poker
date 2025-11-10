using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Api.Contracts.Request;

[PublicAPI]
public record JiraAuthorizeRequest(
    [property: JsonPropertyName("audience")] string Audience,
    [property: JsonPropertyName("client_id")] string ClientId,
    [property: JsonPropertyName("scope")] string Scope,
    [property: JsonPropertyName("redirect_uri")] string RedirectUri,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("response_type")] string ResponseType,
    [property: JsonPropertyName("prompt")] string Prompt
)
{
    public Dictionary<string, string?> ToDictionary() => new ()
    {
        { "audience", Audience },
        { "client_id", ClientId },
        { "scope", Scope },
        { "redirect_uri", RedirectUri },
        { "state", State },
        { "response_type", ResponseType },
        { "prompt", Prompt }
    };
}