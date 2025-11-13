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
        NextRound,
        OwnerChange,
        PlayerUpdate
    }

    private readonly string _name;
    private readonly IDisposable _playerCleanupTimer;

    private Guid? _originalOwnerId;
    private string? _originalOwnerName;

    private Player? _owner;
    private int _currentTicketIndex;

    public Room(string name, string code)
    {
        _name = name;
        Id = code;

        _currentTicketIndex = 0;
        _playerCleanupTimer = Observable.Interval(TimeSpan.FromMinutes(1))
            .TakeUntil(Channel)
            .Subscribe(_ => CleanupDisconnectedPlayers());
    }

    public string Id { get; }

    public Subject<(EventType, object)> Channel { get; } = new();
    private List<Vote> Votes { get; } = new();
    private List<Player> Players { get; } = new();
    public List<Ticket> Tickets { get; } = new();
    private Ticket CurrentTicket => 
        _currentTicketIndex >= 0 && _currentTicketIndex < Tickets.Count ? Tickets[_currentTicketIndex] : Ticket.Empty;
    private Dictionary<Guid, DateTime> DisconnectedPlayers { get; } = new();
    public DateTime? EmptySince { get; set; }
    public bool IsDisposed { get; private set; }
    private bool Revealed { get; set; }

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

    public void NextRound()
    {
        if (IsDisposed)
            return;

        Votes.Clear();
        Revealed = false;
        if (_currentTicketIndex < Tickets.Count) ++_currentTicketIndex;
        var nextTicket = CurrentTicket;
        Channel.OnNext((EventType.NextRound, nextTicket));
    }

    public void Reveal()
    {
        if (IsDisposed)
            return;

        Revealed = true;
        Channel.OnNext((EventType.Reveal, "reveal"));
    }

    public bool HasVotes()
    {
        return Votes.Count > 0;
    }

    public void QueueTicket(Ticket ticket)
    {
        if (IsDisposed) return;

        Tickets.Add(ticket);
    }
    
    public bool ReorderTicket(int indexFrom, int indexTo)
    {
        if (IsDisposed) return false;
        if (indexFrom < 0 || indexTo < 0 || indexFrom >= Tickets.Count || indexTo >= Tickets.Count) return false;
        if (indexFrom == indexTo || indexFrom == _currentTicketIndex || indexTo == _currentTicketIndex) return false;
        
        var ticket = Tickets[indexFrom];
        Tickets.RemoveAt(indexFrom);
        Tickets.Insert(indexTo, ticket);
        
        if (indexFrom < _currentTicketIndex && indexTo > _currentTicketIndex) --_currentTicketIndex;
        else if (indexFrom > _currentTicketIndex && indexTo < _currentTicketIndex) ++_currentTicketIndex;
        return true;
    }
    
    public bool RemoveTicket(int index)
    {
        if (IsDisposed) return false;
        if (index < 0 || index >= Tickets.Count) return false;
        if (index == _currentTicketIndex) return false;

        Tickets.RemoveAt(index);
        if (index < _currentTicketIndex) --_currentTicketIndex;
        return true;
    }

    public RoomState ConnectToRoom(Guid playerId)
    {
        EmptySince = null;
        
        var existingPlayer = Players.FirstOrDefault(p => p.Id == playerId);
        if (existingPlayer is not null)
        {
            var isOwner = _owner?.Id == existingPlayer.Id;
            return new RoomState(existingPlayer.Id, isOwner, _name, Players, Votes, _owner?.Id ?? Guid.Empty, Revealed, CurrentTicket, isOwner ? Tickets : null, isOwner ? _currentTicketIndex : null);
        }
        
        return new RoomState(playerId, false, _name, Players, Votes, _owner?.Id ?? Guid.Empty, Revealed, CurrentTicket);
    }

    public RoomState JoinRoom(Guid playerId, string playerName, bool isSpectator)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(Room), "Cannot join a disposed room");

        if (IsExistingPlayer(playerId, out var existingPlayer))
        {
            DisconnectedPlayers.Remove(playerId);
            existingPlayer.IsConnected = true;
            Channel.OnNext((EventType.PlayerUpdate, existingPlayer));
            var isOwner = _owner?.Id == existingPlayer.Id;
            return new RoomState(existingPlayer.Id, isOwner, _name, Players, Votes, _owner?.Id ?? Guid.Empty, Revealed, CurrentTicket, isOwner ? Tickets : null, isOwner ? _currentTicketIndex : null);
        }

        var reconnectingPlayer = Players.FirstOrDefault(p => p.Name == playerName);
        if (reconnectingPlayer is not null)
        {
            DisconnectedPlayers.Remove(reconnectingPlayer.Id);
            reconnectingPlayer.IsConnected = true;
            Channel.OnNext((EventType.PlayerUpdate, reconnectingPlayer));
            var isOwner = _owner?.Id == reconnectingPlayer.Id;
            return new RoomState(reconnectingPlayer.Id, isOwner, _name, Players, Votes, _owner?.Id ?? Guid.Empty, Revealed, CurrentTicket, isOwner ? Tickets : null, isOwner ? _currentTicketIndex : null);
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

        var isPlayerOwner = _owner?.Id == player.Id;
        return new RoomState(player.Id, isPlayerOwner, _name, Players, Votes, _owner?.Id ?? Guid.Empty, Revealed, CurrentTicket, isPlayerOwner ? Tickets : null, isPlayerOwner ? _currentTicketIndex : null);
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

        if (IsDisposed)
            return;

        player.IsConnected = false;
        Channel.OnNext((EventType.PlayerUpdate, player));

        DisconnectedPlayers[id] = DateTime.UtcNow;
    }

    public void ExplicitLeaveRoom(Guid id)
    {
        var player = Players.FirstOrDefault(x => x.Id == id);

        if (player is null)
            return;

        Players.Remove(player);
        Votes.RemoveAll(x => x.Voter == id);
        DisconnectedPlayers.Remove(id);

        if (IsDisposed)
            return;

        Channel.OnNext((EventType.Leave, player));

        if (id == _owner?.Id)
        {
            _owner = null;
        }
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
        var isOwner = _owner?.Id == playerId;
        return new RoomState(playerId, isOwner, _name, Players, Votes, _owner?.Id ?? Guid.Empty, Revealed, CurrentTicket, isOwner ? Tickets : null, isOwner ? _currentTicketIndex : null);
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
            
            var player = Players.FirstOrDefault(p => p.Id == playerId);
            if (player is not null)
            {
                Players.Remove(player);
                Votes.RemoveAll(x => x.Voter == playerId);
            }

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