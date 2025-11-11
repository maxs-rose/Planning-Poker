using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Api.JiraIntegration.Contracts.Request;

[PublicAPI]
public record JiraTokenRequest(
    [property: JsonPropertyName("client_id")] string ClientId,
    [property: JsonPropertyName("client_secret")] string ClientSecret,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("redirect_uri")] string RedirectUri
)
{
    [JsonPropertyName("grant_type")]
    public string GrantType { get; init; } = "authorization_code";
}