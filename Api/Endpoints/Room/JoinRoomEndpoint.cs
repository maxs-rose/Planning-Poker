using Api.Services;
using FastEndpoints;
using JetBrains.Annotations;

namespace Api.Endpoints.Room;

internal sealed class JoinRoomEndpoint(RoomManager roomManager) : Endpoint<JoinRoomRequest, RoomState>
{
    public override void Configure()
    {
        Post("/rooms/{Code}/join");
    }

    public override async Task HandleAsync(JoinRoomRequest req, CancellationToken ct)
    {
        var code = Route<string>("Code");

        if (string.IsNullOrWhiteSpace(code))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var room = roomManager.GetRoom(code);

        if (room is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var state = room.JoinRoom(req.Id, req.Name, req.Spectator);
        if (req.Owner)
        {
            room.SetOwner(req.Id);
            state = state with { Owner = true };
        }

        await Send.OkAsync(state, ct);
    }
}

[PublicAPI]
public sealed record JoinRoomRequest(Guid Id, string Name)
{
    public bool Spectator { get; init; }
    public bool Owner { get; init; }
}