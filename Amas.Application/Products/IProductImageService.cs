using Amas.Application.Common;

namespace Amas.Application.Products;

public interface IProductImageService
{
    Task<Result<IReadOnlyList<ProductImageDto>>> ListAsync(Guid productId, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<ProductImageDto>>> UploadAsync(
        Guid productId,
        IReadOnlyList<UploadProductImageFile> files,
        CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<ProductImageDto>>> ReorderAsync(
        Guid productId,
        IReadOnlyList<ReorderProductImageRequest> request,
        CancellationToken cancellationToken);
    Task<Result<ProductImageDto>> SetPrimaryAsync(Guid productId, Guid imageId, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(Guid productId, Guid imageId, CancellationToken cancellationToken);
}
