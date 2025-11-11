using Api.JiraIntegration.Contracts.Request;
using Api.JiraIntegration.Contracts.Response;
using Refit;

namespace Api.JiraIntegration.Clients;

public interface IJiraAuthApi
{
    [Post("/oauth/token")]
    Task<JiraTokenResponse> GetToken([Body(BodySerializationMethod.UrlEncoded)] JiraTokenRequest authorization, CancellationToken cancellationToken = default);
}