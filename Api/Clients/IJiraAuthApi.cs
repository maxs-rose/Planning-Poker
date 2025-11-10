using Api.Contracts.Request;
using Api.Contracts.Response;
using Refit;

namespace Api.Clients;

public interface IJiraAuthApi
{
    [Post("/oauth/token")]
    Task<JiraTokenResponse> GetToken([Body(BodySerializationMethod.UrlEncoded)] JiraTokenRequest authorization, CancellationToken cancellationToken = default);
}