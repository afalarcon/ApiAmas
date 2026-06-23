using Amas.Application.Abstractions;
using Amas.Application.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Amas.Api.Tests;

public sealed class AmasApiFactory : WebApplicationFactory<Program>
{
    public AmasApiFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Postgres", "Host=localhost;Database=amas_tests;Username=test;Password=test");
        Environment.SetEnvironmentVariable("Redis__Connection", "localhost:6379");
        Environment.SetEnvironmentVariable("Jwt__Issuer", TestJwt.Issuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", TestJwt.Audience);
        Environment.SetEnvironmentVariable("Jwt__Secret", TestJwt.Secret);
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");
        Environment.SetEnvironmentVariable("MediaStorage__Provider", "Local");
        Environment.SetEnvironmentVariable("MediaStorage__LocalPath", Path.Combine(Path.GetTempPath(), "amas-api-tests-media"));
        Environment.SetEnvironmentVariable("MediaStorage__PublicBaseUrl", "/media");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=localhost;Database=amas_tests;Username=test;Password=test",
                ["Redis:Connection"] = "localhost:6379",
                ["Jwt:Issuer"] = TestJwt.Issuer,
                ["Jwt:Audience"] = TestJwt.Audience,
                ["Jwt:Secret"] = TestJwt.Secret,
                ["Jwt:ExpirationMinutes"] = "60",
                ["RateLimiting:GlobalPermitLimit"] = "1000",
                ["RateLimiting:GlobalWindowSeconds"] = "60",
                ["RateLimiting:LoginPermitLimit"] = "5",
                ["RateLimiting:LoginWindowSeconds"] = "60",
                ["RateLimiting:ContactPermitLimit"] = "1000",
                ["RateLimiting:ContactWindowSeconds"] = "60",
                ["ContactWebhook:Enabled"] = "false",
                ["MediaStorage:Provider"] = "Local",
                ["MediaStorage:LocalPath"] = Path.Combine(Path.GetTempPath(), "amas-api-tests-media"),
                ["MediaStorage:PublicBaseUrl"] = "/media"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICacheService>();
            services.RemoveAll<IProductRepository>();
            services.RemoveAll<IContactRequestRepository>();
            services.RemoveAll<IContactRequestNotifier>();
            services.RemoveAll<IInventoryRepository>();
            services.RemoveAll<IIdentityService>();

            services.AddSingleton<ICacheService, TestCacheService>();
            services.AddSingleton<IProductRepository, TestProductRepository>();
            services.AddSingleton<TestContactRequestRepository>();
            services.AddSingleton<IContactRequestRepository>(provider => provider.GetRequiredService<TestContactRequestRepository>());
            services.AddSingleton<IContactRequestNotifier, TestContactRequestNotifier>();
            services.AddSingleton<IInventoryRepository, TestInventoryRepository>();
            services.AddSingleton<IIdentityService, TestIdentityService>();
        });
    }
}
