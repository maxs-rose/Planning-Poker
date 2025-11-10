using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Api.Contracts.Response;

[PublicAPI]
public record JiraTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] long ExpiresIn,
    [property: JsonPropertyName("scope")] string Scope
);