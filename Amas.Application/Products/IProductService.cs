using Amas.Application.Common;

namespace Amas.Application.Products;

public interface IProductService
{
    Task<Result<IReadOnlyList<ProductDto>>> ListAsync(CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<ProductDto>>> ListByCategoryAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task<Result<ProductDto>> AddToCategoryAsync(Guid categoryId, Guid productId, CancellationToken cancellationToken);
    Task<Result> RemoveFromCategoryAsync(Guid categoryId, Guid productId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<ProductDto>>> ReorderCategoryProductsAsync(
        Guid categoryId,
        IReadOnlyList<ReorderCategoryProductRequest> request,
        CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
