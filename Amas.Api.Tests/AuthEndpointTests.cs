using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Amas.Api.Tests;

public sealed class AuthEndpointTests : IClassFixture<AmasApiFactory>
{
    private readonly AmasApiFactory factory;

    public AuthEndpointTests(AmasApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Login_WhenRateLimitExceeded_ReturnsTooManyRequests()
    {
        var client = factory.CreateClient();
        var request = new { email = "unknown@amas.local", password = "invalid-password" };

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        var blocked = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
    }
}
