namespace Api.Models;

public sealed record Player(Guid Id, string Name)
{
    public bool IsSpectator { get; set; }
    public bool IsOwner { get; set; }
}