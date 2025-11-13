using Api.Models;
using JetBrains.Annotations;

namespace Api.Contracts.Response;

[PublicAPI]
public sealed record ModifyTicketQueueResponse(
    IList<Ticket> Tickets,
    bool Success);
