using Amas.Application.Abstractions;
using Amas.Domain.Core;

namespace Amas.Api.Tests;

internal sealed class TestProductRepository : IProductRepository
{
    private readonly List<Product> products = [];

    public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Product>>(products.OrderBy(x => x.Name).ToList());

    public Task<IReadOnlyList<Product>> ListByCategoryAsync(Guid categoryId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Product>>(products
            .Where(x => x.CategoryId == categoryId || x.ProductCategories.Any(category => category.CategoryId == categoryId))
            .OrderBy(x => x.Name)
            .ToList());

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(products.FirstOrDefault(x => x.Id == id));

    public Task<ProductCategory?> GetProductCategoryAsync(Guid categoryId, Guid productId, CancellationToken cancellationToken) =>
        Task.FromResult(products
            .SelectMany(x => x.ProductCategories)
            .FirstOrDefault(x => x.CategoryId == categoryId && x.ProductId == productId));

    public Task<int> GetNextCategoryProductSortOrderAsync(Guid categoryId, CancellationToken cancellationToken) =>
        Task.FromResult(products
            .SelectMany(x => x.ProductCategories)
            .Where(x => x.CategoryId == categoryId)
            .Select(x => (int?)x.SortOrder)
            .Max() ?? 1);

    public Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        products.Add(product);
        return Task.CompletedTask;
    }

    public Task AddProductCategoryAsync(ProductCategory productCategory, CancellationToken cancellationToken)
    {
        var product = productCategory.Product ?? products.FirstOrDefault(x => x.Id == productCategory.ProductId);
        product?.ProductCategories.Add(productCategory);
        return Task.CompletedTask;
    }

    public Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken) =>
        Task.FromResult(false);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        Task.FromResult(products.Any(x => x.Slug == slug && (!excludingId.HasValue || x.Id != excludingId.Value)));

    public Task<bool> SkuExistsAsync(string sku, Guid? excludingId, CancellationToken cancellationToken) =>
        Task.FromResult(products.Any(x => x.Sku == sku && (!excludingId.HasValue || x.Id != excludingId.Value)));

    public void Remove(Product product) => products.Remove(product);

    public void RemoveProductCategory(ProductCategory productCategory) =>
        productCategory.Product.ProductCategories.Remove(productCategory);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
