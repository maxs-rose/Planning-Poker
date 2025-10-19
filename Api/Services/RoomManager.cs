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
        foreach (var room in Rooms.Keys.ToList())
            CleanupEmptyRoom(room);

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

        if (room.IsDisposed)
        {
            Rooms.Remove(code);
            return;
        }

        room.CleanupDisconnectedPlayers();

        if (room.Channel.HasObservers)
        {
            logger.LogDebug("{RoomId} still has members, will not remove", room.Id);
        }
        else if (!room.Channel.HasObservers && room.EmptySince is null)
        {
            logger.LogInformation("{RoomId} has no members, marking as empty", room.Id);
            room.EmptySince = DateTime.UtcNow;
        }
        else if (room.EmptySince.HasValue && room.EmptySince.Value < DateTime.UtcNow.AddMinutes(-30))
        {
            logger.LogInformation("Removing empty room {RoomId} that has been empty since {EmptySince}", room.Id, room.EmptySince);
            Rooms.Remove(code);
            room.Dispose();
        }
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
    private Guid? _originalOwnerId;
    private string? _originalOwnerName;
    private bool _isDisposed;

    public string Id { get; } = code;

    public Subject<(EventType, object)> Channel { get; } = new();
    private List<Vote> Votes { get; } = new();
    private List<Player> Players { get; } = new();
    private Dictionary<Guid, DateTime> DisconnectedPlayers { get; } = new();
    public DateTime? EmptySince { get; set; }
    public bool IsDisposed => _isDisposed;

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        Channel.OnCompleted();
        Channel.Dispose();
    }

    public void Vote(uint? value, Guid playerId)
    {
        if (_isDisposed || Players.FirstOrDefault(p => p.Id == playerId) is not { IsSpectator: false })
            return;

        if (Votes.FirstOrDefault(v => v.Voter == playerId) is not null)
            Votes.Remove(Votes.First(v => v.Voter == playerId));

        Votes.Add(new Vote(playerId, value));
        Channel.OnNext((EventType.Vote, new Vote(playerId, value)));
    }

    public void Reset()
    {
        if (_isDisposed)
            return;

        Votes.Clear();
        Channel.OnNext((EventType.Reset, "reset"));
    }

    public void Reveal()
    {
        if (_isDisposed)
            return;

        Channel.OnNext((EventType.Reveal, "reveal"));
    }

    public bool HasVotes()
    {
        return Votes.Count > 0;
    }

    public RoomState ConnectToRoom(Guid playerId)
    {
        EmptySince = null;
        return new RoomState(playerId, false, name, Players, Votes, _owner?.Id ?? Guid.Empty);
    }

    public RoomState JoinRoom(Guid playerId, string playerName, bool isSpectator)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(Room), "Cannot join a disposed room");

        var existingPlayer = Players.FirstOrDefault(p => p.Id == playerId);
        if (existingPlayer is not null)
        {
            DisconnectedPlayers.Remove(playerId);
            return new RoomState(existingPlayer.Id, _owner?.Id == existingPlayer.Id, name, Players, Votes, _owner?.Id ?? Guid.Empty);
        }

        bool isReconnectingOwner = false;
        if (DisconnectedPlayers.ContainsKey(playerId) && _owner?.Id == playerId)
        {
            isReconnectingOwner = true;
        }
        else if (_originalOwnerId.HasValue && 
                 _originalOwnerName == playerName && 
                 _owner == null &&
                 Players.All(p => p.Id != _originalOwnerId))
        {
            isReconnectingOwner = true;
        }

        var player = new Player(playerId, playerName)
        {
            IsSpectator = Players.Count != 0 && isSpectator && !isReconnectingOwner
        };
        Players.Add(player);

        if (_owner is null || isReconnectingOwner)
        {
            SetOwner(playerId);
            
            if (_originalOwnerId is null)
            {
                _originalOwnerId = playerId;
                _originalOwnerName = playerName;
            }
        }

        DisconnectedPlayers.Remove(playerId);
        Channel.OnNext((EventType.Join, player));
        
        return new RoomState(player.Id, _owner?.Id == player.Id, name, Players, Votes, _owner?.Id ?? Guid.Empty);
    }

    public void SpectatorState(Guid id, bool isSpectator)
    {
        if (_isDisposed)
            return;

        var player = Players.FirstOrDefault(x => x.Id == id);

        if (player is null || _owner?.Id == id)
            return;

        player.IsSpectator = isSpectator;
        Channel.OnNext((EventType.PlayerUpdate, player));
    }

    public void LeaveRoom(Guid id)
    {
        var player = Players.FirstOrDefault(x => x.Id == id);

        if (player is null)
            return;

        Players.Remove(player);
        Votes.RemoveAll(x => x.Voter == id);
        
        if (!_isDisposed)
            Channel.OnNext((EventType.Leave, player));

        if (id == _owner?.Id)
        {
            DisconnectedPlayers[id] = DateTime.UtcNow;
            return;
        }

        DisconnectedPlayers.Remove(id);
    }

    public void CleanupDisconnectedPlayers()
    {
        if (_isDisposed)
            return;

        var timeout = DateTime.UtcNow.AddMinutes(-30);
        
        var timedOutPlayers = DisconnectedPlayers
            .Where(kvp => kvp.Value < timeout)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var playerId in timedOutPlayers)
        {
            DisconnectedPlayers.Remove(playerId);

            if (_owner?.Id == playerId)
            {
                _owner = null;

                if (Players.Count > 0)
                {
                    var newOwner = Players.FirstOrDefault(p => !p.IsSpectator) ?? Players[0];
                    SetOwner(newOwner.Id);
                }
            }
        }
    }

    public void SetOwner(Guid player)
    {
        if (_isDisposed)
            return;

        _owner = Players.FirstOrDefault(p => p.Id == player);

        if (_owner is null)
            return;

        if (_originalOwnerId is null)
        {
            _originalOwnerId = player;
            _originalOwnerName = _owner.Name;
        }

        Players.ForEach(p => p.IsOwner = false);

        _owner.IsSpectator = false;
        _owner.IsOwner = true;
        Channel.OnNext((EventType.PlayerUpdate, _owner));
        Channel.OnNext((EventType.OwnerChange, _owner));
    }

    public void TransferOwnershipPermanently(Guid newOwnerId)
    {
        SetOwner(newOwnerId);
        
        if (_owner is not null)
        {
            _originalOwnerId = newOwnerId;
            _originalOwnerName = _owner.Name;
        }
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
    public bool IsOwner { get; set; }
}

public sealed record Vote(Guid Voter, uint? Value);
