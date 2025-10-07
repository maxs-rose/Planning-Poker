using Api.Services;
using FastEndpoints;

namespace Api.Endpoints.Room;

internal sealed class RoomExistsEndpoint(RoomManager roomManager) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Head("/rooms/{Code}");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var code = Route<string>("Code");

        if (string.IsNullOrWhiteSpace(code))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var room = roomManager.GetRoom(code);

        if (room is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(cancellation: ct);
    }
}