using FastEndpoints;
using JetBrains.Annotations;

namespace Api.Endpoints;

internal sealed class PingEndpoint : EndpointWithoutRequest<PingResponse>
{
    public override void Configure()
    {
        Get("/ping");
    }

    public override Task<PingResponse> ExecuteAsync(CancellationToken _)
    {
        return Task.FromResult(new PingResponse("pong"));
    }
}

[PublicAPI]
public sealed record PingResponse(string Message);