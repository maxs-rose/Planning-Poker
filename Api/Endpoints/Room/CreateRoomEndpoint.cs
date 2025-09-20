using Api.Services;
using FastEndpoints;
using JetBrains.Annotations;

namespace Api.Endpoints.Room;

internal sealed class CreateRoomEndpoint(
    ILogger<CreateRoomEndpoint> logger,
    Moniker moniker,
    RoomManager roomManager
) : Endpoint<CreateRoomRequest, CreateRoomResponse>
{
    public override void Configure()
    {
        Post("/rooms/create");
    }

    public override Task<CreateRoomResponse> ExecuteAsync(CreateRoomRequest req, CancellationToken ct)
    {
        while (true)
        {
            var joinCode = moniker.NewRoom();

            var result = roomManager.CreateRoom(req.Name, joinCode);

            logger.LogDebug("Created room {@Name} with code {@JoinCode}", req.Name, joinCode);

            return Task.FromResult(new CreateRoomResponse(joinCode));
        }
    }
}

[PublicAPI]
public sealed record CreateRoomRequest(string Name);

[PublicAPI]
public sealed record CreateRoomResponse(string JoinCode);