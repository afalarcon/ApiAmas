using Amas.Application.Abstractions;
using Amas.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Amas.Infrastructure.Persistence.Repositories;

public sealed class ProductImageRepository(AmasDbContext dbContext) : IProductImageRepository
{
    public Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(x => x.Id == productId, cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetProductCategoryIdsAsync(Guid productId, CancellationToken cancellationToken)
    {
        var categoryIds = await dbContext.ProductCategories
            .Where(x => x.ProductId == productId)
            .Select(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        var primaryCategoryId = await dbContext.Products
            .Where(x => x.Id == productId && x.CategoryId.HasValue)
            .Select(x => x.CategoryId!.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (primaryCategoryId != Guid.Empty)
        {
            categoryIds.Add(primaryCategoryId);
        }

        return categoryIds.Distinct().ToList();
    }

    public async Task<IReadOnlyList<ProductImage>> ListByProductAsync(Guid productId, CancellationToken cancellationToken) =>
        await dbContext.ProductImages
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<ProductImage?> GetByIdAsync(Guid productId, Guid imageId, CancellationToken cancellationToken) =>
        dbContext.ProductImages.FirstOrDefaultAsync(x => x.ProductId == productId && x.Id == imageId, cancellationToken);

    public async Task<int> GetNextSortOrderAsync(Guid productId, CancellationToken cancellationToken)
    {
        var maxSortOrder = await dbContext.ProductImages
            .Where(x => x.ProductId == productId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken);

        return (maxSortOrder ?? 0) + 1;
    }

    public Task AddRangeAsync(IReadOnlyList<ProductImage> images, CancellationToken cancellationToken) =>
        dbContext.ProductImages.AddRangeAsync(images, cancellationToken);

    public void Remove(ProductImage image) => dbContext.ProductImages.Remove(image);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
