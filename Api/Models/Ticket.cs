using JetBrains.Annotations;

namespace Api.Models;

[PublicAPI]
public record Ticket(
    string Id,
    string Key,
    string TypeName,
    string Title,
    string Icon,
    string Description,
    IEnumerable<string> Labels)
{
    public static readonly Ticket Empty = new(
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        []);
}