using Bogus;

namespace Api.Services;

internal sealed class Moniker
{
    private static readonly Faker Faker = new();

    public string NewRoom()
    {
        return string.Join(
            '-',
            Faker.Hacker.Verb(),
            Faker.Vehicle.Model(),
            Faker.Address.City()
        ).Replace(" ", string.Empty).ToLowerInvariant();
    }
}