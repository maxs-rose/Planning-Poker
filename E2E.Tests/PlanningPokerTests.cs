using System.Text.Json.Serialization;
using E2E.Tests.Infrastructure;
using JetBrains.Annotations;
using Microsoft.Playwright;

namespace E2E.Tests;

public sealed class PlanningPokerTests(AspireManager aspireManager) : BasePlaywriteTest(aspireManager)
{
    [Fact]
    public async Task CanCreateRoom()
    {
        await InteractWithPage(async (page, _) =>
        {
            await page.GotoAsync("/");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.GetByText("Create Room").ClickAsync();

            // Create the Room
            await page.GetByPlaceholder("The Inventory").FillAsync("Testing Room");
            await page.GetByPlaceholder("Claptrap").FillAsync("Testing player");
            var creationRequest = await page.RunAndWaitForResponseAsync(
                async () => { await page.GetByTestId("CreateRoom").ClickAsync(); },
                response => response.Url.EndsWith("/api/rooms/create") && response.Request.Method == "POST"
            );
            Assert.True(creationRequest.Ok);
            var room = await creationRequest.JsonAsync<CreateRoomResponse>();

            // Load the room page
            await page.WaitForURLAsync($"/{room.JoinCode}");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Check the join code & that we are shown in the room :)
            var joinCodeText = await page.GetByTestId("JoinCode").TextContentAsync();
            Assert.Equal($"{room.JoinCode}", joinCodeText?.Trim());
            var players = await page.GetByTestId("PlayerNames").TextContentAsync();
            Assert.Equal(" Testing player ", players);
        });
    }

    [UsedImplicitly]
    private sealed record CreateRoomResponse(
        [property: JsonPropertyName("joinCode")]
        string JoinCode);
}