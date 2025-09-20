using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Api.Services;
using FastEndpoints;
using JetBrains.Annotations;

namespace Api.Endpoints.Room;

internal sealed class RoomEndpoint(RoomManager roomManager) : Endpoint<RoomJoin>
{
    public override void Configure()
    {
        Get("/rooms/{Code}/join");
    }

    public override async Task HandleAsync(RoomJoin req, CancellationToken ct)
    {
        var code = Route<string>("Code");

        if (string.IsNullOrWhiteSpace(code))
            return;

        var room = roomManager.GetRoom(code);

        if (room is null)
            return;

        ct.Register(() => roomManager.CleanupEmptyRoom(room.Id));

        await Send.EventStreamAsync(Stream(room, req, ct), ct);
    }

    private static async IAsyncEnumerable<StreamItem> Stream(
        Services.Room room,
        RoomJoin joinRequest,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var joined = room.JoinRoom(joinRequest.Name);

        ct.Register(() => room.LeaveRoom(joined.PlayerId));

        yield return ToStreamItem(Services.Room.EventType.Init, joined);

        var heartbeat = Observable.Interval(TimeSpan.FromMinutes(1)).Select(_ => new StreamItem(Guid.NewGuid().ToString(), "Heartbeat", "ping"));
        var roomData = room.Channel.Select(x => ToStreamItem(x.Item1, x.Item2));

        await foreach (var message in heartbeat.Merge(roomData).ToAsyncEnumerable().WithCancellation(ct))
            yield return message;
    }

    private static StreamItem ToStreamItem(Services.Room.EventType type, object? data)
    {
        return new StreamItem(Guid.NewGuid().ToString(), type.ToString(), data);
    }
}

[PublicAPI]
public sealed record RoomJoin([property: QueryParam] string Name);