using Amas.Application.Abstractions;
using Amas.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Amas.Infrastructure.Persistence.Repositories;

public sealed class CatalogRepository(AmasDbContext dbContext) : ICatalogRepository
{
    public async Task<IReadOnlyList<Category>> ListCategoriesWithImagesAsync(CancellationToken cancellationToken) =>
        await dbContext.Categories
            .AsNoTracking()
            .Include(x => x.Images)
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> ListProductsAsync(Guid? categoryId, CancellationToken cancellationToken) =>
        await dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductCategories)
                .ThenInclude(x => x.Category)
            .Include(x => x.Images)
            .Where(x => x.IsActive)
            .Where(x => x.Images.Any())
            .Where(x => !categoryId.HasValue || x.CategoryId == categoryId.Value || x.ProductCategories.Any(link => link.CategoryId == categoryId.Value))
            .OrderBy(x => categoryId.HasValue
                ? x.ProductCategories
                    .Where(link => link.CategoryId == categoryId.Value)
                    .Select(link => link.SortOrder)
                    .FirstOrDefault()
                : 0)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
}
