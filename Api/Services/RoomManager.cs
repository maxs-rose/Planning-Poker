using System.Reactive.Subjects;
using Dunet;

namespace Api.Services;

internal sealed class RoomManager(ILogger<RoomManager> logger) : IDisposable
{
    private static Dictionary<string, Room> Rooms { get; } = new();

    public void Dispose()
    {
        foreach (var room in Rooms.Values)
            room.Dispose();
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

    public void CleanupEmptyRoom(string code)
    {
        if (!Rooms.TryGetValue(code, out var room))
            return;

        if (room.Channel.HasObservers)
        {
            logger.LogDebug("{RoomId} still has members, will not remove", room.Id);
            return;
        }

        logger.LogInformation("Removing empty room {RoomId}", room.Id);
        Rooms.Remove(code);
        room.Dispose();
    }
}

[Union]
internal partial record CreateRoom
{
    partial record RoomCreated(Room Room);

    partial record RoomExists;
}

internal class Room(string name, string code) : IDisposable
{
    public enum EventType
    {
        Init,
        Join,
        Leave,
        Vote,
        Reveal,
        Reset,
        OwnerChange,
        PlayerUpdate
    }

    private Player? _owner;

    public string Id { get; } = code;

    public Subject<(EventType, object)> Channel { get; } = new();
    private List<Vote> Votes { get; } = new();
    private List<Player> Players { get; } = new();

    public void Dispose()
    {
        Channel.OnCompleted();
        Channel.Dispose();
    }

    public void Vote(uint? value, Guid playerId)
    {
        if (Players.FirstOrDefault(p => p.Id == playerId) is not { IsSpectator: false })
            return;

        if (Votes.FirstOrDefault(v => v.Voter == playerId) is not null)
            Votes.Remove(Votes.First(v => v.Voter == playerId));

        Votes.Add(new Vote(playerId, value));
        Channel.OnNext((EventType.Vote, new Vote(playerId, value)));
    }

    public void Reset()
    {
        Votes.Clear();
        Channel.OnNext((EventType.Reset, "reset"));
    }

    public void Reveal()
    {
        Channel.OnNext((EventType.Reveal, "reveal"));
    }

    public RoomState ConnectToRoom()
    {
        return new RoomState(Guid.NewGuid(), false, name, Players, Votes, _owner?.Id ?? Guid.Empty);
    }

    public RoomState JoinRoom(Guid playerId, string playerName, bool isSpectator)
    {
        var player = new Player(playerId, playerName)
        {
            IsSpectator = Players.Count != 0 && isSpectator
        };
        Players.Add(player);

        _owner ??= player;

        Channel.OnNext((EventType.Join, player));
        return new RoomState(player.Id, _owner.Id == player.Id, name, Players, Votes, _owner?.Id ?? Guid.Empty);
    }

    public void SpectatorState(Guid id, bool isSpectator)
    {
        var player = Players.FirstOrDefault(x => x.Id == id);

        if (player is null || _owner?.Id == id)
            return;

        player.IsSpectator = isSpectator;
        Channel.OnNext((EventType.PlayerUpdate, player));
    }

    public void LeaveRoom(Guid id)
    {
        if (id == _owner?.Id)
            _owner = null;

        var player = Players.FirstOrDefault(x => x.Id == id);

        if (player is null)
            return;

        Players.Remove(player);
        Channel.OnNext((EventType.Leave, player));

        if (Players.Count == 0)
            return;

        SetOwner(Players[0].Id);
    }

    public void SetOwner(Guid player)
    {
        _owner = Players.FirstOrDefault(p => p.Id == player);

        if (_owner is null)
            return;

        _owner.IsSpectator = false;
        Channel.OnNext((EventType.PlayerUpdate, _owner));
        Channel.OnNext((EventType.OwnerChange, _owner));
    }

    public RoomState State(Guid playerId)
    {
        return new RoomState(playerId, _owner?.Id == playerId, name, Players, Votes, _owner?.Id ?? Guid.Empty);
    }
}

public record RoomState(
    Guid PlayerId,
    bool Owner,
    string FriendlyName,
    List<Player> Players,
    List<Vote> Votes,
    Guid OwnerId);

public sealed record Player(Guid Id, string Name)
{
    public bool IsSpectator { get; set; }
}

public sealed record Vote(Guid Voter, uint? Value);