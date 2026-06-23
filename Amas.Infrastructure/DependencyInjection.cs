using Amas.Application.Abstractions;
using Amas.Infrastructure.Caching;
using Amas.Infrastructure.Notifications;
using Amas.Infrastructure.Persistence;
using Amas.Infrastructure.Persistence.Repositories;
using Amas.Infrastructure.Redis;
using Amas.Infrastructure.Storage;
using Amas.Application.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Amas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var postgres = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is required.");

        services.AddDbContext<AmasDbContext>(options => options.UseNpgsql(postgres));

        var redisOptions = configuration.GetSection("Redis").Get<RedisOptions>() ?? new RedisOptions();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = RedisConnectionStringBuilder.Build(redisOptions);
            options.InstanceName = "amas:";
        });

        services.Configure<MediaStorageOptions>(options => configuration.GetSection("MediaStorage").Bind(options));
        services.Configure<ContactWebhookOptions>(options => configuration.GetSection("ContactWebhook").Bind(options));
        var openAiOptions = configuration.GetSection("OpenAI").Get<OpenAiInvoiceOptions>() ?? new OpenAiInvoiceOptions();
        services.AddSingleton(openAiOptions);
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryImageRepository, CategoryImageRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IContactRequestRepository, ContactRequestRepository>();
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IInvoiceImportRepository, InvoiceImportRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddSingleton<IImageStorage, LocalImageStorage>();
        services.AddSingleton<IInvoiceFileStorage, LocalInvoiceFileStorage>();
        services.AddSingleton<ICacheService, DistributedCacheService>();
        services.AddHttpClient<IContactRequestNotifier, WebhookContactRequestNotifier>();

        return services;
    }
}
