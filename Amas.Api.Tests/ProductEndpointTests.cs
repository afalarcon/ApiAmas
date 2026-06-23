using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Amas.Application.Products;
using Xunit;

namespace Amas.Api.Tests;

public sealed class ProductEndpointTests : IClassFixture<AmasApiFactory>
{
    private readonly AmasApiFactory factory;

    public ProductEndpointTests(AmasApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task CreateProduct_WithoutJwt_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/products", ValidProductRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithJwt_ReturnsCreated()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateToken());

        var response = await client.PostAsJsonAsync("/api/v1/products", ValidProductRequest());

        await AssertStatusCodeAsync(HttpStatusCode.Created, response);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSlug_ReturnsBadRequest()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwt.CreateToken());

        var request = ValidProductRequest($"duplicado-{Guid.NewGuid():N}");
        var first = await client.PostAsJsonAsync("/api/v1/products", request);
        await AssertStatusCodeAsync(HttpStatusCode.Created, first);

        var duplicate = await client.PostAsJsonAsync("/api/v1/products", request);

        await AssertStatusCodeAsync(HttpStatusCode.BadRequest, duplicate);
    }

    private static CreateProductRequest ValidProductRequest(string? slug = null) =>
        new(
            $"Producto prueba {Guid.NewGuid():N}",
            slug,
            "Producto para prueba automatizada",
            $"SKU-{Guid.NewGuid():N}",
            15000,
            true,
            null);

    private static async Task AssertStatusCodeAsync(HttpStatusCode expected, HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == expected, $"Expected {expected}, got {response.StatusCode}. Body: {body}");
    }
}
