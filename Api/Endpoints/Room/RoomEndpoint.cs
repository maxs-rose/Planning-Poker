using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Api.Services;
using FastEndpoints;

namespace Api.Endpoints.Room;

internal sealed class RoomEndpoint(RoomManager roomManager) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/rooms/{Code}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var code = Route<string>("Code");

        if (string.IsNullOrWhiteSpace(code))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var room = roomManager.GetRoom(code);

        if (room is null || room.IsDisposed)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var playerIdParam = Query<string?>("playerId", isRequired: false);
        var playerId = string.IsNullOrEmpty(playerIdParam) || !Guid.TryParse(playerIdParam, out var parsedId) 
            ? Guid.NewGuid() 
            : parsedId;

        ct.Register(() => roomManager.CleanupEmptyRoom(room.Id));

        await Send.EventStreamAsync(Stream(room, playerId, ct), ct);
    }

    private static async IAsyncEnumerable<StreamItem> Stream(
        Services.Room room,
        Guid playerId,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (room.IsDisposed)
            yield break;

        var joined = room.ConnectToRoom(playerId);

        ct.Register(() => room.LeaveRoom(joined.PlayerId));

        yield return ToStreamItem(Services.Room.EventType.Init, joined);

        var heartbeat = Observable.Interval(TimeSpan.FromSeconds(5)).Select(_ =>
            new StreamItem(Guid.NewGuid().ToString(), "Heartbeat", room.State(joined.PlayerId)));
        var roomData = room.Channel.Select(x => ToStreamItem(x.Item1, x.Item2));

        await foreach (var message in heartbeat.Merge(roomData).ToAsyncEnumerable().WithCancellation(ct))
            yield return message;
    }

    private static StreamItem ToStreamItem(Services.Room.EventType type, object? data)
    {
        return new StreamItem(Guid.NewGuid().ToString(), type.ToString(), data);
    }
}