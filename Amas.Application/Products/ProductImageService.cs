using Amas.Application.Abstractions;
using Amas.Application.Common;
using Amas.Domain.Core;

namespace Amas.Application.Products;

public sealed class ProductImageService(
    IProductImageRepository productImages,
    IImageStorage imageStorage,
    ICacheService cache) : IProductImageService
{
    private static readonly TimeSpan ProductImagesTtl = TimeSpan.FromMinutes(30);

    public async Task<Result<IReadOnlyList<ProductImageDto>>> ListAsync(Guid productId, CancellationToken cancellationToken)
    {
        if (!await productImages.ProductExistsAsync(productId, cancellationToken))
        {
            return Result<IReadOnlyList<ProductImageDto>>.Failure("Product not found.");
        }

        var cacheKey = CacheKeys.ProductImages(productId);
        var cached = await cache.GetAsync<IReadOnlyList<ProductImageDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result<IReadOnlyList<ProductImageDto>>.Success(cached);
        }

        var items = (await productImages.ListByProductAsync(productId, cancellationToken)).Select(Map).ToList();
        await cache.SetAsync(cacheKey, items, ProductImagesTtl, cancellationToken);

        return Result<IReadOnlyList<ProductImageDto>>.Success(items);
    }

    public async Task<Result<IReadOnlyList<ProductImageDto>>> UploadAsync(
        Guid productId,
        IReadOnlyList<UploadProductImageFile> files,
        CancellationToken cancellationToken)
    {
        if (files.Count == 0)
        {
            return Result<IReadOnlyList<ProductImageDto>>.Failure("At least one image is required.");
        }

        if (!await productImages.ProductExistsAsync(productId, cancellationToken))
        {
            return Result<IReadOnlyList<ProductImageDto>>.Failure("Product not found.");
        }

        var existing = await productImages.ListByProductAsync(productId, cancellationToken);
        var nextSortOrder = existing.Count == 0 ? 1 : existing.Max(x => x.SortOrder) + 1;
        var shouldSetPrimary = existing.All(x => !x.IsPrimary);
        var images = new List<ProductImage>(files.Count);

        foreach (var file in files)
        {
            if (file.SizeBytes <= 0)
            {
                return Result<IReadOnlyList<ProductImageDto>>.Failure($"Image '{file.FileName}' is empty.");
            }

            StoredImageFile stored;
            try
            {
                stored = await imageStorage.SaveAsync(
                    new ImageStorageRequest(
                        $"products/{productId:N}",
                        file.FileName,
                        file.ContentType,
                        file.SizeBytes,
                        file.Content),
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return Result<IReadOnlyList<ProductImageDto>>.Failure(ex.Message);
            }

            images.Add(new ProductImage
            {
                ProductId = productId,
                Url = stored.Url,
                StoragePath = stored.StoragePath,
                StorageProvider = stored.StorageProvider,
                FileName = stored.FileName,
                ContentType = stored.ContentType,
                SizeBytes = stored.SizeBytes,
                AltText = string.IsNullOrWhiteSpace(file.AltText) ? null : file.AltText.Trim(),
                SortOrder = nextSortOrder++,
                IsPrimary = shouldSetPrimary && images.Count == 0
            });
        }

        await productImages.AddRangeAsync(images, cancellationToken);
        await productImages.SaveChangesAsync(cancellationToken);
        await InvalidateProductImageCaches(productId, cancellationToken);

        var result = (await productImages.ListByProductAsync(productId, cancellationToken)).Select(Map).ToList();
        return Result<IReadOnlyList<ProductImageDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<ProductImageDto>>> ReorderAsync(
        Guid productId,
        IReadOnlyList<ReorderProductImageRequest> request,
        CancellationToken cancellationToken)
    {
        if (request.Count == 0)
        {
            return Result<IReadOnlyList<ProductImageDto>>.Failure("At least one image order is required.");
        }

        var images = (await productImages.ListByProductAsync(productId, cancellationToken)).ToList();
        if (images.Count == 0)
        {
            return Result<IReadOnlyList<ProductImageDto>>.Failure("Product images not found.");
        }

        var requestedIds = request.Select(x => x.ImageId).ToHashSet();
        if (requestedIds.Count != request.Count || request.Any(x => images.All(image => image.Id != x.ImageId)))
        {
            return Result<IReadOnlyList<ProductImageDto>>.Failure("Invalid image order.");
        }

        foreach (var order in request)
        {
            var image = images.First(x => x.Id == order.ImageId);
            image.SortOrder = order.SortOrder;
        }

        await productImages.SaveChangesAsync(cancellationToken);
        await InvalidateProductImageCaches(productId, cancellationToken);

        return Result<IReadOnlyList<ProductImageDto>>.Success(images.OrderBy(x => x.SortOrder).Select(Map).ToList());
    }

    public async Task<Result<ProductImageDto>> SetPrimaryAsync(Guid productId, Guid imageId, CancellationToken cancellationToken)
    {
        var images = (await productImages.ListByProductAsync(productId, cancellationToken)).ToList();
        var selected = images.FirstOrDefault(x => x.Id == imageId);
        if (selected is null)
        {
            return Result<ProductImageDto>.Failure("Product image not found.");
        }

        foreach (var image in images)
        {
            image.IsPrimary = image.Id == imageId;
        }

        await productImages.SaveChangesAsync(cancellationToken);
        await InvalidateProductImageCaches(productId, cancellationToken);

        return Result<ProductImageDto>.Success(Map(selected));
    }

    public async Task<Result> DeleteAsync(Guid productId, Guid imageId, CancellationToken cancellationToken)
    {
        var image = await productImages.GetByIdAsync(productId, imageId, cancellationToken);
        if (image is null)
        {
            return Result.Failure("Product image not found.");
        }

        var wasPrimary = image.IsPrimary;
        productImages.Remove(image);
        await productImages.SaveChangesAsync(cancellationToken);

        if (wasPrimary)
        {
            var remaining = (await productImages.ListByProductAsync(productId, cancellationToken)).ToList();
            var nextPrimary = remaining.OrderBy(x => x.SortOrder).FirstOrDefault();
            if (nextPrimary is not null)
            {
                nextPrimary.IsPrimary = true;
                await productImages.SaveChangesAsync(cancellationToken);
            }
        }

        await InvalidateProductImageCaches(productId, cancellationToken);
        return Result.Success();
    }

    private async Task InvalidateProductImageCaches(Guid productId, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(CacheKeys.ProductImages(productId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.Products, cancellationToken);
        await cache.RemoveAsync(CacheKeys.CatalogProductsAll, cancellationToken);
        await cache.RemoveAsync(CacheKeys.Catalogs, cancellationToken);
        await cache.RemoveAsync(CacheKeys.CatalogImages, cancellationToken);

        var categoryIds = await productImages.GetProductCategoryIdsAsync(productId, cancellationToken);
        foreach (var categoryId in categoryIds)
        {
            await cache.RemoveAsync(CacheKeys.CategoryProducts(categoryId), cancellationToken);
            await cache.RemoveAsync(CacheKeys.CatalogProductsByCategory(categoryId), cancellationToken);
        }
    }

    private static ProductImageDto Map(ProductImage image) =>
        new(
            image.Id,
            image.ProductId,
            image.Url,
            image.FileName,
            image.ContentType,
            image.SizeBytes,
            image.AltText,
            image.SortOrder,
            image.IsPrimary,
            image.StorageProvider);
}
