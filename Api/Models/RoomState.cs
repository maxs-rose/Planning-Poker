namespace Api.Models;

public record RoomState(
    Guid PlayerId,
    bool Owner,
    string FriendlyName,
    List<Player> Players,
    List<Vote> Votes,
    Guid OwnerId,
    bool Revealed,
    Ticket Ticket,
    List<Ticket>? TicketQueue = null,
    int? TicketIndex = null);
