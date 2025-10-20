using Api.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints.Room;

internal sealed class LeaveRoomEndpoint(RoomManager roomManager) : EndpointWithoutRequest<Ok>
{
    public override void Configure()
    {
        Post("/rooms/{Code}/players/{Player:guid}/leave");
    }

    public override Task<Ok> ExecuteAsync(CancellationToken ct)
    {
        var room = roomManager.GetRoom(Route<string>("Code")!)!;

        room.ExplicitLeaveRoom(Route<Guid>("Player"));

        return Task.FromResult(TypedResults.Ok());
    }
}

