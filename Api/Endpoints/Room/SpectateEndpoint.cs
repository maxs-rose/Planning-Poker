using Api.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints.Room;

internal sealed class SpectateEndpoint(RoomManager roomManager) : EndpointWithoutRequest<Ok>
{
    public override void Configure()
    {
        Get("/rooms/{Code}/players/{Player:guid}/spectate");
    }

    public override Task<Ok> ExecuteAsync(CancellationToken ct)
    {
        var room = roomManager.GetRoom(Route<string>("Code")!)!;

        room.SpectatorState(Route<Guid>("Player"), true);

        return Task.FromResult(TypedResults.Ok());
    }
}