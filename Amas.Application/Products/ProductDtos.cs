namespace Amas.Application.Products;

public sealed record ProductDto(
    Guid Id,
    long ProductNumber,
    string Name,
    string Slug,
    string? Description,
    string? Sku,
    decimal Price,
    bool IsActive,
    Guid? CategoryId,
    string? CategoryName,
    IReadOnlyList<ProductCategoryDto> Categories,
    IReadOnlyList<ProductImageDto> Images,
    IReadOnlyList<string> ImageUrls);

public sealed record ProductCategoryDto(
    Guid CategoryId,
    string CategoryName,
    string CategorySlug,
    int SortOrder,
    bool IsFeatured);

public sealed record CreateProductRequest(
    string Name,
    string? Slug,
    string? Description,
    string? Sku,
    decimal Price,
    bool IsActive,
    Guid? CategoryId);

public sealed record ReorderCategoryProductRequest(Guid ProductId, int SortOrder);

public sealed record UpdateProductRequest(
    string Name,
    string? Slug,
    string? Description,
    string? Sku,
    decimal Price,
    bool IsActive,
    Guid? CategoryId);
