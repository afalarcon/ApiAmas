using Amas.Application.Abstractions;
using Amas.Application.Common;
using Amas.Domain.Core;
using FluentValidation;

namespace Amas.Application.Products;

public sealed class ProductService(
    IProductRepository products,
    ICacheService cache,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator) : IProductService
{
    private static readonly TimeSpan ProductsTtl = TimeSpan.FromMinutes(10);

    public async Task<Result<IReadOnlyList<ProductDto>>> ListAsync(CancellationToken cancellationToken)
    {
        var cached = await cache.GetAsync<IReadOnlyList<ProductDto>>(CacheKeys.Products, cancellationToken);
        if (cached is not null)
        {
            return Result<IReadOnlyList<ProductDto>>.Success(cached);
        }

        var items = (await products.ListAsync(cancellationToken)).Select(Map).ToList();
        await cache.SetAsync(CacheKeys.Products, items, ProductsTtl, cancellationToken);

        return Result<IReadOnlyList<ProductDto>>.Success(items);
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> ListByCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        if (!await products.CategoryExistsAsync(categoryId, cancellationToken))
        {
            return Result<IReadOnlyList<ProductDto>>.Failure("Category not found.");
        }

        var cacheKey = CacheKeys.CategoryProducts(categoryId);
        var cached = await cache.GetAsync<IReadOnlyList<ProductDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result<IReadOnlyList<ProductDto>>.Success(cached);
        }

        var items = (await products.ListByCategoryAsync(categoryId, cancellationToken)).Select(Map).ToList();
        await cache.SetAsync(cacheKey, items, ProductsTtl, cancellationToken);

        return Result<IReadOnlyList<ProductDto>>.Success(items);
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken);
        return product is null
            ? Result<ProductDto>.Failure("Product not found.")
            : Result<ProductDto>.Success(Map(product));
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var validation = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<ProductDto>.Failure(validation.Errors[0].ErrorMessage);
        }

        if (request.CategoryId.HasValue && !await products.CategoryExistsAsync(request.CategoryId.Value, cancellationToken))
        {
            return Result<ProductDto>.Failure("Category not found.");
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug) ? SlugHelper.From(request.Name) : SlugHelper.From(request.Slug);
        if (await products.SlugExistsAsync(slug, null, cancellationToken))
        {
            return Result<ProductDto>.Failure("Product slug already exists.");
        }

        var sku = request.Sku?.Trim();
        if (!string.IsNullOrWhiteSpace(sku) && await products.SkuExistsAsync(sku, null, cancellationToken))
        {
            return Result<ProductDto>.Failure("Product SKU already exists.");
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            Sku = sku,
            Price = request.Price,
            IsActive = request.IsActive,
            CategoryId = request.CategoryId
        };

        await products.AddAsync(product, cancellationToken);
        if (request.CategoryId.HasValue)
        {
            await products.AddProductCategoryAsync(new ProductCategory
            {
                Product = product,
                CategoryId = request.CategoryId.Value,
                SortOrder = await products.GetNextCategoryProductSortOrderAsync(request.CategoryId.Value, cancellationToken)
            }, cancellationToken);
        }

        await products.SaveChangesAsync(cancellationToken);
        await InvalidateProductCaches(request.CategoryId, cancellationToken);

        return Result<ProductDto>.Success(Map(product));
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var validation = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<ProductDto>.Failure(validation.Errors[0].ErrorMessage);
        }

        var product = await products.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.Failure("Product not found.");
        }

        if (request.CategoryId.HasValue && !await products.CategoryExistsAsync(request.CategoryId.Value, cancellationToken))
        {
            return Result<ProductDto>.Failure("Category not found.");
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug) ? SlugHelper.From(request.Name) : SlugHelper.From(request.Slug);
        if (await products.SlugExistsAsync(slug, id, cancellationToken))
        {
            return Result<ProductDto>.Failure("Product slug already exists.");
        }

        var sku = request.Sku?.Trim();
        if (!string.IsNullOrWhiteSpace(sku) && await products.SkuExistsAsync(sku, id, cancellationToken))
        {
            return Result<ProductDto>.Failure("Product SKU already exists.");
        }

        var previousCategoryId = product.CategoryId;
        product.Name = request.Name.Trim();
        product.Slug = slug;
        product.Description = request.Description?.Trim();
        product.Sku = sku;
        product.Price = request.Price;
        product.IsActive = request.IsActive;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.CategoryId.HasValue)
        {
            var existingLink = await products.GetProductCategoryAsync(request.CategoryId.Value, id, cancellationToken);
            if (existingLink is null)
            {
                await products.AddProductCategoryAsync(new ProductCategory
                {
                    ProductId = id,
                    CategoryId = request.CategoryId.Value,
                    SortOrder = await products.GetNextCategoryProductSortOrderAsync(request.CategoryId.Value, cancellationToken)
                }, cancellationToken);
            }
        }

        await products.SaveChangesAsync(cancellationToken);
        await InvalidateProductCaches(previousCategoryId, cancellationToken);
        await InvalidateProductCaches(request.CategoryId, cancellationToken);

        return Result<ProductDto>.Success(Map(product));
    }

    public async Task<Result<ProductDto>> AddToCategoryAsync(Guid categoryId, Guid productId, CancellationToken cancellationToken)
    {
        if (!await products.CategoryExistsAsync(categoryId, cancellationToken))
        {
            return Result<ProductDto>.Failure("Category not found.");
        }

        var product = await products.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.Failure("Product not found.");
        }

        if (product.ProductCategories.All(x => x.CategoryId != categoryId))
        {
            await products.AddProductCategoryAsync(new ProductCategory
            {
                ProductId = productId,
                CategoryId = categoryId,
                SortOrder = await products.GetNextCategoryProductSortOrderAsync(categoryId, cancellationToken)
            }, cancellationToken);
        }

        if (!product.CategoryId.HasValue)
        {
            product.CategoryId = categoryId;
        }

        await products.SaveChangesAsync(cancellationToken);
        await InvalidateProductCaches(categoryId, cancellationToken);

        var updatedProduct = await products.GetByIdAsync(productId, cancellationToken);
        return Result<ProductDto>.Success(Map(updatedProduct ?? product));
    }

    public async Task<Result> RemoveFromCategoryAsync(Guid categoryId, Guid productId, CancellationToken cancellationToken)
    {
        var productCategory = await products.GetProductCategoryAsync(categoryId, productId, cancellationToken);
        if (productCategory is null)
        {
            return Result.Failure("Product category not found.");
        }

        if (productCategory.Product.CategoryId == categoryId)
        {
            productCategory.Product.CategoryId = productCategory.Product.ProductCategories
                .Where(x => x.CategoryId != categoryId)
                .OrderBy(x => x.SortOrder)
                .Select(x => (Guid?)x.CategoryId)
                .FirstOrDefault();
        }

        products.RemoveProductCategory(productCategory);
        await products.SaveChangesAsync(cancellationToken);
        await InvalidateProductCaches(categoryId, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> ReorderCategoryProductsAsync(
        Guid categoryId,
        IReadOnlyList<ReorderCategoryProductRequest> request,
        CancellationToken cancellationToken)
    {
        if (request.Count == 0)
        {
            return Result<IReadOnlyList<ProductDto>>.Failure("At least one product order is required.");
        }

        foreach (var order in request)
        {
            var productCategory = await products.GetProductCategoryAsync(categoryId, order.ProductId, cancellationToken);
            if (productCategory is null)
            {
                return Result<IReadOnlyList<ProductDto>>.Failure("Product category not found.");
            }

            productCategory.SortOrder = order.SortOrder;
        }

        await products.SaveChangesAsync(cancellationToken);
        await InvalidateProductCaches(categoryId, cancellationToken);

        var items = (await products.ListByCategoryAsync(categoryId, cancellationToken)).Select(Map).ToList();
        return Result<IReadOnlyList<ProductDto>>.Success(items);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result.Failure("Product not found.");
        }

        products.Remove(product);
        await products.SaveChangesAsync(cancellationToken);
        await InvalidateProductCaches(product.CategoryId, cancellationToken);

        return Result.Success();
    }

    private async Task InvalidateProductCaches(Guid? categoryId, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(CacheKeys.Products, cancellationToken);
        await cache.RemoveAsync(CacheKeys.CatalogProductsAll, cancellationToken);
        await cache.RemoveAsync(CacheKeys.Catalogs, cancellationToken);
        await cache.RemoveAsync(CacheKeys.CatalogImages, cancellationToken);

        if (categoryId.HasValue)
        {
            await cache.RemoveAsync(CacheKeys.CategoryProducts(categoryId.Value), cancellationToken);
            await cache.RemoveAsync(CacheKeys.CatalogProductsByCategory(categoryId.Value), cancellationToken);
        }
    }

    private static ProductDto Map(Product product) => new(
        product.Id,
        product.ProductNumber,
        product.Name,
        product.Slug,
        product.Description,
        product.Sku,
        product.Price,
        product.IsActive,
        product.CategoryId,
        product.Category?.Name,
        product.ProductCategories
            .OrderBy(category => category.SortOrder)
            .Select(category => new ProductCategoryDto(
                category.CategoryId,
                category.Category?.Name ?? product.Category?.Name ?? string.Empty,
                category.Category?.Slug ?? product.Category?.Slug ?? string.Empty,
                category.SortOrder,
                category.IsFeatured))
            .ToList(),
        product.Images
            .OrderByDescending(image => image.IsPrimary)
            .ThenBy(image => image.SortOrder)
            .Select(image => new ProductImageDto(
                image.Id,
                image.ProductId,
                image.Url,
                image.FileName,
                image.ContentType,
                image.SizeBytes,
                image.AltText,
                image.SortOrder,
                image.IsPrimary,
                image.StorageProvider))
            .ToList(),
        product.Images.OrderByDescending(image => image.IsPrimary).ThenBy(image => image.SortOrder).Select(image => image.Url).ToList());
}
