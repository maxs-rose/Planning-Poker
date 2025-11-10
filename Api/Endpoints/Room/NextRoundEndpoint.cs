using Api.Services;
using FastEndpoints;

namespace Api.Endpoints.Room;

internal sealed class NextRoundEndpoint(
    ILogger<NextRoundEndpoint> logger,
    RoomManager roomManager
) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/rooms/{Code}/nextRound");
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        var room = roomManager.GetRoom(Route<string>("Code")!)!;
        logger.LogInformation("subscribers {Subs}", room.Channel.HasObservers);

        room.NextRound();

        return Task.CompletedTask;
    }
}