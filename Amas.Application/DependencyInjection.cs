using Amas.Application.Categories;
using Amas.Application.Catalogs;
using Amas.Application.Configurations;
using Amas.Application.ContactRequests;
using Amas.Application.Identity;
using Amas.Application.Inventory;
using Amas.Application.Products;
using Amas.Application.Suppliers;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Amas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductImageService, ProductImageService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICategoryImageService, CategoryImageService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IContactRequestService, ContactRequestService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IInvoiceImportService, InvoiceImportService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddSingleton<IInvoiceExtractionMonitor, InvoiceExtractionMonitor>();
        services.AddScoped<BasicInvoiceDocumentReader>();
        services.AddScoped<Amas.Application.Abstractions.IInvoiceDocumentReader, OpenAiInvoiceDocumentReader>();

        return services;
    }
}
