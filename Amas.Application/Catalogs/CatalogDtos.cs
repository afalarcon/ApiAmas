using Amas.Application.Categories;

namespace Amas.Application.Catalogs;

public sealed record CatalogCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive,
    IReadOnlyList<CategoryImageDto> Images);

public sealed record CatalogImagesGroupDto(
    Guid CategoryId,
    string CategoryName,
    string CategorySlug,
    IReadOnlyList<CategoryImageDto> Images);

public sealed record CatalogWarmupDto(
    int Categories,
    int Images,
    int Products,
    DateTimeOffset CachedAt);

public sealed record CatalogProductDto(
    Guid Id,
    long ProductNumber,
    string Name,
    string Slug,
    string? Description,
    string? Sku,
    decimal Price,
    Guid? CategoryId,
    string? CategoryName,
    IReadOnlyList<CatalogProductCategoryDto> Categories,
    IReadOnlyList<CatalogProductImageDto> Images);

public sealed record CatalogProductCategoryDto(
    Guid CategoryId,
    string CategoryName,
    string CategorySlug,
    int SortOrder,
    bool IsFeatured);

public sealed record CatalogProductImageDto(
    Guid Id,
    string Url,
    string? AltText,
    int SortOrder,
    bool IsPrimary);
