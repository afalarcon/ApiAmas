using Amas.Domain.Core;

namespace Amas.Application.Abstractions;

public interface IProductImageRepository
{
    Task<bool> ProductExistsAsync(Guid productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> GetProductCategoryIdsAsync(Guid productId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductImage>> ListByProductAsync(Guid productId, CancellationToken cancellationToken);
    Task<ProductImage?> GetByIdAsync(Guid productId, Guid imageId, CancellationToken cancellationToken);
    Task<int> GetNextSortOrderAsync(Guid productId, CancellationToken cancellationToken);
    Task AddRangeAsync(IReadOnlyList<ProductImage> images, CancellationToken cancellationToken);
    void Remove(ProductImage image);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
