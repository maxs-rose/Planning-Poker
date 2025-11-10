using Api.Contracts.Response;
using Api.Services;
using FastEndpoints;
using JetBrains.Annotations;

namespace Api.Endpoints.Room;

internal sealed class ModifyTicketQueueEndpoint(RoomManager roomManager) : Endpoint<ModifyTicketQueueRequest>
{
    public override void Configure()
    {
        Post("/rooms/{Code}/modifyQueue");
    }

    public override async Task HandleAsync(ModifyTicketQueueRequest req, CancellationToken ct)
    {
        var room = roomManager.GetRoom(Route<string>("Code")!)!;
        var success = req.ToIndex.HasValue ? 
            room.ReorderTicket(req.FromIndex, req.ToIndex.Value) : 
            room.RemoveTicket(req.FromIndex);
        
        await Send.OkAsync(new ModifyTicketQueueResponse(room.Tickets, success), ct);
    }
}

[PublicAPI]
public sealed record ModifyTicketQueueRequest([property: QueryParam] int FromIndex, [property: QueryParam] int? ToIndex = null);