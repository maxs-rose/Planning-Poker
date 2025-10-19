using System.Reactive.Linq;
using Api.Models;
using Dunet;

namespace Api.Services;

internal sealed class RoomManager : IDisposable
{
    private readonly IDisposable _cleanupTimer;
    private readonly ILogger<RoomManager> _logger;

    public RoomManager(ILogger<RoomManager> logger)
    {
        _logger = logger;
        _cleanupTimer = Observable.Interval(TimeSpan.FromMinutes(1))
            .Subscribe(_ => RoomCleanup());
    }

    private static Dictionary<string, Room> Rooms { get; } = new();

    public void Dispose()
    {
        _cleanupTimer.Dispose();

        foreach (var room in Rooms.Values)
            room.Dispose();

        Rooms.Clear();
    }

    public CreateRoom CreateRoom(string name, string code)
    {
        if (Rooms.ContainsKey(code))
            return new CreateRoom.RoomExists();

        return new CreateRoom.RoomCreated(Rooms[code] = new Room(name, code));
    }

    public Room? GetRoom(string code)
    {
        return Rooms.GetValueOrDefault(code);
    }

    private void RoomCleanup()
    {
        _logger.LogDebug("Running room cleanup");

        var emptySince = DateTime.UtcNow.AddMinutes(-30);

        foreach (var room in Rooms.Values.ToList())
            if (room.EmptySince.HasValue && room.EmptySince < emptySince)
            {
                _logger.LogInformation("Removing empty room {RoomId} that has been empty since {EmptySince}", room.Id, room.EmptySince);
                Rooms.Remove(room.Id);
                room.Dispose();
            }
            else if (!room.Channel.HasObservers && room.EmptySince is null)
            {
                _logger.LogInformation("{RoomId} has no members, marking as empty", room.Id);
                room.EmptySince = DateTime.UtcNow;
            }
    }
}

[Union]
internal partial record CreateRoom
{
    partial record RoomCreated(Room Room);

    partial record RoomExists;
}