using Amas.Domain.Core;

namespace Amas.Application.Abstractions;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> ListByCategoryAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductCategory?> GetProductCategoryAsync(Guid categoryId, Guid productId, CancellationToken cancellationToken);
    Task<int> GetNextCategoryProductSortOrderAsync(Guid categoryId, CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    Task AddProductCategoryAsync(ProductCategory productCategory, CancellationToken cancellationToken);
    Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> SkuExistsAsync(string sku, Guid? excludingId, CancellationToken cancellationToken);
    void Remove(Product product);
    void RemoveProductCategory(ProductCategory productCategory);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
