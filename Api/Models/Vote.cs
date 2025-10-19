namespace Api.Models;

public sealed record Vote(Guid Voter, uint? Value);