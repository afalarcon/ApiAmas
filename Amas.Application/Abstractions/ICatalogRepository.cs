using Amas.Domain.Core;

namespace Amas.Application.Abstractions;

public interface ICatalogRepository
{
    Task<IReadOnlyList<Category>> ListCategoriesWithImagesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> ListProductsAsync(Guid? categoryId, CancellationToken cancellationToken);
}
