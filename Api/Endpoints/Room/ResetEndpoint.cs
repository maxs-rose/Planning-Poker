using Api.Services;
using FastEndpoints;

namespace Api.Endpoints.Room;

internal sealed class ResetScoresEndpoint(
    ILogger<RevealScoresEndpoint> logger,
    RoomManager roomManager
) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/rooms/{Code}/reset");
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        var room = roomManager.GetRoom(Route<string>("Code")!)!;

        logger.LogInformation("subscrubers {Subs}", room.Channel.HasObservers);

        room.Reset();

        return Task.CompletedTask;
    }
}