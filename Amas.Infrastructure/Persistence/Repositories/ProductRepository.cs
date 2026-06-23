using Amas.Application.Abstractions;
using Amas.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Amas.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(AmasDbContext dbContext) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductCategories)
                .ThenInclude(x => x.Category)
            .Include(x => x.Images)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> ListByCategoryAsync(Guid categoryId, CancellationToken cancellationToken) =>
        await dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductCategories)
                .ThenInclude(x => x.Category)
            .Include(x => x.Images)
            .Where(x => x.IsActive && (x.CategoryId == categoryId || x.ProductCategories.Any(link => link.CategoryId == categoryId)))
            .OrderBy(x => x.ProductCategories
                .Where(link => link.CategoryId == categoryId)
                .Select(link => link.SortOrder)
                .FirstOrDefault())
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Products
            .Include(x => x.Category)
            .Include(x => x.ProductCategories)
                .ThenInclude(x => x.Category)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ProductCategory?> GetProductCategoryAsync(Guid categoryId, Guid productId, CancellationToken cancellationToken) =>
        dbContext.ProductCategories
            .Include(x => x.Product)
                .ThenInclude(x => x.Images)
            .Include(x => x.Product)
                .ThenInclude(x => x.ProductCategories)
                    .ThenInclude(x => x.Category)
            .FirstOrDefaultAsync(x => x.CategoryId == categoryId && x.ProductId == productId, cancellationToken);

    public async Task<int> GetNextCategoryProductSortOrderAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var maxSortOrder = await dbContext.ProductCategories
            .Where(x => x.CategoryId == categoryId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken);

        return (maxSortOrder ?? 0) + 1;
    }

    public Task AddAsync(Product product, CancellationToken cancellationToken) =>
        dbContext.Products.AddAsync(product, cancellationToken).AsTask();

    public Task AddProductCategoryAsync(ProductCategory productCategory, CancellationToken cancellationToken) =>
        dbContext.ProductCategories.AddAsync(productCategory, cancellationToken).AsTask();

    public Task<bool> CategoryExistsAsync(Guid categoryId, CancellationToken cancellationToken) =>
        dbContext.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(x => x.Slug == slug && (!excludingId.HasValue || x.Id != excludingId.Value), cancellationToken);

    public Task<bool> SkuExistsAsync(string sku, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(x => x.Sku == sku && (!excludingId.HasValue || x.Id != excludingId.Value), cancellationToken);

    public void Remove(Product product) => dbContext.Products.Remove(product);

    public void RemoveProductCategory(ProductCategory productCategory) => dbContext.ProductCategories.Remove(productCategory);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
