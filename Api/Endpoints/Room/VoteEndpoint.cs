using Api.Services;
using FastEndpoints;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints.Room;

internal sealed class VoteEndpoint(RoomManager roomManager) : Endpoint<VoteRequest, Ok>
{
    public override void Configure()
    {
        Post("/rooms/{Code}/players/{Player:guid}/vote");
    }

    public override async Task<Ok> ExecuteAsync(VoteRequest req, CancellationToken ct)
    {
        var room = roomManager.GetRoom(Route<string>("Code")!)!;

        room.Vote(req.Value, Route<Guid>("Player"));

        return TypedResults.Ok();
    }
}

[PublicAPI]
public sealed record VoteRequest(uint Value);