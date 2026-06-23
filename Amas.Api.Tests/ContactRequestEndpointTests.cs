using System.Net;
using System.Net.Http.Json;
using Amas.Api.Controllers;
using Amas.Api.Contracts;
using Amas.Application.ContactRequests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Amas.Api.Tests;

public sealed class ContactRequestEndpointTests : IClassFixture<AmasApiFactory>
{
    private readonly AmasApiFactory factory;

    public ContactRequestEndpointTests(AmasApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task CreateContactRequest_WithoutJwt_ReturnsCreated()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/contact-requests", ValidRequest());

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<ContactRequestReceivedDto>>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(body?.Succeeded);
        Assert.True(body?.Data?.ContactRequestNumber > 0);
    }

    [Fact]
    public async Task CreateContactRequest_WithValidPayload_StoresRequest()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/contact-requests", ValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<TestContactRequestRepository>();
        Assert.Contains(repository.Items, item => item.Email == "cliente@amas.test" && item.WebhookDelivered);
    }

    [Fact]
    public async Task CreateContactRequest_WithInvalidEmail_ReturnsBadRequest()
    {
        var client = factory.CreateClient();
        var request = ValidRequest() with { Email = "correo-invalido" };

        var response = await client.PostAsJsonAsync("/api/v1/contact-requests", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static CreateContactRequestPayload ValidRequest() =>
        new(
            "Cliente AMAS",
            "cliente@amas.test",
            "3101234567",
            "Producto personalizado",
            "Quiero cotizar un producto personalizado para un regalo.",
            "/",
            "captcha-token",
            null);
}
