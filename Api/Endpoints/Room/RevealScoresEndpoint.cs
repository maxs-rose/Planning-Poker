using Api.Services;
using FastEndpoints;

namespace Api.Endpoints.Room;

internal sealed class RevealScoresEndpoint(
    ILogger<RevealScoresEndpoint> logger,
    RoomManager roomManager
) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/rooms/{Code}/reveal");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var room = roomManager.GetRoom(Route<string>("Code")!);

        if (room is null || room.IsDisposed)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (!room.HasVotes())
        {
            logger.LogWarning("Cannot reveal scores - no votes have been cast yet");
            ThrowError("No votes have been cast yet", 400);
        }

        logger.LogInformation("Revealing scores for room with {Subs} subscribers", room.Channel.HasObservers);

        room.Reveal();
    }
}
