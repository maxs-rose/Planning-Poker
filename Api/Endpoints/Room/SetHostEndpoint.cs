using Api.Services;
using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints.Room;

internal sealed class SetHostEndpoint(RoomManager roomManager) : Endpoint<SetHostRequest, Ok>
{
    public override void Configure()
    {
        Post("/rooms/{Code}/set-host");
    }

    public override Task<Ok> ExecuteAsync(SetHostRequest req, CancellationToken ct)
    {
        var room = roomManager.GetRoom(Route<string>("Code")!)!;

        room.TransferOwnershipPermanently(req.User);

        return Task.FromResult(TypedResults.Ok());
    }
}

[PublicAPI]
public sealed record SetHostRequest(Guid User);