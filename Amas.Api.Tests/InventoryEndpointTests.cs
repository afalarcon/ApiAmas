using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Amas.Application.Inventory;
using Xunit;

namespace Amas.Api.Tests;

public sealed class InventoryEndpointTests : IClassFixture<AmasApiFactory>
{
    private readonly AmasApiFactory factory;

    public InventoryEndpointTests(AmasApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task CreateInventoryItem_WithoutJwt_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/inventory/items", ValidInventoryItemRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateInventoryMovement_ExitGreaterThanStock_ReturnsBadRequest()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateToken());

        var created = await client.PostAsJsonAsync("/api/v1/inventory/items", ValidInventoryItemRequest());
        await AssertSuccessAsync(created);

        var body = await created.Content.ReadFromJsonAsync<ApiResponse<InventoryItemDto>>();
        var itemId = body!.Data!.Id;

        var response = await client.PostAsJsonAsync(
            $"/api/v1/inventory/items/{itemId}/movements",
            new CreateInventoryMovementRequest("Exit", 20, null, "Prueba salida", "test"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateInventoryMovement_EntryAndExit_UpdatesStock()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateToken());

        var created = await client.PostAsJsonAsync("/api/v1/inventory/items", ValidInventoryItemRequest());
        await AssertSuccessAsync(created);
        var createdBody = await created.Content.ReadFromJsonAsync<ApiResponse<InventoryItemDto>>();
        var itemId = createdBody!.Data!.Id;

        var entry = await client.PostAsJsonAsync(
            $"/api/v1/inventory/items/{itemId}/movements",
            new CreateInventoryMovementRequest("Entry", 5, null, "Compra", "OC-1"));
        await AssertSuccessAsync(entry);

        var exit = await client.PostAsJsonAsync(
            $"/api/v1/inventory/items/{itemId}/movements",
            new CreateInventoryMovementRequest("Exit", 3, null, "Uso", "REQ-1"));
        await AssertSuccessAsync(exit);

        var response = await client.GetFromJsonAsync<ApiResponse<InventoryItemDto>>($"/api/v1/inventory/items/{itemId}");

        Assert.Equal(12, response!.Data!.CurrentStock);
    }

    private static CreateInventoryItemRequest ValidInventoryItemRequest() =>
        new(
            null,
            "Filamento PLA",
            $"PLA-{Guid.NewGuid():N}",
            "Supply",
            "kg",
            10,
            2,
            true);

    private static async Task AssertSuccessAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Expected success, got {response.StatusCode}. Body: {body}");
    }
}

public sealed record ApiResponse<T>(bool Succeeded, T? Data, string? Error);
