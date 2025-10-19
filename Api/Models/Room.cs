using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Api.Models;

internal class Room : IDisposable
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

    private readonly string _name;
    private readonly IDisposable _playerCleanupTimer;

    private Guid? _originalOwnerId;
    private string? _originalOwnerName;

    private Player? _owner;

    public Room(string name, string code)
    {
        _name = name;
        Id = code;

        _playerCleanupTimer = Observable.Interval(TimeSpan.FromMinutes(1))
            .TakeUntil(Channel)
            .Subscribe(_ => CleanupDisconnectedPlayers());
    }

    public string Id { get; }

    public Subject<(EventType, object)> Channel { get; } = new();
    private List<Vote> Votes { get; } = new();
    private List<Player> Players { get; } = new();
    private Dictionary<Guid, DateTime> DisconnectedPlayers { get; } = new();
    public DateTime? EmptySince { get; set; }
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;
        Channel.OnCompleted();
        Channel.Dispose();
        _playerCleanupTimer.Dispose();
    }

    public void Vote(uint? value, Guid playerId)
    {
        if (IsDisposed || Players.FirstOrDefault(p => p.Id == playerId) is not { IsSpectator: false })
            return;

        if (Votes.FirstOrDefault(v => v.Voter == playerId) is not null)
            Votes.Remove(Votes.First(v => v.Voter == playerId));

        Votes.Add(new Vote(playerId, value));
        Channel.OnNext((EventType.Vote, new Vote(playerId, value)));
    }

    public void Reset()
    {
        if (IsDisposed)
            return;

        Votes.Clear();
        Channel.OnNext((EventType.Reset, "reset"));
    }

    public void Reveal()
    {
        if (IsDisposed)
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
        return new RoomState(playerId, false, _name, Players, Votes, _owner?.Id ?? Guid.Empty);
    }

    public RoomState JoinRoom(Guid playerId, string playerName, bool isSpectator)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(Room), "Cannot join a disposed room");

        if (IsExistingPlayer(playerId, out var existingPlayer))
        {
            DisconnectedPlayers.Remove(playerId);
            return new RoomState(existingPlayer.Id, _owner?.Id == existingPlayer.Id, _name, Players, Votes, _owner?.Id ?? Guid.Empty);
        }

        var isReconnectingOwner = IsReconnectingOwner(playerId, playerName);

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

        return new RoomState(player.Id, _owner?.Id == player.Id, _name, Players, Votes, _owner?.Id ?? Guid.Empty);
    }

    public void SpectatorState(Guid id, bool isSpectator)
    {
        if (IsDisposed)
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

        if (IsDisposed)
            return;

        Channel.OnNext((EventType.Leave, player));

        if (id == _owner?.Id)
        {
            DisconnectedPlayers[id] = DateTime.UtcNow;
            return;
        }

        DisconnectedPlayers.Remove(id);
    }

    public void SetOwner(Guid player, bool permanent = false)
    {
        if (IsDisposed)
            return;

        _owner = Players.FirstOrDefault(p => p.Id == player);

        if (_owner is null)
            return;

        if (_originalOwnerId is null || permanent)
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

    public RoomState State(Guid playerId)
    {
        return new RoomState(playerId, _owner?.Id == playerId, _name, Players, Votes, _owner?.Id ?? Guid.Empty);
    }

    private bool IsExistingPlayer(Guid playerId, out Player player)
    {
        var foundPlayer = Players.FirstOrDefault(p => p.Id == playerId);
        player = foundPlayer!;

        return foundPlayer is not null;
    }

    private bool IsReconnectingOwner(Guid playerId, string playerName)
    {
        return (DisconnectedPlayers.ContainsKey(playerId) && _owner?.Id == playerId)
               || (_originalOwnerId.HasValue && _originalOwnerName == playerName && _owner == null && Players.All(p => p.Id != _originalOwnerId));
    }

    private void CleanupDisconnectedPlayers()
    {
        var timeout = DateTime.UtcNow.AddMinutes(-30);

        var timedOutPlayers = DisconnectedPlayers
            .Where(kvp => kvp.Value < timeout)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var playerId in timedOutPlayers)
        {
            DisconnectedPlayers.Remove(playerId);

            if (_owner?.Id != playerId)
                continue;

            _owner = null;
        }

        if (Players.Count <= 0 || _owner is not null)
            return;

        var newOwner = Players.FirstOrDefault(p => !p.IsSpectator) ?? Players[0];
        SetOwner(newOwner.Id);
    }
}